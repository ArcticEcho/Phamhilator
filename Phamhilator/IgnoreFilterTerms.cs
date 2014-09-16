using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System;




namespace Phamhilator
{
	public static class IgnoreFilterTerms
	{
		private static Dictionary<Regex, string> offensiveTerms;
		private static Dictionary<Regex, string> lqTerms;
		private static Dictionary<Regex, string> spamTerms;
		private static Dictionary<Regex, string> badUsernameTerms;

		public static int TermCount
		{
			get
			{
				return offensiveTerms.Count + lqTerms.Count + spamTerms.Count + badUsernameTerms.Count;
			}
		}

		public static Dictionary<Regex, string> OffensiveTerms
		{
			get
			{
				if (offensiveTerms == null)
				{
					PopulateIgnoreOffensiveTerms();
				}

				return offensiveTerms;
			}
		}

		public static Dictionary<Regex, string> LQTerms
		{
			get
			{
				if (lqTerms == null)
				{
					PopulateIgnoreLQTerms();
				}

				return lqTerms;
			}
		}

		public static Dictionary<Regex, string> SpamTerms
		{
			get
			{
				if (spamTerms == null)
				{
					PopulateIgnoreSpamTerms();
				}

				return spamTerms;
			}
		}

		public static Dictionary<Regex, string> BadUsernameTerms
		{
			get
			{
				if (badUsernameTerms == null)
				{
					PopulateIgnoreBadUsernameTerms();
				}

				return badUsernameTerms;
			}
		}



		public static void AddTerm(PostType type, Regex term, string site)
		{
			switch (type)
			{
				case PostType.Offensive:
				{
					if (offensiveTerms.ContainsTerm(term)) { return; }

					offensiveTerms.Add(term, site);

					File.AppendAllText(DirectoryTools.GetIgnoreOffensiveTermsFile(), "\n" + site + "]" + term);

					break;
				}
				case PostType.LowQuality:
				{
					if (lqTerms.ContainsTerm(term)) { return; }

					lqTerms.Add(term, site);

					File.AppendAllText(DirectoryTools.GetIgnoreLQTermsFile(), "\n" + site + "]" + term);

					break;
				}
				case PostType.Spam:
				{
					if (spamTerms.ContainsTerm(term)) { return; }

					spamTerms.Add(term, site);

					File.AppendAllText(DirectoryTools.GetIgnoreSpamTermsFile(), "\n" + site + "]" + term);

					break;
				}
				case PostType.BadUsername:
				{
					if (badUsernameTerms.ContainsTerm(term)) { return; }

					badUsernameTerms.Add(term, site);

					File.AppendAllText(DirectoryTools.GetIgnoreBadUsernameTermsFile(), "\n" + site + "]" + term);

					break;
				}
			}
		}

		public static void RemoveTerm(PostType type, Regex term, string site)
		{
			switch (type)
			{
				case PostType.Offensive:
				{
					RemoveIgnoreOffensiveTerm(term, site);

					break;
				}
				case PostType.LowQuality:
				{
					RemoveIgnoreLQTerm(term, site);

					break;
				}
				case PostType.Spam:
				{
					RemoveIgnoreSpamTerm(term, site);

					break;
				}
				case PostType.BadUsername:
				{
					RemoveIgnoreBadUsernameTerm(term, site);

					break;
				}
			}
		}



		private static void RemoveIgnoreOffensiveTerm(Regex term, string site)
		{
			if (!offensiveTerms.ContainsTerm(term)) { return; }

			offensiveTerms.Remove(term);

			var data = File.ReadAllLines(DirectoryTools.GetIgnoreOffensiveTermsFile()).ToList();

			for (var i = 0; i < data.Count; i++)
			{
				if (data[i].Remove(0, data[i].IndexOf("]", StringComparison.Ordinal) + 1) == term.ToString() && data[i].Substring(0, data[i].IndexOf("]", StringComparison.Ordinal)) == site)
				{
					data.RemoveAt(i);

					break;
				}
			}

			File.WriteAllLines(DirectoryTools.GetIgnoreOffensiveTermsFile(), data);
		}

		private static void RemoveIgnoreSpamTerm(Regex term, string site)
		{
			if (!spamTerms.ContainsTerm(term)) { return; }

			spamTerms.Remove(term);

			var data = File.ReadAllLines(DirectoryTools.GetIgnoreSpamTermsFile()).ToList();

			for (var i = 0; i < data.Count; i++)
			{
				if (data[i].Remove(0, data[i].IndexOf("]", StringComparison.Ordinal) + 1) == term.ToString() && data[i].Substring(0, data[i].IndexOf("]", StringComparison.Ordinal)) == site)
				{
					data.RemoveAt(i);

					break;
				}
			}

			File.WriteAllLines(DirectoryTools.GetIgnoreSpamTermsFile(), data);
		}

