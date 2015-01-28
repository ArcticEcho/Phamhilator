//using System.Collections.Generic;
//using System.Linq;



//namespace Phamhilator
//{
//    public static class AnswerAnalyser
//    {
//        public static AnswerAnalysis AnaylseAnswer(Answer post, BlackFilter blackTerms, WhiteFilter whiteTerms, FilterClass filterType, float accuracyThreshold)
//        {
//            var termsFound = 0;
//            var info = new AnswerAnalysis();

//            foreach (var blackTerm in blackTerms.Terms)
//            {
//                if (blackTerm.Regex.IsMatch(post.Body))
//                {
//                    info.Accuracy += blackTerm.Score;
//                    info.BlackTermsFound.Add(blackTerm);

//                    blackTerm.CaughtCount++;
//                    termsFound++;
//                }
//            }

//            // If no black listed terms were found, assume the post is clean.
//            if (termsFound == 0) { return null; }

//            foreach (var whiteTerm in blackTerms.Terms.Where(t => t.Site == post.Site))
//            {
//                if (whiteTerm.Regex.IsMatch(post.Body))
//                {
//                    info.Accuracy -= whiteTerm.Score;
//                    info.WhiteTermsFound.Add(whiteTerm);
//                    info.FiltersUsed.Add(filterType, FilterType.White);

//                    termsFound++;
//                }
//            }

//            info.AutoTermsFound = info.BlackTermsFound.Any(t => t.IsAuto);
//            info.FiltersUsed.Add(filterType, FilterType.Black);
//            info.Accuracy /= termsFound;
//            info.Accuracy /= blackTerms.HighestScore;
//            info.Accuracy *= 100;
//            info.Type = info.Accuracy >= accuracyThreshold ? filterType.ToPostType() : PostType.Clean;

//            return info;
//        }

//        //public static bool IsSpam(Answer post, out AnswerAnalysis info, BlackFilter blackTerms, WhiteFilter whiteTerms, float accuracyThreshold)
//        //{
//        //    var termsFound = 0;
//        //    info = new AnswerAnalysis();

//        //    foreach (var blackTerm in blackTerms.Terms)
//        //    {
//        //        if (blackTerm.Regex.IsMatch(post.Body))
//        //        {
//        //            info.Accuracy += blackTerm.Score;
//        //            info.BlackTermsFound.Add(blackTerm);

//        //            blackTerm.CaughtCount++;
//        //            termsFound++;
//        //        }
//        //    }

//        //    // If no black listed terms were found, assume the post is clean.
//        //    if (termsFound == 0) { return false; }

//        //    foreach (var whiteTerm in whiteTerms.Terms.Where(t => t.Site == post.Site))
//        //    {
//        //        if (whiteTerm.Regex.IsMatch(post.Body))
//        //        {
//        //            info.Accuracy -= whiteTerm.Score;
//        //            info.WhiteTermsFound.Add(whiteTerm);
//        //            info.FiltersUsed.Add(FilterType.AnswerWhiteSpam);

//        //            termsFound++;
//        //        }
//        //    }

//        //    info.AutoTermsFound = info.BlackTermsFound.Any(t => t.IsAuto);
//        //    info.FiltersUsed.Add(FilterType.AnswerBlackSpam);
//        //    info.Accuracy /= termsFound;
//        //    info.Accuracy /= blackTerms.HighestScore;
//        //    info.Accuracy *= 100;
//        //    info.Type = PostType.Spam;

//        //    return info.Accuracy >= accuracyThreshold;
//        //}

//        //public static bool IsLowQuality(Answer post, out AnswerAnalysis info, BlackFilter blackTerms, WhiteFilter whiteTerms, float accuracyThreshold)
//        //{
//        //    var termsFound = 0;
//        //    info = new AnswerAnalysis();

//        //    for (var i = 0; i < GlobalInfo.BlackFilters[FilterType.AnswerBlackLQ].Terms.Count; i++)
//        //    {
//        //        var blackTerm = GlobalInfo.BlackFilters[FilterType.AnswerBlackLQ].Terms.ElementAt(i);

//        //        if (blackTerm.Regex.IsMatch(post.Body))
//        //        {
//        //            info.Accuracy += blackTerm.Score;
//        //            info.BlackTermsFound.Add(blackTerm);

//        //            blackTerm.CaughtCount++;
//        //            termsFound++;
//        //        }
//        //    }

//        //    // Otherwise, if no blacklist terms were found, assume the post is clean.

//        //    if (termsFound == 0) { return false; }

//        //    // Loop over whitelist.

//        //    foreach (var whiteTerm in GlobalInfo.WhiteFilters[FilterType.AnswerWhiteLQ].Terms.Where(t => t.Site == post.Site))
//        //    {
//        //        if (whiteTerm.Regex.IsMatch(post.Body))
//        //        {
//        //            info.Accuracy -= whiteTerm.Score;
//        //            info.WhiteTermsFound.Add(whiteTerm);
//        //            info.FiltersUsed.Add(FilterType.AnswerWhiteLQ);

