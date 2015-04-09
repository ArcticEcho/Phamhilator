using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChatExchangeDotNet;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using JsonFx.Json;
using Phamhilator.Core;
using GibberishClassification;
using System.Text.RegularExpressions;



namespace Ghamhilator
{
    public class Program
    {
        private static readonly int[] owners = new[] { 227577, 266094, 229438 }; // Sam, Uni & Fox.
        private static Client chatClient;
        private static Room primaryRoom;
        private static PostListener postListener;
        private static bool shutdown;



        private static void Main(string[] args)
        {
            TryLogin();

            Console.Write("Joining room(s)...");

            JoinRooms();

            Console.Write("done.\nStarting sockets...");

            InitialiseSocket();

            Console.Write("done.\nGhamhilator started (press Q to exit).\n");
            primaryRoom.PostMessage("`Ghamhilator started.`");

            while (!shutdown)
            {
                if (Char.ToLowerInvariant(Console.ReadKey(true).KeyChar) == 'q')
                {
                    shutdown = true;
                }
            }

            postListener.Dispose();

            primaryRoom.PostMessage("`Ghamhilator stopped.`");
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
            Console.Write("Joining primary room...");

            primaryRoom = chatClient.JoinRoom("http://chat.meta.stackexchange.com/rooms/773/low-quality-posts-hq");
            primaryRoom.IgnoreOwnEvents = false;
            primaryRoom.StripMentionFromMessages = false;
            primaryRoom.EventManager.ConnectListener(EventType.UserMentioned, new Action<Message>(HandleCommand));
        }

        private static void HandleCommand(Message command)
        {
            if (!owners.Contains(command.AuthorID)) { return; }

            if (command.Content.Trim() == "stop")
            {
                primaryRoom.PostMessage("Stopping...");
                shutdown = true;
            }
        }

        private static void InitialiseSocket()
        {
            postListener = new PostListener();

            postListener.OnActiveQuestion += question =>
            {
                var safeTitle = PostFetcher.ChatEscapeString(question.Title, " ");
                var cleanTitle = GetCleanText(question.Title);

                if (cleanTitle.Length < 5) { return; }

                var titleRes = GibberishClassifier.Classify(cleanTitle);

                if (titleRes > 75)
                {
                    primaryRoom.PostMessage("**Low Quality Q** (" + Math.Round(titleRes, 1) + "%): [" + safeTitle + "](" + question.Url + ").");
                    return;
                }

                var plainBody = GetCleanText(question.Body);

                if (plainBody.Length < 5) { return; }

                var bodyRes = GibberishClassifier.Classify(plainBody);

                if (bodyRes > 75)
                {
                    primaryRoom.PostMessage("**Low Quality Q** (" + Math.Round(bodyRes, 1) + "%): [" + safeTitle + "](" + question.Url + ").");
                }
            };

            postListener.OnActiveAnswer += answer =>
            {
                var plainBody = GetCleanText(answer.Body);

                if (plainBody.Length < 5) { return; }

                var safeTitle = PostFetcher.ChatEscapeString(answer.Title, " ");
                var bodyRes = GibberishClassifier.Classify(plainBody);

                if (bodyRes > 75)
                {
                    primaryRoom.PostMessage("**Low Quality A** (" + Math.Round(bodyRes, 1) + "%): [" + safeTitle + "](" + answer.Url + ").");
                }
            };
        }

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

            return body;
        }
    }
}
