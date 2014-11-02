using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json;



namespace Phamhilator
{
	public class BlackFilter
	{
		public HashSet<Term> Terms { get; private set; }

		public float AverageScore
		{
			get
			{
				return Terms.Count == 0 ? 10 : Terms.Select(t => t.Score).Average();
			}
		}

		public float HighestScore
		{
			get
			{
				return Terms.Count == 0 ? 10 : Terms.Select(t => t.Score).Max();
			}
		}

		public FilterType FilterType { get; private set; }



		public BlackFilter(FilterType filter)
		{
			if ((int)filter > 99) { throw new ArgumentException("Must be a black filter.", "filter"); }

			FilterType = filter;
			Terms = new HashSet<Term>();

			var data = JsonConvert.DeserializeObject<List<TempTerm>>(File.ReadAllText(DirectoryTools.GetFilterFile(filter)));

			foreach (var t in data)
			{
				Terms.Add(t.ToTerm(FilterType));
			}
		}



		public void AddTerm(Term term)
		{
			if (Terms.Contains(term.Regex)) { return; } // Gasp! Silent failure!

			Terms.WriteTerm(FilterType, new Regex(""), term.Regex, "", term.Score);
		}

		public void RemoveTerm(Regex term)
		{
			if (!Terms.Contains(term)) { return; }

			Terms.WriteTerm(FilterType, term, new Regex(""));
		}

		public void EditTerm(Regex oldTerm, Regex newTerm)
		{
			if (!Terms.Contains(oldTerm)) { return; }

			Terms.WriteTerm(FilterType, oldTerm, newTerm);
		}

		public void SetScore(Term term, float newScore)
		{
			if (!Terms.Contains(term.Regex)) { return; }

			Terms.WriteScore(FilterType, term.Regex, newScore);
		}

		public void SetAuto(Regex term, bool isAuto, bool persistence = false)
		{
			if (!Terms.Contains(term)) { return; }

			if (persistence)
			{
				Terms.WriteAuto(FilterType, term, isAuto);
			}
			else
			{
				var t = Terms.GetRealTerm(term);

				Terms.Remove(t);

				Terms.Add(new Term(FilterType, t.Regex, t.Score, t.Site, isAuto));
			}
		}
	}
}
