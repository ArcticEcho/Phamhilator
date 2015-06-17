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
using ServiceStack.Text;

namespace Phamhilator.Pham.UI
{
    public class Program
    {
        private static ManualResetEvent shutdownMre = new ManualResetEvent(false);
        private static readonly HashSet<Post> checkedPosts = new HashSet<Post>();
        private static LocalRequestClient yamClient;
        private static Client chatClient;
        private static Room hq;
        private static Room tavern;
        private static UserAccess userAccess;
        private static Flagger flagger;
        private static LinkClassifier linkClassifier;
        private static DateTime startTime;
        //private static ActiveRooms roomsToJoin;



        static void Main(string[] args)
        {
            Console.Title = "Pham v2";
            Console.WriteLine("Pham v2.\nPress Q to exit.\n");
            Console.CancelKeyPress += (o, oo) => Close();

            InitialiseFlagger();
            InitialiseCore();
            TryLogin();
            JoinRooms();

            //Config.IsRunning = true;
            startTime = DateTime.UtcNow;

#if DEBUG
            Console.WriteLine("\nPham v2 started (debug).");
            hq.PostMessage("`Pham v2 started` (**`debug`**)`.`");
#else
            hq.PostMessage("`Pham v2 started.`");
            Console.WriteLine("\nPham v2 started.");
#endif

            ConnectYamClientEvents();


            Task.Run(() =>
            {
                while (true)
                {
                    if (char.ToLowerInvariant(Console.ReadKey(true).KeyChar) == 'q')
                    {
                        shutdownMre.Set();
                        return;
                    }
                }
            });

            shutdownMre.WaitOne();
            Close();
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

            flagger = new Flagger(name, email, password);

            Thread.Sleep(3000);
            Console.Clear();
        }

