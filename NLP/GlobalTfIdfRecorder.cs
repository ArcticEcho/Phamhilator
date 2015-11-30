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

namespace Phamhilator.NLP
{
    public class GlobalTfIdfRecorder
    {
        public bool MinipulatedSinceLastRecalc { get; private set; } = true;

        public Dictionary<string, Term> Terms { get; } = new Dictionary<string, Term>();



        public GlobalTfIdfRecorder(IDictionary<uint, IEnumerable<string>> docIDWithTerms)
        {
            foreach (var docID in docIDWithTerms.Keys)
            foreach (var term in docIDWithTerms[docID])
            {
                if (Terms.ContainsKey(term))
                {
                    Terms[term].TF++;

                    if (!Terms[term].DocumentIDs.Contains(docID))
                    {
                        Terms[term].DocumentIDs.Add(docID);
                    }
                }
                else
                {
                    Terms[term] = new Term
                    {
                        DocumentIDs = new HashSet<uint>
                        {
                            docID
                        },
                        Value = term,
                        TF = 1
                    };
                }
            }
        }

        public GlobalTfIdfRecorder(IDictionary<string, Term> terms)
        {
            Terms = (Dictionary<string, Term>)terms;
        }

        public GlobalTfIdfRecorder(IEnumerable<Term> terms)
        {
            foreach (var term in terms)
            {
                Terms[term.Value] = term;
            }
        }

        public GlobalTfIdfRecorder() { }



        public void AddDocument(uint documentID, IDictionary<string, ushort> termTFs)
        {
            if (termTFs == null) throw new ArgumentNullException("termTFs");

            foreach (var term in termTFs.Keys)
            {
                if (Terms.ContainsKey(term))
                {
                    Terms[term].TF += termTFs[term];
                    Terms[term].DocumentIDs.Add(documentID);
                }
                else
                {
                    Terms[term] = new Term
                    {
                        DocumentIDs = new HashSet<uint>
                        {
                            documentID
                        },
                        Value = term,
                        TF = termTFs[term]
                    };
                }
            }
        }

        public void RemoveDocument(uint documentID, IDictionary<string, ushort> termTFs)
        {
            if (termTFs == null) throw new ArgumentNullException("termTFs");
            if (Terms.All(x => !x.Value.DocumentIDs.Contains(documentID)))
            {
                throw new KeyNotFoundException("Can not find any terms with the specified document ID.");
            }
            if (!termTFs.Keys.All(Terms.ContainsKey))
            {
                throw new KeyNotFoundException("Not all specified terms can be found in the current collection.");
            }

            foreach (var term in termTFs.Keys)
            {
                if (Terms[term].DocumentIDs.Contains(documentID))
                {
                    if (Terms[term].DocumentIDs.Count == 1)
                    {
                        Terms.Remove(term);
                    }
                    else
                    {
                        Terms[term].DocumentIDs.Remove(documentID);
                        Terms[term].TF -= termTFs[term];
                    }
                }
            }
        }

        public void RecalculateIDFs()
        {
            foreach (var term in Terms.Keys)
            {
                Terms[term].IDF = 0;
            }

            var totalDocs = new HashSet<uint>();

            foreach (var term in Terms.Values)
            foreach (var docID in term.DocumentIDs)
            {
                if (!totalDocs.Contains(docID))
                {
                    totalDocs.Add(docID);
                }
            }

            var totalDocCount = (float)totalDocs.Count;

            foreach (var term in Terms.Keys)
            {
                var docsFound = Terms[term].DocumentIDs.Count;

                Terms[term].IDF = (float)Math.Log(totalDocCount / docsFound);
            }

            MinipulatedSinceLastRecalc = false;
        }

        /// <summary>
        /// Calculates the cosine similarity of the given tokenised string
        /// compared to the current collection of Terms.
        /// </summary>
        /// <param name="terms">A collection of tokens (i.e., words) for a given string.</param>
        /// <returns>
        /// A dictionary containing a collection of highest
        /// matching document IDs (the key) with their given similarity (the value).
        /// </returns>
        public Dictionary<uint, float> GetSimilarity(IEnumerable<string> terms, ushort maxDocsToReturn)
        {
            if (MinipulatedSinceLastRecalc)
            {
                RecalculateIDFs();
            }

            var queryIdf = CalculateQueryIDF(terms);

            // To prevent calculating the similarity for every document,
            // we'll take all the documents which actually contain at least 
            // one of the query's terms.
            var matchingTerms = Terms.Values.Where(x => terms.Any(y => y == x.Value));
            var matchingDocIDs = new HashSet<uint>();
            foreach (var term in matchingTerms)
            foreach (var docID in term.DocumentIDs)
            {
                if (!matchingDocIDs.Contains(docID))
                {
                    matchingDocIDs.Add(docID);
                }
            }

            // Reconstruct the documents from our term collection.
            var docs = new Dictionary<uint, HashSet<string>>();
            foreach (var docID in matchingDocIDs)
            {
                docs[docID] = GetDocument(docID);
            }

            // Calculate the Euclidean lengths of the documents.
            var docLengths = new Dictionary<uint, float>();
            var queryLength = CalculateDocumentLength(new HashSet<string>(terms));
            foreach (var docID in docs.Keys)
            {
                docLengths[docID] = CalculateDocumentLength(docs[docID]);
            }

            // FINALLY, phew! We made it this far. So now we can
            // actually calculate the cosine similarity for the documents.
            var docSimilarities = new Dictionary<uint, float>();
            foreach (var docID in docs.Keys)
            {
                var sim = 0D;

                foreach (var term in queryIdf.Keys)
                {
                    if (docs[docID].Contains(term))
                    {
                        sim += queryIdf[term] * Terms[term].IDF;
                    }
                }

                docSimilarities[docID] = (float)sim / (queryLength * docLengths[docID]);
            }

            // Now lets get the top x docs.
            var topDocs = new Dictionary<uint, float>();
            var temp = docSimilarities.OrderByDescending(x => x.Value);
            var safeMax = Math.Min(docSimilarities.Count, maxDocsToReturn);
            foreach (var doc in temp)
            {
                if (topDocs.Count == safeMax) break;

                topDocs[doc.Key] = doc.Value;
            }

            return topDocs;
        }

        private float CalculateDocumentLength(HashSet<string> terms)
        {
            var len = 0D;

            foreach (var term in terms)
            {
                if (Terms.ContainsKey(term))
                {
                    len += Terms[term].IDF * Terms[term].IDF;
                }
            }

            return (float)Math.Sqrt(len);
        }

        private Dictionary<string, float> CalculateQueryIDF(IEnumerable<string> terms)
        {
            var tf = new Dictionary<string, ushort>();

            foreach (var term in terms)
            {
                if (tf.ContainsKey(term))
                {
                    tf[term]++;
                }
                else
                {
                    tf[term] = 1;
                }
            }

            var maxFrec = (float)tf.Max(x => x.Value);

            var idf = new Dictionary<string, float>();

            foreach (var term in tf.Keys)
            {
                if (Terms.ContainsKey(term))
                {
                    idf[term] = (maxFrec / tf[term]) * Terms[term].IDF;
                }
            }

            return idf;
        }

        private HashSet<string> GetDocument(uint docID)
        {
            var terms = new HashSet<string>();

            foreach (var term in Terms.Values)
            {
                if (term.DocumentIDs.Contains(docID))
                {
                    terms.Add(term.Value);
                }
            }

            return terms;
        }
    }
}
