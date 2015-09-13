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
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Phamhilator.Yam.Core;
using ChatExchangeDotNet;
using Phamhilator.FlagExchangeDotNet;
using System.Linq;

namespace Phamhilator.Pham.UI
{
    public class Program
    {
        private const string thresholdDataManagerKey = "Threshold";
        private static readonly List<Post> checkedPosts = new List<Post>();
        private static readonly ManualResetEvent shutdownMre = new ManualResetEvent(false);
        private static LocalRequestClient yamClient;
        private static Client chatClient;
        private static Room hq;
        private static Room socvr;
        private static UserAccess authUsers;
        private static Flagger flagger;
        private static PostLogModelGenerator autoModelGen;
        private static PostClassifier cvClassifier;
        private static PostClassifier dvQClassifier;
        private static PostClassifier dvAClassifier;
        private static DateTime startTime;
        private static double threshold;



        static void Main(string[] args)
        {
            Console.Title = "Pham v2";
            Console.WriteLine("Pham v2.\nPress Q to exit.\n");
            Console.CancelKeyPress += (o, oo) => shutdownMre.Set();

            InitialiseFlagger();
            InitialiseCore();
            TryLogin();
            JoinRooms();

            startTime = DateTime.UtcNow;
            var startUpMsg = new MessageBuilder();
            startUpMsg.AppendText("Pham v2 started", TextFormattingOptions.InLineCode);

#if DEBUG
            Console.WriteLine("\nPham v2 started (debug).");
            startUpMsg.AppendText(" - debug.", TextFormattingOptions.Bold | TextFormattingOptions.InLineCode);
            hq.PostMessageFast(startUpMsg);
#else
            Console.WriteLine("\nPham v2 started.");
            hq.PostMessageFast(startUpMsg);
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

            Console.WriteLine("Stopping...");

            var shutdownMsg = new MessageBuilder();
            shutdownMsg.AppendText("Shutdown successful.", TextFormattingOptions.InLineCode);

            hq.PostMessageFast(shutdownMsg);
            socvr.PostMessageFast(shutdownMsg);

            hq.Leave();
            socvr.Leave();
        }



        private static void InitialiseFlagger()
        {
            Console.Write("Enable flagging module (Y/N): ");
            var enable = Console.ReadLine().Trim().ToUpperInvariant();

            if (!enable.StartsWith("Y")) { return; }

            Console.WriteLine("Please enter your Stack Exchange OpenID flagging module credentials (account must have 200+ rep).\n");

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

            Console.Write("done.\nGathering config data...");

            authUsers = new UserAccess(ref yamClient);

            if (!yamClient.DataExists("Pham", thresholdDataManagerKey))
            {
                yamClient.UpdateData("Pham", thresholdDataManagerKey, (3 / 2F).ToString());
            }
            threshold = double.Parse(yamClient.RequestData("Pham", thresholdDataManagerKey));

            Console.Write("done.\nLoading models...");

            if (!yamClient.DataExists("Pham", PostLogModelGenerator.CVDataKey))
            {
                yamClient.UpdateData("Pham", PostLogModelGenerator.CVDataKey, "");
            }
            var cvModels = yamClient.RequestData("Pham", PostLogModelGenerator.CVDataKey).Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            cvClassifier = new PostClassifier(cvModels);

            if (!yamClient.DataExists("Pham", PostLogModelGenerator.DVQDataKey))
            {
                yamClient.UpdateData("Pham", PostLogModelGenerator.DVQDataKey, "");
            }
            var dvQModels = yamClient.RequestData("Pham", PostLogModelGenerator.DVQDataKey).Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            dvQClassifier = new PostClassifier(dvQModels);

            if (!yamClient.DataExists("Pham", PostLogModelGenerator.DVADataKey))
            {
                yamClient.UpdateData("Pham", PostLogModelGenerator.DVADataKey, "");
            }
            var dvAModels = yamClient.RequestData("Pham", PostLogModelGenerator.DVADataKey).Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            dvAClassifier = new PostClassifier(dvAModels);

            Console.Write("done.\nStarting auto model generator...");

            autoModelGen = new PostLogModelGenerator(ref yamClient, ref cvClassifier, ref dvQClassifier, ref dvAClassifier);

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
            hq.EventManager.ConnectListener(EventType.UserMentioned, new Action<Message>(m => HandleChatCommand(hq, m)));

            Console.Write("done.\nJoining SOCVR...");

            socvr = chatClient.JoinRoom("http://chat.stackoverflow.com/rooms/68414");//("http://chat.stackoverflow.com/rooms/41570");//
            socvr.EventManager.ConnectListener(EventType.UserMentioned, new Action<Message>(m => HandleChatCommand(socvr, m)));

            Console.WriteLine("done.");
        }

