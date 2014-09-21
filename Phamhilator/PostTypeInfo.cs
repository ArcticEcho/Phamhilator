using System.Collections.Generic;
using System.Text.RegularExpressions;



namespace Phamhilator
{
	public class PostTypeInfo
	{
		public PostType Type;
		public float Score;
		public Dictionary<string, string> BadTags = new Dictionary<string, string>();
		public readonly List<Regex> TermsFound = new List<Regex>();
	}
}