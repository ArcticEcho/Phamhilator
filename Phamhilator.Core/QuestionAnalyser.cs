/*
 * Phamhilator. A .Net based bot network catching spam/low quality posts for Stack Exchange.
 * Copyright © 2015, ArcticEcho.
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */





//using System.Collections.Generic;
//using System.Linq;

//namespace Phamhilator.Pham.Core
//{
//    public static class QuestionAnalyser
//    {
//        public static bool IsSpam(Question post, out QuestionAnalysis info)
//        {
//            if (Config.FullScanEnabled)
//            {
//                var bodyResults = SpamCheckBody(post);

//                if (bodyResults != null && bodyResults.Accuracy >= Config.AccuracyThreshold)
//                {
//                    info = bodyResults;

//                    return true;
//                }
//            }

//            var titleResults = SpamCheckTitle(post);

//            if (titleResults != null && titleResults.Accuracy >= Config.AccuracyThreshold)
//            {
//                info = titleResults;

//                return true;
//            }

//            info = null;

//            return false;
//        }

//        public static bool IsLowQuality(Question post, out QuestionAnalysis info)
//        {
//            if (Config.FullScanEnabled)
//            {
//                var bodyResults = LQCheckBody(post);

//                if (bodyResults != null && bodyResults.Accuracy >= Config.AccuracyThreshold)
//                {
//                    info = bodyResults;

//                    return true;
//                }
//            }

//            var titleResults = LQCheckTitle(post);

//            if (titleResults != null && titleResults.Accuracy >= Config.AccuracyThreshold)
//            {
//                info = titleResults;

//                return true;
//            }

//            info = null;

//            return false;
//        }

//        public static bool IsOffensive(Question post, out QuestionAnalysis info)
//        {
//            if (GlobalInfo.FullScanEnabled)
//            {
//                var bodyResults = OffensiveCheckBody(post);

//                if (bodyResults != null && bodyResults.Accuracy >= Config.AccuracyThreshold)
//                {
//                    info = bodyResults;

//                    return true;
//                }
//            }

//            var titleResults = OffensiveCheckTitle(post);

//            if (titleResults != null && titleResults.Accuracy >= Config.AccuracyThreshold)
//            {
//                info = titleResults;

//                return true;
//            }

//            info = null;

//            return false;
//        }

//        private static QuestionAnalysis AnalyseQuestion(Question post, BlackFilter blackTerms, WhiteFilter whiteTerms, FilterClass filterType)
//        {

//        }

//        public static bool IsBadUsername(Question post, out QuestionAnalysis info)
//        {
//            var termsFound = 0;
//            info = new QuestionAnalysis();

//            // Loop over blacklist.

//            for (var i = 0; i < Config.BlackFilters[FilterClass.QuestionTitleName].Terms.Count; i++)
//            {
//                var blackTerm = Config.BlackFilters[FilterClass.QuestionTitleName].Terms.ElementAt(i);

//                if (blackTerm.Regex.IsMatch(post.AuthorName))
//                {
//                    info.Accuracy += blackTerm.Score;
//                    info.BlackTermsFound.Add(blackTerm);

//                    blackTerm.CaughtCount++;
//                    termsFound++;
//                }
//            }

//            // Otherwise, if no blacklist terms were found, assume the post is clean.

//            if (termsFound == 0) { return false; }

//            // Loop over whitelist.

//            foreach (var whiteTerm in GlobalInfo.WhiteFilters[FilterClass.QuestionTitleWhiteName].Terms.Where(t => t.Site == post.Site))
//            {
//                if (whiteTerm.Regex.IsMatch(post.AuthorName))
//                {
//                    info.Accuracy -= whiteTerm.Score;
//                    info.WhiteTermsFound.Add(whiteTerm);
//                    info.FiltersUsed.Add(FilterClass.QuestionTitleWhiteName);

//                    termsFound++;
//                }
//            }

//            info.AutoTermsFound = info.BlackTermsFound.Any(t => t.IsAuto);
//            info.FiltersUsed.Add(FilterClass.QuestionTitleBlackName);
//            info.Accuracy /= termsFound;
//            info.Accuracy /= GlobalInfo.BlackFilters[FilterClass.QuestionTitleBlackName].HighestScore;
//            info.Accuracy *= 100;
//            info.Type = PostType.BadUsername;