//        //            termsFound++;
//        //        }
//        //    }

//        //    info.AutoTermsFound = info.BlackTermsFound.Any(t => t.IsAuto);
//        //    info.FiltersUsed.Add(FilterType.AnswerBlackLQ);
//        //    info.Accuracy /= termsFound;
//        //    info.Accuracy /= GlobalInfo.BlackFilters[FilterType.AnswerBlackLQ].HighestScore;
//        //    info.Accuracy *= 100;
//        //    info.Type = PostType.LowQuality;

//        //    return info.Accuracy >= GlobalInfo.AccuracyThreshold;
//        //}

//        //public static bool IsOffensive(Answer post, out AnswerAnalysis info, BlackFilter blackTerms, WhiteFilter whiteTerms, float accuracyThreshold)
//        //{
//        //    var termsFound = 0;
//        //    info = new AnswerAnalysis();

//        //    // Loop over blacklist.

//        //    for (var i = 0; i < GlobalInfo.BlackFilters[FilterType.AnswerBlackOff].Terms.Count; i++)
//        //    {
//        //        var blackTerm = GlobalInfo.BlackFilters[FilterType.AnswerBlackOff].Terms.ElementAt(i);

//        //        if (blackTerm.Regex.IsMatch(post.Body))
//        //        {
//        //            info.Accuracy += blackTerm.Score;
//        //            info.BlackTermsFound.Add(blackTerm);

//        //            blackTerm.CaughtCount++;
//        //            termsFound++;
//        //        }
//        //    }

//        //    // Otherwise, if no blacklist terms were found, assume the post is clean.

//        //    if (termsFound == 0) { return false; }

//        //    // Loop over whitelist.

//        //    foreach (var whiteTerm in GlobalInfo.WhiteFilters[FilterType.AnswerWhiteOff].Terms.Where(t => t.Site == post.Site))
//        //    {
//        //        if (whiteTerm.Regex.IsMatch(post.Body))
//        //        {
//        //            info.Accuracy -= whiteTerm.Score;
//        //            info.WhiteTermsFound.Add(whiteTerm);
//        //            info.FiltersUsed.Add(FilterType.AnswerWhiteOff);

//        //            termsFound++;
//        //        }
//        //    }

//        //    info.AutoTermsFound = info.BlackTermsFound.Any(t => t.IsAuto);
//        //    info.FiltersUsed.Add(FilterType.AnswerBlackOff);
//        //    info.Accuracy /= termsFound;
//        //    info.Accuracy /= GlobalInfo.BlackFilters[FilterType.AnswerBlackOff].HighestScore;
//        //    info.Accuracy *= 100;
//        //    info.Type = PostType.Offensive;

//        //    return info.Accuracy >= GlobalInfo.AccuracyThreshold;
//        //}

//        //public static bool IsBadUsername(Answer post, out AnswerAnalysis info, BlackFilter blackTerms, WhiteFilter whiteTerms, float accuracyThreshold)
//        //{
//        //    var termsFound = 0;
//        //    info = new AnswerAnalysis();

//        //    // Loop over blacklist.

//        //    for (var i = 0; i < GlobalInfo.BlackFilters[FilterType.AnswerBlackName].Terms.Count; i++)
//        //    {
//        //        var blackTerm = GlobalInfo.BlackFilters[FilterType.AnswerBlackName].Terms.ElementAt(i);

//        //        if (blackTerm.Regex.IsMatch(post.AuthorName))
//        //        {
//        //            info.Accuracy += blackTerm.Score;
//        //            info.BlackTermsFound.Add(blackTerm);

//        //            blackTerm.CaughtCount++;
//        //            termsFound++;
//        //        }
//        //    }

//        //    // Otherwise, if no blacklist terms were found, assume the post is clean.

//        //    if (termsFound == 0) { return false; }

//        //    // Loop over whitelist.

//        //    foreach (var whiteTerm in GlobalInfo.WhiteFilters[FilterType.AnswerWhiteName].Terms.Where(t => t.Site == post.Site))
//        //    {
//        //        if (whiteTerm.Regex.IsMatch(post.AuthorName))
//        //        {
//        //            info.Accuracy -= whiteTerm.Score;
//        //            info.WhiteTermsFound.Add(whiteTerm);
//        //            info.FiltersUsed.Add(FilterType.AnswerWhiteName);

//        //            termsFound++;
//        //        }
//        //    }

//        //    info.AutoTermsFound = info.BlackTermsFound.Any(t => t.IsAuto);
//        //    info.FiltersUsed.Add(FilterType.AnswerBlackName);
//        //    info.Accuracy /= termsFound;
//        //    info.Accuracy /= GlobalInfo.BlackFilters[FilterType.AnswerBlackName].HighestScore;
//        //    info.Accuracy *= 100;
//        //    info.Type = PostType.BadUsername;

//        //    return info.Accuracy >= GlobalInfo.AccuracyThreshold;
//        //}
//    }
//}
