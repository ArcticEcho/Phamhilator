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
using Phamhilator.Core;
using ChatExchangeDotNet;



namespace Phamhilator.UI
{
    public class Program
    {
        private static Thread postCatcherThread;
        private static MessageHandler messageHandler;
        private static Client chatClient;
        private static ActiveRooms roomsToJoin;
        private static RealtimePostSocket postSocket;


        static void Main(string[] args)
        {
            Console.Title = "Phamhilator";
            Console.WriteLine("Phamhilator.\nPress Ctrl + C to exit.\n");
            AppDomain.CurrentDomain.UnhandledException += (o, ex) => Config.PrimaryRoom.PostMessage("Error:\n" + ex.ExceptionObject.ToString());
            Console.CancelKeyPress += (o, oo) => Close();

            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
            CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;

            InitialiseCore();
            TryLogin();
            JoinRooms();

            Config.IsRunning = true;
            Stats.UpTime = DateTime.UtcNow;

            Config.PrimaryRoom.PostMessage("`Phamhilator™ started.`");
            Console.WriteLine("\nPhamhilator started.");

            InitialiseSocket();
        }



        # region Private static auth/initialisation methods.

        private static void InitialiseCore()
        {
            Console.Write("Loading log...");

            Config.Log = new ReportLog();

            Console.Write("done.\nLoading config data...");

            messageHandler = new MessageHandler();
            roomsToJoin = new ActiveRooms();
            Config.Core = new Pham();
            Stats.PostedReports = new List<Report>();
            Config.UserAccess = new UserAccess("meta.stackexchange.com", 773);
            Config.BannedUsers = new BannedUsers(Config.UserAccess);

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

        private static void TryLogin()
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

                    Console.WriteLine("login successful!");

                    return;
                }
                catch (Exception)
                {
                    Console.WriteLine("failed to login.");
                }
            }
        }

        private static void JoinRooms()
        {
            Console.Write("Joining primary room: " + roomsToJoin.PrimaryRoomUrl + "...");

            Config.PrimaryRoom = chatClient.JoinRoom(roomsToJoin.PrimaryRoomUrl);
            Config.PrimaryRoom.IgnoreOwnEvents = false;
            Config.PrimaryRoom.StripMentionFromMessages = false;
            Config.PrimaryRoom.NewMessage += HandlePrimaryNewMessage;
            Config.PrimaryRoom.MessageEdited += (oldMessage, newMessage) => HandlePrimaryNewMessage(newMessage);

            Console.WriteLine("done.\nJoining secondary room(s):");

            Config.SecondaryRooms = new List<Room>();

            foreach (var roomUrl in roomsToJoin.SecondaryRoomUrls)
            {
                Console.Write("Room: " + roomUrl + "...");

                var secRoom = chatClient.JoinRoom(roomUrl);
                secRoom.IgnoreOwnEvents = false;
                secRoom.StripMentionFromMessages = false;
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
                postSocket = new RealtimePostSocket();

                postSocket.OnActiveQuestion = question =>
                {
                    if (!Config.IsRunning) { return; }

                    lock (Config.Log)
                    {
                        if (Config.Log.Entries.All(p => p.PostUrl != question.Url))
                        {
                            var qResults = PostAnalyser.AnalyseQuestion(question);
                            var qMessage = ReportMessageGenerator.GetQReport(qResults, question);

                            CheckSendReport(question, qMessage, qResults);
                        }
                    }
                };

                postSocket.OnActiveThreadAnswers = answers =>
                {
                    if (!Config.IsRunning) { return; }

                    lock (Config.Log)
                    {
                        foreach (var a in answers.Where(ans => Config.Log.Entries.All(p => p.PostUrl != ans.Url)))
                        {
                            var aResults = PostAnalyser.AnalyseAnswer(a);
                            var aMessage = ReportMessageGenerator.GetPostReport(aResults, a);

                            CheckSendReport(a, aMessage, aResults);
                        }
                    }
                };

                postSocket.OnExcption = ex => Config.PrimaryRoom.PostMessage("Error:\n" + ex);

                postSocket.Connect();
            });

            postCatcherThread.Start();
        }

        # endregion

        private static void HandlePrimaryNewMessage(Message message)
        {
            Task.Factory.StartNew(() =>
            {
                if (message.Content.ToLowerInvariant() == ">>kill-it-with-no-regrets-for-sure")
                {
                    KillBot(message);
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

            if (Stats.ReportedUsers.Any(spammer => spammer.Name == p.AuthorName && spammer.Site == p.Site))
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
                        var autoMessage = room.PostMessage(chatMessage.Message.Content);

                        Stats.PostedReports.Add(new Report
                        {
                            Analysis = info,
                            Message = autoMessage,
                            Post = p
                        });
                    }
                }
            }

            Stats.PostsCaught++;
        }

        private static void KillBot(Message message)
        {
            if (Config.Shutdown) { return; }

            if (message.IsAuthorOwner())
            {
                Close("Kill command issued, closing Pham...", "`Killing...`", "`Kill successful!`");
            }
            else
            {
                var m = new ChatAction(Config.PrimaryRoom, () => Config.PrimaryRoom.PostReply(message, "`Access denied (this incident will be reported)..`"));
                messageHandler.QueueItem(m);
            }
        }

        private static void Close(string consoleCloseMessage = "Closing Phamhilator...", string roomClosingMessage = "`Stopping Phamhilator™...`", string roomClosedMessage = "`Phamhilator™ stopped.`")
        {
            // Check if the user has been auth'd, if so the bot has already been fully initialised
            // so post the shutdown message, otherwise, the bot hasn't been initialised so just exit.
            if (chatClient != null)
            {
                Config.Shutdown = true;

                if (!String.IsNullOrEmpty(consoleCloseMessage))
                {
                    Console.WriteLine(consoleCloseMessage);
                }

                if (!String.IsNullOrEmpty(roomClosingMessage))
                {
                    Config.PrimaryRoom.PostMessage(roomClosingMessage);
                }

                postSocket.Dispose();
                messageHandler.Dispose();

                lock (Config.Log)
                {
                    Config.Log.Dispose();
                }

                if (!String.IsNullOrEmpty(roomClosedMessage))
                {
                    Config.PrimaryRoom.PostMessage(roomClosedMessage);
                }

                Thread.Sleep(3000); // Give the primary room a chance to post the messages before disposing the client.

                chatClient.Dispose();
            }

            Process.GetCurrentProcess().Close();
        }
    }
}