//            return true;
//        }

//        public static Dictionary<string, string> IsBadTagUsed(Question post, out QuestionAnalysis info)
//        {
//            var tags = new Dictionary<string, string>();
//            info = new QuestionAnalysis();

//            if (!GlobalInfo.BadTagDefinitions.BadTags.Keys.Contains(post.Site)) { return tags; }

//            foreach (var tag in post.Tags)
//            {
//                if (GlobalInfo.BadTagDefinitions.BadTags[post.Site].ContainsKey(tag.ToLowerInvariant()))
//                {
//                    tags.Add(tag, GlobalInfo.BadTagDefinitions.BadTags[post.Site][tag]);
//                }
//            }

//            if (tags.Count != 0)
//            {
//                info.Accuracy = 100;
//                info.Type = PostType.BadTagUsed;
//            }

//            return tags;
//        }



//        private static QuestionAnalysis SpamCheckTitle(Question post)
//        {
//            var termsFound = 0;
//            var info = new QuestionAnalysis();

//            // Loop over blacklist.

//            for (var i = 0; i < GlobalInfo.BlackFilters[FilterClass.QuestionTitleBlackSpam].Terms.Count; i++)
//            {
//                var blackTerm = GlobalInfo.BlackFilters[FilterClass.QuestionTitleBlackSpam].Terms.ElementAt(i);

//                if (blackTerm.Regex.IsMatch(post.Title))
//                {
//                    info.Accuracy += blackTerm.Score;
//                    info.BlackTermsFound.Add(blackTerm);

//                    blackTerm.CaughtCount++;
//                    termsFound++;
//                }
//            }

//            // Otherwise, if no blacklist terms were found, assume the post is clean.

//            if (termsFound == 0) { return null; }

//            // Loop over whitelist.

//            foreach (var whiteTerm in GlobalInfo.WhiteFilters[FilterClass.QuestionTitleWhiteSpam].Terms.Where(t => t.Site == post.Site))
//            {
//                if (whiteTerm.Regex.IsMatch(post.Title))
//                {
//                    info.Accuracy -= whiteTerm.Score;
//                    info.WhiteTermsFound.Add(whiteTerm);
//                    info.FiltersUsed.Add(FilterClass.QuestionTitleWhiteSpam);

//                    termsFound++;
//                }
//            }

//            info.AutoTermsFound = info.BlackTermsFound.Any(t => t.IsAuto);
//            info.FiltersUsed.Add(FilterClass.QuestionTitleBlackSpam);
//            info.Accuracy /= termsFound;
//            info.Accuracy /= GlobalInfo.BlackFilters[FilterClass.QuestionTitleBlackSpam].HighestScore;
//            info.Accuracy *= 100;
//            info.Type = PostType.Spam;

//            return info;
//        }

//        private static QuestionAnalysis SpamCheckBody(Question post)
//        {
//            if (post.PopulateExtraDataFailed) { return null; }

//            var termsFound = 0;
//            var info = new QuestionAnalysis();

//            // Loop over blacklist.

//            for (var i = 0; i < GlobalInfo.BlackFilters[FilterClass.QuestionBodyBlackSpam].Terms.Count; i++)
//            {
//                var blackTerm = GlobalInfo.BlackFilters[FilterClass.QuestionBodyBlackSpam].Terms.ElementAt(i);

//                if (blackTerm.Regex.IsMatch(post.Body))
//                {
//                    info.Accuracy += blackTerm.Score;
//                    info.BlackTermsFound.Add(blackTerm);

//                    blackTerm.CaughtCount++;
//                    termsFound++;
//                }
//            }

//            // Otherwise, if no blacklist terms were found, assume the post is clean.

//            if (termsFound == 0) { return null; }

//            // Loop over whitelist.

//            foreach (var whiteTerm in GlobalInfo.WhiteFilters[FilterClass.QuestionBodyWhiteSpam].Terms.Where(t => t.Site == post.Site))
//            {
//                if (whiteTerm.Regex.IsMatch(post.Body))
//                {
//                    info.Accuracy -= whiteTerm.Score;
//                    info.WhiteTermsFound.Add(whiteTerm);
//                    info.FiltersUsed.Add(FilterClass.QuestionBodyWhiteSpam);

