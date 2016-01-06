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
using Phamhilator.Yam.Core;
using ChatExchangeDotNet;
using System.Linq;
using System.Collections.Concurrent;
using Phamhilator.Updater;
using System.IO;
using System.Diagnostics;

namespace Phamhilator.Pham.UI
{
    public class Program
    {
        private const string wikiCmdsLink = "https://github.com/ArcticEcho/Phamhilator/wiki/Chat-Commands";
        private static readonly ConcurrentStack<Post> checkedPosts = new ConcurrentStack<Post>();
        private static readonly ManualResetEvent shutdownMre = new ManualResetEvent(false);
        private static RealtimePostSocket postSocket;
        private static PostClassifier cvClassifier;
        private static PostClassifier qdvClassifier;
        private static PostClassifier advClassifier;
        private static PostCheckBack checkBack;
        //private static AppveyorUpdater updater; //TODO: Not used yet.
        private static Client chatClient;
        private static Room socvr;
        private static Room lqphq;
        private static DateTime startTime;



        static void Main(string[] args)
        {
            Console.Title = "Pham v2";
            Console.CancelKeyPress += (o, oo) =>
            {
                oo.Cancel = true;
                shutdownMre.Set();
            };

            Console.Write("Authenticating...");
            AuthenticateChatClient();
            //Console.Write("done.\nInitialising updater...");
            //InitialiseUpdater();
            Console.Write("done.\nStarting post websocket...");
            StartPostWebSocket();
            Console.Write("done.\nStarting post model generator...");
            StartPostCheckBack();
            Console.Write("done.\nInitialising CV classifier...");
            InitialiseCVClassifier();
            Console.Write("done.\nInitialising Q DV classifier...");
            InitialiseQDVClassifier();
            Console.Write("done.\nInitialising A DV classifier...");
            InitialiseQDVClassifier();
            Console.Write("done.\nJoining chat room(s)...");
            JoinRooms();
            Console.WriteLine("done.\n");

            startTime = DateTime.UtcNow;

            var startupMsg = $"Pham v2 started.";
            Console.WriteLine(startupMsg);
            socvr.PostMessageFast(startupMsg);
            lqphq.PostMessageFast(startupMsg);

            shutdownMre.WaitOne();

            Console.Write("Stopping...");
            socvr?.PostMessageFast("Bye.");
            lqphq?.PostMessageFast("Bye.");

            lqphq?.Leave();
            socvr?.Leave();
            postSocket?.Dispose();
            shutdownMre?.Dispose();
            chatClient?.Dispose();
            cvClassifier?.Dispose();
            qdvClassifier?.Dispose();
            advClassifier?.Dispose();
            checkBack?.Dispose();

            Console.WriteLine("done.");
        }



        #region Program initialisation.

        private static void AuthenticateChatClient()
        {
            var cr = new ConfigReader();

            var email = cr.GetSetting("se email");
            var pwd = cr.GetSetting("se pass");

            chatClient = new Client(email, pwd);
        }

        //private static void InitialiseUpdater()
        //{
        //    var cr = new ConfigReader();
        //    updater = new AppveyorUpdater(cr.GetSetting("appveyor"), "ArcticEcho", "Phamhilator");
        //}

        private static void StartPostWebSocket()
        {
            postSocket = new RealtimePostSocket(true);
            postSocket.OnActiveQuestion += p => CheckPost(p);
            postSocket.OnActiveAnswer += p => CheckPost(p);
        }

        private static void StartPostCheckBack()
        {
            checkBack = new PostCheckBack("post-checkback-log.txt", TimeSpan.FromMinutes(1));
            checkBack.ClosedQuestionFound = new Action<Post>(q =>
            {
                cvClassifier.AddPostToModels(q);
            });
            checkBack.DeletedQuestionFound = new Action<Post>(p =>
            {
                qdvClassifier.AddPostToModels(p);
            });
            checkBack.DeletedAnswerFound = new Action<Post>(p =>
            {
                qdvClassifier.AddPostToModels(p);
            });
        }

        private static void InitialiseCVClassifier()
        {
            cvClassifier = new PostClassifier("cv-models.txt", ClassificationResults.SuggestedAction.Close);
        }

        private static void InitialiseQDVClassifier()
        {
            qdvClassifier = new PostClassifier("qdv-models.txt", ClassificationResults.SuggestedAction.Delete);
        }

        private static void InitialiseADVClassifier()
        {
            advClassifier = new PostClassifier("adv-models.txt", ClassificationResults.SuggestedAction.Delete);
        }

        private static void JoinRooms()
        {
            var cr = new ConfigReader();

            socvr = chatClient.JoinRoom(cr.GetSetting("room"));

            socvr.EventManager.ConnectListener(EventType.UserMentioned,
                new Action<Message>(m => HandleChatCommand(socvr, m)));

            socvr.EventManager.ConnectListener(EventType.MessageReply,
                new Action<Message, Message>((p, c) => HandleChatCommand(socvr, c)));

            lqphq = chatClient.JoinRoom("http://chat.meta.stackexchange.com/rooms/773");

            lqphq.EventManager.ConnectListener(EventType.UserMentioned,
                new Action<Message>(m => HandleChatCommand(lqphq, m)));

            lqphq.EventManager.ConnectListener(EventType.MessageReply,
                new Action<Message, Message>((p, c) => HandleChatCommand(lqphq, c)));
        }

        #endregion

        #region Post checking.

