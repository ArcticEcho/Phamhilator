using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;



namespace Phamhilator
{
	public class BlackFilter
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

		public FilterType FilterType { get; private set; }



		public BlackFilter(FilterType filter)
		{
			if ((int)filter > 99) { throw new ArgumentException("Must be a black filter.", "filter"); }

			FilterType = filter;
			Terms = new Dictionary<Regex, float>();
			var data = File.ReadAllLines(DirectoryTools.GetFilterFile(filter));

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
			if (Terms.ContainsTerm(term)) { return; } // Gasp! Silent failure!

			Terms.WriteTerm(FilterType, new Regex(""), term);
		}

		public void RemoveTerm(Regex term)
		{
			if (!Terms.ContainsTerm(term)) { return; }

			Terms.WriteTerm(FilterType, term, new Regex(""));
		}

		public void EditTerm(Regex oldTerm, Regex newTerm)
		{
			if (!Terms.ContainsTerm(oldTerm)) { return; }

			Terms.WriteTerm(FilterType, oldTerm, newTerm);
		}

		public void SetScore(Regex term, float newScore)
		{
			if (!Terms.ContainsTerm(term)) { return; }

			Terms.WriteScore(FilterType, term, newScore);
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
