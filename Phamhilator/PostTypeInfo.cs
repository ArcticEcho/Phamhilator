using System.Collections.Generic;
using System.Text.RegularExpressions;



namespace Phamhilator
{
	public class PostTypeInfo
	{
		public PostType Type;
		public float Accuracy;
		public List<string> BadTags = new List<string>();
		public List<Regex> TermsFound = new List<Regex>();
	}
}