/*
 * Phamhilator. A .Net based bot network catching spam/low quality posts for Stack Exchange.
 * Copyright © 2015, ArcticEcho.
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */





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
using Phamhilator.Yam.Core;
using Phamhilator.Pham.Core;
using ChatExchangeDotNet;
using Phamhilator.FlagExchangeDotNet;

namespace Phamhilator.Pham.UI
{
    public class Program
    {
        private static LocalRequestClient yamClient;
        private static Client chatClient;
        private static ActiveRooms roomsToJoin;



        static void Main(string[] args)
        {
            Console.Title = "Pham v2";
            Console.WriteLine("Pham v2.\nPress Ctrl + C to exit.\n");
            AppDomain.CurrentDomain.UnhandledException += (o, ex) => Config.PrimaryRoom.PostMessage("Error:\n" + ex.ExceptionObject.ToString());
            Console.CancelKeyPress += (o, oo) => Close();

            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
            CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;

            InitialiseFlagger();
            InitialiseCore();
            TryLogin();
            JoinRooms();

            Config.IsRunning = true;
            Stats.UpTime = DateTime.UtcNow;

#if DEBUG
            Console.Write("\nPham v2 started (debug).");
            Config.PrimaryRoom.PostMessage("`Pham v2 started` (**`debug`**)`.`");
#else
            Config.PrimaryRoom.PostMessage("`Pham v2 started.`");
            Console.WriteLine("\nPham v2 started.");
#endif

            ConnectYamClientEvents();
        }



        # region Private static auth/initialisation methods.

        private static void InitialiseCore()
        {
            Console.Write("Initialising Yam client...");
            yamClient = new LocalRequestClient("PHAM");

            yamClient.UpdateData("Yam", "Authorised Users", "201151");

            Console.Write("done.\nLoading log...");
            Config.Log = new ReportLog();

            Console.Write("done.\nLoading config data...");
            roomsToJoin = new ActiveRooms();
            Config.Core = new Pham.Core.Pham();
            Stats.PostedReports = new List<Report>();
            Config.UserAccess = new UserAccess(ref yamClient);

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

        private static void InitialiseFlagger()
        {
            Console.WriteLine("Please enter your Stack Exchange OpenID credentials (for the flagging module; account must have 200+ rep).\n");

            Console.Write("Username (case sensitive): ");
            var name = Console.ReadLine();

            Console.Write("Email: ");
            var email = Console.ReadLine();

            Console.Write("Password: ");
            var password = Console.ReadLine();

            Config.Flagger = new Flagger(name, email, password);

            Thread.Sleep(3000);
            Console.Clear();
        }

        private static void TryLogin()
        {
            var success = false;
            while (true)
            {
                Console.WriteLine("Please enter your Stack Exchange OpenID credentials.\n");

                Console.Write("Email: ");
                var email = Console.ReadLine();

                Console.Write("Password: ");
                var password = Console.ReadLine();

                try
                {
                    Console.Write("\nAuthenticating...");
                    chatClient = new Client(email, password);
                    Console.WriteLine("login successful!");
                    success = true;
                }
                catch (Exception)
                {
                    Console.WriteLine("failed to login.");
                }
                Thread.Sleep(3000);
                Console.Clear();
                if (success) { return; }
            }
        }

        private static void JoinRooms()
        {
            Console.Write("Joining primary room: " + roomsToJoin.PrimaryRoomUrl + "...");

            Config.PrimaryRoom = chatClient.JoinRoom(roomsToJoin.PrimaryRoomUrl);
            Config.PrimaryRoom.IgnoreOwnEvents = false;
            Config.PrimaryRoom.StripMentionFromMessages = false;
            Config.PrimaryRoom.EventManager.ConnectListener(EventType.MessagePosted, new Action<Message>(HandlePrimaryNewMessage));
            Config.PrimaryRoom.EventManager.ConnectListener(EventType.MessageEdited, new Action<Message, Message>((oldMessage, newMessage) => HandlePrimaryNewMessage(newMessage)));

            Console.WriteLine("done.\nJoining secondary room(s):");

            Config.SecondaryRooms = new List<Room>();

            foreach (var roomUrl in roomsToJoin.SecondaryRoomUrls)
            {
                Console.Write("Room: " + roomUrl + "...");

                var secRoom = chatClient.JoinRoom(roomUrl);
                secRoom.IgnoreOwnEvents = false;
                secRoom.StripMentionFromMessages = false;
                secRoom.EventManager.ConnectListener(EventType.MessagePosted, new Action<Message>(message => HandleSecondaryNewMessage(secRoom, message)));
                secRoom.EventManager.ConnectListener(EventType.MessageEdited, new Action<Message, Message>((oldMessage, newMessage) => HandleSecondaryNewMessage(secRoom, newMessage)));

                Config.SecondaryRooms.Add(secRoom);

                Console.WriteLine("done.");
            }
        }

        private static void ConnectYamClientEvents()
        {
            yamClient.EventManager.ConnectListener(LocalRequest.RequestType.Question, new Action<Question>(question =>
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
            }));

            yamClient.EventManager.ConnectListener(LocalRequest.RequestType.Answer, new Action<Answer>(answer =>
            {
                if (!Config.IsRunning) { return; }

                lock (Config.Log)
                {
                    if (Config.Log.Entries.All(p => p.PostUrl != answer.Url))
                    {
                        var aResults = PostAnalyser.AnalyseAnswer(answer);
                        var aMessage = ReportMessageGenerator.GetPostReport(aResults, answer);

                        CheckSendReport(answer, aMessage, aResults);
                    }
                }
            }));
        }

