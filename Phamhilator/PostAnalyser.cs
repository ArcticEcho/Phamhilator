using System.Collections.Generic;
using System.Linq;



namespace Phamhilator
{
    public static class PostAnalyser
    {
        public static QuestionAnalysis AnalyseQuestion(Question question)
        {
            var info = new QuestionAnalysis();

            if (question.Score >= 2 || question.AuthorRep >= 1000)
            {
                return null;
            }

            if ((info.BadTags = FindBadTags(question, out info)) != null && info.BadTags.Count != 0)
            {
                return info;
            }

            foreach (var blackFilter in Config.BlackFilters.Where(f => f.Key.Class.IsQuestion()))
            {
                var whiteFilterConfig = new FilterConfig(blackFilter.Key.Class, FilterType.White);

                if ((info = AnaylsePost(question, blackFilter.Value, Config.WhiteFilters[whiteFilterConfig], blackFilter.Key.Class).ToQuestionAnalysis()) != null)
                {
                    return info;
                }
            }

            return info;
        }

        public static PostAnalysis AnalyseAnswer(Post answer)
        {
            var info = new AnswerAnalysis();

            if (answer.Score >= 2 || answer.AuthorRep >= 1000)
            {
                return info;
            }

            foreach (var blackFilter in Config.BlackFilters.Where(f => !f.Key.Class.IsQuestion()))
            {
                var whiteFilterConfig = new FilterConfig(blackFilter.Key.Class, FilterType.White);

                if ((info = AnaylsePost(answer, blackFilter.Value, Config.WhiteFilters[whiteFilterConfig], blackFilter.Key.Class)) != null)
                {
                    return info;
                }
            }

            return info;
        }



        private static AnswerAnalysis AnaylsePost(Post post, BlackFilter blackTerms, WhiteFilter whiteTerms, FilterClass filterType)
        {
            var termsFound = 0;
            var info = new AnswerAnalysis();

            for (var i = 0; i < blackTerms.Terms.Count; i++)
            {
                var blackTerm = blackTerms.Terms.ElementAt(i);

                if (blackTerm.Regex.IsMatch(post.Body) && !filterType.IsQuestionTitle() && !filterType.ToString().ToLowerInvariant().Contains("name"))
                {
                    info.Accuracy += blackTerm.Score;
                    info.BlackTermsFound.Add(blackTerm);

                    blackTerm.CaughtCount++;
                    termsFound++;
                }
            }

            // If no black listed terms were found, assume the post is clean.
            if (termsFound == 0) { return null; }

            for (var i = 0; i < whiteTerms.Terms.Count; i++)
            {
                var whiteTerm = whiteTerms.Terms.ElementAt(i);

                if (whiteTerm.Site != post.Site) { continue; }

                if (whiteTerm.Regex.IsMatch(post.Body))
                {
                    info.Accuracy -= whiteTerm.Score;
                    info.WhiteTermsFound.Add(whiteTerm);
                    info.FiltersUsed.Add(filterType, FilterType.White);

                    termsFound++;
                }
            }

            info.AutoTermsFound = info.BlackTermsFound.Any(t => t.IsAuto);
            info.FiltersUsed.Add(filterType, FilterType.Black);
            info.Accuracy /= termsFound;
            info.Accuracy /= blackTerms.HighestScore;
            info.Accuracy *= 100;
            info.Type = info.Accuracy >= Config.AccuracyThreshold ? filterType.ToPostType() : PostType.Clean;

            return info;
        }

        private static Dictionary<string, string> FindBadTags(Question question, out QuestionAnalysis info)
        {
            var tags = new Dictionary<string, string>();
            info = new QuestionAnalysis();

            if (!Config.BadTags.Tags.Keys.Contains(question.Site)) { return tags; }

            foreach (var tag in question.Tags)
            {
                if (Config.BadTags.Tags[question.Site].ContainsKey(tag.ToLowerInvariant()))
                {
                    tags.Add(tag, Config.BadTags.Tags[question.Site][tag]);
                }
            }

            if (tags.Count != 0)
            {
                info.Accuracy = 100;
                info.Type = PostType.BadTagUsed;
            }

            return tags;
        }
    }
}
