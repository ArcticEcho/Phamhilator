using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;



namespace Phamhilator
{
	namespace WhiteFilters
	{
		public class LQ
		{
			public Dictionary<string, Dictionary<Regex, float>> Terms { get; private set; }



			public LQ()
			{
				Terms = new Dictionary<string, Dictionary<Regex, float>>();

				var sites = Directory.EnumerateDirectories(DirectoryTools.GetWhiteLQTermsDir()).ToArray();

				for (var i = 0; i < sites.Length; i++)
				{
					sites[i] = Path.GetFileName(sites[i]);
				}

				foreach (var site in sites)
				{
					var data = File.ReadAllLines(Path.Combine(DirectoryTools.GetWhiteLQTermsDir(), site, "Terms.txt"));

					Terms.Add(site, new Dictionary<Regex, float>());

					foreach (var termAndScore in data)
					{
						if (termAndScore.IndexOf("]", StringComparison.Ordinal) == -1)
						{
							continue;
						}

						var termScore = float.Parse(termAndScore.Substring(0, termAndScore.IndexOf("]", StringComparison.Ordinal)));
						var termString = termAndScore.Substring(termAndScore.IndexOf("]", StringComparison.Ordinal) + 1);
						var term = new Regex(termString);

						if (Terms[site].ContainsTerm(term))
						{
							continue;
						}

						Terms[site].Add(term, termScore);
					}
				}
			}



			public void AddTerm(Regex term, string site, float score = -1)
			{
				var file = Path.Combine(DirectoryTools.GetWhiteLQTermsDir(), site, "Terms.txt");

				if (!Terms.ContainsKey(site))
				{
					Terms.Add(site, new Dictionary<Regex, float>());

					Directory.CreateDirectory(Path.Combine(DirectoryTools.GetWhiteLQTermsDir(), site));
				}

				if (score == -1)
				{
					if (Terms[site].Count == 0)
					{
						score = 10;
					}
					else
					{
						score = Terms[site].Values.Average();
					}
				}

				Terms[site].Add(term, score);

				File.AppendAllText(file, "\n" + score + "]" + term);
			}

			public void RemoveTerm(Regex term, string site)
			{
				if (!Terms.ContainsKey(site) || !Terms[site].ContainsTerm(term))
				{
					return;
				}

				Terms[site].Remove(term);

				var file = Path.Combine(DirectoryTools.GetWhiteLQTermsDir(), site, "Terms.txt");
				var data = File.ReadAllLines(file).ToList();

				for (var i = 0; i < data.Count; i++)
				{
					if (data[i].Remove(0, data[i].IndexOf("]", StringComparison.Ordinal) + 1) == term.ToString())
					{
						data.RemoveAt(i);

						break;
					}
				}

				File.WriteAllLines(file, data);
			}

			public void SetScore(Regex term, string site, float newScore)
			{
				if (!Terms.ContainsKey(site)) { return; }

				var file = Path.Combine(DirectoryTools.GetWhiteLQTermsDir(), site, "Terms.txt");

				for (var i = 0; i < Terms[site].Count; i++)
				{
					var key = Terms[site].Keys.ToArray()[i];

					if (key.ToString() == term.ToString())
					{
						Terms[site][key] = newScore;

						var data = File.ReadAllLines(file);

						for (var ii = 0; ii < data.Length; ii++)
						{
							var line = data[ii];

							if (line.IndexOf("]", StringComparison.Ordinal) != -1)
							{
								var t = line.Remove(0, line.IndexOf("]", StringComparison.Ordinal) + 1);

								if (t == key.ToString())
								{
									data[ii] = newScore + "]" + t;

									break;
								}
							}
						}

						File.WriteAllLines(file, data);

						return;
					}
				}
			}

			public float GetScore(Regex term, string site)
			{
				if (!Terms.ContainsKey(site)) { return -1; }

				for (var i = 0; i < Terms[site].Count; i++)
				{
					var key = Terms[site].Keys.ToArray()[i];

					if (key.ToString() == term.ToString())
					{
						return Terms[site][key];
					}
				}

				return -1;
			}
		}
	}
}
