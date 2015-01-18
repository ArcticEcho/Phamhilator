using System.Collections.Generic;
using System.Linq;



namespace Phamhilator
{
    public static class QuestionAnalyser
    {
        public static bool IsSpam(Question post, out QuestionAnalysis info)
        {
            if (GlobalInfo.FullScanEnabled)
            {
                var bodyResults = SpamCheckBody(post);

                if (bodyResults != null && bodyResults.Accuracy >= GlobalInfo.AccuracyThreshold)
                {
                    info = bodyResults;

                    return true;
                }
            }

            var titleResults = SpamCheckTitle(post);

            if (titleResults != null && titleResults.Accuracy >= GlobalInfo.AccuracyThreshold)
            {
                info = titleResults;

                return true;
            }

            info = null;

            return false;
        }

        public static bool IsLowQuality(Question post, out QuestionAnalysis info)
        {
            if (GlobalInfo.FullScanEnabled)
            {
                var bodyResults = LQCheckBody(post);

                if (bodyResults != null && bodyResults.Accuracy >= GlobalInfo.AccuracyThreshold)
                {
                    info = bodyResults;

                    return true;
                }
            }

            var titleResults = LQCheckTitle(post);

            if (titleResults != null && titleResults.Accuracy >= GlobalInfo.AccuracyThreshold)
            {
                info = titleResults;

                return true;
            }

            info = null;

            return false;
        }

        public static bool IsOffensive(Question post, out QuestionAnalysis info)
        {
            if (GlobalInfo.FullScanEnabled)
            {
                var bodyResults = OffensiveCheckBody(post);

                if (bodyResults != null && bodyResults.Accuracy >= GlobalInfo.AccuracyThreshold)
                {
                    info = bodyResults;

                    return true;
                }
            }

            var titleResults = OffensiveCheckTitle(post);

            if (titleResults != null && titleResults.Accuracy >= GlobalInfo.AccuracyThreshold)
            {
                info = titleResults;

                return true;
            }

            info = null;

            return false;
        }

        public static bool IsBadUsername(Question post, out QuestionAnalysis info)
        {
            var termsFound = 0;
            info = new QuestionAnalysis();

            // Loop over blacklist.

            for (var i = 0; i < GlobalInfo.BlackFilters[FilterType.QuestionTitleBlackName].Terms.Count; i++)
            {
                var blackTerm = GlobalInfo.BlackFilters[FilterType.QuestionTitleBlackName].Terms.ElementAt(i);

                if (blackTerm.Regex.IsMatch(post.AuthorName))
                {
                    info.Accuracy += blackTerm.Score;
                    info.BlackTermsFound.Add(blackTerm);

                    blackTerm.CaughtCount++;
                    termsFound++;
                }
            }

            // Otherwise, if no blacklist terms were found, assume the post is clean.

            if (termsFound == 0) { return false; }

            // Loop over whitelist.

            foreach (var whiteTerm in GlobalInfo.WhiteFilters[FilterType.QuestionTitleWhiteName].Terms.Where(t => t.Site == post.Site))
            {
                if (whiteTerm.Regex.IsMatch(post.AuthorName))
                {
                    info.Accuracy -= whiteTerm.Score;
                    info.WhiteTermsFound.Add(whiteTerm);
                    info.FiltersUsed.Add(FilterType.QuestionTitleWhiteName);

                    termsFound++;
                }
            }

            info.AutoTermsFound = info.BlackTermsFound.Any(t => t.IsAuto);
            info.FiltersUsed.Add(FilterType.QuestionTitleBlackName);
            info.Accuracy /= termsFound;
            info.Accuracy /= GlobalInfo.BlackFilters[FilterType.QuestionTitleBlackName].HighestScore;
            info.Accuracy *= 100;
            info.Type = PostType.BadUsername;

            return true;
        }