        private static void InitialiseCore()
        {
            Console.Write("Initialising Yam client...");
            yamClient = new LocalRequestClient("Pham");
            AppDomain.CurrentDomain.UnhandledException += (o, ex) => yamClient.SendData(new LocalRequest
            {
                Type = LocalRequest.RequestType.Exception,
                ID = LocalRequest.GetNewID(),
                Data = ex.ExceptionObject
            });

            Console.Write("done.\nInitialising link classifier...");
            linkClassifier = new LinkClassifier(ref yamClient);

            //Console.Write("done.\nLoading log...");
            //Config.Log = new ReportLog();

            Console.Write("done.\nLoading config data...");
            //roomsToJoin = new ActiveRooms();
            //Config.Core = new Pham.Core.Pham();
            //Stats.PostedReports = new List<Report>();
            userAccess = new UserAccess(ref yamClient);

            //Console.Write("done.\nLoading bad tag definitions...");
            //Config.BadTags = new BadTags();

            //Console.Write("done.\nLoading black terms...");
            //Config.BlackFilters = new Dictionary<FilterConfig, BlackFilter>()
            //{
            //    { new FilterConfig(FilterClass.QuestionTitleName, FilterType.Black), new BlackFilter(FilterClass.QuestionTitleName) },
            //    { new FilterConfig(FilterClass.QuestionTitleOff, FilterType.Black), new BlackFilter(FilterClass.QuestionTitleOff) },
            //    { new FilterConfig(FilterClass.QuestionTitleSpam, FilterType.Black), new BlackFilter(FilterClass.QuestionTitleSpam) },
            //    { new FilterConfig(FilterClass.QuestionTitleLQ, FilterType.Black), new BlackFilter(FilterClass.QuestionTitleLQ) },

            //    { new FilterConfig(FilterClass.QuestionBodySpam, FilterType.Black), new BlackFilter(FilterClass.QuestionBodySpam) },
            //    { new FilterConfig(FilterClass.QuestionBodyLQ, FilterType.Black), new BlackFilter(FilterClass.QuestionBodyLQ) },
            //    { new FilterConfig(FilterClass.QuestionBodyOff, FilterType.Black), new BlackFilter(FilterClass.QuestionBodyOff) },

            //    { new FilterConfig(FilterClass.AnswerSpam, FilterType.Black), new BlackFilter(FilterClass.AnswerSpam) },
            //    { new FilterConfig(FilterClass.AnswerLQ, FilterType.Black), new BlackFilter(FilterClass.AnswerLQ) },
            //    { new FilterConfig(FilterClass.AnswerOff, FilterType.Black), new BlackFilter(FilterClass.AnswerOff) },
            //    { new FilterConfig(FilterClass.AnswerName, FilterType.Black), new BlackFilter(FilterClass.AnswerName) }
            //};

            //Console.Write("done.\nLoading white terms...");
            //Config.WhiteFilters = new Dictionary<FilterConfig, WhiteFilter>()
            //{
            //    { new FilterConfig(FilterClass.QuestionTitleName, FilterType.White), new WhiteFilter(FilterClass.QuestionTitleName) },
            //    { new FilterConfig(FilterClass.QuestionTitleOff, FilterType.White), new WhiteFilter(FilterClass.QuestionTitleOff) },
            //    { new FilterConfig(FilterClass.QuestionTitleSpam, FilterType.White), new WhiteFilter(FilterClass.QuestionTitleSpam) },
            //    { new FilterConfig(FilterClass.QuestionTitleLQ, FilterType.White), new WhiteFilter(FilterClass.QuestionTitleLQ) },

            //    { new FilterConfig(FilterClass.QuestionBodySpam, FilterType.White), new WhiteFilter(FilterClass.QuestionBodySpam) },
            //    { new FilterConfig(FilterClass.QuestionBodyLQ, FilterType.White), new WhiteFilter(FilterClass.QuestionBodyLQ) },
            //    { new FilterConfig(FilterClass.QuestionBodyOff, FilterType.White), new WhiteFilter(FilterClass.QuestionBodyOff) },

            //    { new FilterConfig(FilterClass.AnswerSpam, FilterType.White), new WhiteFilter(FilterClass.AnswerSpam) },
            //    { new FilterConfig(FilterClass.AnswerLQ, FilterType.White), new WhiteFilter(FilterClass.AnswerLQ) },
            //    { new FilterConfig(FilterClass.AnswerOff, FilterType.White), new WhiteFilter(FilterClass.AnswerOff) },
            //    { new FilterConfig(FilterClass.AnswerName, FilterType.White), new WhiteFilter(FilterClass.AnswerName) }
            //};

            Console.WriteLine("done.\n");
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
            Console.Write("Joining HQ...");

            hq = chatClient.JoinRoom("http://chat.meta.stackexchange.com/rooms/773");
            //hq.IgnoreOwnEvents = false;
            //hq.StripMentionFromMessages = false;
            hq.EventManager.ConnectListener(EventType.MessagePosted, new Action<Message>(HandleHqNewMessage));
            hq.EventManager.ConnectListener(EventType.MessageEdited, new Action<Message>(newMessage => HandleHqNewMessage(newMessage)));

            Console.Write("done.\nJoining Tavern...");

            tavern = chatClient.JoinRoom("http://chat.meta.stackexchange.com/rooms/89");
            //tavern.IgnoreOwnEvents = false;
            //tavern.StripMentionFromMessages = false;
            tavern.EventManager.ConnectListener(EventType.MessagePosted, new Action<Message>(HandleTavernNewMessage));
            tavern.EventManager.ConnectListener(EventType.MessageEdited, new Action<Message>(newMessage => HandleTavernNewMessage(newMessage)));

            Console.WriteLine("done.");
        }

        private static void ConnectYamClientEvents()
        {
            yamClient.EventManager.ConnectListener(LocalRequest.RequestType.Question, new Action<Question>(question =>
            {
                if (question.Score <= 2 || question.AuthorRep <= 1000)
                {
                    CheckPost(question.Base);
                }
                //if (!Config.IsRunning) { return; }

                //lock (Config.Log)
                //{
                //    if (Config.Log.Entries.All(p => p.PostUrl != question.Url))
                //    {
                //        var qResults = PostAnalyser.AnalyseQuestion(question);
                //        var qMessage = ReportMessageGenerator.GetQReport(qResults, question);

                //        CheckSendReport(question, qMessage, qResults);
                //    }
                //}
            }));

            yamClient.EventManager.ConnectListener(LocalRequest.RequestType.Answer, new Action<Answer>(answer =>
            {
                if (answer.Score <= 2 || answer.AuthorRep <= 1000)
                {
                    CheckPost(answer);
                }
                //if (!Config.IsRunning) { return; }

                //lock (Config.Log)
                //{
                //    if (Config.Log.Entries.All(p => p.PostUrl != answer.Url))
                //    {
                //        var aResults = PostAnalyser.AnalyseAnswer(answer);
                //        var aMessage = ReportMessageGenerator.GetPostReport(aResults, answer);

                //        CheckSendReport(answer, aMessage, aResults);
                //    }
                //}
            }));

            yamClient.EventManager.ConnectListener(LocalRequest.RequestType.Exception, new Action<LocalRequest>(ex =>
            {
                //if (!Config.IsRunning) { return; }

                yamClient.SendData(new LocalRequest
                {
                    Type = LocalRequest.RequestType.Exception,
                    ID = LocalRequest.GetNewID(),
                    Data = ex.Data.ToString(),
                    Options = ex.Options
                });
            }));
        }

