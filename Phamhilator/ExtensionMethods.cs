using System;
using System.Collections.Generic;
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

		public static bool Contains(this HashSet<Term> input, Regex term, string site = "")
		{
			return input.Count != 0 && input.Contains(new Term(term, 0, site));
		}

		public static void WriteTerm(this HashSet<Term> terms, FilterType filter, Regex oldTerm, Regex newTerm, string site = "", float newScore = 0)
		{
			if (String.IsNullOrEmpty(oldTerm.ToString()) && String.IsNullOrEmpty(newTerm.ToString())) { throw new Exception("oldTerm and newTerm can not both be empty."); }

			var file = String.IsNullOrEmpty(site) ? DirectoryTools.GetFilterFile(filter) : Path.Combine(DirectoryTools.GetFilterFile(filter), site, "Terms.txt");

			if (!File.Exists(file))
			{
				if (!Directory.Exists(Directory.GetParent(file).FullName))
				{
					Directory.CreateDirectory(Directory.GetParent(file).FullName);
				}

				File.Create(file).Dispose();
			}			
			
			var data = File.ReadAllLines(file).Where(line => !String.IsNullOrEmpty(line) && line.IndexOf(']') != -1).ToList();

			if (String.IsNullOrEmpty(oldTerm.ToString())) // Add new term.
			{
				terms.Add(new Term(newTerm, newScore, site));

				data.Add(newScore + "]" + newTerm);
			}
			else if (String.IsNullOrEmpty(newTerm.ToString())) // Remove old term.
			{
				terms.Remove(terms.GetRealTerm(oldTerm, site));

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
						data[i] = data[i].Replace("]" + oldTerm, "]" + newTerm);
					}
				}

				var realTerm = terms.GetRealTerm(oldTerm, site);

				terms.Remove(realTerm);
				terms.Add(new Term(newTerm, realTerm.Score, realTerm.Site, realTerm.IsAuto));
			}

			File.WriteAllLines(file, data);
		}

		public static void WriteScore(this HashSet<Term> terms, FilterType filter, Regex term, float newScore, string site = "")
		{
			if (String.IsNullOrEmpty(term.ToString())) { throw new ArgumentException("Can not be empty.", "term"); }

			var file = String.IsNullOrEmpty(site) ? DirectoryTools.GetFilterFile(filter) : Path.Combine(DirectoryTools.GetFilterFile(filter), site, "Terms.txt");
			var realTerm = terms.GetRealTerm(term, site);

			if (!File.Exists(file))
			{
				if (!Directory.Exists(Directory.GetParent(file).FullName))
				{
					Directory.CreateDirectory(Directory.GetParent(file).FullName);
				}

				File.Create(file).Dispose();
			}

			var data =  File.ReadAllLines(file).Where(line => !String.IsNullOrEmpty(line) && line.IndexOf(']') != -1).ToList();
			
			for (var i = 0; i < data.Count; i++)
			{
				if (!data[i].EndsWith("]" + term)) { continue; }

				data.RemoveAt(i);

				data.Add(newScore + "]" + term);

				break;
			}

			terms.Remove(realTerm);
			terms.Add(new Term(realTerm.Regex, newScore, realTerm.Site, realTerm.IsAuto));

			File.WriteAllLines(file, data);
		}

		public static void WriteAuto(this HashSet<Term> terms, FilterType filter, Regex term, bool isAuto, string site = "")
		{
			if (String.IsNullOrEmpty(term.ToString())) { throw new ArgumentException("Can not be empty.", "term"); }

			var file = String.IsNullOrEmpty(site) ? DirectoryTools.GetFilterFile(filter) : Path.Combine(DirectoryTools.GetFilterFile(filter), site, "Terms.txt");
			var data = File.ReadAllLines(file).Where(line => !String.IsNullOrEmpty(line) && line.IndexOf(']') != -1).ToList();
			var realTerm = terms.GetRealTerm(term, site);

			for (var i = 0; i < data.Count; i++)
			{
				if (!data[i].EndsWith("]" + term)) { continue; }

				data.RemoveAt(i);

				data.Add((isAuto ? "A" : "") + realTerm.Score + "]" + term);

				break;
			}

			terms.Remove(realTerm);
			terms.Add(new Term(realTerm.Regex, realTerm.Score, realTerm.Site, isAuto));

			File.WriteAllLines(file, data);
		}

		public static Term GetRealTerm(this HashSet<Term> terms, Regex term, string site = "")
		{
			if (String.IsNullOrEmpty(site))
			{
				foreach (var t in terms)
				{
					if (t.Regex.ToString() == term.ToString())
					{
						return t;
					}
				}
			}
			else
			{
				foreach (var t in terms)
				{
					if (t.Equals(term, site))
					{
						return t;
					}
				}
			}

			throw new KeyNotFoundException();
		}
	}
}
