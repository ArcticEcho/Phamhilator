namespace Phamhilator
{
	public static class PostAnalyser
	{
		public static QAAnalysis CheckPost(Question q)
		{
			var analysis = new QAAnalysis { QResults = AnalyseQuestion(q) };

			foreach (var a in q.Answers)
			{
				analysis.AResults.Add(a, AnalyseAnswer(a));
			}

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

		private static AnswerAnalysis AnalyseAnswer(Answer a)
		{
			var info = new AnswerAnalysis();

			if (Analyser.AnswerAnalyser.IsSpam(a, ref info))
			{
				return info;
			}

			if (Analyser.AnswerAnalyser.IsOffensive(a, ref info))
			{
				return info;
			}

			if (Analyser.AnswerAnalyser.IsLowQuality(a, ref info))
			{
				return info;
			}

			if (Analyser.AnswerAnalyser.IsBadUsername(a, ref info))
			{
				return info;
			}

			return info;
		}
	}
}
