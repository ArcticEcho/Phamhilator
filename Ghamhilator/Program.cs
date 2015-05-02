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
using NLP;
using System.IO;



namespace Ghamhilator
{
    public class Program
    {
        private static readonly int[] owners = new[] { 227577, 266094, 229438 }; // Sam, Uni & Fox.
        private static Client chatClient;
        private static Room primaryRoom;
        private static PostListener postListener;
        private static bool shutdown;
        private static PoSTagger tagger;
        private static HashSet<PoSTModel> models;
        private static Thread nlpProcessor;
        private static List<Question> nlpQuestionQueue = new List<Question>();
        private static List<Answer> nlpAnswerQueue = new List<Answer>();



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



            TryLogin();

            JoinRooms();

            Console.Write("done.\nStarting sockets...");

            InitialiseSocket();

            Console.Write("done.\nLoading core PoS tagger: ");

            tagger = new PoSTagger();

            Console.Write("\nLoading model(s)...");

            models = new HashSet<PoSTModel>();
            foreach (var m in Directory.EnumerateFiles(PoSTModelFDBManager.modelDir))
            {
                models.Add(new PoSTModelFDBManager(Path.GetFileName(m)).LoadModel());
            }
             
            nlpProcessor = new Thread(NLPProcessor);
            nlpProcessor.Start();

            Console.WriteLine("done.\nGhamhilator started (press Q to exit).\n");

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
            postListener.OnActiveQuestion += CheckQuestionNLP;//CheckQuestionForGibberish;
            postListener.OnActiveAnswer += CheckAnswerNLP;//CheckAnswerForGibberish;
        }

        private static void CheckQuestionNLP(Question q)
        {
            if (nlpQuestionQueue.Count < 3 && q.Score < 2)
            {
                nlpQuestionQueue.Add(q);
            }
        }

        private static void CheckAnswerNLP(Answer a)
        {
            if (nlpAnswerQueue.Count < 3 && a.Score < 2)
            {
                nlpAnswerQueue.Add(a);
            }
        }

        private static void NLPProcessor()
        {
            var processQuestionNext = true;

            while (!shutdown)
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
                            primaryRoom.PostMessage("**Spam Q** (`" + model.ModelID + "` @ "+ Math.Round(score * 100, 1) + "%): [" + safeTitle + "](" + q.Url + "), by [" + q.AuthorName + "](" + q.AuthorLink + "), on `" + q.Site + "`.");
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
                            primaryRoom.PostMessage("**Spam A** (`" + model.ModelID + "` @ " + Math.Round(score * 100, 1) + "%): [" + safeTitle + "](" + a.Url + "), by [" + a.AuthorName + "](" + a.AuthorLink + "), on `" + a.Site + "`.");
                            break;
                        }
                    }
                }

                processQuestionNext = !processQuestionNext;
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
                primaryRoom.PostMessage("**Low Quality Q** (" + Math.Round(titleRes, 1) + "%): [" + safeTitle + "](" + question.Url + ").");
                return;
            }

            var plainBody = GetCleanText(question.Body);

            if (plainBody.Length < 10) { return; }

            var bodyRes = GibberishClassifier.Classify(plainBody);

            if (bodyRes > 75)
            {
                primaryRoom.PostMessage("**Low Quality Q** (" + Math.Round(bodyRes, 1) + "%): [" + safeTitle + "](" + question.Url + ").");
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
                primaryRoom.PostMessage("**Low Quality A** (" + Math.Round(bodyRes, 1) + "%): [" + safeTitle + "](" + answer.Url + ").");
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
