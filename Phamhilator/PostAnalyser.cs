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

			if (q.Score >= 2 || q.AuthorRep >= 1000)
			{
				return info;
			}

			if ((info.BadTags = Analysers.QuestionTitle.IsBadTagUsed(q, ref info)).Count != 0)
			{
				return info;
			}

			if (Analysers.QuestionTitle.IsSpam(q, ref info) || Analysers.QuestionBody.IsSpam(q, ref info))
			{
				return info;
			}

			if (Analysers.QuestionTitle.IsOffensive(q, ref info) || Analysers.QuestionBody.IsOffensive(q, ref info))
			{
				return info;
			}

			if (Analysers.QuestionTitle.IsLowQuality(q, ref info) || Analysers.QuestionBody.IsLowQuality(q, ref info))
			{
				return info;
			}

			if (Analysers.QuestionTitle.IsBadUsername(q, ref info))
			{
				return info;
			}

			return info;
		}

		private static AnswerAnalysis AnalyseAnswer(Answer a)
		{
			var info = new AnswerAnalysis();

			if (a.Score >= 2 || a.AuthorRep >= 1000)
			{
				return info;
			}

			if (Analysers.Answer.IsSpam(a, ref info))
			{
				return info;
			}

			if (Analysers.Answer.IsOffensive(a, ref info))
			{
				return info;
			}

			if (Analysers.Answer.IsLowQuality(a, ref info))
			{
				return info;
			}

			if (Analysers.Answer.IsBadUsername(a, ref info))
			{
				return info;
			}

			return info;
		}
	}
}
