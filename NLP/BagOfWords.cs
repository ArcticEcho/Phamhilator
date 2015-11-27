using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Phamhilator.Yam.Core;

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
