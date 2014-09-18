using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;



namespace Phamhilator
{
	namespace IgnoreFilters
	{
		public class LQ
		{
			public Dictionary<Regex, string> Terms { get; private set; }



			public LQ()
			{
				var data = File.ReadAllLines(DirectoryTools.GetIgnoreLQTermsFile());
				Terms = new Dictionary<Regex, string>();

				foreach (var termAndScore in data)
				{
					if (termAndScore.IndexOf("]", StringComparison.Ordinal) == -1) { continue; }

					var termSite = termAndScore.Substring(0, termAndScore.IndexOf("]", StringComparison.Ordinal));
					var termString = termAndScore.Substring(termAndScore.IndexOf("]", StringComparison.Ordinal) + 1);
					var term = new Regex(termString);

					if (Terms.ContainsTerm(term)) { continue; }

					Terms.Add(term, termSite);
				}
			}



			public void AddTerm(Regex term, string site)
			{
				if (Terms.ContainsTerm(term)) { return; }

				Terms.Add(term, site);

				File.AppendAllText(DirectoryTools.GetIgnoreLQTermsFile(), "\n" + site + "]" + term);
			}

			public void RemoveTerm(Regex term, string site)
			{
				if (!Terms.ContainsTerm(term)) { return; }

				Terms.Remove(term);

				var data = File.ReadAllLines(DirectoryTools.GetIgnoreLQTermsFile()).ToList();

				for (var i = 0; i < data.Count; i++)
				{
					if (data[i].Remove(0, data[i].IndexOf("]", StringComparison.Ordinal) + 1) == term.ToString() && data[i].Substring(0, data[i].IndexOf("]", StringComparison.Ordinal)) == site)
					{
						data.RemoveAt(i);

						break;
					}
				}

				File.WriteAllLines(DirectoryTools.GetIgnoreLQTermsFile(), data);
			}
		}
	}
}
