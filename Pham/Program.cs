﻿/*
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

namespace Phamhilator.Pham.UI
{
    public class Program
    {
        private static readonly ConcurrentStack<Post> checkedPosts = new ConcurrentStack<Post>();
        private static readonly ManualResetEvent shutdownMre = new ManualResetEvent(false);
        private static LocalRequestClient yamClient;
        private static PostClassifier cvClassifier;
        private static PostClassifier dvClassifier;
        private static PostCheckBack checkBack;
        private static Client chatClient;
        private static Room socvr;
        private static UserAccess authUsers;
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
            Console.Write("done.\nConnecting Yam client...");
            ConnectYamCLient();
            Console.Write("done.\nStarting post model generator...");
            StartPostCheckBack();
            Console.Write("done.\nInitialising CV classifier...");
            InitialiseCVClassifier();
            Console.Write("done.\nInitialising DV classifier...");
            InitialiseDVClassifier();
            Console.Write("done.\nJoining chat room...");
            JoinRooms();
            Console.WriteLine("done.\n");

            startTime = DateTime.UtcNow;

#if DEBUG
            Console.WriteLine("Pham v2 started (debug).");
#else
            Console.WriteLine("Pham v2 started.");
#endif

            SendYamInfo("ALIVE");

            shutdownMre.WaitOne();

            Console.Write("Stopping...");
            socvr?.Leave();
            shutdownMre?.Dispose();
            chatClient?.Dispose();
            authUsers?.Dispose();
            cvClassifier?.Dispose();
            dvClassifier?.Dispose();

            SendYamInfo("DEAD");

            yamClient?.Dispose();
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

        private static void StartPostCheckBack()
        {
            var cr = new ConfigReader();
            var mins = 0;
            if (!int.TryParse(cr.GetSetting("postmodel"), out mins))
            {
                mins = 3;
            }

            checkBack = new PostCheckBack("Post Log.txt", TimeSpan.FromHours(24), TimeSpan.FromMinutes(mins));
            checkBack.ClosedPostFound = new Action<Post>(q =>
            {
                cvClassifier.AddPostToModels(q);
            });
            checkBack.DeletedPostFound = new Action<Post>(p =>
            {
                dvClassifier.AddPostToModels(p);
            });
        }

        private static void ConnectYamCLient()
        {
            yamClient = new LocalRequestClient("Pham");

            yamClient.EventManager.ConnectListener(LocalRequest.RequestType.Question, new Action<Question>(CheckQuestion));

            yamClient.EventManager.ConnectListener(LocalRequest.RequestType.Answer, new Action<Answer>(CheckAnswer));

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

            yamClient.EventManager.ConnectListener(LocalRequest.RequestType.Command, new Action<LocalRequest>(req =>
            {
                var cmd = ((string)req?.Data ?? "").Trim().ToUpperInvariant();

                switch (cmd)
                {
                    case "SHUTDOWN":
                    {
                        shutdownMre.Set();
                        break;
                    }
                    case "STATUS":
                    {
                        SendYamInfo("ALIVE");
                        break;
                    }
                } 
            }));

            authUsers = new UserAccess(ref yamClient);
        }

        private static void InitialiseCVClassifier()
        {
            cvClassifier = new PostClassifier("CV Terms.txt", ClassificationResults.SuggestedAction.Close);
        }

        private static void InitialiseDVClassifier()
        {
            dvClassifier = new PostClassifier("DV Terms.txt", ClassificationResults.SuggestedAction.Delete);
        }

        private static void JoinRooms()
        {
            var cr = new ConfigReader();

            socvr = chatClient.JoinRoom(cr.GetSetting("room"));
            socvr.EventManager.ConnectListener(EventType.UserMentioned, new Action<Message>(m => HandleChatCommand(socvr, m)));
        }

        private static void SendYamInfo(string data)
        {
            yamClient.SendData(new LocalRequest
            {
                ID = LocalRequest.GetNewID(),
                Type = LocalRequest.RequestType.Info,
                Data = data
            });
        }

        #endregion

        #region Post checking.

        private static void CheckQuestion(Question q)
        {
            if (checkedPosts.Contains(q) || q.Site != "stackoverflow.com" ||
                q.AuthorRep > 10000 || q.Score > 2) return;
            while (checkedPosts.Count > 500)
            {
                Post temp;
                checkedPosts.TryPop(out temp);
            }
            checkedPosts.Push(q);

            Task.Run(() => checkBack.AddPost(q));

            //var edRes = ???
            var cvRes = cvClassifier.ClassifyPost(q);
            var dvRes = dvClassifier.ClassifyPost(q);

            //var edScore = ???
            var cvScore = cvRes.Similarity * (cvRes.Severity * 0.5);
            var dvScore = dvRes.Similarity * (dvRes.Severity * 0.5);

            //if (edScore > 0.5 && edScore > cvScore * 0.9 && edScore > dvScore * 0.8)
            //{
            //    ReportPost(q, edRes);
            //    return;
            //}
            if (cvScore > 0.5 && /*cvScore > edScore * 1.1 &&*/ cvScore > dvScore * 0.9)
            {
                ReportPost(q, cvRes);
                return;
            }
            if (dvScore > 0.5 && /*dvScore > edScore * 1.2 &&*/ dvScore > cvScore * 1.1)
            {
                ReportPost(q, dvRes);
                return;
            }

            // If that ^ goes weird, resort to requesting an edit (if within threshold).
            //if (edScore > 0.6)
            //{
            //    ReportPost(q, edRes);
            //}
        }

        private static void CheckAnswer(Answer a)
        {
            if (checkedPosts.Contains(a) || a.Site != "stackoverflow.com" ||
                a.AuthorRep > 1000 || a.Score > 1) return;
            while (checkedPosts.Count > 500)
            {
                Post temp;
                checkedPosts.TryPop(out temp);
            }
            checkedPosts.Push(a);

            Task.Run(() => checkBack.AddPost(a));

            //var edRes = ???
            var cvRes = cvClassifier.ClassifyPost(a);
            var dvRes = dvClassifier.ClassifyPost(a);

            //var edScore = ???
            var cvScore = cvRes.Similarity * (cvRes.Severity * 0.5);
            var dvScore = dvRes.Similarity * (dvRes.Severity * 0.5);

            //if (edScore > 0.5 && edScore > cvScore * 0.9 && edScore > dvScore * 0.8)
            //{
            //    ReportPost(a, edRes);
            //    return;
            //}
            if (cvScore > 0.5 && /*cvScore > edScore * 1.1 &&*/ cvScore > dvScore * 0.9)
            {
                ReportPost(a, cvRes);
                return;
            }
            if (dvScore > 0.5 && /*dvScore > edScore * 1.2 &&*/ dvScore > cvScore * 1.1)
            {
                ReportPost(a, cvRes);
                return;
            }

            // If that ^ goes weird, resort to requesting an edit (if within threshold).
            //if (edScore > 0.6)
            //{
            //    ReportPost(a, edRes);
            //}
        }

        private static void ReportPost(Post post, ClassificationResults results)
        {
            var report = ReportFormatter.FormatReport(post, results);

            if (string.IsNullOrWhiteSpace(report)) return;

            socvr.PostMessageFast(report);
        }

        #endregion

        #region Chat command handling.

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
                            HandleNormalUserCommand(room, command);
                        }
                    }
                }
                else if (authUsers.AuthorisedUsers.Any(id => id == command.Author.ID))
                {
                    var cmdMatches = HandlePrivilegedUserCommand(room, command, false);

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
            return false;
        }

        private static bool HandlePrivilegedUserCommand(Room room, Message command, bool isOwner)
        {
            return false;
        }

        private static bool HandleOwnerCommand(Room room, Message command)
        {
            var cmd = command.Content.Trim().ToUpperInvariant();

            if (cmd == "STOP")
            {
                shutdownMre.Set();
            }
            else
            {
                return false;
            }

            return true;
        } 

        #endregion
    }
}
