using System.Linq;



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
    }
}
