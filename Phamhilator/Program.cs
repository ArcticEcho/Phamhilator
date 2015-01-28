using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Threading;
using System.Diagnostics;
using System.Globalization;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using ChatExchangeDotNet;
using WebSocketSharp;



namespace Phamhilator
{
    public class Program
    {
        private static Thread postCatcherThread;
        private static DateTime requestedDieTime;
        private static MessageHandler messageHandler;
        //private static ReportLog log;
        //private static UserAccess userAccess;
        //private static BannedUsers bannedUsers;
        private static Client chatClient;
        private static ActiveRooms roomsToJoin;
        //private static Room primaryRoom;
        //private static List<Room> secondaryRooms;
        //private static Dictionary<FilterConfig, BlackFilter> blackFilters;
        //private static Dictionary<FilterConfig, WhiteFilter> whiteFilters;
        //private static BadTags badTags;



        static void Main(string[] args)
        {
            Console.Title = "Phamhilator";
            AppDomain.CurrentDomain.UnhandledException += GlobalExceptionHandler;
            AppDomain.CurrentDomain.DomainUnload += (o, oo) => Close();

            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
            CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;

            InitialiseCore();

            var credMan = new CredManager();
            var success = false;

            //if (String.IsNullOrEmpty(credMan.Email) || String.IsNullOrEmpty(credMan.Password))
            //{
                success = TryManualLogin(credMan);
            //}
            //else
            //{
            //    success = TryAutoLogin(credMan);
            //}

            if (!success)
            {
                Console.WriteLine("\n\nPress any key to close Yam...");
                Console.ReadKey(true);
                return;
            }

            JoinRooms();

            Config.IsRunning = true;
            Stats.UpTime = DateTime.UtcNow;

            Config.PrimaryRoom.PostMessage("`Phamhilator™ started.`");

            InitialiseSocket();
        }



        # region Private static auth/initialisation methods.

