using System.Linq;
using System.Collections.Generic;



namespace Phamhilator
{
	public static class PostAnalyser
	{
		public static PostAnalysis CheckPost(Question q)
		{
			var analysis = new PostAnalysis { QRsults = AnalyseQuestion(q) };

			return analysis;
		}

		private static QuestionAnalysis AnalyseQuestion(Question q)
		{
			var info = new QuestionAnalysis();

			if ((info.BadTags = Analyser.QuestionAnalyser.IsBadTagUsed(q, ref info)).Count != 0)
			{
				return info;
			}

			if (Analyser.QuestionAnalyser.IsSpam(q, ref info))
			{
				return info;
			}

			if (Analyser.QuestionAnalyser.IsOffensive(q, ref info))
			{
				return info;
			}

			if (Analyser.QuestionAnalyser.IsLowQuality(q, ref info))
			{
				return info;
			}

			if (Analyser.QuestionAnalyser.IsBadUsername(q, ref info))
			{
				return info;
			}

			return info;
		}
	}
}
