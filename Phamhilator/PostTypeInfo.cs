using System.Collections.Generic;
using System.Text.RegularExpressions;



namespace Phamhilator
{
	public class PostTypeInfo
	{
		public PostType Type;
		public float Score;
		public List<string> BadTags = new List<string>();
		public readonly List<Regex> TermsFound = new List<Regex>();
	}
}