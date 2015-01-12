namespace Phamhilator
{
    public static class PostAnalyser
    {
        public static QuestionAnalysis AnalyseQuestion(Question q)
        {
            var info = new QuestionAnalysis();

            if (q.Score >= 2 || q.AuthorRep >= 1000)
            {
                return info;
            }

            if ((info.BadTags = QuestionAnalyser.IsBadTagUsed(q, out info)) != null && info.BadTags.Count != 0)
            {
                return info;
            }

            if (QuestionAnalyser.IsSpam(q, out info))
            {
                return info;
            }

            if (QuestionAnalyser.IsLowQuality(q, out info))
            {
                return info;
            }

            if (QuestionAnalyser.IsOffensive(q, out info))
            {
                return info;
            }

            if (QuestionAnalyser.IsBadUsername(q, out info))
            {
                return info;
            }

            return info;
        }

        public static AnswerAnalysis AnalyseAnswer(Answer a)
        {
            var info = new AnswerAnalysis();

            if (a.Score >= 2 || a.AuthorRep >= 1000)
            {
                return info;
            }

            if (AnswerAnalyser.IsSpam(a, out info))
            {
                return info;
            }

            if (AnswerAnalyser.IsLowQuality(a, out info))
            {
                return info;
            }

            if (AnswerAnalyser.IsOffensive(a, out info))
            {
                return info;
            }

            if (AnswerAnalyser.IsBadUsername(a, out info))
            {
                return info;
            }

            return info;
        }
    }
}
