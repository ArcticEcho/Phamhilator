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
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using ChatExchangeDotNet;
using GibberishClassification;
using Phamhilator.NLP;
using Phamhilator.Yam.Core;
using Newtonsoft.Json;

namespace Phamhilator.Gham
{
    public class Program
    {
        //private static readonly int[] owners = new[] { 227577, 266094, 229438 }; // Sam, Uni & Fox.
        private static readonly List<Question> nlpQuestionQueue = new List<Question>();
        private static readonly List<Answer> nlpAnswerQueue = new List<Answer>();
        private static Client chatClient;
        private static Room chatRoom;
        private static YamClientLocal yamClient;
        private static PoSTagger tagger;
        private static HashSet<PoSTModel> models;
        private static Thread nlpProcessor;
        private static bool shutdown;



        private static void Main(string[] args)
        {



            //tagger = new POST();



            //var f = tagger.TagString("Have you thought about plastic surgery, Botox or laser treatments to give you the skin you desire?").Split(' ');
            //var tags = new List<PoSTag>();

            //foreach (var t in f)
            //{
            //    tags.Add(new PoSTag(t));
            //}

            //var m = new PoSTModel(tags.ToArray(), "M0");
            //new PoSTModelFDBManager("M0").UpdateModel(m);

            //f = tagger.TagString("With so many teeth whitening products on store shelves and at dental offices, choosing the best teeth whitener can prove challenging.").Split(' ');
            //tags = new List<PoSTag>();

            //foreach (var t in f)
            //{
            //    tags.Add(new PoSTag(t));
            //}

            //m = new PoSTModel(tags.ToArray(), "M1");
            //new PoSTModelFDBManager("M1").UpdateModel(m);

            //f = tagger.TagString("Do not initiate intense cardio upbringing with metric training.").Split(' ');
            //tags = new List<PoSTag>();

            //foreach (var t in f)
            //{
            //    tags.Add(new PoSTag(t));
            //}

            //m = new PoSTModel(tags.ToArray(), "M2");
            //new PoSTModelFDBManager("M2").UpdateModel(m);

            Console.Title = "Gham v2";
            TryLogin();
            Console.Write("Joining chat room...");
            JoinRooms();
            Console.Write("done.\nStarting sockets...");
            InitialiseClient();
            Console.Write("done.\nLoading core PoS tagger: ");
            tagger = new PoSTagger();
            Console.Write("\nRequesting model(s)...");



            yamClient.UpdateData("gham", "Model Count", "2");
            yamClient.UpdateData("gham", "PoST Model:0", File.ReadAllText(@"C:\Users\Samuel\Documents\GitHub\Phamhilator\Ghamhilator\bin\Debug\PoST Models\M0"));
            yamClient.UpdateData("gham", "PoST Model:1", File.ReadAllText(@"C:\Users\Samuel\Documents\GitHub\Phamhilator\Ghamhilator\bin\Debug\PoST Models\M1"));




            try
            {
                models = new HashSet<PoSTModel>();
                var modelCount = int.Parse(yamClient.RequestData("gham", "Model Count"));
                for (var i = 0; i < modelCount; i++)
                {
                    var modelJson = yamClient.RequestData("gham", "PoST Model:" + i);
                    models.Add(JsonConvert.DeserializeObject<PoSTModel>(modelJson));
                }
                nlpProcessor = new Thread(NLPProcessor);
                nlpProcessor.Start();
            }
            catch (Exception ex)
            {
                yamClient.SendData(new LocalRequest
                {
                    ID = LocalRequest.GetNewID(),
                    Type = LocalRequest.RequestType.Exception,
                    Data = ex
                });
            }

            Console.WriteLine("done.\nGham started (press Q to exit).\n");
            chatRoom.PostMessage("`Gham started.`");

            while (!shutdown)
            {
                if (Char.ToLowerInvariant(Console.ReadKey(true).KeyChar) == 'q')
                {
                    shutdown = true;
                }
            }

            yamClient.Dispose();
            chatRoom.PostMessage("`Gham stopped.`");
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
            yamClient = new YamClientLocal("GHAM");
            yamClient.EventManager.ConnectListener(LocalRequest.RequestType.Question, new Action<Question>(CheckQuestionNLP));
            yamClient.EventManager.ConnectListener(LocalRequest.RequestType.Answer, new Action<Answer>(CheckAnswerNLP));
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
            if (!UserAccess.Owners.Select(u => u.ID).Contains(command.AuthorID)) { return; }

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

        private static void CheckQuestionNLP(Question q)
        {
            if (nlpQuestionQueue.Count < 3 && q.Score < 2 && q.AuthorRep < 1000)
            {
                nlpQuestionQueue.Add(q);
            }
        }

        private static void CheckAnswerNLP(Answer a)
        {
            if (nlpAnswerQueue.Count < 3 && a.Score < 2 && a.AuthorRep < 1000)
            {
                nlpAnswerQueue.Add(a);
            }
        }

        private static void NLPProcessor()
        {
            var processQuestionNext = true;

            while (!shutdown)
            {
                try
                {
                    Thread.Sleep(50);

                    if (nlpQuestionQueue.Count < 0 && nlpAnswerQueue.Count < 0) { Thread.Sleep(100); continue; }

                    if (processQuestionNext && nlpQuestionQueue.Count > 0)
                    {
                        var q = nlpQuestionQueue.First();
                        nlpQuestionQueue.Remove(q);
                        var cleanText = GetCleanText(q.Body);

                        if (cleanText.Length < 10) { continue; }

                        var safeTitle = PostFetcher.ChatEscapeString(q.Title, "");
                        var cleanSentence = StringTools.GetSentences(cleanText)[0];
                        var tags = tagger.TagString(cleanSentence).Split(' ');
                        var words = cleanSentence.Split(' ').Where(s => !String.IsNullOrEmpty(s)).ToArray();

                        foreach (var model in models)
                        {
                            var score = CheckTags(model, words, tags);

                            if (score > 0.4)
                            {
                                chatRoom.PostMessage("**Spam Q** (`" + model.ModelID + "` @ "+ Math.Round(score * 100, 1) + "%): [" + safeTitle + "](" + q.Url + "), by [" + q.AuthorName + "](" + q.AuthorLink + "), on `" + q.Site + "`.");
                                break;
                            }
                        }
                    }

                    if (!processQuestionNext && nlpAnswerQueue.Count > 0)
                    {
                        var a = nlpAnswerQueue.First();
                        nlpAnswerQueue.Remove(a);
                        var cleanText = GetCleanText(a.Body);

                        if (cleanText.Length < 10) { continue; }

                        var safeTitle = PostFetcher.ChatEscapeString(a.Title, " ");
                        var cleanSentence = StringTools.GetSentences(cleanText)[0];
                        var tags = tagger.TagString(cleanSentence).Split(' ');
                        var words = cleanSentence.Split(' ').Where(s => !String.IsNullOrEmpty(s)).ToArray();

                        foreach (var model in models)
                        {
                            var score = CheckTags(model, words, tags);

                            if (score > 0.4)
                            {
                                chatRoom.PostMessage("**Spam A** (`" + model.ModelID + "` @ " + Math.Round(score * 100, 1) + "%): [" + safeTitle + "](" + a.Url + "), by [" + a.AuthorName + "](" + a.AuthorLink + "), on `" + a.Site + "`.");
                                break;
                            }
                        }
                    }

                    processQuestionNext = !processQuestionNext;
                }
                catch (Exception ex)
                {
                    yamClient.SendData(new LocalRequest
                    {
                        ID = LocalRequest.GetNewID(),
                        Type = LocalRequest.RequestType.Exception,
                        Data = ex
                    });
                }
            }
        }

        private static float CheckTags(PoSTModel model, string[] words, string[] sentenceTags)
        {
            var score = 0F;
            var totalScore = model.Tags.Sum(t => t.SpamRating.Rating * t.SpamRating.Maturity);
            var k = Math.Min(sentenceTags.Length, model.Tags.Length);

            for (var i = 0; i < k; i++)
            {
                if (model.Tags[i].Tag == sentenceTags[i])
                {
                    score += model.Tags[i].SpamRating.Rating * model.Tags[i].SpamRating.Maturity;

                    foreach (var blackWord in model.Tags[i].BlackKeyWords)
                    {
                        if (words.Contains(blackWord.Word))
                        {
                            score += blackWord.SpamRating.Rating;
                            break;
                        }
                    }

                    foreach (var whiteWord in model.Tags[i].WhiteKeyWords)
                    {
                        if (words.Contains(whiteWord.Word))
                        {
                            score -= whiteWord.SpamRating.Rating;
                            break;
                        }
                    }
                }
            }

            return score / totalScore;
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
