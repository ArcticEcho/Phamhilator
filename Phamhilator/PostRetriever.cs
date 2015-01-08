using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using CsQuery;
using Newtonsoft.Json.Linq;
using WebSocketSharp;



namespace Phamhilator
{
    public static class PostRetriever
    {
        private static readonly Regex shareLinkIDParser = new Regex(@".*(q|a)/|/\d*", RegexOptions.Compiled | RegexOptions.CultureInvariant);
        private static readonly Regex isShareLink = new Regex(@"(q|a)/\d*/\d*$", RegexOptions.Compiled | RegexOptions.CultureInvariant);
        private static readonly Regex escapeChars = new Regex(@"[_*`\[\]]", RegexOptions.Compiled | RegexOptions.CultureInvariant);
        
        public static readonly Regex HostParser = new Regex(@".*//|/.*", RegexOptions.Compiled | RegexOptions.CultureInvariant);
        public static readonly Regex PostIDParser = new Regex(@"\D*/|\D.*", RegexOptions.Compiled | RegexOptions.CultureInvariant);



        public static Question GetQuestion(MessageEventArgs message)
        {
            var data = JToken.Parse(JObject.Parse(message.Data)["data"].ToString());

            var url = TrimUrl((string)data["url"]);

            var host = (string)data["siteBaseHostAddress"];
            var title = WebUtility.HtmlDecode((string)data["titleEncodedFancy"]);
            var authorName = WebUtility.HtmlDecode((string)data["ownerDisplayName"]);
            var authorLink = TrimUrl((string)data["ownerUrl"]);
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

            var html = new StringDownloader().DownloadString(postUrl);
            var dom = CQ.Create(html, Encoding.UTF8);
            var tags = new List<string>();

            foreach (var tag in dom[".post-taglist a"])
            {
                var t = tag.Attributes["href"];

                t = t.Remove(0, t.LastIndexOf('/') + 1);

                tags.Add(t);
            }

            var title = WebUtility.HtmlDecode(dom[".question-hyperlink"].Html());
            var body = WebUtility.HtmlDecode(dom[".post-text"].Html().Trim());
            var score = int.Parse(dom[".vote-count-post"].Html());

            string authorName;
            string authorLink;
            int authorRep;

            if (dom[".reputation-score"][0] != null)
            {
                // Normal answer.
                authorName = WebUtility.HtmlDecode(StripTags(dom[".user-details a"][0].InnerHTML));
                authorLink = TrimUrl("http://" + host + dom[".user-details a"][0].Attributes["href"]);
                authorRep = ParseRep(dom[".reputation-score"][0].InnerHTML);
            }
            else
            {
                if (dom[".user-details a"].Any(e => e.Attributes["href"] != null && e.Attributes["href"].Contains("/users/")))
                {
                    // Community wiki.
                    authorName = WebUtility.HtmlDecode(StripTags(dom[".user-details a"][1].InnerHTML));
                    authorLink = TrimUrl("http://" + host + dom[".user-details a"][1].Attributes["href"]);
                    authorRep = 1;
                }
                else
                {
                    // Dead account owner.
                    authorName = WebUtility.HtmlDecode(StripTags(dom[ ".user-details"][0].InnerHTML));
                    authorName = authorName.Remove(authorName.Length - 4);
                    authorLink = null;
                    authorRep = 1;
                }
            }

            return new Question(postUrl, title, body, host, score, authorName, authorLink, authorRep, tags);
        }

        public static Answer GetAnswer(string postUrl)
        {
            string host;
            int id;

            GetPostInfo(postUrl, out host, out id);

            var getUrl = "http://" + host + "/posts/ajax-load-realtime/" + id;
            var html = new StringDownloader().DownloadString(getUrl);
            var dom = CQ.Create(html, Encoding.UTF8);

            return GetAnswer(dom, host, id.ToString(CultureInfo.InvariantCulture));
        }

        public static List<Answer> GetLatestAnswers(Question question)
        {
            if (string.IsNullOrEmpty(question.Html)) { return new List<Answer>(); }

            var dom = CQ.Create(question.Html, Encoding.UTF8);
            var host = "";
            var questionID = 0; 
            var answers = new List<Answer>();

            GetPostInfo(question.Url, out host, out questionID);

            foreach (var a in dom[".answer"])
            {
                var id = a.Attributes["data-answerid"];
                
                answers.Add(GetAnswer(dom, host, id));
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

        public static string EscapeString(string input, string newlineReplace)
        {
            var output = input.Replace("\n", newlineReplace).Replace("\\n", newlineReplace);

            for (var i = 0; i < output.Length; i++)
            {
                if (escapeChars.IsMatch(output[i].ToString(CultureInfo.InvariantCulture)))
                {
                    output = output.Insert(i, "\\");
                    i++;
                }
            }

            return output.Trim();
        }


        private static Answer GetAnswer(CQ dom, string host, string id)
        {
            var aDom = "#answer-" + id + " ";

            var score = int.Parse(dom[aDom + ".vote-count-post"][0].InnerHTML);
            var body = WebUtility.HtmlDecode(dom[aDom + ".post-text"][0].InnerHTML.Trim());
            var url = "http://" + host + "/a/" + id;
            string authorName;
            string authorLink;
            int authorRep;

            if (dom[aDom + ".reputation-score"][0] != null)
            {
                // Normal answer.
                authorName = WebUtility.HtmlDecode(StripTags(dom[aDom + ".user-details a"][0].InnerHTML));
                authorLink = TrimUrl("http://" + host + dom[aDom + ".user-details a"][0].Attributes["href"]);
                authorRep = ParseRep(dom[aDom + ".reputation-score"][0].InnerHTML);
            }
            else
            {
                if (dom[aDom + ".user-details a"].Any(e => e.Attributes["href"] != null && e.Attributes["href"].Contains("/users/")))
                {
                    // Community wiki.
                    authorName = WebUtility.HtmlDecode(StripTags(dom[aDom + ".user-details a"][1].InnerHTML));
                    authorLink = TrimUrl("http://" + host + dom[aDom + ".user-details a"][1].Attributes["href"]);
                    authorRep = 1;
                }
                else
                {
                    // Dead account owner.
                    authorName = WebUtility.HtmlDecode(StripTags(dom[aDom + ".user-details"][0].InnerHTML));
                    authorName = authorName.Remove(authorName.Length - 4);
                    authorLink = null;
                    authorRep = 1;
                }
            }

            var excerpt = StripTags(body);

            excerpt = excerpt.Length > 75 ? excerpt.Substring(0, 72) + "..." : excerpt;

            return new Answer(url, excerpt, body, host, score, authorName, authorLink, authorRep);
        }

        private static string TrimUrl(string url)
        {
            if (String.IsNullOrEmpty(url)) { return null; }

            var trimmed = "";
            var fsCount = 0;

            for (var i = 0; i < url.Length; i++)
            {
                if (url[i] == '/')
                {
                    fsCount++;
                }

                if (fsCount == 5)
                {
                    break;
                }

                trimmed += url[i];
            }

            return trimmed;
        }

        private static void GetPostInfo(string postUrl, out string host, out int id)
        {
            host = HostParser.Replace(postUrl, "");

            if (isShareLink.IsMatch(postUrl))
            {
                id = int.Parse(shareLinkIDParser.Replace(postUrl, ""));
            }
            else
            {
                id = int.Parse(PostIDParser.Replace(postUrl, ""));
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
