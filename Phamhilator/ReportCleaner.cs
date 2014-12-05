using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;



namespace Phamhilator
{
	public static class ReportCleaner
	{
		public static string GetCleanReport(int messageID)
		{
			var oldTitle = GlobalInfo.PostedReports[messageID].Post.Title;
			var newTitle = CensorString(GlobalInfo.PostedReports[messageID].Post.Title);

			var oldName = GlobalInfo.PostedReports[messageID].Post.AuthorName;
			var newName = CensorString(GlobalInfo.PostedReports[messageID].Post.AuthorName);

			return GlobalInfo.PostedReports[messageID].Body.Replace(oldTitle, newTitle).Replace(oldName, newName);
		}

	    public static string GetSemiCleanReport(int messageID, HashSet<Term> blackTermsFound)
        {
            var oldTitle = GlobalInfo.PostedReports[messageID].Post.Title;
            var newTitle = SemiCensorString(oldTitle, blackTermsFound);

            var oldName = GlobalInfo.PostedReports[messageID].Post.AuthorName;
            var newName = SemiCensorString(oldName, blackTermsFound);

            return GlobalInfo.PostedReports[messageID].Body.Replace(oldTitle, newTitle).Replace(oldName, newName);
        }



	    private static string SemiCensorString(string input, IEnumerable<Term> blackTerms)
	    {
            var censored = input.ToArray();

            foreach (var term in blackTerms)
            {
                var titleMatches = term.Regex.Matches(new string(censored));

                foreach (var match in titleMatches.Cast<Match>())
                {
                    for (var i = match.Index; i < match.Index + match.Length; i++)
                    {
                        censored[i] = '*';
                    }
                }
            }

	        return new string(censored);
	    }

		private static string CensorString(string input)
		{
			var censored = new StringBuilder();

			foreach (var c in input)
			{
				censored.Append(c == ' ' ? ' ' : '*');
			}

			return censored.ToString();
		}
	}
}
