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
using System.Linq;
using System.Threading;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;
using Phamhilator.Yam.Core;
using ChatExchangeDotNet;
using Phamhilator.FlagExchangeDotNet;
using ServiceStack.Text;
using System.Text.RegularExpressions;
using CsQuery;

namespace Phamhilator.Pham.UI
{
    public class Program
    {
        private const string thresholdDataManagerKey = "Threshold";
        private const string cvModelsDataManagerKey = "CV Models";
        private const string dvModelsDataManagerKey = "DV Models";
        private static readonly HashSet<Post> checkedPosts = new HashSet<Post>();
        private static readonly ManualResetEvent shutdownMre = new ManualResetEvent(false);
        private static LocalRequestClient yamClient;
        private static Client chatClient;
        private static Room hq;
        private static Room socvr;
        private static UserAccess userAccess;
        private static Flagger flagger;
        private static PostClassifier cvClassifier;
        private static PostClassifier dvClassifier;
        private static DateTime startTime;
        private static float threshold;



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
            shutdownMsg.AppendText("Pham v2 stopped.", TextFormattingOptions.InLineCode);

            hq.PostMessageFast(shutdownMsg);

            socvr.Leave();
            hq.Leave();
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

            Console.Write("done.\nSetting up...");

            userAccess = new UserAccess(ref yamClient);
            if (!yamClient.DataExists("Pham", thresholdDataManagerKey))
            {
                yamClient.UpdateData("Pham", thresholdDataManagerKey, "0");
            }
            threshold = float.Parse(yamClient.RequestData("Pham", thresholdDataManagerKey));

            if (!yamClient.DataExists("Pham", cvModelsDataManagerKey))
            {
                yamClient.UpdateData("Pham", cvModelsDataManagerKey, "");
            }
            var cvModels = yamClient.RequestData("Pham", cvModelsDataManagerKey).Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            cvClassifier = new PostClassifier(cvModels);

            if (!yamClient.DataExists("Pham", dvModelsDataManagerKey))
            {
                yamClient.UpdateData("Pham", dvModelsDataManagerKey, "");
            }
            var dvModels = yamClient.RequestData("Pham", dvModelsDataManagerKey).Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            dvClassifier = new PostClassifier(dvModels);

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
            hq.EventManager.ConnectListener(EventType.UserMentioned, new Action<Message>(HandleHqCommand));

            Console.Write("done.\nJoining SOCVR...");

            socvr = chatClient.JoinRoom("http://http://chat.stackoverflow.com/rooms/68414");//chat.stackoverflow.com/rooms/41570");
            socvr.EventManager.ConnectListener(EventType.UserMentioned, new Action<Message>(HandleSocvrCommand));

            Console.WriteLine("done.");
        }

        private static void ConnectYamClientEvents()
        {
            yamClient.EventManager.ConnectListener(LocalRequest.RequestType.Question, new Action<Question>(q =>
            {
                if (q.Score > 2 || q.AuthorRep > 1000) { return; }

                CheckPost(new Post
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
                if (a.Score > 2 || a.AuthorRep > 1000) { return; }

                CheckPost(a);
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

        private static void CheckPost(Post post)
        {
            if (checkedPosts.Contains(post) || post.Site != "stackoverflow.com") { return; }
            checkedPosts.Add(post);

            var cvScore = cvClassifier.ClassifyPost(post);
            var dvScore = dvClassifier.ClassifyPost(post);

            if (Math.Max(cvScore, dvScore) < (3 / 2D))
            {
                return;
            }

            ReportPost(post, cvScore > dvScore);
        }

        private static void ReportPost(Post post, bool dvWorthy)
        {
            var msg = new MessageBuilder();

            msg.AppendText((dvWorthy ? "DV" : "CV") + "-plz", TextFormattingOptions.Tag);
            msg.AppendText(": ");
            msg.AppendLink(post.Title.Replace("\n", " "), post.Url, "Score: " + post.Score, TextFormattingOptions.None, WhiteSpace.None);
            msg.AppendText(", by ");
            msg.AppendLink(post.AuthorName, post.AuthorLink, "Reputation: " + post.AuthorRep, TextFormattingOptions.None, WhiteSpace.None);
            msg.AppendText(".");

            hq.PostMessageFast(msg);
            socvr.PostMessageFast(msg);
        }

        private static void HandleHqCommand(Message message)
        {

        }

        private static void HandleSocvrCommand(Message message)
        {

        }
    }
}
