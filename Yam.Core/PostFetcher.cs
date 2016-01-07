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
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using CsQuery;
using WebSocketSharp;
using ServiceStack.Text;
using System.IO;

namespace Phamhilator.Yam.Core
{
    public static class PostFetcher
    {
        private const RegexOptions regOpts = RegexOptions.CultureInvariant | RegexOptions.Compiled;
        private static readonly Regex hostParser = new Regex(@".*//|/.*", regOpts);
        private static readonly Regex isQuestionUrl = new Regex(@"(?i)/(q(uestions)?)/(\d+)/?", regOpts);
        private static readonly Regex postIDParser = new Regex(@"(?i)/(q(uestions)?|a)/(\d+)/?", regOpts);
        private static readonly Regex userNetworkID = new Regex(@"accountId: \d+", regOpts);
        private static readonly Regex questionStatusDiv = new Regex("(?s)<div class=\"question-status.*?</div>", regOpts);



        public static Question GetQuestion(MessageEventArgs message, out CQ dom)
        {
            var obj = JsonObject.Parse(message.Data);
            var data = obj.Get("data");
            var innerObj = JsonSerializer.DeserializeFromString<Dictionary<string, object>>(data);

            var url = TrimUrl((string)innerObj["url"]);
            var id = 0u;
            var host = "";
            ParsePostLink(url, out host, out id);
            var title = WebUtility.HtmlDecode((string)innerObj["titleEncodedFancy"]);
            var authorName = WebUtility.HtmlDecode((string)innerObj["ownerDisplayName"]);
            var tags = JsonSerializer.DeserializeFromString<string[]>((string)innerObj["tags"]);
            var networkID = -1;
            var authorLink = "";

            if (innerObj.ContainsKey("ownerUrl") && innerObj["ownerUrl"] != null)
            {
                authorLink = TrimUrl((string)innerObj["ownerUrl"]);
            }

            if (!string.IsNullOrEmpty(authorLink))
            {
                networkID = GetUserNetworkID(authorLink);
            }

            var html = new StringDownloader().DownloadString(url);
            dom = CQ.Create(html, Encoding.UTF8);

            var body = WebUtility.HtmlDecode(questionStatusDiv.Replace(dom[".post-text"].Html(), "").Trim());
            var score = int.Parse(dom[".vote-count-post"].Html());
            var isClosed = IsQuestionClosed(dom, url);
            var authorRep = ParseRep(dom[".post-signature.owner .reputation-score"].Html());
            var creationDate = DateTime.MaxValue;
            foreach (var timestamp in dom[".post-signature .user-info .user-action-time .relativetime"])
            {
                var dt = DateTime.Parse(timestamp.Attributes["title"]);

                if (dt < creationDate)
                {
                    creationDate = dt;
                }
            }

            return new Question(id, url, host, title, body, score, isClosed, creationDate, authorName, authorLink, networkID, authorRep, tags);
        }