        private static void ConnectYamClientEvents()
        {
            yamClient.EventManager.ConnectListener(LocalRequest.RequestType.Question, new Action<Question>(q =>
            {
                CheckPost(true, new Post
                {
                    Url = q.Url,
                    Site = q.Site,
                    Title = q.Title,
                    Body = q.Body,
                    Score = q.Score,
                    CreationDate = q.CreationDate,
                    AuthorName = q.AuthorName,
                    AuthorLink = q.AuthorLink,
                    AuthorNetworkID = q.AuthorNetworkID,
                    AuthorRep = q.AuthorRep
                });
            }));

            yamClient.EventManager.ConnectListener(LocalRequest.RequestType.Answer, new Action<Answer>(a =>
            {
                CheckPost(false, a);
            }));

            yamClient.EventManager.ConnectListener(LocalRequest.RequestType.Exception, new Action<LocalRequest>(ex =>
            {
                yamClient.SendData(new LocalRequest
                {
                    Type = LocalRequest.RequestType.Exception,
                    ID = LocalRequest.GetNewID(),
                    Data = ex.Data.ToString(),
                    Options = ex.Options
                });
            }));
        }

        private static void CheckPost(bool isQuestion, Post post)
        {
            if (checkedPosts.Contains(post) || post.Site != "stackoverflow.com") { return; }
            while (checkedPosts.Count > 3000)
            {
                checkedPosts.RemoveAt(0);
            }
            checkedPosts.Add(post);

            var cvScore = -1D;
            var dvScore = -1D;

            if (isQuestion)
            {
                cvScore = cvClassifier.ClassifyPost(post);
                dvScore = dvQClassifier.ClassifyPost(post);
            }
            else
            {
                dvScore = dvAClassifier.ClassifyPost(post);
            }

            if (Math.Max(cvScore, dvScore) < threshold)
            {
                return;
            }

            ReportPost(post, dvScore, cvScore);
        }

        private static void ReportPost(Post post,  double scoreDV, double scoreCV)
        {
            var msg = new MessageBuilder();

            msg.AppendText("Low quality (DV: " + Math.Round(scoreDV, 2) + " • CV: " + Math.Round(scoreCV, 2) + ")? ", TextFormattingOptions.InLineCode);
            msg.AppendLink(post.Title.Replace("\n", " "), post.Url, "Score: " + post.Score, TextFormattingOptions.InLineCode, WhiteSpace.None);
            msg.AppendText(", by ", TextFormattingOptions.InLineCode);
            msg.AppendLink(post.AuthorName, post.AuthorLink, "Reputation: " + post.AuthorRep, TextFormattingOptions.InLineCode, WhiteSpace.None);
            msg.AppendText(".", TextFormattingOptions.InLineCode);

            hq.PostMessageFast(msg);
            socvr.PostMessageFast(msg);
        }

        private static void HandleChatCommand(Room room, Message command)
        {
            try
            {
                if (UserAccess.Owners.Any(id => id == command.Author.ID) || command.Author.IsRoomOwner || command.Author.IsMod)
                {
                    var cmdMatches = HandleOwnerCommand(room, command);

                    if (!cmdMatches)
                    {
                        cmdMatches = HandlePrivilegedUserCommand(room, command, true);

                        if (!cmdMatches)
                        {
                            cmdMatches = HandleNormalUserCommand(room, command);

                            if (!cmdMatches)
                            {
                                room.PostReplyFast(command, "`Command not recognised.`");
                            }
                        }
                    }
                }
                else if (authUsers.AuthorisedUsers.Any(id => id == command.Author.ID))
                {
                    var cmdMatches = HandlePrivilegedUserCommand(room, command, false);

                    if (!cmdMatches)
                    {
                        cmdMatches = HandleNormalUserCommand(room, command);

                        if (!cmdMatches)
                        {
                            room.PostReplyFast(command, "`Command not recognised (at your current access level).`");
                        }
                    }
                }
                else
                {
                    var cmdMatches = HandleNormalUserCommand(room, command);

                    if (!cmdMatches)
                    {
                        room.PostReplyFast(command, "`Command not recognised (at your current access level).`");
                    }
                }
            }
            catch (Exception ex)
            {
                room.PostReplyFast(command, "`Unable to execute command: " + ex.Message + "`");
            }
        }

        private static bool HandleNormalUserCommand(Room room, Message command)
        {
            if (command.Content.Trim().ToUpperInvariant() == "THRESHOLD")
            {
                room.PostReplyFast(command, "`Current threshold set to: " + threshold * 100 + "%.`");
                return true;
            }

            return false;
        }

        private static bool HandlePrivilegedUserCommand(Room room, Message command, bool isOwner)
        {
            return false;
        }

        private static bool HandleOwnerCommand(Room room, Message command)
        {
            var cmd = command.Content.Trim().ToUpperInvariant();

            if (cmd.StartsWith("SET THRESHOLD"))
            {
                var newVal = cmd.Remove(0, 14);
                var newThreshold = 0D;

                if (!double.TryParse(newVal, out newThreshold) || newThreshold < 1 || newThreshold > 100)
                {
                    room.PostReply(command, "`Please specify a valid percentage.`");
                    return true;
                }

                threshold = newThreshold / 100;
                yamClient.UpdateData("Pham", thresholdDataManagerKey, threshold.ToString());

                room.PostReply(command, "`Threshold successfully updated.`");
            }
            else if (cmd == "STOP")
            {
                room.PostReply(command, "`Stopping...`");
                shutdownMre.Set();
            }
            else
            {
                return false;
            }

            return true;
        }

    }
}