//                    termsFound++;
//                }
//            }

//            info.AutoTermsFound = info.BlackTermsFound.Any(t => t.IsAuto);
//            info.FiltersUsed.Add(FilterClass.QuestionBodyBlackSpam);
//            info.Accuracy /= termsFound;
//            info.Accuracy /= GlobalInfo.BlackFilters[FilterClass.QuestionBodyBlackSpam].HighestScore;
//            info.Accuracy *= 100;
//            info.Type = PostType.Spam;

//            return info;
//        }

//        private static QuestionAnalysis LQCheckTitle(Question post)
//        {
//            var termsFound = 0;
//            var info = new QuestionAnalysis();

//            // Loop over blacklist.

//            for (var i = 0; i < GlobalInfo.BlackFilters[FilterClass.QuestionTitleBlackLQ].Terms.Count; i++)
//            {
//                var blackTerm = GlobalInfo.BlackFilters[FilterClass.QuestionTitleBlackLQ].Terms.ElementAt(i);

//                if (blackTerm.Regex.IsMatch(post.Title))
//                {
//                    info.Accuracy += blackTerm.Score;
//                    info.BlackTermsFound.Add(blackTerm);

//                    blackTerm.CaughtCount++;
//                    termsFound++;
//                }
//            }

//            // Otherwise, if no blacklist terms were found, assume the post is clean.

//            if (termsFound == 0) { return null; }

//            // Loop over whitelist.

//            foreach (var whiteTerm in GlobalInfo.WhiteFilters[FilterClass.QuestionTitleWhiteLQ].Terms.Where(t => t.Site == post.Site))
//            {
//                if (whiteTerm.Regex.IsMatch(post.Title))
//                {
//                    info.Accuracy -= whiteTerm.Score;
//                    info.WhiteTermsFound.Add(whiteTerm);
//                    info.FiltersUsed.Add(FilterClass.QuestionTitleWhiteLQ);

//                    termsFound++;
//                }
//            }

//            info.AutoTermsFound = info.BlackTermsFound.Any(t => t.IsAuto);
//            info.FiltersUsed.Add(FilterClass.QuestionTitleBlackLQ);
//            info.Accuracy /= termsFound;
//            info.Accuracy /= GlobalInfo.BlackFilters[FilterClass.QuestionTitleBlackLQ].HighestScore;
//            info.Accuracy *= 100;
//            info.Type = PostType.LowQuality;

//            return info;
//        }

//        private static QuestionAnalysis LQCheckBody(Question post)
//        {
//            if (post.PopulateExtraDataFailed) { return null; }

//            var termsFound = 0;
//            var info = new QuestionAnalysis();

//            // Loop over blacklist.

//            for (var i = 0; i < GlobalInfo.BlackFilters[FilterClass.QuestionBodyBlackLQ].Terms.Count; i++)
//            {
//                var blackTerm = GlobalInfo.BlackFilters[FilterClass.QuestionBodyBlackLQ].Terms.ElementAt(i);

//                if (blackTerm.Regex.IsMatch(post.Body))
//                {
//                    info.Accuracy += blackTerm.Score;
//                    info.BlackTermsFound.Add(blackTerm);

//                    blackTerm.CaughtCount++;
//                    termsFound++;
//                }
//            }

//            // Otherwise, if no blacklist terms were found, assume the post is clean.

//            if (termsFound == 0) { return null; }

//            // Loop over whitelist.

//            foreach (var whiteTerm in GlobalInfo.WhiteFilters[FilterClass.QuestionBodyWhiteLQ].Terms.Where(t => t.Site == post.Site))
//            {
//                if (whiteTerm.Regex.IsMatch(post.Body))
//                {
//                    info.Accuracy -= whiteTerm.Score;
//                    info.WhiteTermsFound.Add(whiteTerm);
//                    info.FiltersUsed.Add(FilterClass.QuestionBodyWhiteLQ);

//                    termsFound++;
//                }
//            }

