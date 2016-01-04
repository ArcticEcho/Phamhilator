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
using System.Text.RegularExpressions;

namespace Phamhilator.NLP
{
    public static class StringTools
    {
        private static RegexOptions regOpts = RegexOptions.Compiled | RegexOptions.CultureInvariant;
        private static Regex multiWhitespace = new Regex(@"\s+", regOpts);
        private static Regex ieEg = new Regex(@"\b(i\.e|e\.g)\.?", regOpts);
        private static Regex inncorrectIs = new Regex(@"\bi\b", regOpts);
        private static Regex inncorrectAfterPeriod = new Regex(@"\.\s*?[a-z]", regOpts);



        /// <summary>
        /// Calculates a text's punctuation-chars to normal word-chars ratio.
        /// </summary>
        public static double PunctuationRatio(string text)
        {
            var punctCharCount = 0F;
            var wordCharCount = 0;

            foreach (var c in text)
            {
                if (char.IsPunctuation(c))
                {
                    punctCharCount++;
                }
                else if (char.IsLetterOrDigit(c))
                {
                    wordCharCount++;
                }
            }

            return punctCharCount / wordCharCount;
        }

        /// <summary>
        /// Estimates how well capitalised a post is.
        /// </summary>
        /// <returns>A number between 0 and 1.
        /// Where 1 indicates good capitalisation.</returns>
        public static double CapitalisationScore(string text)
        {
            throw new NotImplementedException();

            var t = multiWhitespace.Replace(text, " ");
            var badCapCount = 0D;

            badCapCount += inncorrectIs.Matches(t).Count;
            badCapCount += inncorrectAfterPeriod.Matches(ieEg.Replace(t, "")).Count;

            //TODO: Finish off implementation.

            return 1 - (badCapCount / t.Length);
        }
    }
}
