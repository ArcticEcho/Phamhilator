using System;
using System.Collections.Generic;
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

		//public static void WriteTerm(this IDictionary<string, IDictionary<Regex, float>> terms, string site, string oldTerm, string newTerm)
		//{
		//	if (!terms.ContainsKey(site)) { return; }

		//	var file = Path.Combine(DirectoryTools.GetQTWLQTermsDir(), site, "Terms.txt");
		//	var data = File.ReadAllLines(file);

		//	if (String.IsNullOrEmpty(oldTerm))
		//	{
		//		data.ToList().Add(terms[site].Values.Average().ToString() + ']' + newTerm);
		//	}
		//	else
		//	{
		//		for (var i = 0; i < data.Length; i++)
		//		{
		//			if (data[i].EndsWith(']' + oldTerm))
		//			{
		//				data[i].Replace(oldTerm, newTerm);
		//			}
		//		}
		//	}

		//	File.WriteAllLines(file, data.Where(line => !String.IsNullOrEmpty(line)));
		//}
	}
}
