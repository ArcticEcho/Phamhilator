using System.Collections.Generic;
using System.Text.RegularExpressions;



namespace Phamhilator
{
	public abstract class PostAnalysis
	{
		//public QuestionAnalysis QRsults = new QuestionAnalysis();

		//public List<AnswerAnalysis> AResults = new List<AnswerAnalysis>();

		public PostType Type;
		public float Accuracy;
		public readonly Dictionary<Regex, float> WhiteTermsFound = new Dictionary<Regex, float>();
		public readonly Dictionary<Regex, float> BlackTermsFound = new Dictionary<Regex, float>();

	}
}