        private static void InitialiseCore()
        {
            Console.Write("Loading log...");

            Config.Log = new ReportLog();

            Console.Write("done.\nLoading config data...");

            Config.Core = new Pham();
            roomsToJoin = new ActiveRooms();
            Config.UserAccess = new UserAccess("meta.stackexchange.com", 773);
            Config.BannedUsers = new BannedUsers(Config.UserAccess);
            Stats.PostedReports = new List<Report>();
            messageHandler = new MessageHandler();

            Console.Write("done.\nLoading bad tag definitions...");

            Config.BadTags = new BadTags();

            Console.Write("done.\nLoading black terms...");

            Config.BlackFilters = new Dictionary<FilterConfig, BlackFilter>()
            {
                { new FilterConfig(FilterClass.QuestionTitleName, FilterType.Black), new BlackFilter(FilterClass.QuestionTitleName) },
                { new FilterConfig(FilterClass.QuestionTitleOff, FilterType.Black), new BlackFilter(FilterClass.QuestionTitleOff) },
                { new FilterConfig(FilterClass.QuestionTitleSpam, FilterType.Black), new BlackFilter(FilterClass.QuestionTitleSpam) },
                { new FilterConfig(FilterClass.QuestionTitleLQ, FilterType.Black), new BlackFilter(FilterClass.QuestionTitleLQ) },

                { new FilterConfig(FilterClass.QuestionBodySpam, FilterType.Black), new BlackFilter(FilterClass.QuestionBodySpam) },
                { new FilterConfig(FilterClass.QuestionBodyLQ, FilterType.Black), new BlackFilter(FilterClass.QuestionBodyLQ) },
                { new FilterConfig(FilterClass.QuestionBodyOff, FilterType.Black), new BlackFilter(FilterClass.QuestionBodyOff) },

                { new FilterConfig(FilterClass.AnswerSpam, FilterType.Black), new BlackFilter(FilterClass.AnswerSpam) },
                { new FilterConfig(FilterClass.AnswerLQ, FilterType.Black), new BlackFilter(FilterClass.AnswerLQ) },
                { new FilterConfig(FilterClass.AnswerOff, FilterType.Black), new BlackFilter(FilterClass.AnswerOff) },
                { new FilterConfig(FilterClass.AnswerName, FilterType.Black), new BlackFilter(FilterClass.AnswerName) }
            };

            Console.Write("done.\nLoading white terms...");

            Config.WhiteFilters = new Dictionary<FilterConfig, WhiteFilter>()
            {
                { new FilterConfig(FilterClass.QuestionTitleName, FilterType.White), new WhiteFilter(FilterClass.QuestionTitleName) },
                { new FilterConfig(FilterClass.QuestionTitleOff, FilterType.White), new WhiteFilter(FilterClass.QuestionTitleOff) },
                { new FilterConfig(FilterClass.QuestionTitleSpam, FilterType.White), new WhiteFilter(FilterClass.QuestionTitleSpam) },
                { new FilterConfig(FilterClass.QuestionTitleLQ, FilterType.White), new WhiteFilter(FilterClass.QuestionTitleLQ) },

                { new FilterConfig(FilterClass.QuestionBodySpam, FilterType.White), new WhiteFilter(FilterClass.QuestionBodySpam) },
                { new FilterConfig(FilterClass.QuestionBodyLQ, FilterType.White), new WhiteFilter(FilterClass.QuestionBodyLQ) },
                { new FilterConfig(FilterClass.QuestionBodyOff, FilterType.White), new WhiteFilter(FilterClass.QuestionBodyOff) },

                { new FilterConfig(FilterClass.AnswerSpam, FilterType.White), new WhiteFilter(FilterClass.AnswerSpam) },
                { new FilterConfig(FilterClass.AnswerLQ, FilterType.White), new WhiteFilter(FilterClass.AnswerLQ) },
                { new FilterConfig(FilterClass.AnswerOff, FilterType.White), new WhiteFilter(FilterClass.AnswerOff) },
                { new FilterConfig(FilterClass.AnswerName, FilterType.White), new WhiteFilter(FilterClass.AnswerName) }
            };

            Console.WriteLine("done.\n");
        }

        private static bool TryManualLogin(CredManager credMan)
        {
            Console.WriteLine("Please enter your Stack Exchange OpenID credentials.\n");

            while (true)
            {
                Console.Write("Email: ");
                var email = Console.ReadLine();

                Console.Write("Password: ");
                var password = Console.ReadLine();

                try
                {
                    Console.Write("\nAuthenticating...");

                    chatClient = new Client(email, password);

                    Console.Write("login successful!\nShall I remember your creds? ");

                    try
                    {
                        var remCreds = Console.ReadLine();

                        if (Regex.IsMatch(remCreds, @"(?i)^y(e[sp]|up|)?\s*$"))
                        {
                            credMan.Email = email;
                            credMan.Password = password;

                            Console.WriteLine("Creds successfully remembered!");
                        }
                    }
                    catch (Exception)
                    {
                        credMan.Email = "";
                        credMan.Password = "";

                        Console.WriteLine("Failed to save your creds (creds not remembered).");
                    }

                    return true;
                }
                catch (Exception)
                {
                    Console.WriteLine("failed to login.");

                    return false;
                }
            }
        }

        private static bool TryAutoLogin(CredManager credMan)
        {
            Console.WriteLine("Email: " + credMan.Email);
            Console.WriteLine("Password: " + credMan.Password);
            Console.WriteLine("\nPress the enter key to login...");
            Console.Read();

            try
            {
                Console.Write("Authenticating...");

                chatClient = new Client(credMan.Email, credMan.Password);

                Console.Write("login successful!\nShall I forget your creds?");

                try
                {
                    var clrCreds = Console.ReadLine();

                    if (Regex.IsMatch(clrCreds, @"(?i)^y(e[sp]|up|)?\s*$"))
                    {
                        credMan.Email = "";
                        credMan.Password = "";

                        Console.WriteLine("Creds successfully forgotten!");
                    }
                }
                catch (Exception)
                {
                    Console.WriteLine("Failed to forget your creds.");
                }
            }
            catch (Exception)
            {
                Console.WriteLine("failed to login.");

                return false;
            }

            return true;
        }

