using System.Collections.Generic;
using System.Text.RegularExpressions;



namespace Phamhilator
{
	public class PostTypeInfo
	{
		public PostType Type;
		public float Accuracy;
		public Dictionary<string, string> BadTags = new Dictionary<string, string>();
		public readonly Dictionary<Regex, float> WhiteTermsFound = new Dictionary<Regex, float>();
		public readonly Dictionary<Regex, float> BlackTermsFound = new Dictionary<Regex, float>();
	}
}