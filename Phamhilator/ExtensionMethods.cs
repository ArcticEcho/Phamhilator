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

		//public static void WriteTerm(this IDictionary<string, IDictionary<Regex, float>> terms, Filters filter, string site, string oldTerm, string newTerm)
		//{
		//	if ((int)filter < 100) { throw new ArgumentException("Must be a white filter.", "filter"); }

		//	if (!terms.ContainsKey(site)) { return; }

		//	var file = Path.Combine(DirectoryTools.GetFilterFile(filter), site, "Terms.txt");
		//	var data = File.ReadAllLines(file);

		//	if (String.IsNullOrEmpty(oldTerm))
		//	{
		//		data.ToList().Add(terms[site].Values.Average().ToString(CultureInfo.InvariantCulture) + ']' + newTerm);
		//	}
		//	else
		//	{
		//		for (var i = 0; i < data.Length; i++)
		//		{
		//			if (data[i].EndsWith(']' + oldTerm))
		//			{
		//				data[i] = data[i].Replace(oldTerm, newTerm);
		//			}
		//		}
		//	}

		//	File.WriteAllLines(file, data.Where(line => !String.IsNullOrEmpty(line)));
		//}

		//public static void WriteTerm(this IDictionary<Regex, float> terms, Filters filter, Regex oldTerm, Regex newTerm)
		//{
		//	if ((int)filter > 99) { throw new ArgumentException("Must be a black filter.", "filter"); }

		//	var file = Path.Combine(DirectoryTools.GetFilterFile(filter), "Terms.txt");
		//	var data = File.ReadAllLines(file);

		//	if (String.IsNullOrEmpty(oldTerm.ToString()))
		//	{
		//		terms.Add(new KeyValuePair<Regex, float>(new Regex(newTerm.ToString()), terms.Values.Average()));

		//		data.ToList().Add(terms.Values.Average().ToString(CultureInfo.InvariantCulture) + ']' + newTerm);
		//	}
		//	else
		//	{
		//		var val = terms[oldTerm];

		//		for (var i = 0; i < data.Length; i++)
		//		{
		//			if (data[i].EndsWith(']' + oldTerm))
		//			{
		//				data[i] = data[i].Replace(oldTerm, newTerm);
		//			}
		//		}
		//	}

		//	File.WriteAllLines(file, data.Where(line => !String.IsNullOrEmpty(line)));
		//}
	}
}
