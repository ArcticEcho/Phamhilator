using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;



namespace Phamhilator
{
	public class WhiteFilter
	{
		public Dictionary<string, Dictionary<Regex, float>> Terms { get; private set; }

		public FilterType FilterType { get; private set; }



		public WhiteFilter(FilterType filter)
		{
			if ((int)filter < 100) { throw new ArgumentException("Must be a white filter.", "filter"); }

			FilterType = filter;
			Terms = new Dictionary<string, Dictionary<Regex, float>>();

			var sites = Directory.EnumerateDirectories(DirectoryTools.GetFilterFile(filter)).ToArray();

			for (var i = 0; i < sites.Length; i++)
			{
				sites[i] = Path.GetFileName(sites[i]);
			}

			foreach (var site in sites)
			{
				var data = File.ReadAllLines(Path.Combine(DirectoryTools.GetFilterFile(filter), site, "Terms.txt"));

				Terms.Add(site, new Dictionary<Regex, float>());

				foreach (var termAndScore in data)
				{
					if (termAndScore.IndexOf("]", StringComparison.Ordinal) == -1) { continue; }

					var termScore = float.Parse(termAndScore.Substring(0, termAndScore.IndexOf("]", StringComparison.Ordinal)));
					var termString = termAndScore.Substring(termAndScore.IndexOf("]", StringComparison.Ordinal) + 1);
					var term = new Regex(termString);

					if (Terms[site].ContainsTerm(term)) { continue; }

					Terms[site].Add(term, termScore);
				}
			}
		}



		public void AddTerm(string site, Regex term)
		{
			if (Terms[site].ContainsTerm(term)) { return; } // Gasp! Silent failure!

			Terms.WriteTerm(FilterType, site, new Regex(""), term);
		}

		public void RemoveTerm(string site, Regex term)
		{
			if (!Terms[site].ContainsTerm(term)) { return; }

			Terms.WriteTerm(FilterType, site, term, new Regex(""));
		}

		public void EditTerm(string site, Regex oldTerm, Regex newTerm)
		{
			if (!Terms[site].ContainsTerm(oldTerm)) { return; }

			Terms.WriteTerm(FilterType, site, oldTerm, newTerm);
		}

		public void SetScore(string site, Regex term, float newScore)
		{
			if (!Terms[site].ContainsTerm(term)) { return; }

			Terms.WriteScore(FilterType, site, term, newScore);
		}

		public float GetScore(string site, Regex term)
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