        private static void CheckPost(Post post)
        {
            if (checkedPosts.Contains(post)) { return; }
            checkedPosts.Add(post);

            var results = linkClassifier.ClassifyLinks(post);
            if (results == null || results.Count == 0 || results.All(r => r.Value.Type == LinkType.Clean)) { return; }

            if (results.Values.Any(r => r.BlackSiteFound))
            {
                hq.PostMessage("`Link Classifier:` [`blacklisted site`](" + post.Url + ")`.`");
                return;
            }
            var logLink = Hastebin.PostDocument(results.Dump());

            var linksFound = results.Count;
            var phrasesFoundAll = 0;
            var phrasesFoundDistinct = 0;
            var diversity = 0;
            var density = 0;
            phrasesFoundAll = results.Values.Where(r => r.PhrasesFound != null).Sum(r => phrasesFoundAll += r.PhrasesFound.Values.Sum());
            phrasesFoundDistinct = results.Values.Where(r => r.PhrasesFound != null).Select(r => r.PhrasesFound.Keys).Distinct().Count();

            var report = "`Link Classifier:` [`" + linksFound + " link" + (linksFound > 1 ? "s" : "") + " found & " + phrasesFoundAll + " spam phrases detected`](" + post.Url + ") `(`[`log`](" + logLink +  ")`).`";

            hq.PostMessage(report);
        }

        private static void HandleHqNewMessage(Message message)
        {
            if (UserAccess.Owners.All(u => u.ID != message.Author.ID)) { return; }

            var cmd = message.Content.ToLowerInvariant().Trim();

            if (cmd.StartsWith("add spam phrase "))
            {
                var phrase = cmd.Remove(0, 16);
                linkClassifier.AddSpamPhrase(phrase);
                hq.PostReply(message, "`Phrase added.`");
            }
            else if (cmd.StartsWith("remove spam phrase "))
            {
                var phrase = cmd.Remove(0, 18);
                linkClassifier.RemoveSpamPhrase(phrase);
                hq.PostReply(message, "`Phrase removed.`");
            }
            else if (cmd.StartsWith("add black site "))
            {
                var phrase = cmd.Remove(0, 15);
                linkClassifier.AddBlackSite(phrase);
                hq.PostReply(message, "`Black site added.`");
            }
            else if (cmd.StartsWith("remove black site "))
            {
                var phrase = cmd.Remove(0, 18);
                linkClassifier.RemoveBlackSite(phrase);
                hq.PostReply(message, "`Black removed added.`");
            }
            else if (cmd.StartsWith("add white site "))
            {
                var phrase = cmd.Remove(0, 15);
                linkClassifier.AddWhiteSite(phrase);
                hq.PostReply(message, "`White site added.`");
            }
            else if (cmd.StartsWith("remove white site "))
            {
                var phrase = cmd.Remove(0, 18);
                linkClassifier.RemoveWhiteSite(phrase);
                hq.PostReply(message, "`White site removed.`");
            }
            else if (cmd == "sync data")
            {
                linkClassifier.SyncData(ref yamClient);
                hq.PostReply(message, "`Data sync'd.`");
            }

            //if (message.Content.ToLowerInvariant() == ">>kill-it-with-no-regrets-for-sure")
            //{
            //    KillBot(message);
            //}
            //else
            //{
            //    var messages = CommandProcessor.ExacuteCommand(Config.PrimaryRoom, message);

            //    if (messages == null || messages.Length == 0) { return; }

            //    foreach (var m in messages.Where(m => m != null && !String.IsNullOrEmpty(m.Content)))
            //    {
            //        if (m.IsReply)
            //        {
            //            Config.PrimaryRoom.PostReply(message, m.Content);
            //        }
            //        else
            //        {
            //            Config.PrimaryRoom.PostMessage(m.Content);
            //        }
            //    }
            //}
        }

        private static void HandleTavernNewMessage(Message message)
        {
            //if (!CommandProcessor.IsValidCommand(room, message)) { return; }

            //var messages = CommandProcessor.ExacuteCommand(room, message);

            //if (messages == null || messages.Length == 0) { return; }

            //foreach (var m in messages.Where(m => m != null && !String.IsNullOrEmpty(m.Content)))
            //{
            //    if (m.IsReply)
            //    {
            //        room.PostReply(message, m.Content);
            //    }
            //    else
            //    {
            //        room.PostMessage(m.Content);
            //    }
            //}
        }