        private static void JoinRooms()
        {
            Console.Write("Joining primary room: " + roomsToJoin.PrimaryRoomUrl + "...");

            Config.PrimaryRoom = chatClient.JoinRoom(roomsToJoin.PrimaryRoomUrl);
            Config.PrimaryRoom.IgnoreOwnEvents = false;
            Config.PrimaryRoom.NewMessage += HandlePrimaryNewMessage;
            Config.PrimaryRoom.MessageEdited += (oldMessage, newMessage) => HandlePrimaryNewMessage(newMessage);

            Console.WriteLine("done.\nJoining secondary room(s):");

            Config.SecondaryRooms = new List<Room>();

            foreach (var roomUrl in roomsToJoin.SecondaryRoomUrls)
            {
                Console.Write("Room: " + roomUrl + "...");

                var secRoom = chatClient.JoinRoom(roomUrl);
                secRoom.IgnoreOwnEvents = false;
                secRoom.NewMessage += message => HandleSecondaryNewMessage(secRoom, message);
                secRoom.MessageEdited += (oldMessage, newMessage) => HandleSecondaryNewMessage(secRoom, newMessage);

                Config.SecondaryRooms.Add(secRoom);

                Console.WriteLine("done.");
            }
        }

        private static void InitialiseSocket()
        {
            postCatcherThread = new Thread(() =>
            {
                var socket = new WebSocket("ws://qa.sockets.stackexchange.com");

                socket.OnOpen += (o, oo) => socket.Send("155-questions-active");

                socket.OnMessage += (o, message) =>
                {
                    if (!Config.IsRunning) { return; }

                    try
                    {
                        var question = PostFetcher.GetQuestion(message);

                        if (Config.Log.Entries.All(p => p.PostUrl != question.Url))
                        {
                            var qResults = PostAnalyser.AnalyseQuestion(question);
                            var qMessage = MessageGenerator.GetQReport(qResults, question);

                            CheckSendReport(question, qMessage, qResults);
                        }

                        if (Config.FullScanEnabled)
                        {
                            var answers = PostFetcher.GetLatestAnswers(question);

                            foreach (var a in answers.Where(ans => Config.Log.Entries.All(p => p.PostUrl != ans.Url)))
                            {
                                var aResults = PostAnalyser.AnalyseAnswer(a);
                                var aMessage = MessageGenerator.GetPostReport(aResults, a);

                                CheckSendReport(a, aMessage, aResults);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Config.PrimaryRoom.PostMessage("Error:\n" + ex + "\n\nReceived message:\n" + message.Data);
                    }
                };

                socket.OnClose += (o, oo) =>
                {
                    if (Config.Shutdown) { return; }

                    Config.PrimaryRoom.PostMessage("`Warning: global post websocket has died. Attempting to restart...`");

                    InitialiseSocket();
                };

                socket.Connect();
            });

            postCatcherThread.Start();
        }

        # endregion

        private static void KillBot()
        {
            Console.WriteLine("Kill command issued, closing Pham...");

            Config.Shutdown = true;
            requestedDieTime = DateTime.UtcNow;

            var warningMessagePosted = false;

            while (postCatcherThread.IsAlive)
            {
                Thread.Sleep(250);

                if ((DateTime.UtcNow - requestedDieTime).TotalSeconds > 10 && !warningMessagePosted)
                {
                    Config.PrimaryRoom.PostMessage("`Warning: 10 seconds have past since the kill command was issued and the post catcher thread is still alive. Now forcing thread death...`");

                    warningMessagePosted = true;

                    postCatcherThread.Abort();
                }

                if (!postCatcherThread.IsAlive) { break; }
            }

            Thread.Sleep(5000);

            Config.Log.Dispose();
            Config.PrimaryRoom.PostMessage("`All threads have died. Kill successful!`");

            Thread.Sleep(5000);

            Environment.Exit(Environment.ExitCode);
        }

        private static void HandlePrimaryNewMessage(Message message)
        {
            Task.Factory.StartNew(() =>
            {
                if (message.Content == ">>kill-it-with-no-regrets-for-sure")
                {
                    ChatAction m;

                    if (Config.UserAccess.Owners.Any(user => user.ID == message.AuthorID))
                    {
                        m = new ChatAction(Config.PrimaryRoom, () =>
                        {
                            Config.PrimaryRoom.PostReply(message, "`Killing...`");
                            KillBot();
                        });
                    }
                    else
                    {
                        m = new ChatAction(Config.PrimaryRoom, () => Config.PrimaryRoom.PostReply(message, "`Access denied.`"));
                    }

                    messageHandler.QueueItem(m);
                }
                else
                {
                    var messages = CommandProcessor.ExacuteCommand(Config.PrimaryRoom, message);

                    if (messages == null || messages.Length == 0) { return; }

                    foreach (var m in messages.Where(m => !String.IsNullOrEmpty(m.Content)))
                    {
                        ChatAction action;

                        if (m.IsReply)
                        {
                            action = new ChatAction(Config.PrimaryRoom, () => Config.PrimaryRoom.PostReply(message, m.Content));
                        }
                        else
                        {
                            action = new ChatAction(Config.PrimaryRoom, () => Config.PrimaryRoom.PostMessage(m.Content));
                        }

                        messageHandler.QueueItem(action);
                    }
                }
            });
        }

        private static void HandleSecondaryNewMessage(Room room, Message message)
        {
            Task.Factory.StartNew(() =>
            {
                if (!CommandProcessor.IsValidCommand(room, message)) { return; }

                var messages = CommandProcessor.ExacuteCommand(room, message);

                if (messages == null || messages.Length == 0) { return; }

                foreach (var m in messages.Where(m => !String.IsNullOrEmpty(m.Content)))
                {
                    ChatAction action;

                    if (m.IsReply)
                    {
                        action = new ChatAction(room, () => room.PostReply(message, m.Content));
                    }
                    else
                    {
                        action = new ChatAction(room, () => room.PostMessage(m.Content));
                    }

                    messageHandler.QueueItem(action);
                }
            });
        }

        private static void CheckSendReport(Post p, string messageBody, PostAnalysis info)
        {
            Stats.TotalCheckedPosts++;

            if (p == null || String.IsNullOrEmpty(messageBody) || info == null) { return; }

            Message message = null;
            Report chatMessage = null;

            if (Stats.Spammers.Any(spammer => spammer.Name == p.AuthorName && spammer.Site == p.Site))
            {
                message = Config.PrimaryRoom.PostMessage("**Spam**" + messageBody);
                chatMessage = new Report { Message = message, Post = p, Analysis = info };
            }

            switch (info.Type)
            {
                case PostType.Offensive:
                {
                    message = Config.PrimaryRoom.PostMessage("**Offensive**" + messageBody);
                    chatMessage = new Report { Message = message, Post = p, Analysis = info };

                    break;
                }

                case PostType.BadUsername:
                {
                    message = Config.PrimaryRoom.PostMessage("**Bad Username**" + messageBody);
                    chatMessage = new Report { Message = message, Post = p, Analysis = info };

                    break;
                }

                case PostType.BadTagUsed:
                {
                    message = Config.PrimaryRoom.PostMessage("**Bad Tag(s) Used**" + messageBody);
                    chatMessage = new Report { Message = message, Post = p, Analysis = info };

                    break;
                }

                case PostType.LowQuality:
                {
                    message = Config.PrimaryRoom.PostMessage("**Low Quality**" + messageBody);
                    chatMessage = new Report { Message = message, Post = p, Analysis = info };

                    break;
                }

                case PostType.Spam:
                {
                    message = Config.PrimaryRoom.PostMessage("**Spam**" + messageBody);
                    chatMessage = new Report { Message = message, Post = p, Analysis = info };

                    break;
                }
            }

            if (message != null && chatMessage != null)
            {
                Config.Log.AddEntry(new LogItem
                {
                    ReportLink = "http://chat." + message.Host + "/transcript/message/" + message.ID,
                    PostUrl = p.Url,
                    Site = p.Site,
                    Title = p.Title,
                    Body = p.Body,
                    TimeStamp = DateTime.UtcNow,
                    ReportType = info.Type,
                    BlackTerms = info.BlackTermsFound.ToLogTerms().ToList(),
                    WhiteTerms = info.WhiteTermsFound.ToLogTerms().ToList()
                });
                Stats.PostedReports.Add(chatMessage);

                if (info.AutoTermsFound)
                {
                    foreach (var room in Config.SecondaryRooms)
                    {
                        room.PostMessage(chatMessage.Message.Content);
                    }
                }
            }

            Stats.PostsCaught++;
        }

        private static void GlobalExceptionHandler(object o, UnhandledExceptionEventArgs args)
        {
            //Clipboard.SetText(args.ExceptionObject.ToString());

            Config.PrimaryRoom.PostMessage(args.ExceptionObject.ToString());

            if (args.IsTerminating)
            {
                //MessageBox.Show(args.ExceptionObject + Environment.NewLine + Environment.NewLine + "The above error details have be copied to your clipboard. Now closing Pham...", "Oops...", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                //var res = MessageBox.Show(args.ExceptionObject + Environment.NewLine + Environment.NewLine + "The above error details have be copied to your clipboard. Do you wish to continue?", "Oops...", MessageBoxButton.YesNo, MessageBoxImage.Error);

                //if (res == MessageBoxResult.No) { Environment.Exit(Environment.ExitCode); }
            }
        }

        private static void Close()
        {
            Config.PrimaryRoom.PostMessage("`Phamhilator™ stopped.`");
        }

        # region UI Events

        //private void MetroWindow_Closing(object sender, CancelEventArgs e)
        //{
        //    statusL.Content = "Shutting Down...";

        //    e.Cancel = true;

        //    if (!GlobalInfo.BotRunning) { Environment.Exit(0); }

        //    Task.Factory.StartNew(() => GlobalInfo.PrimaryRoom.PostMessage("`Phamhilator™ stopped.`"));

        //    GlobalInfo.Shutdown = true;

        //    Thread.Sleep(3000);

        //    Task.Factory.StartNew(() => Dispatcher.Invoke(() => Environment.Exit(0)));
        //}

        //private void Button_Click(object sender, RoutedEventArgs e)
        //{
        //    if (String.IsNullOrEmpty(emailTB.Text) || String.IsNullOrEmpty(passwordTB.Text))
        //    {
        //        MessageBox.Show("Please fill out all fields.", "Phamhilator", MessageBoxButton.OK, MessageBoxImage.Exclamation);
        //    }

        //    progressBar.IsIndeterminate = true;
        //    ((Button)sender).IsEnabled = false;
        //    emailTB.IsEnabled = false;
        //    passwordTB.IsEnabled = false;
        //    remCredsCB.IsEnabled = false;
        //    loginTitleL.Content = "Logging In...";

        //    var user = emailTB.Text;
        //    var pass = passwordTB.Text;
        //    var remCreds = remCredsCB.IsChecked ?? false;

        //    Task.Factory.StartNew(() =>
        //    {
        //        try
        //        {
        //            GlobalInfo.ChatClient = new Client(user, pass);
        //        }
        //        catch (Exception ex)
        //        {
        //            Dispatcher.Invoke(() =>
        //            {
        //                progressBar.IsIndeterminate = false;
        //                ((Button)sender).IsEnabled = true;
        //                emailTB.IsEnabled = true;
        //                passwordTB.IsEnabled = true;
        //                remCredsCB.IsEnabled = true;
        //                loginTitleL.Content = "Could Not Login";
        //                Clipboard.SetText(ex.ToString());
        //            });

        //            return;
        //        }

        //        try
        //        {
        //            if (remCreds)
        //            {
        //                File.WriteAllText(DirectoryTools.GetCredsFile(), user + "¬" + pass);
        //            }
        //            else
        //            {
        //                File.WriteAllText(DirectoryTools.GetCredsFile(), "");
        //            }

        //            GlobalInfo.PrimaryRoom = GlobalInfo.ChatClient.JoinRoom("http://chat.meta.stackexchange.com/rooms/773/low-quality-posts-hq");
        //            GlobalInfo.PrimaryRoom.NewMessage += HandlePrimaryNewMessage;
        //            GlobalInfo.PrimaryRoom.MessageEdited += (oldMessage, newMessage) => HandlePrimaryNewMessage(newMessage);
        //            GlobalInfo.PrimaryRoom.IgnoreOwnEvents = false;

        //            GlobalInfo.ChatClient.JoinRoom("http://chat.meta.stackexchange.com/rooms/89/tavern-on-the-meta");//("http://chat.meta.stackexchange.com/rooms/651");

        //            for (var i = 0; i < GlobalInfo.ChatClient.Rooms.Count; i++)
        //            {
        //                if (GlobalInfo.ChatClient.Rooms[i].ID == GlobalInfo.PrimaryRoom.ID) { continue; }

        //                GlobalInfo.ChatClient.Rooms[i].NewMessage += message => HandleSecondaryNewMessage(GlobalInfo.ChatClient.Rooms.First(r => r.ID == message.RoomID), message);
        //                GlobalInfo.ChatClient.Rooms[i].MessageEdited += (oldMessage, newMessage) => HandleSecondaryNewMessage(GlobalInfo.ChatClient.Rooms.First(r => r.ID == newMessage.RoomID), newMessage);

        //                GlobalInfo.ChatClient.Rooms[i].IgnoreOwnEvents = false;
        //            }

        //            Dispatcher.Invoke(() =>
        //            {
        //                transContentC.Content = operationC;

        //                progressBar.IsIndeterminate = false;
        //            });
        //        }
        //        catch (Exception ex)
        //        {
        //            Dispatcher.Invoke(() =>
        //            {
        //                progressBar.IsIndeterminate = false;
        //                ((Button)sender).IsEnabled = true;
        //                emailTB.IsEnabled = true;
        //                passwordTB.IsEnabled = true;
        //                remCredsCB.IsEnabled = true;
        //                loginTitleL.Content = "An Error Occoured (See Clipboard for Details)";
        //                Clipboard.SetText(ex.ToString());
        //            });
        //        }
        //    });
        //}

        //private void Button_Click_2(object sender, RoutedEventArgs e)
        //{
        //    statusL.Content = "Monitoring Enabled";

        //    ((Button)sender).IsEnabled = false;

        //    messageHandler = new MessageHandler();
        //    GlobalInfo.BotRunning = true;

        //    InitialiseSocket();

        //    if (Debugger.IsAttached)
        //    {
        //        GlobalInfo.PrimaryRoom.PostMessage("`Phamhilator™ started` (**`debug mode`**)`.`");
        //    }
        //    else
        //    {
        //        GlobalInfo.PrimaryRoom.PostMessage("`Phamhilator™ started.`");
        //    }

        //    GlobalInfo.UpTime = DateTime.UtcNow;
        //}

        # endregion
    }
}
