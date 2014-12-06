using System.Collections.Generic;
using System.Text.RegularExpressions;
using CsQuery;



namespace Phamhilator
{
    public static class PostRetriever
    {
        private static readonly Regex hostParser = new Regex(@".*//|/.*", RegexOptions.Compiled | RegexOptions.CultureInvariant);
        private static readonly Regex postIDParser1 = new Regex(@"\D*/|\D.*", RegexOptions.Compiled | RegexOptions.CultureInvariant);
        private static readonly Regex postIDParser2 = new Regex(@".*(q|a)/|/\d*", RegexOptions.Compiled | RegexOptions.CultureInvariant);
        private static readonly Regex isShareLink = new Regex(@"(q|a)/\d*/\d*$", RegexOptions.Compiled | RegexOptions.CultureInvariant);



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

            var title = dom[".question-hyperlink"].Html();
            var body = dom[".post-text"].Html().Trim();
            var score = int.Parse(dom[".vote-count-post"].Html());
            var authorName = dom[".user-details a"].Html();
            var authorLink = "http://" + host + dom[".user-details a"].Attr("href");
            var authorRep = int.Parse(dom[".reputation-score"].Html());

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
            var body = dom[".post-text"].Html().Trim();
            var authorName = dom[".user-details a"].Html();
            var authorLink = "http://" + host + dom[".user-details a"].Attr("href");
            var authorRep = int.Parse(dom[".reputation-score"].Html());
            var excerpt = StripTags(body).Trim();

            excerpt = excerpt.Length > 50 ? excerpt.Substring(0, 47) + "..." : excerpt;

            return new Answer(postUrl, excerpt, body, host, score, authorName, authorLink, authorRep);
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
