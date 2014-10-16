using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;



namespace Phamhilator.AnswerFilters.Black
{
	public class Spam
	{
		public Dictionary<Regex, float> Terms { get; private set; }

		public float AverageScore
		{
			get
			{
				return Terms.Count == 0 ? 5 : Terms.Values.Average();
			}
		}

		public float HighestScore
		{
			get
			{
				return Terms.Count == 0 ? 10 : Terms.Values.Max();
			}
		}



		public Spam()
		{
			var data = File.ReadAllLines(DirectoryTools.GetABSpamTermsFile());
			Terms = new Dictionary<Regex, float>();

			foreach (var termAndScore in data)
			{
				if (termAndScore.IndexOf("]", StringComparison.Ordinal) == -1) { continue; }

				var termScore = termAndScore.Substring(0, termAndScore.IndexOf("]", StringComparison.Ordinal));
				var termString = termAndScore.Substring(termAndScore.IndexOf("]", StringComparison.Ordinal) + 1);
				var term = new Regex(termString);

				if (Terms.ContainsTerm(term)) { continue; }

				Terms.Add(term, float.Parse(termScore));
			}
		}



		public void AddTerm(Regex term)
		{
			if (Terms.ContainsTerm(term)) { return; }

			Terms.Add(term, AverageScore);

			File.AppendAllText(DirectoryTools.GetABSpamTermsFile(), "\n" + AverageScore + "]" + term);
		}

		public void RemoveTerm(Regex term)
		{
			if (!Terms.ContainsTerm(term)) { return; }

			Terms.Remove(term);

			var data = File.ReadAllLines(DirectoryTools.GetABSpamTermsFile()).ToList();

			for (var i = 0; i < data.Count; i++)
			{
				if (data[i].Remove(0, data[i].IndexOf("]", StringComparison.Ordinal) + 1) == term.ToString())
				{
					data.RemoveAt(i);

					break;
				}
			}

			File.WriteAllLines(DirectoryTools.GetABSpamTermsFile(), data);
		}

		public void SetScore(Regex term, float newScore)
		{
			for (var i = 0; i < Terms.Count; i++)
			{
				var key = Terms.Keys.ToArray()[i];

				if (key.ToString() == term.ToString())
				{
					Terms[key] = newScore;

					var data = File.ReadAllLines(DirectoryTools.GetABSpamTermsFile());

					for (var ii = 0; ii < data.Length; ii++)
					{
						var line = data[ii];

						if (!String.IsNullOrEmpty(line) && line.IndexOf("]", StringComparison.Ordinal) != -1)
						{
							var t = line.Remove(0, line.IndexOf("]", StringComparison.Ordinal) + 1);

							if (t == key.ToString())
							{
								data[ii] = newScore + "]" + t;
							}
						}
					}

					File.WriteAllLines(DirectoryTools.GetABSpamTermsFile(), data);

					return;
				}
			}
		}

		public float GetScore(Regex term)
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
