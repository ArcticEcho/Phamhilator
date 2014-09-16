using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;




namespace Phamhilator
{
	public static class FilterTerms
	{
		private static Dictionary<Regex, int> offensiveTerms;
		private static Dictionary<Regex, int> lqTerms;
		private static Dictionary<Regex, int> spamTerms;
		private static Dictionary<Regex, int> badUsernameTerms;

		public static int TermCount
		{
			get
			{
				return offensiveTerms.Count + lqTerms.Count + spamTerms.Count + badUsernameTerms.Count;
			}
		}

		public static Dictionary<Regex, int> OffensiveTerms
		{
			get
			{
				if (offensiveTerms == null)
				{
					PopulateOffensiveTerms();
				}

				return offensiveTerms;
			}
		}

		public static Dictionary<Regex, int> LQTerms
		{
			get
			{
				if (lqTerms == null)
				{
					PopulateLQTerms();
				}

				return lqTerms;
			}
		}

		public static Dictionary<Regex, int> SpamTerms
		{
			get
			{
				if (spamTerms == null)
				{
					PopulateSpamTerms();
				}

				return spamTerms;
			}
		}

		public static Dictionary<Regex, int> BadUsernameTerms
		{
			get
			{
				if (badUsernameTerms == null)
				{
					PopulateBadUsernameTerms();
				}

				return badUsernameTerms;
			}
		}



		public static void AddTerm(PostType type, Regex term)
		{
			switch (type)
			{
				case PostType.Offensive:
				{
					if (offensiveTerms.ContainsTerm(term)) { return; }

					offensiveTerms.Add(term, 0);

					File.AppendAllText(DirectoryTools.GetOffensiveTermsFile(), "\n0]" + term);

					break;
				}
				case PostType.LowQuality:
				{
					if (lqTerms.ContainsTerm(term)) { return; }

					lqTerms.Add(term, 0); // TODO: set to half score of highest scoring term

					File.AppendAllText(DirectoryTools.GetLQTermsFile(), "\n0]" + term);

					break;
				}
				case PostType.Spam:
				{
					if (spamTerms.ContainsTerm(term)) { return; }

					spamTerms.Add(term, 0);

					File.AppendAllText(DirectoryTools.GetSpamTermsFile(), "\n0]" + term);

					break;
				}
				case PostType.BadUsername:
				{
					if (badUsernameTerms.ContainsTerm(term)) { return; }

					badUsernameTerms.Add(term, 0);

					File.AppendAllText(DirectoryTools.GetBadUsernameTermsFile(), "\n0]" + term);

					break;
				}
			}
		}

		public static void RemoveTerm(PostType type, Regex term)
		{
			switch (type)
			{
				case PostType.Offensive:
				{
					RemoveOffensiveTerm(term);

					break;
				}
				case PostType.LowQuality:
				{
					RemoveLQTerm(term);

					break;
				}
				case PostType.Spam:
				{
					RemoveSpamTerm(term);

					break;
				}
				case PostType.BadUsername:
				{
					RemoveBadUsernameTerm(term);

					break;
				}
			}
		}



		private static void RemoveOffensiveTerm(Regex term)
		{
			if (!offensiveTerms.ContainsTerm(term)) { return; }

			offensiveTerms.Remove(term);

			var data = File.ReadAllLines(DirectoryTools.GetOffensiveTermsFile()).ToList();

			for (var i = 0; i < data.Count; i++)
			{
				if (data[i].Remove(0, data[i].IndexOf("]") + 1) == term.ToString())
				{
					data.RemoveAt(i);

					break;
				}
			}

			File.WriteAllLines(DirectoryTools.GetOffensiveTermsFile(), data);
		}

		private static void RemoveSpamTerm(Regex term)
		{
			if (!spamTerms.ContainsTerm(term)) { return; }

			spamTerms.Remove(term);

			var data = File.ReadAllLines(DirectoryTools.GetSpamTermsFile()).ToList();

			for (var i = 0; i < data.Count; i++)
			{
				if (data[i].Remove(0, data[i].IndexOf("]") + 1) == term.ToString())
				{
					data.RemoveAt(i);

					break;
				}
			}

			File.WriteAllLines(DirectoryTools.GetSpamTermsFile(), data);
		}

