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
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace Phamhilator.NLP
{
    public static class StringTools
    {
        private static Regex multiWhiteSpace = new Regex(@"\s{2,}", RegexOptions.Compiled | RegexOptions.CultureInvariant);
        private static Regex realWords = new Regex(@"[a-zA-Z]+\b", RegexOptions.Compiled | RegexOptions.CultureInvariant);
        private static Regex realWordsMergedContraction = new Regex(@"[a-zA-Z']+\b", RegexOptions.Compiled | RegexOptions.CultureInvariant);


        public static List<string> SplitSentences(this string input)
        {
            return GetSentences(input);
        }

        public static int WordCount(this string input, bool realWordsOnly = true, bool mergeContractions = true)
        {
            return GetWordCount(input, realWordsOnly, mergeContractions);
        }

        public static List<string> GetSentences(string input)
        {
            var sentences = new List<string>();
            var endOfSentence = @"(\!|\?|\.\s)";
            var endChar = @"(\!|\?|\.)";
            var split = Regex.Split(input, endOfSentence);

            for (var i = 0; i < split.Length; i++)
            {
                if (!String.IsNullOrEmpty(split[i]) && !String.IsNullOrWhiteSpace(split[i]) && split[i].Length > 2)
                {
                    if (Regex.IsMatch(split[i], endChar))
                    {
                        sentences.Add(split[i].Trim());
                    }
                    else
                    {
                        sentences.Add((split[i] + (i + 1 == split.Length ? "" : split[i + 1])).Trim());
                    }
                }
            }

            return sentences;
        }

        public static int GetWordCount(string input, bool realWordsOnly = true, bool mergeContractions = true)
        {
            var trimmed = multiWhiteSpace.Replace(input.Trim(), " ");
            var count = 0;

            if (realWordsOnly)
            {
                if (mergeContractions)
                {
                    count = realWordsMergedContraction.Matches(trimmed).Count;
                }
                else
                {
                    count = realWords.Matches(trimmed).Count;
                }
            }
            else
            {
                foreach (var c in trimmed)
                {
                    if (c == ' ')
                    {
                        count++;
                    }
                }
            }
           
            return count;
        }
    }
}
