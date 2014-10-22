using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;



namespace Phamhilator
{
	public static class ExtensionMethods
	{
		public static FilterType GetCorrespondingWhiteFilter(this FilterType input)
		{
			return (FilterType)Enum.Parse(typeof(FilterType), input.ToString().Replace("Black", "White"));
		}

		public static FilterType GetCorrespondingBlackFilter(this FilterType input)
		{
			return (FilterType)Enum.Parse(typeof(FilterType), input.ToString().Replace("White", "Black"));
		}

		public static bool ContainsTerm(this IDictionary<Regex, string> input, Regex term)
		{
			if (input.Count == 0) { return false; }

			foreach (var t in input)
			{
				if (t.ToString() == term.ToString())
				{
					return true;
				}
			}

			return false;
		}

		public static bool ContainsTerm(this IDictionary<Regex, float> input, Regex term)
		{
			if (input.Count == 0) { return false; }

			foreach (var t in input.Keys)
			{
				if (t.ToString() == term.ToString())
				{
					return true;
				}
			}

			return false;
		}

		public static void WriteTerm(this Dictionary<Regex, float> terms, FilterType filter, Regex oldTerm, Regex newTerm)
		{
			if ((int)filter > 99) { throw new ArgumentException("Must be a black filter.", "filter"); }
			if (String.IsNullOrEmpty(oldTerm.ToString()) && String.IsNullOrEmpty(newTerm.ToString())) { throw new Exception("oldTerm and newTerm can not both be empty."); }

			var file = DirectoryTools.GetFilterFile(filter);
			var data = File.ReadAllLines(file).Where(line => !String.IsNullOrEmpty(line) && line.IndexOf(']') != -1).ToList();

			if (String.IsNullOrEmpty(oldTerm.ToString())) // Add new term.
			{
				terms.Add(newTerm, terms.Values.Average());

				data.Add(terms.Values.Average().ToString(CultureInfo.InvariantCulture) + ']' + newTerm);
			}
			else if (String.IsNullOrEmpty(newTerm.ToString())) // Remove old term.
			{
				terms.Remove(terms.GetRealTerm(oldTerm));

				for (var i = 0; i < data.Count; i++)
				{
					if (!data[i].EndsWith("]" + oldTerm)) { continue; }

					data.RemoveAt(i);

					break;
				}
			}
			else // Edit existing term.
			{
				for (var i = 0; i < data.Count; i++)
				{
					if (data[i].EndsWith("]" + oldTerm))
					{
						data[i] = data[i].Replace(oldTerm.ToString(), newTerm.ToString());
					}
				}

				var realTerm = terms.GetRealTerm(oldTerm);
				var score = terms[realTerm];

				terms.Remove(realTerm);
				terms.Add(newTerm, score);
			}

			File.WriteAllLines(file, data);
		}

		public static void WriteTerm(this Dictionary<string, Dictionary<Regex, float>> terms, FilterType filter, string site, Regex oldTerm, Regex newTerm)
		{
			if ((int)filter < 100) { throw new ArgumentException("Must be a white filter.", "filter"); }
			if (String.IsNullOrEmpty(site)) { throw new ArgumentException("Can not be null or empty.", "site"); }
			if (String.IsNullOrEmpty(oldTerm.ToString()) && String.IsNullOrEmpty(newTerm.ToString())) { throw new Exception("oldTerm and newTerm can not both be empty."); }

			var file = Path.Combine(DirectoryTools.GetFilterFile(filter), site, "Terms.txt");
			var data = File.ReadAllLines(file).Where(line => !String.IsNullOrEmpty(line) && line.IndexOf(']') != -1).ToList();

			if (String.IsNullOrEmpty(oldTerm.ToString())) // Add new term.
			{
				terms[site].Add(newTerm, terms[site].Values.Average());

				data.Add(terms[site].Values.Average().ToString(CultureInfo.InvariantCulture) + ']' + newTerm);
			}
			else if (String.IsNullOrEmpty(newTerm.ToString())) // Remove old term.
			{
				terms[site].Remove(terms[site].GetRealTerm(oldTerm));

				for (var i = 0; i < data.Count; i++)
				{
					if (!data[i].EndsWith("]" + oldTerm)) { continue; }

					data.RemoveAt(i);

					break;
				}
			}
			else // Edit existing term.
			{
				for (var i = 0; i < data.Count; i++)
				{
					if (data[i].EndsWith("]" + oldTerm))
					{
						data[i] = data[i].Replace(oldTerm.ToString(), newTerm.ToString());
					}
				}

				var realTerm = terms[site].GetRealTerm(oldTerm);
				var score = terms[site][realTerm];

				terms[site].Remove(realTerm);
				terms[site].Add(newTerm, score);
			}

			File.WriteAllLines(file, data);
		}

		public static void WriteScore(this Dictionary<Regex, float> terms, FilterType filter, Regex term, float newScore)
		{
			if ((int)filter > 99) { throw new ArgumentException("Must be a black filter.", "filter"); }
			if (String.IsNullOrEmpty(term.ToString())) { throw new ArgumentException("Can not both be empty.", "filter"); }

			var file = DirectoryTools.GetFilterFile(filter);
			var data = File.ReadAllLines(file).Where(line => !String.IsNullOrEmpty(line) && line.IndexOf(']') != -1).ToList();
			var realTerm = terms.GetRealTerm(term);

			for (var i = 0; i < data.Count; i++)
			{
				if (!data[i].EndsWith("]" + term)) { continue; }

				data.RemoveAt(i);

				data.Add(newScore + "]" + term);

				break;
			}

			terms[realTerm] = newScore;

			File.WriteAllLines(file, data);
		}

		public static void WriteScore(this Dictionary<string, Dictionary<Regex, float>> terms, FilterType filter, string site, Regex term, float newScore)
		{
			if ((int)filter > 99) { throw new ArgumentException("Must be a black filter.", "filter"); }
			if (String.IsNullOrEmpty(site)) { throw new ArgumentException("Can not be null or empty.", "site"); }
			if (String.IsNullOrEmpty(term.ToString())) { throw new ArgumentException("Can not both be empty.", "filter"); }

			var file = Path.Combine(DirectoryTools.GetFilterFile(filter), site, "Terms.txt");
			var data = File.ReadAllLines(file).Where(line => !String.IsNullOrEmpty(line) && line.IndexOf(']') != -1).ToList();
			var realTerm = terms[site].GetRealTerm(term);

			for (var i = 0; i < data.Count; i++)
			{
				if (!data[i].EndsWith("]" + term)) { continue; }

				data.RemoveAt(i);

				data.Add(newScore + "]" + term);

				break;
			}

			terms[site][realTerm] = newScore;

			File.WriteAllLines(file, data);
		}



		private static Regex GetRealTerm(this IDictionary<Regex, float> terms, Regex term)
		{
			foreach (var t in terms.Keys)
			{
				if (term.ToString() == t.ToString())
				{
					return t;
				}
			}

			return new Regex("");
		}
	}
}
