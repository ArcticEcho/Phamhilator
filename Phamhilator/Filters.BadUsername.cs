using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;



namespace Phamhilator
{
	namespace Filters
	{
		public class BadUsername
		{
			public Dictionary<Regex, int> Terms { get; private set; }

			public int AverageScore
			{
				get
				{
					return (int)Math.Round(Terms.Values.Average(), 0);
				}
			}

			public int HighestScore
			{
				get
				{
					return Terms.Values.Max();
				}
			}



			public BadUsername()
			{
				var data = File.ReadAllLines(DirectoryTools.GetBadUsernameTermsFile());
				Terms = new Dictionary<Regex, int>();

				foreach (var termAndScore in data)
				{
					if (termAndScore.IndexOf("]", StringComparison.Ordinal) == -1) { continue; }

					var termScore = termAndScore.Substring(0, termAndScore.IndexOf("]", StringComparison.Ordinal));
					var termString = termAndScore.Substring(termAndScore.IndexOf("]", StringComparison.Ordinal) + 1);
					var term = new Regex(termString);

					if (Terms.ContainsTerm(term)) { continue; }

					Terms.Add(term, int.Parse(termScore));
				}
			}



			public void AddTerm(Regex term)
			{
				if (Terms.ContainsTerm(term)) { return; }

				Terms.Add(term, AverageScore);

				File.AppendAllText(DirectoryTools.GetBadUsernameTermsFile(), "\n" + AverageScore + "]" + term);
			}

			public void RemoveTerm(Regex term)
			{
				if (!Terms.ContainsTerm(term)) { return; }

				Terms.Remove(term);

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

			public void SetScore(Regex term, int newScore)
			{
				for (var i = 0; i < Terms.Count; i++)
				{
					var key = Terms.Keys.ToArray()[i];

					if (key.ToString() == term.ToString())
					{
						Terms[key] = newScore;

						var data = File.ReadAllLines(DirectoryTools.GetBadUsernameTermsFile());

						for (int ii = 0; ii < data.Length; ii++)
						{
							var line = data[ii];

							if (!String.IsNullOrEmpty(line) && line.IndexOf("]", StringComparison.Ordinal) != 1)
							{
								var t = line.Remove(0, line.IndexOf("]", StringComparison.Ordinal) + 1);

								if (t == key.ToString())
								{
									data[ii] = newScore + "]" + t;
								}
							}
						}
					}
				}
			}

			public int GetScore(Regex term)
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
		}
	}
}
