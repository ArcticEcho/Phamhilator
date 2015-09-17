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
using Phamhilator.Yam.Core;

namespace Phamhilator.Pham.UI
{
    public static class GenericLQClassifier
    {
        public static KeyValuePair<string, double> ClassifyAnswer(string[] model, Answer post)
        {
            // Link-only
            if (model.Length < 4 && !model.ContainsCodeBlockTag() &&
                !model.ContainsBlockQuoteTag() && !model.ContainsInlineCodeTag() &&
                !model.ContainsPictureTag() && model.ContainsLinkTag())
            {
                return new KeyValuePair<string, double>("Link-only", 1);
            }

            // Code-only (depending on accuracy, we may need to filter by code block size too).
            if (!post.IsAccepted && model.Length < 4 && !model.ContainsLinkTag() &&
                !model.ContainsBlockQuoteTag() && !model.ContainsInlineCodeTag() &&
                !model.ContainsPictureTag() && post.Score < 1 && model.Any(t => t == "•CB-L•"))
            {
                return new KeyValuePair<string, double>("Code-only", 1);
            }

            var genScore = ClassifyPost(model, post, 60, 10);
            genScore *= post.IsAccepted ? 0.8 : 1;

            return new KeyValuePair<string, double>("General LQ", genScore);
        }

        public static KeyValuePair<string, double> ClassifyQuestion(string[] model, Question post)
        {
            // Code-only (depending on accuracy, we may need to filter by code block size too).
            if (model.Length < 4 && !model.ContainsLinkTag() &&
                !model.ContainsBlockQuoteTag() && !model.ContainsInlineCodeTag() &&
                !model.ContainsPictureTag() && post.Score < 6 && model.Any(t => t == "•CB-L•"))
            {
                return new KeyValuePair<string, double>("Code dump", 1);
            }

            var genScore = ClassifyPost(model, post, 30, 5);
            genScore *= post.IsClosed ? 1.2 : 1;

            return new KeyValuePair<string, double>("General LQ", genScore);
        }



        private static double ClassifyPost(string[] model, Post post, int rotPeriodDays, double voteDecayMax)
        {
            var modLen = model.Sum(t => t.Length);
            var lexDensScore = ((1D / modLen) * Math.Log10(post.Body.Length)) * 3 * (2 / 3D);
            lexDensScore += (1 - ((double)modLen / post.Body.Length)) * (1 / 3D);
            lexDensScore *= model.ContainsBlockQuoteTag() ? 0.87 : 1;
            lexDensScore *= model.ContainsCodeBlockTag() ? 0.87 : 1;
            lexDensScore *= model.ContainsInlineCodeTag() ? 0.87 : 1;
            lexDensScore *= model.ContainsLinkTag() ? 0.87 : 1;
            lexDensScore *= model.ContainsPictureTag() ? 0.87 : 1;
            lexDensScore *= 1 - Math.Min((DateTime.UtcNow - post.CreationDate).TotalDays / rotPeriodDays, 0.25);
            lexDensScore *= 1 - Math.Min(post.Score / voteDecayMax, 0.2);

            return lexDensScore;
        }
    }
}
