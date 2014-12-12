using System.Linq;



namespace Phamhilator.Analysers
{
    public static class QuestionBody
    {
        public static bool IsSpam(Question post, ref QuestionAnalysis info)
        {
            if (post.PopulateExtraDataFailed) { return false; }

            var filtersUsed = 0;

            // Loop over blacklist.

            for (var i = 0; i < GlobalInfo.BlackFilters[FilterType.QuestionBodyBlackSpam].Terms.Count; i++)
            {
                var blackTerm = GlobalInfo.BlackFilters[FilterType.QuestionBodyBlackSpam].Terms.ElementAt(i);

                if (blackTerm.Regex.IsMatch(post.Body))
                {
                    info.Accuracy += blackTerm.Score;
                    info.BlackTermsFound.Add(blackTerm);

                    blackTerm.CaughtCount++;
                    filtersUsed++;
                }
            }

            // Otherwise, if no blacklist terms were found, assume the post is clean.

            if (filtersUsed == 0) { return false; }

            // Loop over whitelist.

            foreach (var whiteTerm in GlobalInfo.WhiteFilters[FilterType.QuestionBodyWhiteSpam].Terms.Where(t => t.Site == post.Site))
            {
                if (whiteTerm.Regex.IsMatch(post.Body))
                {
                    info.Accuracy -= whiteTerm.Score;
                    info.WhiteTermsFound.Add(whiteTerm);
                    info.FiltersUsed.Add(FilterType.QuestionBodyWhiteSpam);

                    filtersUsed++;
                }
            }

            info.AutoTermsFound = info.BlackTermsFound.Any(t => t.IsAuto);
            info.FiltersUsed.Add(FilterType.QuestionBodyBlackSpam);
            info.Accuracy /= filtersUsed;
            info.Accuracy /= GlobalInfo.BlackFilters[FilterType.QuestionBodyBlackSpam].HighestScore;
            info.Accuracy *= 100;
            info.Type = PostType.Spam;

            return true;
        }

        public static bool IsLowQuality(Question post, ref QuestionAnalysis info)
        {
            if (post.PopulateExtraDataFailed) { return false; }

            var filtersUsed = 0;

            // Loop over blacklist.

            for (var i = 0; i < GlobalInfo.BlackFilters[FilterType.QuestionBodyBlackLQ].Terms.Count; i++)
            {
                var blackTerm = GlobalInfo.BlackFilters[FilterType.QuestionBodyBlackLQ].Terms.ElementAt(i);

                if (blackTerm.Regex.IsMatch(post.Body))
                {
                    info.Accuracy += blackTerm.Score;
                    info.BlackTermsFound.Add(blackTerm);

                    blackTerm.CaughtCount++;
                    filtersUsed++;
                }
            }

            // Otherwise, if no blacklist terms were found, assume the post is clean.

            if (filtersUsed == 0) { return false; }

            // Loop over whitelist.

            foreach (var whiteTerm in GlobalInfo.WhiteFilters[FilterType.QuestionBodyWhiteLQ].Terms.Where(t => t.Site == post.Site))
            {
                if (whiteTerm.Regex.IsMatch(post.Body))
                {
                    info.Accuracy -= whiteTerm.Score;
                    info.WhiteTermsFound.Add(whiteTerm);
                    info.FiltersUsed.Add(FilterType.QuestionBodyWhiteLQ);

                    filtersUsed++;
                }
            }

            info.AutoTermsFound = info.BlackTermsFound.Any(t => t.IsAuto);
            info.FiltersUsed.Add(FilterType.QuestionBodyBlackLQ);
            info.Accuracy /= filtersUsed;
            info.Accuracy /= GlobalInfo.BlackFilters[FilterType.QuestionBodyBlackLQ].HighestScore;
            info.Accuracy *= 100;
            info.Type = PostType.LowQuality;

            return true;
        }

        public static bool IsOffensive(Question post, ref QuestionAnalysis info)
        {
            if (post.PopulateExtraDataFailed) { return false; }

            var filtersUsed = 0;

            // Loop over blacklist.

            for (var i = 0; i < GlobalInfo.BlackFilters[FilterType.QuestionBodyBlackOff].Terms.Count; i++)
            {
                var blackTerm = GlobalInfo.BlackFilters[FilterType.QuestionBodyBlackOff].Terms.ElementAt(i);

                if (blackTerm.Regex.IsMatch(post.Body))
                {
                    info.Accuracy += blackTerm.Score;
                    info.BlackTermsFound.Add(blackTerm);

                    blackTerm.CaughtCount++;
                    filtersUsed++;
                }
            }

            // Otherwise, if no blacklist terms were found, assume the post is clean.

            if (filtersUsed == 0) { return false; }

            // Loop over whitelist.

            foreach (var whiteTerm in GlobalInfo.WhiteFilters[FilterType.QuestionBodyWhiteOff].Terms.Where(t => t.Site == post.Site))
            {
                if (whiteTerm.Regex.IsMatch(post.Body))
                {
                    info.Accuracy -= whiteTerm.Score;
                    info.WhiteTermsFound.Add(whiteTerm);
                    info.FiltersUsed.Add(FilterType.QuestionBodyWhiteOff);

                    filtersUsed++;
                }
            }

            info.AutoTermsFound = info.BlackTermsFound.Any(t => t.IsAuto);
            info.FiltersUsed.Add(FilterType.QuestionBodyBlackOff);
            info.Accuracy /= filtersUsed;
            info.Accuracy /= GlobalInfo.BlackFilters[FilterType.QuestionBodyBlackOff].HighestScore;
            info.Accuracy *= 100;
            info.Type = PostType.Offensive;

            return true;
        }
    }
}
