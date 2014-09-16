using System;
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

		public static int AverageTermScore
		{
			get
			{
				var average = offensiveTerms.Values.Average();

				average += lqTerms.Values.Average();
				average += spamTerms.Values.Average();
				average += badUsernameTerms.Values.Average();

				return (int)average / 4;
			}
		}

		public static int HighestTermScore
		{
			get
			{
				var highest = offensiveTerms.Values.Max();

				highest = Math.Max(lqTerms.Values.Max(), highest);
				highest = Math.Max(spamTerms.Values.Max(), highest);
				highest = Math.Max(badUsernameTerms.Values.Max(), highest);

				return highest;
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

					File.AppendAllText(DirectoryTools.GetOffensiveTermsFile(), "\n" + AverageTermScore + "]" + term);

					break;
				}
				case PostType.LowQuality:
				{
					if (lqTerms.ContainsTerm(term)) { return; }

					lqTerms.Add(term, 0); // TODO: set to half score of highest scoring term

					File.AppendAllText(DirectoryTools.GetLQTermsFile(), "\n" + AverageTermScore + "]" + term);

					break;
				}
				case PostType.Spam:
				{
					if (spamTerms.ContainsTerm(term)) { return; }

					spamTerms.Add(term, 0);

					File.AppendAllText(DirectoryTools.GetSpamTermsFile(), "\n" + AverageTermScore + "]" + term);

					break;
				}
				case PostType.BadUsername:
				{
					if (badUsernameTerms.ContainsTerm(term)) { return; }

					badUsernameTerms.Add(term, 0);

					File.AppendAllText(DirectoryTools.GetBadUsernameTermsFile(), "\n" + AverageTermScore + "]" + term);

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

		public static void SetTermScore(PostType type, Regex term, int newScore)
		{
			switch (type)
			{
				case PostType.BadUsername:
				{
					for (var i = 0; i < BadUsernameTerms.Count; i++)
					{
						var key = BadUsernameTerms.Keys.ToArray()[i];

						if (key.ToString() == term.ToString())
						{
							BadUsernameTerms[key] = newScore;
						}
					}

					break;
				}

				case PostType.LowQuality:
				{
					for (var i = 0; i < LQTerms.Count; i++)
					{
						var key = LQTerms.Keys.ToArray()[i];

						if (key.ToString() == term.ToString())
						{
							LQTerms[key] = newScore;
						}
					}

					break;
				}

				case PostType.Offensive:
				{
					for (var i = 0; i < OffensiveTerms.Count; i++)
					{
						var key = OffensiveTerms.Keys.ToArray()[i];

						if (key.ToString() == term.ToString())
						{
							OffensiveTerms[key] = newScore;
						}
					}

					break;
				}

				case PostType.Spam:
				{
					for (var i = 0; i < SpamTerms.Count; i++)
					{
						var key = SpamTerms.Keys.ToArray()[i];

						if (key.ToString() == term.ToString())
						{
							SpamTerms[key] = newScore;
						}
					}

					break;
				}
			}
		}

		public static int GetTermScore(PostType type, Regex term)
		{
			switch (type)
			{
				case PostType.BadUsername:
				{
					for (var i = 0; i < BadUsernameTerms.Count; i++)
					{
						var key = BadUsernameTerms.Keys.ToArray()[i];

						if (key.ToString() == term.ToString())
						{
							return BadUsernameTerms[key];
						}
					}

					break;
				}

				case PostType.LowQuality:
				{
					for (var i = 0; i < LQTerms.Count; i++)
					{
						var key = LQTerms.Keys.ToArray()[i];

						if (key.ToString() == term.ToString())
						{
							return LQTerms[key];
						}
					}

					break;
				}

				case PostType.Offensive:
				{
					for (var i = 0; i < OffensiveTerms.Count; i++)
					{
						var key = OffensiveTerms.Keys.ToArray()[i];

						if (key.ToString() == term.ToString())
						{
							return OffensiveTerms[key];
						}
					}

					break;
				}

				case PostType.Spam:
				{
					for (var i = 0; i < SpamTerms.Count; i++)
					{
						var key = SpamTerms.Keys.ToArray()[i];

						if (key.ToString() == term.ToString())
						{
							return SpamTerms[key];
						}
					}

					break;
				}
			}

			return 0;
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