        public static Question GetQuestion(CQ dom, string postUrl)
        {
            string host;
            uint id;

            ParsePostLink(postUrl, out host, out id);

            var tags = new List<string>();
            foreach (var tag in dom[".post-taglist a"])
            {
                var t = tag.Attributes["href"];

                t = t.Remove(0, t.LastIndexOf('/') + 1);

                tags.Add(t);
            }

            var title = WebUtility.HtmlDecode(dom[".question-hyperlink"].Html());
            var body = WebUtility.HtmlDecode(questionStatusDiv.Replace(dom[".post-text"].Html(), "").Trim());
            var score = int.Parse(dom[".vote-count-post"].Html());
            var isClosed = IsQuestionClosed(dom, postUrl);
            var creationDate = DateTime.MaxValue;
            foreach (var timestamp in dom[".post-signature .user-info .user-action-time .relativetime"])
            {
                var dt = DateTime.Parse(timestamp.Attributes["title"]);

                if (dt < creationDate)
                {
                    creationDate = dt;
                }
            }

            string authorName;
            string authorLink;
            var networkID = -1;
            int authorRep;

            if (dom[".post-signature.owner .reputation-score"][0] != null)
            {
                // Normal answer.
                authorName = WebUtility.HtmlDecode(StripTags(dom[".post-signature.owner .user-details a"][0].InnerHTML));
                authorLink = TrimUrl("http://" + host + dom[".post-signature.owner .user-details a"][0].Attributes["href"]);
                authorRep = ParseRep(dom[".post-signature.owner .reputation-score"][0].InnerHTML);
            }
            else
            {
                if (dom[".post-signature.owner .user-details a"].Any(e => e.Attributes["href"] != null && e.Attributes["href"].Contains("/users/")))
                {
                    // Community wiki.
                    authorName = WebUtility.HtmlDecode(StripTags(dom[".post-signature.owner .user-details a"][1].InnerHTML));
                    authorLink = TrimUrl("http://" + host + dom[".post-signature.owner .user-details a"][1].Attributes["href"]);
                    authorRep = 1;
                }
                else
                {
                    // Dead account owner.
                    authorName = WebUtility.HtmlDecode(StripTags(dom[".post-signature.owner .user-details"][0].InnerHTML));
                    authorName = authorName.Remove(authorName.Length - 4);
                    authorLink = null;
                    authorRep = 1;
                }
            }

            if (!string.IsNullOrEmpty(authorLink))
            {
                networkID = GetUserNetworkID(authorLink);
            }

            return new Question(id, postUrl, host, title, body, score, isClosed, creationDate, authorName, authorLink, networkID, authorRep, tags.ToArray());
        }

        public static Answer GetAnswer(string postUrl)
        {
            string host;
            uint id;

            ParsePostLink(postUrl, out host, out id);

            var getUrl = "http://" + host + "/posts/ajax-load-realtime/" + id;
            var html = new StringDownloader().DownloadString(getUrl);
            var dom = CQ.Create(html, Encoding.UTF8);

            return GetAnswer(dom, host, id.ToString(CultureInfo.InvariantCulture));
        }

        public static Answer GetLatestAnswer(CQ questionDom, string questionUrl)
        {
            if (questionDom == null) return null;

            var host = "";
            var questionID = 0u;

            ParsePostLink(questionUrl, out host, out questionID);

            foreach (var a in questionDom[".answer"])
            {
                var id = a.Attributes["data-answerid"];
                return GetAnswer(questionDom, host, id);
            }

            return null;
        }

