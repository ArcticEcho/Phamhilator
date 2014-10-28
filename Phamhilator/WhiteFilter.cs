﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;



namespace Phamhilator
{
	public class WhiteFilter
	{
		public HashSet<Term> Terms { get; private set; }

		public FilterType FilterType { get; private set; }



		public WhiteFilter(FilterType filter)
		{
			if ((int)filter < 100) { throw new ArgumentException("Must be a white filter.", "filter"); }

			FilterType = filter;
			Terms = new HashSet<Term>();

			var sites = Directory.EnumerateDirectories(DirectoryTools.GetFilterFile(filter)).ToArray();

			for (var i = 0; i < sites.Length; i++)
			{
				sites[i] = Path.GetFileName(sites[i]);
			}

			foreach (var site in sites)
			{
				var data = File.ReadAllLines(Path.Combine(DirectoryTools.GetFilterFile(filter), site, "Terms.txt"));

				//Terms.Add(site, new Dictionary<Term, float>());

				foreach (var termAndScore in data)
				{
					if (termAndScore.IndexOf("]", StringComparison.Ordinal) == -1) { continue; }

					var scoreAuto = termAndScore.Substring(0, termAndScore.IndexOf("]", StringComparison.Ordinal));

					var termScore = float.Parse(new String(scoreAuto.Where(c => Char.IsDigit(c) || c == '.' || c == ',').ToArray()));
					var termIsAuto = scoreAuto[0] == 'A';
					var termRegex = new Regex(termAndScore.Substring(termAndScore.IndexOf("]", StringComparison.Ordinal) + 1));

					if (Terms.Contains(termRegex)) { continue; }

					Terms.Add(new Term(termRegex, termScore, site, termIsAuto));
				}
			}
		}



		public void AddTerm(Term term)
		{
			if (Terms.Contains(term.Regex, term.Site)) { return; } // Gasp! Silent failure!

			Terms.WriteTerm(FilterType, new Regex(""), term.Regex, term.Site, term.Score);
		}

		public void RemoveTerm(Term term)
		{
			if (!Terms.Contains(term.Regex, term.Site)) { return; }

			Terms.WriteTerm(FilterType, term.Regex, new Regex(""), term.Site);
		}

		public void EditTerm(Regex oldTerm, Regex newTerm, string site)
		{
			if (!Terms.Contains(oldTerm, site)) { return; }

			Terms.WriteTerm(FilterType, oldTerm, newTerm, site);
		}

		public void SetScore(Term term, float newScore)
		{
			if (!Terms.Contains(term.Regex, term.Site)) { return; }

			Terms.WriteScore(FilterType, term.Regex, newScore, term.Site);
		}

		//public float GetScore(string site, Term term)
		//{
		//	if (!Terms.ContainsKey(site)) { return -1; }

		//	for (var i = 0; i < Terms[site].Count; i++)
		//	{
		//		var key = Terms[site].Keys.ToArray()[i];

		//		if (key.ToString() == term.Regex.ToString())
		//		{
		//			return Terms[site][key];
		//		}
		//	}

		//	return -1;
		//}
	}
}