using System.Linq;
using System.Net;
using CsQuery;



namespace Phamhilator
{
    public static class Pham
    {
        public static void RegisterFP(Post post, PostAnalysis report)
        {
            GlobalInfo.Stats.TotalFPCount++;

            var newWhiteTermScore = report.BlackTermsFound.Select(t => t.Score).Max() / 2;

            foreach (var filter in report.FiltersUsed)
            {
                if ((int)filter > 99) // White filter
                {
                    for (var i = 0; i < report.WhiteTermsFound.Count; i++)
                    {
                        var term = report.WhiteTermsFound.ElementAt(i);

                        if (term.Site == post.Site)
                        {
                            GlobalInfo.WhiteFilters[filter].SetScore(term, term.Score + 1);
                        }
                    }
                }
                else // Black filter
                {
                    foreach (var term in report.BlackTermsFound)
                    {
                        term.FPCount++;

                        var corFilter = filter.GetCorrespondingWhiteFilter();

                        if (GlobalInfo.WhiteFilters[corFilter].Terms.All(tt => tt.Site != term.Site && tt.Regex.ToString() != term.Regex.ToString()))
                        {
                            GlobalInfo.WhiteFilters[corFilter].AddTerm(new Term(corFilter, term.Regex, newWhiteTermScore, post.Site));
                        }
                    }
                }
            }
        }

        public static void RegisterTP(Post post, PostAnalysis report)
        {
            GlobalInfo.Stats.TotalTPCount++;

            foreach (var filter in report.FiltersUsed.Where(filter => (int)filter < 100)) // Make sure we only get black filters.
            foreach (var blackTerm in report.BlackTermsFound.Where(blackTerm => GlobalInfo.BlackFilters[filter].Terms.Contains(blackTerm)))
            {
                GlobalInfo.BlackFilters[filter].SetScore(blackTerm, blackTerm.Score + 1);

                blackTerm.TPCount++;

                for (var i = 0; i < GlobalInfo.WhiteFilters[filter.GetCorrespondingWhiteFilter()].Terms.Count; i++) 
                {
                    var whiteTerm = GlobalInfo.WhiteFilters[filter.GetCorrespondingWhiteFilter()].Terms.ElementAt(i);

                    if (whiteTerm.Regex.ToString() != blackTerm.Regex.ToString() || whiteTerm.Site == post.Site) { continue; }

                    var x = whiteTerm.Score / blackTerm.Score;

                    GlobalInfo.WhiteFilters[filter.GetCorrespondingWhiteFilter()].SetScore(whiteTerm, x * (blackTerm.Score + 1));
                }
            }
        }

        public static bool IsAnswerTP(string postUrl)
        {
            // "http://" + host + "/posts/ajax-load-realtime/" + id

            var html = "";

            try
            {
                var host = PostRetriever.HostParser.Replace(postUrl, "");
                var id = PostRetriever.PostIDParser.Replace(postUrl, "");

                html = new WebClient().DownloadString("http://" + host + "/posts/ajax-load-realtime/" + id);
            }
            catch (WebException ex)
            {
                if (((HttpWebResponse)ex.Response).StatusCode == HttpStatusCode.NotFound)
                {
                    // The post has been deleted.
                    return true;
                }
            }

            var dom = CQ.Create(html);

            var score = dom[".vote-count-post"].Html();

            return int.Parse(score) <= -5;
        }

        public static bool IsQuestionTP(string postUrl)
        {
            var html = "";

            try
            {
                html = new WebClient().DownloadString(postUrl);
            }
            catch (WebException ex)
            {
                if (((HttpWebResponse)ex.Response).StatusCode == HttpStatusCode.NotFound)
                {
                    // The post has been deleted.
                    return true;

                    // Maybe (later) add a "TP factor" (the effects of the TP will be multiplied by this factor). 
                    // If post was deleted due to moderation, TPF = 3. If post was deleted by owner, TPF = 2. If post is closed + score < -1, TPF = 1/

                    //using (var stream = ex.Response.GetResponseStream())
                    //using (var reader = new StreamReader(stream, Encoding.UTF8))
                    //{
                    //    html = reader.ReadToEnd();
                    //}
                }
            }

            var dom = CQ.Create(html);

            //var deleteionReason = dom[".revision-comment"].Html();

            // Check if the question is deleted (and if so, by who).
            //if (string.IsNullOrEmpty(deleteionReason) || !deleteionReason.Contains("moderation"))
            //{
            //    
            //}

            var isClosed = false;
            var score = dom[".vote-count-post"].Html();

            foreach (var e in dom[".question-status b"])
            {
                if (e.InnerHTML == "closed" || e.InnerHTML == "put on hold")
                {
                    isClosed = true;
                }
            }

            return isClosed || int.Parse(score) <= -3;
        }
    }
}