        private static void CheckPost(Post p)
        {
            if (checkedPosts.Contains(p) || p.Site != "stackoverflow.com" ||
                p.AuthorRep > 10000 || p.Score > 2) return;
            while (checkedPosts.Count > 1000)
            {
                Post temp;
                checkedPosts.TryPop(out temp);
            }
            checkedPosts.Push(p);

            Task.Run(() => checkBack.AddPost(p));

            if (p.IsQuestion)
            {
                CheckQuestion(p);
            }
            else
            {
                CheckAnswer(p);
            }
        }

        private static void CheckQuestion(Post p)
        {
            var cvRes = cvClassifier.ClassifyPost(p);
            var dvRes = qdvClassifier.ClassifyPost(p);

            if (cvRes.Similarity > 0.5 && cvRes.Similarity > dvRes.Similarity * 0.9)
            {
                ReportPost(p, cvRes);
            }
            else if (dvRes.Similarity > 0.5 && dvRes.Similarity > cvRes.Similarity * 1.1)
            {
                ReportPost(p, dvRes);
            }
        }

        private static void CheckAnswer(Post p)
        {
            var dvRes = advClassifier.ClassifyPost(p);

            if (dvRes.Similarity > 0.5)
            {
                ReportPost(p, dvRes);
                return;
            }
        }

        private static void ReportPost(Post post, ClassificationResults results)
        {
            var report = ReportFormatter.FormatReport(post, results);

            if (string.IsNullOrWhiteSpace(report)) return;

            socvr.PostMessageFast(report);
            lqphq.PostMessageFast(report);
        }

        #endregion

        #region Chat command handling.

        private static void HandleChatCommand(Room room, Message command)
        {
            try
            {
                if (UserAccess.Owners.Any(id => id == command.Author.ID) ||
                    command.Author.IsRoomOwner || command.Author.IsMod)
                {
                    var cmdMatches = HandleOwnerCommand(room, command);

                    if (!cmdMatches)
                    {
                        cmdMatches = HandlePrivilegedUserCommand(room, command);

                        if (!cmdMatches)
                        {
                            HandleNormalUserCommand(room, command);
                        }
                    }
                }
                else if (command.Author.Reputation >= 3000)
                {
                    var cmdMatches = HandlePrivilegedUserCommand(room, command);

                    if (!cmdMatches)
                    {
                        HandleNormalUserCommand(room, command);
                    }
                }
                else
                {
                    HandleNormalUserCommand(room, command);
                }
            }
            catch (Exception ex)
            {
                room.PostReplyFast(command, $"`Unable to execute command: {ex.Message}`");
            }
        }

        private static bool HandleNormalUserCommand(Room room, Message command)
        {
            var cmd = command.Content.Trim().ToUpperInvariant();

            switch (cmd)
            {
                case "ALIVE":
                {
                    var statusReport = $"Yes, I'm alive (`{DateTime.UtcNow - startTime}`).";
                    room.PostMessageFast(statusReport);
                    return true;
                }
                case "COMMANDS":
                {
                    var msg = $"See [here]({wikiCmdsLink} \"Chat Commands Wiki\").";
                    room.PostReplyFast(command, msg);
                    return true;
                }
                //case "VERSION":
                //{
                //    var msg = $"My current version is: `{updater.CurrentVersion}`.";
                //    room.PostReplyFast(command, msg);
                //    return true;
                //}
                default:
                {
                    return false;
                }
            }
        }

        private static bool HandlePrivilegedUserCommand(Room room, Message command)
        {
            var cmd = command.Content.Trim().ToUpperInvariant();

            switch (cmd)
            {
                case "DEL":
                {
                    room.DeleteMessage(command.ParentID);
                    return true;
                }
                default:
                {
                    return false;
                }
            }
        }

        private static bool HandleOwnerCommand(Room room, Message command)
        {
            var cmd = command.Content.Trim().ToUpperInvariant();

            switch (cmd)
            {
                case "STOP":
                {
                    shutdownMre.Set();
                    return true;
                }
                //case "UPDATE":
                //{
                //    UpdateBot(room, command);
                //    return true;
                //}
                default:
                {
                    return false;
                }
            }
        }

        //private static void UpdateBot(Room rm, Message cmd)
        //{
        //    if (updater == null)
        //    {
        //        rm.PostReplyFast(cmd, "This feature has been disabled by the host (API key not specified).");
        //        return;
        //    }

        //    var remVer = updater.LatestVersion;

        //    if (updater.CurrentVersion == remVer)
        //    {
        //        rm.PostReplyFast(cmd, "There aren't any updates available at the moment.");
        //        return;
        //    }

        //    rm.PostReplyFast(cmd, $"Updating to `{remVer}`:");
        //    rm.PostMessageFast($"> {updater.LatestVerMessage}");

        //    var exes = updater.UpdateAssemblies();

        //    if (exes != null)
        //    {
        //        rm.PostReplyFast(cmd, "Update successful! Now rebooting...");

        //        var phamExe = exes.First(x => Path.GetFileName(x).Contains("Pham"));

        //        if (Environment.OSVersion.Platform == PlatformID.Win32NT)
        //        {
        //            Process.Start(phamExe);
        //        }
        //        else
        //        {
        //            Process.Start($"mono {phamExe}");
        //        }

        //        shutdownMre.Set();
        //    }
        //    else
        //    {
        //        rm.PostReplyFast(cmd, "Update failed!");
        //    }
        //}

        #endregion
    }
}
