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

namespace Phamhilator.NLP
{
    public class BagOfWords
    {
        public enum WeightMode
        {
            /// <summary>
            /// Term frequency.
            /// </summary>
            TF,

            /// <summary>
            /// Inverse document frequency.
            /// </summary>
            IDF,

            /// <summary>
            /// Term frequency-Inverse document frequency.
            /// </summary>
            TFIDF,

        }



        public HashSet<string> RootTerms { get; } = new HashSet<string>();

        public Dictionary<string, int> Terms { get; } = new Dictionary<string, int>();

        public WeightMode Mode { get; set; }



        public BagOfWords(IEnumerable<string> terms, WeightMode mode = WeightMode.TFIDF)
        {
            Mode = mode;

            foreach (var term in terms)
            {
                if (Terms.ContainsKey(term))
                {
                    Terms[term]++;
                }
                else
                {
                    Terms[term] = 1;
                }
            }
        }



        public float GetWeightedTermValue(string term, ICollection<IEnumerable<string>> termsCollection = null)
        {
            if (string.IsNullOrWhiteSpace(term) || !Terms.ContainsKey(term)) return -1;

            var val = -1F;

            switch (Mode)
            {
                case WeightMode.TF:
                {
                    val = Terms[term];
                    break;
                }
                case WeightMode.IDF:
                {
                    if (termsCollection == null || termsCollection.Count == 0) break;

                    val = (float)CalcIDF(term, termsCollection);

                    break;
                }
                case WeightMode.TFIDF:
                {
                    if (termsCollection == null || termsCollection.Count == 0) break;

                    var idf = (float)CalcIDF(term, termsCollection);
                    val = Terms[term] * idf;

                    break;
                }
            }

            return val;
        }



        private double CalcIDF(string term, ICollection<IEnumerable<string>> termsCollection)
        {
            var termsFound = 0D;
            var totalCols = 0;

            foreach (var termCol in termsCollection)
            {
                totalCols++;

                foreach (var t in termCol)
                {
                    if (t == term)
                    {
                        termsFound++;
                    }
                }
            }

            return Math.Log(totalCols / termsFound);
        }
    }
}
