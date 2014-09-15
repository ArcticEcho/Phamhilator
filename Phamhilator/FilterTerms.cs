using System.Collections.Generic;
using System.IO;
using System.Linq;




namespace Phamhilator
{
	public static class FilterTerms
	{
		private static readonly Dictionary<string, int> offensiveTerms = new Dictionary<string, int>();
		private static readonly Dictionary<string, int> lqTerms = new Dictionary<string, int>();
		private static readonly Dictionary<string, int> spamTerms = new Dictionary<string, int>();
		private static readonly Dictionary<string, int> badUsernameTerms = new Dictionary<string, int>();



		public static Dictionary<string, int> OffensiveTerms
		{
			get
			{
				if (offensiveTerms.Count == 0)
				{
					PopulateOffensiveTerms();
				}

				return offensiveTerms;
			}
		}

		public static Dictionary<string, int> LQTerms
		{
			get
			{
				if (lqTerms.Count == 0)
				{
					PopulateLQTerms();
				}

				return lqTerms;
			}
		}

		public static Dictionary<string, int> SpamTerms
		{
			get
			{
				if (spamTerms.Count == 0)
				{
					PopulateSpamTerms();
				}

				return spamTerms;
			}
		}

		public static Dictionary<string, int> BadUsernameTerms
		{
			get
			{
				if (badUsernameTerms.Count == 0)
				{
					PopulateBadUsernameTerms();
				}

				return badUsernameTerms;
			}
		}



		public static void AddTerm(PostType type, string term)
		{
			switch (type)
			{
				case PostType.Offensive:
				{
					if (offensiveTerms.ContainsKey(term)) { return; }

					offensiveTerms.Add(term, 0);

					File.AppendAllText(DirectoryTools.GetOffensiveTermsFile(), "0]" + term);

					break;
				}
				case PostType.LowQuality:
				{
					if (lqTerms.ContainsKey(term)) { return; }

					lqTerms.Add(term, 0);

					File.AppendAllText(DirectoryTools.GetLQTermsFile(), "0]" + term);

					break;
				}
				case PostType.Spam:
				{
					if (spamTerms.ContainsKey(term)) { return; }

					spamTerms.Add(term, 0);

					File.AppendAllText(DirectoryTools.GetSpamTermsFile(), "0]" + term);

					break;
				}
				case PostType.BadUsername:
				{
					if (badUsernameTerms.ContainsKey(term)) { return; }

					badUsernameTerms.Add(term, 0);

					File.AppendAllText(DirectoryTools.GetBadUsernameTermsFile(), "0]" + term);

					break;
				}
			}
		}

		public static void RemoveTerm(PostType type, string term)
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



		private static void RemoveOffensiveTerm(string term)
		{
			if (!offensiveTerms.ContainsKey(term)) { return; }

			offensiveTerms.Remove(term);

			var data = File.ReadAllLines(DirectoryTools.GetOffensiveTermsFile()).ToList();

			for (var i = 0; i < data.Count; i++)
			{
				if (data[i].Split(']')[1] == term)
				{
					data.RemoveAt(i);

					break;
				}
			}

			File.WriteAllLines(DirectoryTools.GetOffensiveTermsFile(), data);
		}

		private static void RemoveSpamTerm(string term)
		{
			if (!spamTerms.ContainsKey(term)) { return; }

			spamTerms.Remove(term);

			var data = File.ReadAllLines(DirectoryTools.GetSpamTermsFile()).ToList();

			for (var i = 0; i < data.Count; i++)
			{
				if (data[i].Split(']')[1] == term)
				{
					data.RemoveAt(i);

					break;
				}
			}

			File.WriteAllLines(DirectoryTools.GetSpamTermsFile(), data);
		}

		private static void RemoveLQTerm(string term)
		{
			if (!lqTerms.ContainsKey(term)) { return; }

			lqTerms.Remove(term);

			var data = File.ReadAllLines(DirectoryTools.GetLQTermsFile()).ToList();

			for (var i = 0; i < data.Count; i++)
			{
				if (data[i].Split(']')[1] == term)
				{
					data.RemoveAt(i);

					break;
				}
			}

			File.WriteAllLines(DirectoryTools.GetLQTermsFile(), data);
		}

		private static void RemoveBadUsernameTerm(string term)
		{
			if (!badUsernameTerms.ContainsKey(term)) { return; }

			badUsernameTerms.Remove(term);

			var data = File.ReadAllLines(DirectoryTools.GetBadUsernameTermsFile()).ToList();

			for (var i = 0; i < data.Count; i++)
			{
				if (data[i].Split(']')[1] == term)
				{
					data.RemoveAt(i);

					break;
				}
			}

			File.WriteAllLines(DirectoryTools.GetBadUsernameTermsFile(), data);
		}

		private static void PopulateOffensiveTerms()
		{
			var data = File.ReadAllText(DirectoryTools.GetOffensiveTermsFile()).Split('\n');
			var termsWithScore = new List<string>();

			foreach (var termAndScore in data)
			{
				var term = termAndScore.Split(']');

				if (term.Length == 1 || offensiveTerms.ContainsKey(term[1])) { continue; }

				offensiveTerms.Add(term[1].Trim(), int.Parse(term[0]));
			}
		}

		private static void PopulateLQTerms()
		{
			var data = File.ReadAllText(DirectoryTools.GetLQTermsFile()).Split('\n');
			var termsWithScore = new List<string>();

			foreach (var termAndScore in data)
			{
				var term = termAndScore.Split(']');

				if (term.Length == 1 || lqTerms.ContainsKey(term[1])) { continue; }

				lqTerms.Add(term[1].Trim(), int.Parse(term[0]));
			}
		}

		private static void PopulateSpamTerms()
		{
			var data = File.ReadAllText(DirectoryTools.GetSpamTermsFile()).Split('\n');
			var termsWithScore = new List<string>();

			foreach (var termAndScore in data)
			{
				var term = termAndScore.Split(']');

				if (term.Length == 1 || spamTerms.ContainsKey(term[1])) { continue; }

				spamTerms.Add(term[1].Trim(), int.Parse(term[0]));
			}
		}

		private static void PopulateBadUsernameTerms()
		{
			var data = File.ReadAllText(DirectoryTools.GetBadUsernameTermsFile()).Split('\n');
			var termsWithScore = new List<string>();

			foreach (var termAndScore in data)
			{
				var term = termAndScore.Split(']');

				if (term.Length == 1 || badUsernameTerms.ContainsKey(term[1])) { continue; }

				badUsernameTerms.Add(term[1].Trim(), int.Parse(term[0]));
			}
		}
	}
}