        public static Dictionary<string, string> IsBadTagUsed(Question post, out QuestionAnalysis info)
        {
            var tags = new Dictionary<string, string>();
            info = new QuestionAnalysis();

            if (!GlobalInfo.BadTagDefinitions.BadTags.Keys.Contains(post.Site)) { return tags; }

            foreach (var tag in post.Tags)
            {
                if (GlobalInfo.BadTagDefinitions.BadTags[post.Site].ContainsKey(tag.ToLowerInvariant()))
                {
                    tags.Add(tag, GlobalInfo.BadTagDefinitions.BadTags[post.Site][tag]);
                }
            }

            if (tags.Count != 0)
            {
                info.Accuracy = 100;
                info.Type = PostType.BadTagUsed;
            }

            return tags;
        }



        private static QuestionAnalysis SpamCheckTitle(Question post)
        {
            var termsFound = 0;
            var info = new QuestionAnalysis();

            // Loop over blacklist.

            for (var i = 0; i < GlobalInfo.BlackFilters[FilterType.QuestionTitleBlackSpam].Terms.Count; i++)
            {
                var blackTerm = GlobalInfo.BlackFilters[FilterType.QuestionTitleBlackSpam].Terms.ElementAt(i);

                if (blackTerm.Regex.IsMatch(post.Title))
                {
                    info.Accuracy += blackTerm.Score;
                    info.BlackTermsFound.Add(blackTerm);

                    blackTerm.CaughtCount++;
                    termsFound++;
                }
            }

            // Otherwise, if no blacklist terms were found, assume the post is clean.

            if (termsFound == 0) { return null; }

            // Loop over whitelist.

            foreach (var whiteTerm in GlobalInfo.WhiteFilters[FilterType.QuestionTitleWhiteSpam].Terms.Where(t => t.Site == post.Site))
            {
                if (whiteTerm.Regex.IsMatch(post.Title))
                {
                    info.Accuracy -= whiteTerm.Score;
                    info.WhiteTermsFound.Add(whiteTerm);
                    info.FiltersUsed.Add(FilterType.QuestionTitleWhiteSpam);

                    termsFound++;
                }
            }

            info.AutoTermsFound = info.BlackTermsFound.Any(t => t.IsAuto);
            info.FiltersUsed.Add(FilterType.QuestionTitleBlackSpam);
            info.Accuracy /= termsFound;
            info.Accuracy /= GlobalInfo.BlackFilters[FilterType.QuestionTitleBlackSpam].HighestScore;
            info.Accuracy *= 100;
            info.Type = PostType.Spam;

            return info;
        }

        private static QuestionAnalysis SpamCheckBody(Question post)
        {
            if (post.PopulateExtraDataFailed) { return null; }

            var termsFound = 0;
            var info = new QuestionAnalysis();

            // Loop over blacklist.

            for (var i = 0; i < GlobalInfo.BlackFilters[FilterType.QuestionBodyBlackSpam].Terms.Count; i++)
            {
                var blackTerm = GlobalInfo.BlackFilters[FilterType.QuestionBodyBlackSpam].Terms.ElementAt(i);

                if (blackTerm.Regex.IsMatch(post.Body))
                {
                    info.Accuracy += blackTerm.Score;
                    info.BlackTermsFound.Add(blackTerm);

                    blackTerm.CaughtCount++;
                    termsFound++;
                }
            }

            // Otherwise, if no blacklist terms were found, assume the post is clean.

            if (termsFound == 0) { return null; }

            // Loop over whitelist.

            foreach (var whiteTerm in GlobalInfo.WhiteFilters[FilterType.QuestionBodyWhiteSpam].Terms.Where(t => t.Site == post.Site))
            {
                if (whiteTerm.Regex.IsMatch(post.Body))
                {
                    info.Accuracy -= whiteTerm.Score;
                    info.WhiteTermsFound.Add(whiteTerm);
                    info.FiltersUsed.Add(FilterType.QuestionBodyWhiteSpam);

                    termsFound++;
                }
            }

            info.AutoTermsFound = info.BlackTermsFound.Any(t => t.IsAuto);
            info.FiltersUsed.Add(FilterType.QuestionBodyBlackSpam);
            info.Accuracy /= termsFound;
            info.Accuracy /= GlobalInfo.BlackFilters[FilterType.QuestionBodyBlackSpam].HighestScore;
            info.Accuracy *= 100;
            info.Type = PostType.Spam;

            return info;
        }