		private static void RemoveIgnoreLQTerm(Regex term, string site)
		{
			if (!lqTerms.ContainsTerm(term)) { return; }

			lqTerms.Remove(term);

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

		private static void RemoveIgnoreBadUsernameTerm(Regex term, string site)
		{
			if (!badUsernameTerms.ContainsTerm(term)) { return; }

			badUsernameTerms.Remove(term);

			var data = File.ReadAllLines(DirectoryTools.GetIgnoreBadUsernameTermsFile()).ToList();

			for (var i = 0; i < data.Count; i++)
			{
				if (data[i].Remove(0, data[i].IndexOf("]", StringComparison.Ordinal) + 1) == term.ToString() && data[i].Substring(0, data[i].IndexOf("]", StringComparison.Ordinal)) == site)
				{
					data.RemoveAt(i);

					break;
				}
			}

			File.WriteAllLines(DirectoryTools.GetIgnoreBadUsernameTermsFile(), data);
		}


		private static void PopulateIgnoreOffensiveTerms()
		{
			var data = File.ReadAllLines(DirectoryTools.GetIgnoreOffensiveTermsFile());
			offensiveTerms = new Dictionary<Regex, string>();

			foreach (var termAndScore in data)
			{
				if (termAndScore.IndexOf("]", StringComparison.Ordinal) == -1) { continue; }

				var termSite = termAndScore.Substring(0, termAndScore.IndexOf("]", StringComparison.Ordinal));
				var termString = termAndScore.Substring(termAndScore.IndexOf("]", StringComparison.Ordinal) + 1);
				var term = new Regex(termString);

				if (offensiveTerms.ContainsTerm(term)) { continue; }

				offensiveTerms.Add(term, termSite);
				Stats.TermCount++;
			}
		}

		private static void PopulateIgnoreLQTerms()
		{
			var data = File.ReadAllLines(DirectoryTools.GetIgnoreLQTermsFile());
			lqTerms = new Dictionary<Regex, string>();

			foreach (var termAndScore in data)
			{
				if (termAndScore.IndexOf("]", StringComparison.Ordinal) == -1) { continue; }

				var termSite = termAndScore.Substring(0, termAndScore.IndexOf("]", StringComparison.Ordinal));
				var termString = termAndScore.Substring(termAndScore.IndexOf("]", StringComparison.Ordinal) + 1);
				var term = new Regex(termString);

				if (lqTerms.ContainsTerm(term)) { continue; }

				lqTerms.Add(term, termSite);
				Stats.TermCount++;
			}
		}

		private static void PopulateIgnoreSpamTerms()
		{
			var data = File.ReadAllLines(DirectoryTools.GetIgnoreSpamTermsFile());
			spamTerms = new Dictionary<Regex, string>();

			foreach (var termAndScore in data)
			{
				if (termAndScore.IndexOf("]", StringComparison.Ordinal) == -1) { continue; }
				
				var termSite = termAndScore.Substring(0, termAndScore.IndexOf("]", StringComparison.Ordinal));
				var termString = termAndScore.Substring(termAndScore.IndexOf("]", StringComparison.Ordinal) + 1);
				var term = new Regex(termString);

				if (spamTerms.ContainsTerm(term)) { continue; }

				spamTerms.Add(term, termSite);
				Stats.TermCount++;
			}
		}

		private static void PopulateIgnoreBadUsernameTerms()
		{
			var data = File.ReadAllLines(DirectoryTools.GetIgnoreBadUsernameTermsFile());
			badUsernameTerms = new Dictionary<Regex, string>();

			foreach (var termAndScore in data)
			{
				if (termAndScore.IndexOf("]", StringComparison.Ordinal) == -1) { continue; }

				var termSite = termAndScore.Substring(0, termAndScore.IndexOf("]", StringComparison.Ordinal));
				var termString = termAndScore.Substring(termAndScore.IndexOf("]", StringComparison.Ordinal) + 1);
				var term = new Regex(termString);

				if (badUsernameTerms.ContainsTerm(term)) { continue; }

				badUsernameTerms.Add(term, termSite);
				Stats.TermCount++;
			}
		}
	}
}
