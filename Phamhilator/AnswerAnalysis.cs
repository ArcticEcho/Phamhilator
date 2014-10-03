using System.Collections.Generic;
using System.Text.RegularExpressions;



namespace Phamhilator
{
	public class AnswerAnalysis
	{
		public PostType AnswerType;
		public float Accuracy;
		public readonly Dictionary<Regex, float> WhiteTermsFound = new Dictionary<Regex, float>();
		public readonly Dictionary<Regex, float> BlackTermsFound = new Dictionary<Regex, float>();
	}
}