        private static QuestionAnalysis LQCheckTitle(Question post)
        {
            var termsFound = 0;
            var info = new QuestionAnalysis();

            // Loop over blacklist.

            for (var i = 0; i < GlobalInfo.BlackFilters[FilterType.QuestionTitleBlackLQ].Terms.Count; i++)
            {
                var blackTerm = GlobalInfo.BlackFilters[FilterType.QuestionTitleBlackLQ].Terms.ElementAt(i);

                if (blackTerm.Regex.IsMatch(post.Title))
                {
                    info.Accuracy += blackTerm.Score;
                    info.BlackTermsFound.Add(blackTerm);

                    blackTerm.CaughtCount++;
                    termsFound++;
                }
            }

            // Otherwise, if no blacklist terms were found, assume the post is clean.

            if (termsFound == 0) { return null; }

            // Loop over whitelist.

            foreach (var whiteTerm in GlobalInfo.WhiteFilters[FilterType.QuestionTitleWhiteLQ].Terms.Where(t => t.Site == post.Site))
            {
                if (whiteTerm.Regex.IsMatch(post.Title))
                {
                    info.Accuracy -= whiteTerm.Score;
                    info.WhiteTermsFound.Add(whiteTerm);
                    info.FiltersUsed.Add(FilterType.QuestionTitleWhiteLQ);

                    termsFound++;
                }
            }

            info.AutoTermsFound = info.BlackTermsFound.Any(t => t.IsAuto);
            info.FiltersUsed.Add(FilterType.QuestionTitleBlackLQ);
            info.Accuracy /= termsFound;
            info.Accuracy /= GlobalInfo.BlackFilters[FilterType.QuestionTitleBlackLQ].HighestScore;
            info.Accuracy *= 100;
            info.Type = PostType.LowQuality;

            return info;
        }

        private static QuestionAnalysis LQCheckBody(Question post)
        {
            if (post.PopulateExtraDataFailed) { return null; }

            var termsFound = 0;
            var info = new QuestionAnalysis();

            // Loop over blacklist.

            for (var i = 0; i < GlobalInfo.BlackFilters[FilterType.QuestionBodyBlackLQ].Terms.Count; i++)
            {
                var blackTerm = GlobalInfo.BlackFilters[FilterType.QuestionBodyBlackLQ].Terms.ElementAt(i);

                if (blackTerm.Regex.IsMatch(post.Body))
                {
                    info.Accuracy += blackTerm.Score;
                    info.BlackTermsFound.Add(blackTerm);

                    blackTerm.CaughtCount++;
                    termsFound++;
                }
            }

            // Otherwise, if no blacklist terms were found, assume the post is clean.

            if (termsFound == 0) { return null; }

            // Loop over whitelist.

            foreach (var whiteTerm in GlobalInfo.WhiteFilters[FilterType.QuestionBodyWhiteLQ].Terms.Where(t => t.Site == post.Site))
            {
                if (whiteTerm.Regex.IsMatch(post.Body))
                {
                    info.Accuracy -= whiteTerm.Score;
                    info.WhiteTermsFound.Add(whiteTerm);
                    info.FiltersUsed.Add(FilterType.QuestionBodyWhiteLQ);

                    termsFound++;
                }
            }

            info.AutoTermsFound = info.BlackTermsFound.Any(t => t.IsAuto);
            info.FiltersUsed.Add(FilterType.QuestionBodyBlackLQ);
            info.Accuracy /= termsFound;
            info.Accuracy /= GlobalInfo.BlackFilters[FilterType.QuestionBodyBlackLQ].HighestScore;
            info.Accuracy *= 100;
            info.Type = PostType.LowQuality;

            return info;
        }