		private static void RemoveLQTerm(Regex term)
		{
			if (!lqTerms.ContainsTerm(term)) { return; }

			lqTerms.Remove(term);

			var data = File.ReadAllLines(DirectoryTools.GetLQTermsFile()).ToList();

			for (var i = 0; i < data.Count; i++)
			{
				if (data[i].Remove(0, data[i].IndexOf("]") + 1) == term.ToString())
				{
					data.RemoveAt(i);

					break;
				}
			} 

			File.WriteAllLines(DirectoryTools.GetLQTermsFile(), data);
		}

		private static void RemoveBadUsernameTerm(Regex term)
		{
			if (!badUsernameTerms.ContainsTerm(term)) { return; }

			badUsernameTerms.Remove(term);

			var data = File.ReadAllLines(DirectoryTools.GetBadUsernameTermsFile()).ToList();

			for (var i = 0; i < data.Count; i++)
			{
				if (data[i].Remove(0, data[i].IndexOf("]") + 1) == term.ToString())
				{
					data.RemoveAt(i);

					break;
				}
			}

			File.WriteAllLines(DirectoryTools.GetBadUsernameTermsFile(), data);
		}


		private static void PopulateOffensiveTerms()
		{
			var data = File.ReadAllLines(DirectoryTools.GetOffensiveTermsFile());
			offensiveTerms = new Dictionary<Regex, int>();

			foreach (var termAndScore in data)
			{
				if (termAndScore.IndexOf("]") == -1) { continue; }

				var termScore = termAndScore.Substring(0, termAndScore.IndexOf("]"));
				var termString = termAndScore.Substring(termAndScore.IndexOf("]") + 1);
				var term = new Regex(termString);

				if (offensiveTerms.ContainsTerm(term)) { continue; }

				offensiveTerms.Add(term, int.Parse(termScore));
				Stats.TermCount++;
			}
		}

		private static void PopulateLQTerms()
		{
			var data = File.ReadAllLines(DirectoryTools.GetLQTermsFile());
			lqTerms = new Dictionary<Regex, int>();

			foreach (var termAndScore in data)
			{
				if (termAndScore.IndexOf("]") == -1) { continue; }

				var termScore = termAndScore.Substring(0, termAndScore.IndexOf("]"));
				var termString = termAndScore.Substring(termAndScore.IndexOf("]") + 1);
				var term = new Regex(termString);

				if (lqTerms.ContainsTerm(term)) { continue; }

				lqTerms.Add(term, int.Parse(termScore));
				Stats.TermCount++;
			}
		}

		private static void PopulateSpamTerms()
		{
			var data = File.ReadAllLines(DirectoryTools.GetSpamTermsFile());
			spamTerms = new Dictionary<Regex, int>();

			foreach (var termAndScore in data)
			{
				if (termAndScore.IndexOf("]") == -1) { continue; }

				var termScore = termAndScore.Substring(0, termAndScore.IndexOf("]"));
				var termString = termAndScore.Substring(termAndScore.IndexOf("]") + 1);
				var term = new Regex(termString);

				if (spamTerms.ContainsTerm(term)) { continue; }

				spamTerms.Add(term, int.Parse(termScore));
				Stats.TermCount++;
			}
		}

		private static void PopulateBadUsernameTerms()
		{
			var data = File.ReadAllLines(DirectoryTools.GetBadUsernameTermsFile());
			badUsernameTerms = new Dictionary<Regex, int>();

			foreach (var termAndScore in data)
			{
				if (termAndScore.IndexOf("]") == -1) { continue; }

				var termScore = termAndScore.Substring(0, termAndScore.IndexOf("]"));
				var termString = termAndScore.Substring(termAndScore.IndexOf("]") + 1);
				var term = new Regex(termString);

				if (badUsernameTerms.ContainsTerm(term)) { continue; }

				badUsernameTerms.Add(term, int.Parse(termScore));
				Stats.TermCount++;
			}
		}
	}
}