        //private static void CheckSendReport(Post p, string messageBody, PostAnalysis info)
        //{
        //    Stats.TotalCheckedPosts++;

        //    if (p == null || String.IsNullOrEmpty(messageBody) || info == null) { return; }

        //    Message chatMessage = null;
        //    Report report = null;

        //    if (info.Type == PostType.Clean) { return; }

        //    chatMessage = Config.PrimaryRoom.PostMessage(messageBody);

        //    switch (info.Type)
        //    {
        //        case PostType.Offensive:
        //        {
        //            report = new Report { Message = chatMessage, Post = p, Analysis = info };
        //            break;
        //        }

        //        case PostType.BadUsername:
        //        {
        //            report = new Report { Message = chatMessage, Post = p, Analysis = info };
        //            break;
        //        }

        //        case PostType.BadTagUsed:
        //        {
        //            report = new Report { Message = chatMessage, Post = p, Analysis = info };
        //            break;
        //        }

        //        case PostType.LowQuality:
        //        {
        //            report = new Report { Message = chatMessage, Post = p, Analysis = info };
        //            break;
        //        }

        //        case PostType.Spam:
        //        {
        //            report = new Report { Message = chatMessage, Post = p, Analysis = info };
        //            break;
        //        }

        //        default:
        //        {
        //            if (Stats.ReportedUsers.Any(spammer => spammer.Name == p.AuthorName && spammer.Site == p.Site))
        //            {
        //                report = new Report { Message = chatMessage, Post = p, Analysis = info };
        //            }
        //            break;
        //        }
        //    }

        //    if (chatMessage != null && report != null)
        //    {
        //        Config.Log.AddEntry(new LogItem
        //        {
        //            ReportLink = "http://chat." + chatMessage.Host + "/transcript/message/" + chatMessage.ID,
        //            PostUrl = p.Url,
        //            Site = p.Site,
        //            Title = p.Title,
        //            Body = p.Body,
        //            TimeStamp = DateTime.UtcNow,
        //            ReportType = info.Type,
        //            BlackTerms = info.BlackTermsFound.ToLogTerms().ToList(),
        //            WhiteTerms = info.WhiteTermsFound.ToLogTerms().ToList()
        //        });
        //        Stats.PostedReports.Add(report);

        //        if (info.AutoTermsFound)
        //        {
        //            foreach (var room in Config.SecondaryRooms)
        //            {
        //                var autoMessage = room.PostMessage(report.Message.Content);

        //                Stats.PostedReports.Add(new Report
        //                {
        //                    Analysis = info,
        //                    Message = autoMessage,
        //                    Post = p
        //                });
        //            }
        //        }
        //    }

        //    Stats.PostsCaught++;
        //}

        private static void KillBot(Message message)
        {
            //if (Config.Shutdown) { return; }

            //if (message.IsAuthorOwner())
            //{
            //    Close("Kill command issued, closing Pham...", "`Killing...`", "`Kill successful!`");
            //}
            //else
            //{
            //    Config.PrimaryRoom.PostReply(message, "`Access denied (this incident will be reported).`");
            //}
        }

        private static void Close(string consoleCloseMessage = "Closing Pham...", string roomClosingMessage = "`Stopping Pham v2...`", string roomClosedMessage = "`Pham v2 stopped.`")
        {
            // Check if the user has been auth'd, if so the bot has already been fully initialised
            // so post the shutdown message, otherwise, the bot hasn't been initialised so just exit.
            if (chatClient != null)
            {
                //Config.Shutdown = true;

                if (!String.IsNullOrEmpty(consoleCloseMessage))
                {
                    Console.WriteLine(consoleCloseMessage);
                }

                if (!String.IsNullOrEmpty(roomClosingMessage))
                {
                    hq.PostMessage(roomClosingMessage);
                }

                yamClient.Dispose();

                //lock (Config.Log)
                //{
                //    Config.Log.Dispose();
                //}

                if (!String.IsNullOrEmpty(roomClosedMessage))
                {
                    hq.PostMessage(roomClosedMessage);
                }

                tavern.Leave();
                hq.Leave();

                Thread.Sleep(5000);

                chatClient.Dispose();
            }

            linkClassifier.SyncData(ref yamClient);
            yamClient.Dispose();
            Process.GetCurrentProcess().Close();
        }
    }
}