        private static QuestionAnalysis OffensiveCheckTitle(Question post)
        {
            var termsFound = 0;
            var info = new QuestionAnalysis();

            // Loop over blacklist.

            for (var i = 0; i < GlobalInfo.BlackFilters[FilterType.QuestionTitleBlackOff].Terms.Count; i++)
            {
                var blackTerm = GlobalInfo.BlackFilters[FilterType.QuestionTitleBlackOff].Terms.ElementAt(i);

                if (blackTerm.Regex.IsMatch(post.Title))
                {
                    info.Accuracy += blackTerm.Score;
                    info.BlackTermsFound.Add(blackTerm);

                    blackTerm.CaughtCount++;
                    termsFound++;
                }
            }

            // Otherwise, if no blacklist terms were found, assume the post is clean.

            if (termsFound == 0) { return null; }

            // Loop over whitelist.

            foreach (var whiteTerm in GlobalInfo.WhiteFilters[FilterType.QuestionTitleWhiteOff].Terms.Where(t => t.Site == post.Site))
            {
                if (whiteTerm.Regex.IsMatch(post.Title))
                {
                    info.Accuracy -= whiteTerm.Score;
                    info.WhiteTermsFound.Add(whiteTerm);
                    info.FiltersUsed.Add(FilterType.QuestionTitleWhiteOff);

                    termsFound++;
                }
            }

            info.AutoTermsFound = info.BlackTermsFound.Any(t => t.IsAuto);
            info.FiltersUsed.Add(FilterType.QuestionTitleBlackOff);
            info.Accuracy /= termsFound;
            info.Accuracy /= GlobalInfo.BlackFilters[FilterType.QuestionTitleBlackOff].HighestScore;
            info.Accuracy *= 100;
            info.Type = PostType.Offensive;

            return info;
        }

        private static QuestionAnalysis OffensiveCheckBody(Question post)
        {
            if (post.PopulateExtraDataFailed) { return null; }

            var termsFound = 0;
            var info = new QuestionAnalysis();

            // Loop over blacklist.

            for (var i = 0; i < GlobalInfo.BlackFilters[FilterType.QuestionBodyBlackOff].Terms.Count; i++)
            {
                var blackTerm = GlobalInfo.BlackFilters[FilterType.QuestionBodyBlackOff].Terms.ElementAt(i);

                if (blackTerm.Regex.IsMatch(post.Body))
                {
                    info.Accuracy += blackTerm.Score;
                    info.BlackTermsFound.Add(blackTerm);

                    blackTerm.CaughtCount++;
                    termsFound++;
                }
            }

            // Otherwise, if no blacklist terms were found, assume the post is clean.

            if (termsFound == 0) { return null; }

            // Loop over whitelist.

            foreach (var whiteTerm in GlobalInfo.WhiteFilters[FilterType.QuestionBodyWhiteOff].Terms.Where(t => t.Site == post.Site))
            {
                if (whiteTerm.Regex.IsMatch(post.Body))
                {
                    info.Accuracy -= whiteTerm.Score;
                    info.WhiteTermsFound.Add(whiteTerm);
                    info.FiltersUsed.Add(FilterType.QuestionBodyWhiteOff);

                    termsFound++;
                }
            }

            info.AutoTermsFound = info.BlackTermsFound.Any(t => t.IsAuto);
            info.FiltersUsed.Add(FilterType.QuestionBodyBlackOff);
            info.Accuracy /= termsFound;
            info.Accuracy /= GlobalInfo.BlackFilters[FilterType.QuestionBodyBlackOff].HighestScore;
            info.Accuracy *= 100;
            info.Type = PostType.Offensive;

            return null;
        }
    }
}
