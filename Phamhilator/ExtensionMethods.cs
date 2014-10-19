using System.Collections.Generic;
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
	}
}