        public static bool IsPostDeleted(string url, out CQ dom)
        {
            dom = null;
            try
            {
                if (isQuestionUrl.IsMatch(url))
                {
                    new WebClient().DownloadData(url);
                }
                else
                {
                    var id = 0u;
                    var host = "";
                    ParsePostLink(url, out host, out id);
                    dom = CQ.CreateFromUrl("http://" + host + "/posts/ajax-load-realtime/" + id);
                }
            }
            catch (WebException ex)
            {
                if (ex.Response != null && ((HttpWebResponse)ex.Response).StatusCode == HttpStatusCode.NotFound)
                {
                    if (!isQuestionUrl.IsMatch(url)) return true;

                    using (var str = ex.Response.GetResponseStream())
                    using (var sr = new StreamReader(str))
                    {
                        var html = sr.ReadToEnd();

                        dom = CQ.Create(html);

                        if ((dom[".leftcol"]?.Html() ?? "").Contains("reasons of moderation"))
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        public static bool IsQuestionClosed(CQ dom, string url, bool includeDupe = true)
        {
            if (!isQuestionUrl.IsMatch(url))
            {
                return false;
            }
            else
            {
                try
                {
                    var qStatus = dom[".question-status"]?.Html();

                    if (qStatus != null &&
                        ((qStatus.Contains("on hold") || qStatus.Contains("closed")) ||
                        (includeDupe && qStatus.Contains("duplicate"))))
                    {
                        return true;
                    }
                }
                catch { }
            }

            return false;
        }

        public static bool IsAnswerAccepted(CQ dom)
        {
            try
            {
                var qStatus = dom[".vote-accepted-on"];

                if (qStatus != null && !string.IsNullOrWhiteSpace(qStatus.Html()))
                {
                    return true;
                }
            }
            catch { }

            return false;
        }

        public static void ParsePostLink(string postUrl, out string host, out uint id)
        {
            host = hostParser.Replace(postUrl, "");

            id = uint.Parse(postIDParser.Match(postUrl).Groups[3].Value);
        }



        private static int ParseRep(string rep)
        {
            if (string.IsNullOrEmpty(rep))  {  return 1; }

            var trimmed = rep.Trim();
            trimmed = trimmed.EndsWith("mil") ? trimmed.Substring(0, trimmed.Length - 3) : trimmed;

            if (trimmed.ToLowerInvariant().Contains("k"))
            {
                if (trimmed.Contains("."))
                {
                    var charsAfterPeriod = trimmed.Substring(0, trimmed.IndexOf(".", StringComparison.Ordinal) + 1).Length;
                    var e = float.Parse(trimmed.Replace("k", ""));
                    var p = Math.Pow(10, charsAfterPeriod);

                    return (int)Math.Round(e * p);
                }

                return (int)float.Parse(trimmed.ToLowerInvariant().Replace("k", "000"));
            }

            return (int)float.Parse(trimmed);
        }

        private static int GetUserNetworkID(string authorProfileLink)
        {
            try
            {
                var req = (HttpWebRequest)WebRequest.Create(authorProfileLink);
                req.AddRange(0, 4096);
                var res = (HttpWebResponse)req.GetResponse();
                using (var strm = res.GetResponseStream())
                {
                    var bytes = new byte[4096];
                    strm.Read(bytes, 0, 4096);
                    var html = Encoding.UTF8.GetString(bytes);
                    var id = new string(userNetworkID.Match(html).Value.Where(char.IsDigit).ToArray());
                    return int.Parse(id);
                }
            }
            catch (Exception)
            {
                return -2;
            }
        }

        private static Answer GetAnswer(CQ dom, string host, string id)
        {
            var aDom = "#answer-" + id + " ";

            var postId = uint.Parse(id);
            var url = "http://" + host + "/a/" + id;
            var body = WebUtility.HtmlDecode(dom[aDom + ".post-text"][0].InnerHTML.Trim());
            var score = int.Parse(dom[aDom + ".vote-count-post"][0].InnerHTML);
            var isAccepted = IsAnswerAccepted(dom[aDom]);
            var creationDate = DateTime.Parse(dom[".post-signature .user-info .user-action-time .relativetime"].Last()[0].Attributes["title"]);
            var authorName = "";
            var authorLink = "";
            var networkID = -1;
            var authorRep = 0;

            var authorE = dom[aDom + ".post-signature .user-details"].Last()[0];

            if (authorE.InnerHTML.Contains("<a href=\"/users/"))
            {
                authorName = WebUtility.HtmlDecode(StripTags(dom[aDom + ".post-signature .user-details a"].Last()[0].InnerHTML).Trim());
                authorLink = TrimUrl("http://" + host + dom[aDom + ".post-signature .user-details a"].Last()[0].Attributes["href"].Trim());

                if (authorE.InnerHTML.Contains("class=\"reputation-score\""))
                {
                    authorRep = ParseRep(dom[aDom + ".post-signature .reputation-score"].Last()[0].InnerHTML.Trim());
                }
            }
            else
            {
                if (Regex.IsMatch(authorE.InnerHTML, "(?s)^\\s*<a.*?/revisions\".*?>.*</a>\\s*$", RegexOptions.CultureInvariant))
                {
                    authorName = WebUtility.HtmlDecode(StripTags(dom[aDom + ".post-signature .user-details a"].Last()[0].InnerHTML).Trim());
                }
                else
                {
                    authorName = WebUtility.HtmlDecode(StripTags(authorE.InnerHTML).Trim());
                }
            }

            if (!string.IsNullOrEmpty(authorLink))
            {
                networkID = GetUserNetworkID(authorLink);
            }

            var excerpt = Regex.Replace(StripTags(body).Replace("\n", " "), @"\s+", " ");
            if (excerpt.Length > 80)
            {
                var lstWrdI = excerpt.IndexOf(' ', 75);
                excerpt = excerpt.Remove(lstWrdI) + "...";
            }

            return new Answer(postId, url, excerpt, body, host, score, isAccepted, creationDate, authorName, authorLink, networkID, authorRep);
        }

        private static string TrimUrl(string url)
        {
            if (string.IsNullOrEmpty(url)) { return null; }

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

            return trimmed.Trim();
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
