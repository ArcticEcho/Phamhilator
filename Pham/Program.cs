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

namespace Phamhilator.Pham.UI
{
    public class Program
    {
        private static string thresholdDataManagerKey = "Threshold";
        private static readonly HashSet<Post> checkedPosts = new HashSet<Post>();
        private static readonly ManualResetEvent shutdownMre = new ManualResetEvent(false);
        private static LocalRequestClient yamClient;
        private static Client chatClient;
        private static Room hq;
        private static Room tavern;
        private static UserAccess userAccess;
        private static Flagger flagger;
        private static CueManager cueManager;
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

            tavern.Leave();
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

            Console.Write("done.\nLoading config data...");

            userAccess = new UserAccess(ref yamClient);
            if (!yamClient.DataExists("Pham", thresholdDataManagerKey))
            {
                yamClient.UpdateData("Pham", thresholdDataManagerKey, "0");
            }
            threshold = float.Parse(yamClient.RequestData("Pham", thresholdDataManagerKey));

            Console.Write("done.\nInitialising CueManager...");

            cueManager = new CueManager(ref yamClient);

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
            hq.EventManager.ConnectListener(EventType.MessagePosted, new Action<Message>(HandleHqNewMessage));
            hq.EventManager.ConnectListener(EventType.MessageEdited, new Action<Message>(newMessage => HandleHqNewMessage(newMessage)));

            Console.Write("done.\nJoining Tavern...");

            tavern = chatClient.JoinRoom("http://chat.meta.stackexchange.com/rooms/89");
            tavern.EventManager.ConnectListener(EventType.MessagePosted, new Action<Message>(HandleTavernNewMessage));
            tavern.EventManager.ConnectListener(EventType.MessageEdited, new Action<Message>(newMessage => HandleTavernNewMessage(newMessage)));

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
            if (checkedPosts.Contains(post)) { return; }
            checkedPosts.Add(post);

            // Check title.
            var titleCues = cueManager.FindCues(post.Title, post.Site);
            var titleType = CheckCues(titleCues);

            // Check body.
            var bodyCues = cueManager.FindCues(post.Body, post.Site);
            var bodyType = CheckCues(titleCues);

            if (titleType.Key != PostType.Clean)
            {
                ReportPost(post, titleType.Key, titleType.Value, true);
            }
            else if (bodyType.Key != PostType.Clean)
            {
                ReportPost(post, bodyType.Key, bodyType.Value, false);
            }
        }

        private static KeyValuePair<PostType, float> CheckCues(Dictionary<CueType, HashSet<Cue>> foundCues)
        {
            var clean = new KeyValuePair<PostType, float>(PostType.Clean, 0);
            if (foundCues == null || foundCues.Count == 0) { return clean; }
            if (foundCues.Keys.Count == 1 && foundCues.ContainsKey(CueType.Clean)) { return clean; }

            var scores = new Dictionary<CueType, List<float>>();

            foreach (CueType cueType in Enum.GetValues(typeof(CueType)))
            {
                var cueScores = new List<float>();

                foreach (var cue in foundCues[cueType])
                {
                    var score = cue.Weight;
                    score -= cue.Negative / cue.Found;
                    score += cue.Positive / cue.Found;
                    cueScores.Add(score);
                }

                scores[cueType] = cueScores;
            }

            if (foundCues.ContainsKey(CueType.Clean))
            {
                var cleanTotal = scores[CueType.Clean].Sum();
                var badTotal = scores.Sum(x => x.Value.Sum()) - cleanTotal;
                if (badTotal - cleanTotal < 0) { return clean; }
            }

            var highestScore = new KeyValuePair<CueType, float>(CueType.Clean, 0);

            foreach (var scoreSet in scores)
            {
                var sum = scoreSet.Value.Sum();

                if (sum > highestScore.Value)
                {
                    highestScore = new KeyValuePair<CueType, float>(scoreSet.Key, sum);
                }
            }

            return new KeyValuePair<PostType, float>((PostType)(int)highestScore.Key, highestScore.Value);
        }

        private static void ReportPost(Post post, PostType type, float score, bool title)
        {
            var msg = new MessageBuilder();

            msg.AppendText("Junk title detected ", TextFormattingOptions.Bold);
            msg.AppendText("(" + Math.Round(score, 2) + ")");
            msg.AppendLink(post.Title, post.Url, null, TextFormattingOptions.InLineCode, WhiteSpace.None);
            msg.AppendText(".");

            hq.PostMessageFast(msg);
        }

        private static void HandleHqNewMessage(Message message)
        {
            if (!UserAccess.Owners.Contains(message.Author) &&
                !userAccess.AuthorisedUsers.Contains(message.Author.ID))
            {
                return;
            }

            var cmd = message.Content.Trim().ToUpperInvariant();

            if (cmd.Length < 6) { return; }

            switch (cmd.Substring(0, 6))
            {
                case "ADD CL":
                {

                    // Add clean cue.
                    break;
                }
                case "ADD LQ":
                {
                    // Add LQ cue.
                    break;
                }
                case "ADD SP":
                {
                    // Add spam cue.
                    break;
                }
                case "ADD OF":
                {
                    // Add offensive cue.
                    break;
                }
                case "DEL CL":
                {
                    // Remove clean cue.
                    break;
                }
                case "DEL LQ":
                {
                    // Remove LQ cue.
                    break;
                }
                case "DEL SP":
                {
                    // Remove spam cue.
                    break;
                }
                case "DEL OF":
                {
                    // Remove offensive cue.
                    break;
                }
            }
        }

        private static void HandleTavernNewMessage(Message message)
        {

        }
    }
}
