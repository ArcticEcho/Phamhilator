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
using Microsoft.CSharp.RuntimeBinder;
using Newtonsoft.Json.Linq;

namespace Phamhilator.Yam.Core
{
    public static class PostFetcher
    {
        private const RegexOptions regOpts = RegexOptions.CultureInvariant | RegexOptions.Compiled;
        private static readonly Regex shareLinkIDParser = new Regex(@".*(q|a)/|/\d*", regOpts);
        private static readonly Regex isShareLink = new Regex(@"(q|a)/\d*/\d*$", regOpts);
        private static readonly Regex escapeChars = new Regex(@"[_*`\[\]]", regOpts);
        public static readonly Regex userNetworkID = new Regex(@"accountId: \d+", regOpts);

        public static readonly Regex HostParser = new Regex(@".*//|/.*", regOpts);
        public static readonly Regex PostIDParser = new Regex(@"\D*/|\D.*", regOpts);



        public static Question GetQuestion(MessageEventArgs message)
        {
            var t = ((dynamic)JObject.Parse(message.Data)).data;
            var data = (dynamic)JObject.Parse(t.ToString());
            var url = TrimUrl((string)data.url);
            var host = (string)data.siteBaseHostAddress;
            var title = WebUtility.HtmlDecode((string)data.titleEncodedFancy);
            var authorName = WebUtility.HtmlDecode((string)data.ownerDisplayName);
            var tags = new List<string>();
            var networkID = -1;
            var authorLink = "";

            try
            {
                authorLink = TrimUrl((string)data.ownerUrl);
            }
            catch (RuntimeBinderException) { }

            if (!String.IsNullOrEmpty(authorLink))
            {
                networkID = GetUserNetworkID(authorLink);
            }

            foreach (var tag in data.tags)
            {
                tags.Add((string)tag);
            }

            var html = new StringDownloader().DownloadString(url);
            var dom = CQ.Create(html);

            var body = WebUtility.HtmlDecode(dom[".post-text"].Html().Trim());
            var score = int.Parse(dom[".vote-count-post"].Html());
            var authorRep = PostFetcher.ParseRep(dom[".reputation-score"].Html());

            return new Question(url, host, title, body, score, authorName, authorLink, networkID, authorRep, tags, html);
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
            var body = UnshortifyLinks(WebUtility.HtmlDecode(dom[".post-text"].Html().Trim()));
            var score = int.Parse(dom[".vote-count-post"].Html());

            string authorName;
            string authorLink;
            var networkID = -1;
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

            if (!String.IsNullOrEmpty(authorLink))
            {
                networkID = GetUserNetworkID(authorLink);
            }

            return new Question(postUrl, host, title, body, score, authorName, authorLink, networkID, authorRep, tags, html);
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

        public static Answer GetLatestAnswer(Question question)
        {
            if (string.IsNullOrEmpty(question.Html)) { return null; }

            var dom = CQ.Create(question.Html, Encoding.UTF8);
            var host = "";
            var questionID = 0;

            GetPostInfo(question.Url, out host, out questionID);

            foreach (var a in dom[".answer"])
            {
                var id = a.Attributes["data-answerid"];
                return GetAnswer(dom, host, id);
            }

            return null;
        }

        public static int ParseRep(string rep)
        {
            if (String.IsNullOrEmpty(rep))  {  return 1; }

            var trimmed = rep.Trim();

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

        public static string ChatEscapeString(string input, string newlineReplace = "")
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



        private static int GetUserNetworkID(string authorProfileLink)
        {
            try
            {
                var req = (HttpWebRequest)WebRequest.Create(authorProfileLink);
                req.AddRange(0, 2048);
                var res = (HttpWebResponse)req.GetResponse();
                using (var strm = res.GetResponseStream())
                {
                    var bytes = new byte[2048];
                    strm.Read(bytes, 0, 2048);
                    var html = Encoding.UTF8.GetString(bytes);
                    var id = new string(userNetworkID.Match(html).Value.Where(Char.IsDigit).ToArray());
                    return int.Parse(id);
                }
            }
            catch (Exception)
            {
                return -1;
            }
        }

        private static Answer GetAnswer(CQ dom, string host, string id)
        {
            var aDom = "#answer-" + id + " ";

            var score = int.Parse(dom[aDom + ".vote-count-post"][0].InnerHTML);
            var body = UnshortifyLinks(WebUtility.HtmlDecode(dom[aDom + ".post-text"][0].InnerHTML.Trim()));
            var url = "http://" + host + "/a/" + id;
            var authorName = "";
            var authorLink = "";
            var networkID = -1;
            var authorRep = 0;

            var authorE = dom[aDom + ".user-details"].Last()[0];

            if (authorE.InnerHTML.Contains("<a href=\"/users/"))
            {
                authorName = WebUtility.HtmlDecode(StripTags(dom[aDom + ".user-details a"].Last()[0].InnerHTML).Trim());
                authorLink = TrimUrl("http://" + host + dom[aDom + ".user-details a"].Last()[0].Attributes["href"].Trim());

                if (authorE.InnerHTML.Contains("class=\"reputation-score\""))
                {
                    authorRep = ParseRep(dom[aDom + ".reputation-score"].Last()[0].InnerHTML.Trim());
                }
            }
            else
            {
                if (Regex.IsMatch(authorE.InnerHTML, "(?s)^\\s*<a.*?/revisions\".*?>.*</a>\\s*$", RegexOptions.CultureInvariant))
                {
                    authorName = WebUtility.HtmlDecode(StripTags(dom[aDom + ".user-details a"].Last()[0].InnerHTML).Trim());
                }
                else
                {
                    authorName = WebUtility.HtmlDecode(StripTags(authorE.InnerHTML).Trim());
                }
            }

            if (!String.IsNullOrEmpty(authorLink))
            {
                networkID = GetUserNetworkID(authorLink);
            }

            var excerpt = StripTags(body);

            excerpt = excerpt.Length > 75 ? excerpt.Substring(0, 72) + "..." : excerpt;

            return new Answer(url, excerpt, body, host, score, authorName, authorLink, networkID, authorRep);
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

            return trimmed.Trim();
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

        private static string UnshortifyLinks(string body)
        {
            var dom = CQ.Create(body);
            var bodyCopy = body;
            var shortLinks = new List<string>();

            foreach (var link in dom["a"])
            {
                var url = link.Attributes["href"];

                if (String.IsNullOrEmpty(url)) { continue; }

                if (LinkUnshortifier.IsShortLink(url) && !shortLinks.Contains(url))
                {
                    shortLinks.Add(url);

                    var longUrl = LinkUnshortifier.UnshortifyLink(url);

                    bodyCopy = bodyCopy.Replace(url, longUrl);
                }
            }

            return bodyCopy;
        }
    }
}
