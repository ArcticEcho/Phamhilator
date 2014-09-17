using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;



namespace Phamhilator
{
	namespace Filters
	{
		internal class BadUsername
		{
			private static Dictionary<Regex, int> terms;

			public static Dictionary<Regex, int> Terms
			{
				get
				{
					if (terms == null)
					{
						PopulateTerms();
					}

					return terms;
				}
			}

			public static int AverageScore
			{
				get { return (int)Math.Round(terms.Values.Average(), 0); }
			}

			public static int HighestScore
			{
				get { return terms.Values.Max(); }
			}



			public static void AddTerm(PostType type, Regex term)
			{
				if (terms.ContainsTerm(term)) { return; }

				terms.Add(term, AverageScore);

				File.AppendAllText(DirectoryTools.GetBadUsernameTermsFile(), "\n" + AverageScore + "]" + term);
			}

			public static void RemoveTerm(Regex term)
			{
				if (!terms.ContainsTerm(term)) { return; }

				terms.Remove(term);

				var data = File.ReadAllLines(DirectoryTools.GetBadUsernameTermsFile()).ToList();

				for (var i = 0; i < data.Count; i++)
				{
					if (data[i].Remove(0, data[i].IndexOf("]", StringComparison.Ordinal) + 1) == term.ToString())
					{
						data.RemoveAt(i);

						break;
					}
				}

				File.WriteAllLines(DirectoryTools.GetBadUsernameTermsFile(), data);
			}

			public static void SetScore(Regex term, int newScore)
			{
				for (var i = 0; i < Terms.Count; i++)
				{
					var key = Terms.Keys.ToArray()[i];

					if (key.ToString() == term.ToString())
					{
						Terms[key] = newScore;
					}
				}
			}

			public static int GetScore(Regex term)
			{
				for (var i = 0; i < Terms.Count; i++)
				{
					var key = Terms.Keys.ToArray()[i];

					if (key.ToString() == term.ToString())
					{
						return Terms[key];
					}
				}

				return -1;
			}



			private static void PopulateTerms()
			{
				var data = File.ReadAllLines(DirectoryTools.GetBadUsernameTermsFile());
				terms = new Dictionary<Regex, int>();

				foreach (var termAndScore in data)
				{
					if (termAndScore.IndexOf("]", StringComparison.Ordinal) == -1) { continue; }

					var termScore = termAndScore.Substring(0, termAndScore.IndexOf("]", StringComparison.Ordinal));
					var termString = termAndScore.Substring(termAndScore.IndexOf("]", StringComparison.Ordinal) + 1);
					var term = new Regex(termString);

					if (terms.ContainsTerm(term)) { continue; }

					terms.Add(term, int.Parse(termScore));
				}
			}
		}
	}
}
