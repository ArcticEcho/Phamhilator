using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Threading;
using System.Diagnostics;
using System.Globalization;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Controls;
using ChatExchangeDotNet;
using WebSocketSharp;



namespace Phamhilator
{
    public partial class MainWindow
    {
        private Thread postCatcherThread;
        private DateTime requestedDieTime;
        private MessageHandler messageHandler;



        public MainWindow()
        {
            try
            {
                AppDomain.CurrentDomain.UnhandledException += GlobalExceptionHandler;

                CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
                CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;

                InitializeComponent();

                var data = File.ReadAllText(DirectoryTools.GetCredsFile());

                if (!String.IsNullOrEmpty(data))
                {
                    emailTB.Text = data.Remove(data.IndexOf("¬", StringComparison.InvariantCulture));
                    passwordTB.Text = data.Substring(data.IndexOf("¬", StringComparison.InvariantCulture) + 1);
                }

                loginC.Children.Remove(operationC);

                ReportLog.Initialise();
            }
            catch (Exception ex)
            {
                if (ex is TypeInitializationException && ex.InnerException != null)
                {
                    ex = ex.InnerException;
                }

                Clipboard.SetText(ex.ToString());

                MessageBox.Show(ex + Environment.NewLine + Environment.NewLine + "The above error details have be copied to your clipboard. Now closing Pham...");

                Environment.Exit(Environment.ExitCode);
            }
        }