        # endregion

        private static void HandlePrimaryNewMessage(Message message)
        {
            if (message.Content.ToLowerInvariant() == ">>kill-it-with-no-regrets-for-sure")
            {
                KillBot(message);
            }
            else
            {
                var messages = CommandProcessor.ExacuteCommand(Config.PrimaryRoom, message);

                if (messages == null || messages.Length == 0) { return; }

                foreach (var m in messages.Where(m => m != null && !String.IsNullOrEmpty(m.Content)))
                {
                    if (m.IsReply)
                    {
                        Config.PrimaryRoom.PostReply(message, m.Content);
                    }
                    else
                    {
                        Config.PrimaryRoom.PostMessage(m.Content);
                    }
                }
            }
        }

        private static void HandleSecondaryNewMessage(Room room, Message message)
        {
            if (!CommandProcessor.IsValidCommand(room, message)) { return; }

            var messages = CommandProcessor.ExacuteCommand(room, message);

            if (messages == null || messages.Length == 0) { return; }

            foreach (var m in messages.Where(m => m != null && !String.IsNullOrEmpty(m.Content)))
            {
                if (m.IsReply)
                {
                    room.PostReply(message, m.Content);
                }
                else
                {
                    room.PostMessage(m.Content);
                }
            }
        }

        private static void CheckSendReport(Post p, string messageBody, PostAnalysis info)
        {
            Stats.TotalCheckedPosts++;

            if (p == null || String.IsNullOrEmpty(messageBody) || info == null) { return; }

            Message chatMessage = null;
            Report report = null;

            if (info.Type == PostType.Clean) { return; }

            chatMessage = Config.PrimaryRoom.PostMessage(messageBody);

            switch (info.Type)
            {
                case PostType.Offensive:
                {
                    report = new Report { Message = chatMessage, Post = p, Analysis = info };
                    break;
                }

                case PostType.BadUsername:
                {
                    report = new Report { Message = chatMessage, Post = p, Analysis = info };
                    break;
                }

                case PostType.BadTagUsed:
                {
                    report = new Report { Message = chatMessage, Post = p, Analysis = info };
                    break;
                }

                case PostType.LowQuality:
                {
                    report = new Report { Message = chatMessage, Post = p, Analysis = info };
                    break;
                }

                case PostType.Spam:
                {
                    report = new Report { Message = chatMessage, Post = p, Analysis = info };
                    break;
                }

                default:
                {
                    if (Stats.ReportedUsers.Any(spammer => spammer.Name == p.AuthorName && spammer.Site == p.Site))
                    {
                        report = new Report { Message = chatMessage, Post = p, Analysis = info };
                    }
                    break;
                }
            }

            if (chatMessage != null && report != null)
            {
                Config.Log.AddEntry(new LogItem
                {
                    ReportLink = "http://chat." + chatMessage.Host + "/transcript/message/" + chatMessage.ID,
                    PostUrl = p.Url,
                    Site = p.Site,
                    Title = p.Title,
                    Body = p.Body,
                    TimeStamp = DateTime.UtcNow,
                    ReportType = info.Type,
                    BlackTerms = info.BlackTermsFound.ToLogTerms().ToList(),
                    WhiteTerms = info.WhiteTermsFound.ToLogTerms().ToList()
                });
                Stats.PostedReports.Add(report);

                if (info.AutoTermsFound)
                {
                    foreach (var room in Config.SecondaryRooms)
                    {
                        var autoMessage = room.PostMessage(report.Message.Content);

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
                Config.PrimaryRoom.PostReply(message, "`Access denied (this incident will be reported).`");
            }
        }

        private static void Close(string consoleCloseMessage = "Closing Pham...", string roomClosingMessage = "`Stopping Pham v2...`", string roomClosedMessage = "`Pham v2 stopped.`")
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

                yamClient.Dispose();

                lock (Config.Log)
                {
                    Config.Log.Dispose();
                }

                if (!String.IsNullOrEmpty(roomClosedMessage))
                {
                    Config.PrimaryRoom.PostMessage(roomClosedMessage);
                }

                foreach (var room in Config.SecondaryRooms)
                {
                    room.Leave();
                }
                Config.PrimaryRoom.Leave();

                Thread.Sleep(5000);

                chatClient.Dispose();
            }

            Process.GetCurrentProcess().Close();
        }
    }
}
