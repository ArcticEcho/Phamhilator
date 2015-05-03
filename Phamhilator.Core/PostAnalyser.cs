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





using System;
using System.Collections.Generic;
using System.Linq;



namespace Phamhilator.Core
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

        private static AnswerAnalysis AnaylsePost(Post post, BlackFilter blackTerms, WhiteFilter whiteTerms, FilterClass filterClass)
        {
            var info = new AnswerAnalysis();

            for (var i = 0; i < blackTerms.Terms.Count; i++)
            {
                var blackTerm = blackTerms.Terms.ElementAt(i);

                if (filterClass.IsQuestionTitle() && !filterClass.IsName())
                {
                    info = BlackTermCheckString(blackTerm, post.Title, info);
                }
                else if (filterClass.IsName())
                {
                    info = BlackTermCheckString(blackTerm, post.AuthorName, info);
                }
                else
                {
                    info = BlackTermCheckString(blackTerm, post.Body, info);
                }
            }

            // If no black listed terms were found, assume the post is clean.
            if (info.BlackTermsFound.Count == 0) { return null; }

            for (var i = 0; i < whiteTerms.Terms.Count; i++)
            {
                var whiteTerm = whiteTerms.Terms.ElementAt(i);

                if (whiteTerm.Site != post.Site) { continue; }

                if (filterClass.IsQuestionTitle() && !filterClass.IsName())
                {
                    info = WhiteTermCheckString(whiteTerm, post.Title, info);
                }
                else if (filterClass.IsName())
                {
                    info = WhiteTermCheckString(whiteTerm, post.AuthorName, info);
                }
                else
                {
                    info = WhiteTermCheckString(whiteTerm, post.Body, info);
                }
            }

            if (info.WhiteTermsFound.Count != 0)
            {
                info.FiltersUsed.Add(new FilterConfig(filterClass, FilterType.White));
            }

            info.AutoTermsFound = info.BlackTermsFound.Any(t => t.IsAuto);
            info.FiltersUsed.Add(new FilterConfig(filterClass, FilterType.Black));
            info.Accuracy /= (info.BlackTermsFound.Count + info.WhiteTermsFound.Count);
            info.Accuracy /= blackTerms.HighestScore;
            info.Accuracy *= 100;
            info.Type = info.Accuracy >= Config.AccuracyThreshold ? filterClass.ToPostType() : PostType.Clean;

            return info;
        }

        private static AnswerAnalysis BlackTermCheckString(Term blackTerm, string postString, AnswerAnalysis info)
        {
            if (!String.IsNullOrEmpty(postString) && blackTerm.Regex.IsMatch(postString))
            {
                info.Accuracy += blackTerm.Score;
                info.BlackTermsFound.Add(blackTerm);

                blackTerm.CaughtCount++;
            }

            return info;
        }

        private static AnswerAnalysis WhiteTermCheckString(Term whiteTerm, string postString, AnswerAnalysis info)
        {
            if (!String.IsNullOrEmpty(postString) && whiteTerm.Regex.IsMatch(postString))
            {
                info.Accuracy -= whiteTerm.Score;
                info.WhiteTermsFound.Add(whiteTerm);
            }

            return info;
        }
    }
}