        private void InitialiseSocket()
        {
            postCatcherThread = new Thread(() =>
            {
                var socket = new WebSocket("ws://qa.sockets.stackexchange.com");

                socket.OnOpen += (o, oo) => socket.Send("155-questions-active");

                socket.OnMessage += (o, message) =>
                {
                    if (!GlobalInfo.BotRunning) { return; }

                    try
                    {
                        var question = PostRetriever.GetQuestion(message);

                        if (ReportLog.Messages.All(p => p != question.Url))
                        {
                            var qResults = PostAnalyser.AnalyseQuestion(question);
                            var qMessage = MessageGenerator.GetQReport(qResults, question);

                            CheckSendReport(question, qMessage, qResults);
                        }

                        if (GlobalInfo.FullScanEnabled)
                        {
                            var answers = PostRetriever.GetLatestAnswers(question);

                            foreach (var a in answers.Where(ans => ReportLog.Messages.All(p => p != ans.Url)))
                            {
                                var aResults = PostAnalyser.AnalyseAnswer(a);
                                var aMessage = MessageGenerator.GetAReport(aResults, a);

                                CheckSendReport(a, aMessage, aResults);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        GlobalInfo.PrimaryRoom.PostMessage("    Error: \n" + ex.ToString().Replace("\n", "\n    ") + "\n    \n    Received message: " + message.Data.Replace("\n", "\n    "));
                    }
                };

                socket.OnClose += (o, oo) =>
                {
                    GlobalInfo.PrimaryRoom.PostMessage("`Warning: global post websocket has died. Attempting to restart...`");

                    if (!GlobalInfo.Shutdown)
                    {
                        InitialiseSocket();
                    }
                };

                socket.Connect();
            });

            postCatcherThread.Start();
        }

        private void KillBot()
        {
            Dispatcher.Invoke(() => statusL.Content = "Shutting Down Pham...");

            GlobalInfo.Shutdown = true;
            requestedDieTime = DateTime.UtcNow;

            var warningMessagePosted = false;

            while (postCatcherThread.IsAlive)
            {
                Thread.Sleep(250);

                if ((DateTime.UtcNow - requestedDieTime).TotalSeconds > 10 && !warningMessagePosted)
                {
                    GlobalInfo.PrimaryRoom.PostMessage("`Warning: 10 seconds have past since the kill command was issued and the post catcher thread is still alive. Now forcing thread death...`");

                    warningMessagePosted = true;

                    postCatcherThread.Abort();
                }

                if (!postCatcherThread.IsAlive) { break; }
            }

            Thread.Sleep(5000);

            GlobalInfo.PrimaryRoom.PostMessage("`All threads have died. Kill successful!`");

            Thread.Sleep(5000);

            Dispatcher.Invoke(() => Environment.Exit(0));
        }

        private void HandlePrimaryNewMessage(Message message)
        {
            Task.Factory.StartNew(() =>
            {
                if (message.Content == ">>kill-it-with-no-regrets-for-sure")
                {
                    ChatAction m;

                    if (UserAccess.Owners.Contains(message.AuthorID))
                    {
                        m = new ChatAction(GlobalInfo.PrimaryRoom, () =>
                        {
                            GlobalInfo.PrimaryRoom.PostReply(message, "`Killing...`");
                            KillBot();
                        });
                    }
                    else
                    {
                        m = new ChatAction(GlobalInfo.PrimaryRoom, () => GlobalInfo.PrimaryRoom.PostReply(message, "`Access denied.`"));
                    }

                    messageHandler.QueueItem(m);
                }
                else
                {
                    var messages = CommandProcessor.ExacuteCommand(GlobalInfo.PrimaryRoom, message);

                    if (messages == null || messages.Length == 0) { return; }

                    foreach (var m in messages.Where(m => !String.IsNullOrEmpty(m.Content)))
                    {
                        ChatAction action;

                        if (m.IsReply)
                        {
                            action = new ChatAction(GlobalInfo.PrimaryRoom, () => GlobalInfo.PrimaryRoom.PostReply(message, m.Content));
                        }
                        else
                        {
                            action = new ChatAction(GlobalInfo.PrimaryRoom, () => GlobalInfo.PrimaryRoom.PostMessage(m.Content));
                        }

                        messageHandler.QueueItem(action);
                    }
                }
            });
        }

        private void HandleSecondaryNewMessage(Room room, Message message)
        {
            Task.Factory.StartNew(() =>
            {
                if (!CommandProcessor.IsValidCommand(message)) { return; }

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

        private void CheckSendReport(Post p, string messageBody, PostAnalysis info)
        {
            Message message = null;
            MessageInfo chatMessage = null;

            if (GlobalInfo.Spammers.Any(spammer => spammer.Name == p.AuthorName && spammer.Site == p.Site))
            {
                message = GlobalInfo.PrimaryRoom.PostMessage("**Spam**" + messageBody);
                chatMessage = new MessageInfo { Message = message, Post = p, Report = info };
            }

            if (info.Accuracy <= GlobalInfo.AccuracyThreshold) { return; }

            switch (info.Type)
            {
                case PostType.Offensive:
                {
                    message = GlobalInfo.PrimaryRoom.PostMessage("**Offensive**" + messageBody);
                    chatMessage = new MessageInfo { Message = message, Post = p, Report = info };

                    break;
                }

                case PostType.BadUsername:
                {
                    message = GlobalInfo.PrimaryRoom.PostMessage("**Bad Username**" + messageBody);
                    chatMessage = new MessageInfo { Message = message, Post = p, Report = info };

                    break;
                }

                case PostType.BadTagUsed:
                {
                    message = GlobalInfo.PrimaryRoom.PostMessage("**Bad Tag(s) Used**" + messageBody);
                    chatMessage = new MessageInfo { Message = message, Post = p, Report = info };

                    break;
                }

                case PostType.LowQuality:
                {
                    message = GlobalInfo.PrimaryRoom.PostMessage("**Low Quality**" + messageBody);
                    chatMessage = new MessageInfo { Message = message, Post = p, Report = info };

                    break;
                }

                case PostType.Spam:
                {
                    message = GlobalInfo.PrimaryRoom.PostMessage("**Spam**" + messageBody);
                    chatMessage = new MessageInfo { Message = message, Post = p, Report = info };

                    break;
                }
            }

            if (message != null)
            {
                ReportLog.AddPost(p.Url);
                GlobalInfo.PostedReports.Add(message.ID, chatMessage);

                if (info.AutoTermsFound)
                {
                    foreach (var room in GlobalInfo.ChatClient.Rooms.Where(r => r.ID != GlobalInfo.PrimaryRoom.ID))
                    {
                        room.PostMessage(chatMessage.Message.Content);
                    }
                }
            }

            GlobalInfo.Stats.TotalCheckedPosts++;
        }

        private void GlobalExceptionHandler(object o, UnhandledExceptionEventArgs args)
        {
            Clipboard.SetText(args.ExceptionObject.ToString());

            GlobalInfo.PrimaryRoom.PostMessage(args.ExceptionObject.ToString());

            if (args.IsTerminating)
            {
                MessageBox.Show(args.ExceptionObject + Environment.NewLine + Environment.NewLine + "The above error details have be copied to your clipboard. Now closing Pham...", "Oops...", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                var res = MessageBox.Show(args.ExceptionObject + Environment.NewLine + Environment.NewLine + "The above error details have be copied to your clipboard. Do you wish to continue?", "Oops...", MessageBoxButton.YesNo, MessageBoxImage.Error);

                if (res == MessageBoxResult.No) { Environment.Exit(Environment.ExitCode); }
            }
        }

        # region UI Events

        private void MetroWindow_Closing(object sender, CancelEventArgs e)
        {
            statusL.Content = "Shutting Down Pham...";

            e.Cancel = true;

            if (!GlobalInfo.BotRunning) { Environment.Exit(0); }

            Task.Factory.StartNew(() => GlobalInfo.PrimaryRoom.PostMessage("`Phamhilator™ stopped.`"));

            GlobalInfo.Shutdown = true;

            Thread.Sleep(3000);

            Task.Factory.StartNew(() => Dispatcher.Invoke(() => Environment.Exit(0)));
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrEmpty(emailTB.Text) || String.IsNullOrEmpty(passwordTB.Text))
            {
                MessageBox.Show("Please fill out all fields.", "Phamhilator", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }

            progressBar.IsIndeterminate = true;
            ((Button)sender).IsEnabled = false;
            emailTB.IsEnabled = false;
            passwordTB.IsEnabled = false;
            remCredsCB.IsEnabled = false;
            loginTitleL.Content = "Logging In...";

            var user = emailTB.Text;
            var pass = passwordTB.Text;
            var remCreds = remCredsCB.IsChecked ?? false;

            Task.Factory.StartNew(() =>
            {
                try
                {
                    GlobalInfo.ChatClient = new Client(user, pass);
                }
                catch (Exception ex)
                {
                    Dispatcher.Invoke(() =>
                    {
                        progressBar.IsIndeterminate = false;
                        ((Button)sender).IsEnabled = true;
                        emailTB.IsEnabled = true;
                        passwordTB.IsEnabled = true;
                        remCredsCB.IsEnabled = true;
                        statusL.Content = "Could Not Login";
                        Clipboard.SetText(ex.ToString());
                    });

                    return;
                }

                if (remCreds)
                {
                    File.WriteAllText(DirectoryTools.GetCredsFile(), user + "¬" + pass);
                }
                else
                {
                    File.WriteAllText(DirectoryTools.GetCredsFile(), "");
                }

                GlobalInfo.PrimaryRoom = GlobalInfo.ChatClient.JoinRoom("http://chat.meta.stackexchange.com/rooms/773/low-quality-posts-hq");
                GlobalInfo.PrimaryRoom.NewMessage += HandlePrimaryNewMessage;
                GlobalInfo.PrimaryRoom.MessageEdited += (oldMessage, newMessage) => HandlePrimaryNewMessage(newMessage);
                GlobalInfo.PrimaryRoom.IgnoreOwnEvents = false;

                GlobalInfo.ChatClient.JoinRoom("http://chat.meta.stackexchange.com/rooms/651");//("http://chat.meta.stackexchange.com/rooms/89/tavern-on-the-meta");//

                for (var i = 0; i < GlobalInfo.ChatClient.Rooms.Count; i++)
                {
                    if (GlobalInfo.ChatClient.Rooms[i].ID == GlobalInfo.PrimaryRoom.ID) { continue; }

                    GlobalInfo.ChatClient.Rooms[i].NewMessage += message => HandleSecondaryNewMessage(GlobalInfo.ChatClient.Rooms.First(r => r.ID == message.RoomID), message);
                    GlobalInfo.ChatClient.Rooms[i].MessageEdited += (oldMessage, newMessage) => HandleSecondaryNewMessage(GlobalInfo.ChatClient.Rooms.First(r => r.ID == newMessage.RoomID), newMessage);

                    GlobalInfo.ChatClient.Rooms[i].IgnoreOwnEvents = false;
                }

                Dispatcher.Invoke(() =>
                {
                    transContentC.Content = operationC;

                    progressBar.IsIndeterminate = false;
                });
            });
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            statusL.Content = "Monitoring Enabled";

            ((Button)sender).IsEnabled = false;

            messageHandler = new MessageHandler();
            GlobalInfo.BotRunning = true;

            InitialiseSocket();

            if (Debugger.IsAttached)
            {
                GlobalInfo.PrimaryRoom.PostMessage("`Phamhilator™ started (debug mode).`");
            }
            else
            {
                GlobalInfo.PrimaryRoom.PostMessage("`Phamhilator™ started.`");
            }

            GlobalInfo.UpTime = DateTime.UtcNow;
        }

        # endregion
    }
}
