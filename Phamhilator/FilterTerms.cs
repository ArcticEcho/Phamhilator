using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;




namespace Phamhilator
{
	public static class FilterTerms
	{
		private static Dictionary<string, int> terms = new Dictionary<string, int>();



		public static Dictionary<string, int> Terms
		{
			get
			{
				return terms;
			}
		}



		private static void PopulateTerms()
		{
			var data = File.ReadAllText(DirectoryTools.GetFilterTermsFile()).Split('\n');
			var termsWithScore = new List<string>();

			foreach (var termAndScore in data)
			{
				var term = termAndScore.Split(']');

				terms.Add(term[1].Trim(), int.Parse(term[0]));
			}
		}
	}
}
