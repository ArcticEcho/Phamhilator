using System;
using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;
using CsQuery;
using Newtonsoft.Json.Linq;
using WebSocketSharp;



namespace Phamhilator
{
    public static class PostRetriever
    {
        private static readonly Regex hostParser = new Regex(@".*//|/.*", RegexOptions.Compiled | RegexOptions.CultureInvariant);
        private static readonly Regex postIDParser1 = new Regex(@"\D*/|\D.*", RegexOptions.Compiled | RegexOptions.CultureInvariant);
        private static readonly Regex postIDParser2 = new Regex(@".*(q|a)/|/\d*", RegexOptions.Compiled | RegexOptions.CultureInvariant);
        private static readonly Regex isShareLink = new Regex(@"(q|a)/\d*/\d*$", RegexOptions.Compiled | RegexOptions.CultureInvariant);
        private static readonly Regex escapeChars = new Regex(@"[_*\\`\[\]]|\n", RegexOptions.Compiled);


        public static Question GetQuestion(MessageEventArgs message)
        {
            var data = JToken.Parse(JObject.Parse(message.Data)["data"].ToString());

            var url = (string)data["url"];

            var host = (string)data["siteBaseHostAddress"];
            var title = escapeChars.Replace(WebUtility.HtmlDecode((string)data["titleEncodedFancy"]), "");
            var authorName = escapeChars.Replace(WebUtility.HtmlDecode((string)data["ownerDisplayName"]), "");
            var authorLink = (string)data["ownerUrl"];
            var tags = new List<string>();

            foreach (var tag in JArray.Parse(data["tags"].ToString()))
            {
                tags.Add((string)tag);
            }

            return new Question(url, title, host, authorName, authorLink, tags);
        }

        public static Question GetQuestion(string postUrl)
        {
            string host;
            int id;

            GetPostInfo(postUrl, out host, out id);

            var html = StringDownloader.DownloadString(postUrl);
            var dom = CQ.Create(html);
            var tags = new List<string>();

            foreach (var tag in dom[".post-taglist a"])
            {
                var t = tag.Attributes["href"];

                t = t.Remove(0, t.LastIndexOf('/') + 1);

                tags.Add(t);
            }

            var title = escapeChars.Replace(WebUtility.HtmlDecode(dom[".question-hyperlink"].Html()), "");
            var body = WebUtility.HtmlDecode(dom[".post-text"].Html().Trim());
            var score = int.Parse(dom[".vote-count-post"].Html());
            var authorName = escapeChars.Replace(WebUtility.HtmlDecode(dom[".user-details a"].Html()), "");
            var authorLink = "http://" + host + dom[".user-details a"].Attr("href");
            var authorRep = ParseRep(dom[".reputation-score"].Html());

            return new Question(postUrl, title, body, host, score, authorName, authorLink, authorRep, tags);
        }

        public static Answer GetAnswer(string postUrl)
        {
            string host;
            int id;

            GetPostInfo(postUrl, out host, out id);

            var getUrl = "http://" + host + "/posts/ajax-load-realtime/" + id;
            var html = StringDownloader.DownloadString(getUrl);
            var dom = CQ.Create(html);

            var score = int.Parse(dom[".vote-count-post"].Html());
            var body = WebUtility.HtmlDecode(dom[".post-text"].Html().Trim());
            var authorName = escapeChars.Replace(WebUtility.HtmlDecode(dom[".user-details a"].Html()), "");
            var authorLink = "http://" + host + dom[".user-details a"].Attr("href");
            var authorRep = ParseRep(dom[".reputation-score"].Html());
            var excerpt = escapeChars.Replace(StripTags(body), "").Trim();

            excerpt = excerpt.Length > 50 ? excerpt.Substring(0, 47) + "..." : excerpt;

            return new Answer(postUrl, excerpt, body, host, score, authorName, authorLink, authorRep);
        }

        public static List<Answer> GetLatestAnswers(Question question)
        {
            var answers = new List<Answer>();
            var host = "";
            var questionID = 0;

            GetPostInfo(question.Url, out host, out questionID);

		    var html = StringDownloader.DownloadString(question.Url + "?answertab=active#tab-top");
		    var dom = CQ.Create(html);

            for (var i = 0; i < dom[".answer"].Length; i++)
            {
                var id = dom[".answer"][i].Attributes["data-answerid"];
                var score = int.Parse(dom[".vote-count-post"][i + 1].InnerHTML);
                var body = WebUtility.HtmlDecode(dom[".post-text"][i + 1].InnerHTML.Trim());
                var url = "http://" + host + "/a/" + id;
                var authorName = escapeChars.Replace(WebUtility.HtmlDecode(dom[".user-details a"][i + 1].InnerHTML), "");
                var authorLink = "http://" + host + dom[".user-details a"][i + 1].Attributes["href"];
                var authorRep = ParseRep(dom[".reputation-score"][i + 1].InnerHTML);
                var excerpt = escapeChars.Replace(StripTags(body), "").Trim();

                excerpt = excerpt.Length > 75 ? excerpt.Substring(0, 72) + "..." : excerpt;

                answers.Add(new Answer(url, excerpt, body, host, score, authorName, authorLink, authorRep));
            }

            return answers;
        }

        public static int ParseRep(string rep)
        {
            if (String.IsNullOrEmpty(rep))  {  return 1; }

            if (rep.ToLowerInvariant().Contains("k"))
            {
                if (rep.Contains("."))
                {
                    var charsAfterPeriod = rep.Substring(0, rep.IndexOf(".", StringComparison.Ordinal) + 1).Length;
                    var e = float.Parse(rep.Replace("k", ""));
                    var p = Math.Pow(10, charsAfterPeriod);

                    return (int)Math.Round(e * p);
                }

                return (int)float.Parse(rep.ToLowerInvariant().Replace("k", "000"));
            }

            return (int)float.Parse(rep);
        }



        private static void GetPostInfo(string postUrl, out string host, out int id)
        {
            host = hostParser.Replace(postUrl, "");

            if (isShareLink.IsMatch(postUrl))
            {
                id = int.Parse(postIDParser2.Replace(postUrl, ""));
            }
            else
            {
                id = int.Parse(postIDParser1.Replace(postUrl, ""));
            }
        }

        private static string StripTags(string source)
        {
            var array = new char[source.Length];
            var arrayIndex = 0;
            var inside = false;

            for (var i = 0; i < source.Length; i++)
            {
                var let = source[i];

                if (let == '<')
                {
                    inside = true;

                    continue;
                }

                if (let == '>')
                {
                    inside = false;

                    continue;
                }

                if (!inside)
                {
                    array[arrayIndex] = let;
                    arrayIndex++;
                }
            }

            return new string(array, 0, arrayIndex);
        }
    }
}