//            info.AutoTermsFound = info.BlackTermsFound.Any(t => t.IsAuto);
//            info.FiltersUsed.Add(FilterClass.QuestionBodyBlackLQ);
//            info.Accuracy /= termsFound;
//            info.Accuracy /= GlobalInfo.BlackFilters[FilterClass.QuestionBodyBlackLQ].HighestScore;
//            info.Accuracy *= 100;
//            info.Type = PostType.LowQuality;

//            return info;
//        }

//        private static QuestionAnalysis OffensiveCheckTitle(Question post)
//        {
//            var termsFound = 0;
//            var info = new QuestionAnalysis();

//            // Loop over blacklist.

//            for (var i = 0; i < GlobalInfo.BlackFilters[FilterClass.QuestionTitleBlackOff].Terms.Count; i++)
//            {
//                var blackTerm = GlobalInfo.BlackFilters[FilterClass.QuestionTitleBlackOff].Terms.ElementAt(i);

//                if (blackTerm.Regex.IsMatch(post.Title))
//                {
//                    info.Accuracy += blackTerm.Score;
//                    info.BlackTermsFound.Add(blackTerm);

//                    blackTerm.CaughtCount++;
//                    termsFound++;
//                }
//            }

//            // Otherwise, if no blacklist terms were found, assume the post is clean.

//            if (termsFound == 0) { return null; }

//            // Loop over whitelist.

//            foreach (var whiteTerm in GlobalInfo.WhiteFilters[FilterClass.QuestionTitleWhiteOff].Terms.Where(t => t.Site == post.Site))
//            {
//                if (whiteTerm.Regex.IsMatch(post.Title))
//                {
//                    info.Accuracy -= whiteTerm.Score;
//                    info.WhiteTermsFound.Add(whiteTerm);
//                    info.FiltersUsed.Add(FilterClass.QuestionTitleWhiteOff);

//                    termsFound++;
//                }
//            }

//            info.AutoTermsFound = info.BlackTermsFound.Any(t => t.IsAuto);
//            info.FiltersUsed.Add(FilterClass.QuestionTitleBlackOff);
//            info.Accuracy /= termsFound;
//            info.Accuracy /= GlobalInfo.BlackFilters[FilterClass.QuestionTitleBlackOff].HighestScore;
//            info.Accuracy *= 100;
//            info.Type = PostType.Offensive;

//            return info;
//        }

//        private static QuestionAnalysis OffensiveCheckBody(Question post)
//        {
//            if (post.PopulateExtraDataFailed) { return null; }

//            var termsFound = 0;
//            var info = new QuestionAnalysis();

//            // Loop over blacklist.

//            for (var i = 0; i < GlobalInfo.BlackFilters[FilterClass.QuestionBodyBlackOff].Terms.Count; i++)
//            {
//                var blackTerm = GlobalInfo.BlackFilters[FilterClass.QuestionBodyBlackOff].Terms.ElementAt(i);

//                if (blackTerm.Regex.IsMatch(post.Body))
//                {
//                    info.Accuracy += blackTerm.Score;
//                    info.BlackTermsFound.Add(blackTerm);

//                    blackTerm.CaughtCount++;
//                    termsFound++;
//                }
//            }

//            // Otherwise, if no blacklist terms were found, assume the post is clean.

//            if (termsFound == 0) { return null; }

//            // Loop over whitelist.

//            foreach (var whiteTerm in GlobalInfo.WhiteFilters[FilterClass.QuestionBodyWhiteOff].Terms.Where(t => t.Site == post.Site))
//            {
//                if (whiteTerm.Regex.IsMatch(post.Body))
//                {
//                    info.Accuracy -= whiteTerm.Score;
//                    info.WhiteTermsFound.Add(whiteTerm);
//                    info.FiltersUsed.Add(FilterClass.QuestionBodyWhiteOff);

//                    termsFound++;
//                }
//            }

//            info.AutoTermsFound = info.BlackTermsFound.Any(t => t.IsAuto);
//            info.FiltersUsed.Add(FilterClass.QuestionBodyBlackOff);
//            info.Accuracy /= termsFound;
//            info.Accuracy /= GlobalInfo.BlackFilters[FilterClass.QuestionBodyBlackOff].HighestScore;
//            info.Accuracy *= 100;
//            info.Type = PostType.Offensive;

//            return null;
//        }
//    }
//}
