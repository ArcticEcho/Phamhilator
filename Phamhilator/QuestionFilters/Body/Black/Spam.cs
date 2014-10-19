using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;



namespace Phamhilator.QuestionFilters.Body.Black
{
	public class Spam
	{
		public Dictionary<Regex, float> Terms { get; private set; }

		public float AverageScore
		{
			get
			{
				return Terms.Count == 0 ? 10 : Terms.Values.Average();
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
			var data = File.ReadAllLines(DirectoryTools.GetQBBSpamTermsFile());
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

			File.AppendAllText(DirectoryTools.GetQBBSpamTermsFile(), "\n" + AverageScore + "]" + term);
		}

		public void RemoveTerm(Regex term)
		{
			if (!Terms.ContainsTerm(term)) { return; }

			for (var i = 0; i < Terms.Count; i++)
			{
				var t = Terms.Keys.ElementAt(i);

				if (term.ToString() != t.ToString()) { continue; }

				Terms.Remove(t);

				break;
			}

			var data = File.ReadAllLines(DirectoryTools.GetQBBSpamTermsFile()).ToList();

			for (var i = 0; i < data.Count; i++)
			{
				if (data[i].Remove(0, data[i].IndexOf("]", StringComparison.Ordinal) + 1) != term.ToString()) { continue; }

				data.RemoveAt(i);

				break;
			}

			File.WriteAllLines(DirectoryTools.GetQBBSpamTermsFile(), data);
		}

		public void EditTerm(Regex oldTerm, Regex newTerm)
		{
			for (var i = 0; i < Terms.Count; i++)
			{
				var t = Terms.Keys.ElementAt(i);

				if (oldTerm.ToString() != t.ToString()) { continue; }

				var score = Terms[t];

				Terms.Remove(t);

				Terms.Add(newTerm, score);

				break;
			}
		}

		public void SetScore(Regex term, float newScore)
		{
			for (var i = 0; i < Terms.Count; i++)
			{
				var key = Terms.Keys.ToArray()[i];

				if (key.ToString() != term.ToString()) { continue; }

				Terms[key] = newScore;

				var data = File.ReadAllLines(DirectoryTools.GetQBBSpamTermsFile());

				for (var ii = 0; ii < data.Length; ii++)
				{
					var line = data[ii];

					if (String.IsNullOrEmpty(line) || line.IndexOf("]", StringComparison.Ordinal) == -1) { continue; }

					var t = line.Remove(0, line.IndexOf("]", StringComparison.Ordinal) + 1);

					if (t != key.ToString()) { continue; }

					data[ii] = newScore + "]" + t;

					break;
				}

				File.WriteAllLines(DirectoryTools.GetQBBSpamTermsFile(), data);

				return;
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
