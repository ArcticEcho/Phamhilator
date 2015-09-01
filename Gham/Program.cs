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
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using ChatExchangeDotNet;
using GibberishClassification;
using Phamhilator.Yam.Core;

namespace Phamhilator.Gham
{
    public class Program
    {
        private static readonly List<Question> nlpQuestionQueue = new List<Question>();
        private static readonly List<Answer> nlpAnswerQueue = new List<Answer>();
        private static Client chatClient;
        private static Room chatRoom;
        private static LocalRequestClient yamClient;
        private static HashSet<PoSTModel> models;
        private static Thread nlpProcessor;
        private static bool shutdown;



        private static void Main(string[] args)
        {
            Console.Title = "Gham v2";
            TryLogin();
            Console.Write("Joining chat room...");
            JoinRooms();
            Console.Write("done.\nStarting sockets...");
            InitialiseClient();

#if DEBUG
            Console.Write("done.\nGham v2 started (debug), press Q to exit.\n");
            chatRoom.PostMessage("`Gham v2 started` (**`debug`**)`.`");
#else
            Console.Write("done.\nGham v2 started, press Q to exit.\n");
            chatRoom.PostMessage("`Gham v2 started.`");
#endif

            while (!shutdown)
            {
                if (Char.ToLowerInvariant(Console.ReadKey(true).KeyChar) == 'q')
                {
                    shutdown = true;
                }
            }

            yamClient.Dispose();
            chatRoom.PostMessage("`Gham v2 stopped.`");
            chatRoom.Leave();
            chatClient.Dispose();
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
            chatRoom = chatClient.JoinRoom("http://chat.meta.stackexchange.com/rooms/773/low-quality-posts-hq");
            chatRoom.IgnoreOwnEvents = true;
            chatRoom.StripMentionFromMessages = true;
            chatRoom.EventManager.ConnectListener(EventType.UserMentioned, new Action<Message>(HandleChatCommand));
        }

        private static void InitialiseClient()
        {
            yamClient = new LocalRequestClient("GHAM");
            //yamClient.EventManager.ConnectListener(LocalRequest.RequestType.Question, new Action<Question>(???));
            //yamClient.EventManager.ConnectListener(LocalRequest.RequestType.Answer, new Action<Answer>(???));
            yamClient.EventManager.ConnectListener(LocalRequest.RequestType.Exception, new Action<LocalRequest>(req =>
            {
                yamClient.SendData(new LocalRequest
                {
                    ID = LocalRequest.GetNewID(),
                    Type = LocalRequest.RequestType.Exception,
                    Options = req.Options,
                    Data = req.Data
                });
            }));
        }

        private static void HandleChatCommand(Message command)
        {
            if (UserAccess.Owners.All(id => id != command.Author.ID)) { return; }

            var cmd = command.Content.Trim().ToUpperInvariant();

            switch (cmd)
            {
                case "STOP":
                {
                    chatRoom.PostReply(command, "Stopping...");
                    yamClient.SendData(new LocalRequest
                    {
                        ID = LocalRequest.GetNewID(),
                        Type = LocalRequest.RequestType.Info,
                        Data = "Shutting down (user command)."
                    });
                    shutdown = true;
                    break;
                }
                default:
                {
                    chatRoom.PostReply(command, "`Command not recognised.`");
                    return;
                }
            }
        }

        # region Gibberish checker.

        private static void CheckQuestionForGibberish(Question question)
        {
            var safeTitle = PostFetcher.ChatEscapeString(question.Title, " ");
            var cleanTitle = GetCleanText(question.Title);

            if (cleanTitle.Length < 10) { return; }

            var titleRes = GibberishClassifier.Classify(cleanTitle);

            if (titleRes > 75)
            {
                chatRoom.PostMessage("**Low Quality Q** (" + Math.Round(titleRes, 1) + "%): [" + safeTitle + "](" + question.Url + ").");
                return;
            }

            var plainBody = GetCleanText(question.Body);

            if (plainBody.Length < 10) { return; }

            var bodyRes = GibberishClassifier.Classify(plainBody);

            if (bodyRes > 75)
            {
                chatRoom.PostMessage("**Low Quality Q** (" + Math.Round(bodyRes, 1) + "%): [" + safeTitle + "](" + question.Url + ").");
            }
        }

        private static void CheckAnswerForGibberish(Answer answer)
        {
            var plainBody = GetCleanText(answer.Body);

            if (plainBody.Length < 10) { return; }

            var safeTitle = PostFetcher.ChatEscapeString(answer.Title, " ");
            var bodyRes = GibberishClassifier.Classify(plainBody);

            if (bodyRes > 75)
            {
                chatRoom.PostMessage("**Low Quality A** (" + Math.Round(bodyRes, 1) + "%): [" + safeTitle + "](" + answer.Url + ").");
            }
        }

        # endregion

        private static string GetCleanText(string bodyHtml)
        {
            // Remove code.
            var body = Regex.Replace(bodyHtml, @"(?is)(<pre>)?<code>.*?</code>(</pre>)?", "");

            // Remove big chunks of MathJax.
            body = Regex.Replace(body, @"(?s)\$\$.*?\$\$", "");

            // Remove all weird unicode chars.
            body = Regex.Replace(body, @"[^\u0000-\u007F]", "");

            // Remove all HTML tags.
            body = Regex.Replace(body, @"(?is)<.*?>", "");

            return body.Trim();
        }
    }
}
