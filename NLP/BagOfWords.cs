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
    public class BagOfWords
    {
        private bool minipulatedSinceLastRecalc = true;

        public Dictionary<string, Term> Terms { get; } = new Dictionary<string, Term>();



        public BagOfWords(IDictionary<string, Term> terms)
        {
            Terms = (Dictionary<string, Term>)terms;
        }

        public BagOfWords(IEnumerable<Term> terms)
        {
            foreach (var term in terms)
            {
                Terms[term.Value] = term;
            }
        }

        public BagOfWords() { }



        public bool ContainsDocument(uint docID)
        {
            return Terms.Values.Any(x => x.DocumentIDsByTFs.ContainsKey(docID));
        }

        public void AddDocument(uint documentID, IDictionary<string, ushort> termTFs)
        {
            if (termTFs == null) throw new ArgumentNullException("termTFs");
            if (ContainsDocument(documentID))
            {
                throw new ArgumentException("A document with this ID already exists.", "documentID");
            }

            minipulatedSinceLastRecalc = true;

            foreach (var term in termTFs.Keys)
            {
                if (Terms.ContainsKey(term))
                {
                    Terms[term].DocumentIDsByTFs[documentID] = termTFs[term];
                }
                else
                {
                    Terms[term] = new Term
                    {
                        DocumentIDsByTFs = new Dictionary<uint, int>
                        {
                            [documentID] = termTFs[term]
                        },
                        Value = term
                    };
                }
            }
        }

        public void RemoveDocument(uint documentID, IDictionary<string, ushort> termTFs)
        {
            if (termTFs == null) throw new ArgumentNullException("termTFs");
            if (!ContainsDocument(documentID))
            {
                throw new KeyNotFoundException("Cannot find any documents with the specified ID.");
            }
            if (!termTFs.Keys.All(Terms.ContainsKey))
            {
                throw new KeyNotFoundException("Not all the specified terms can be found in the current collection.");
            }

            minipulatedSinceLastRecalc = true;

            foreach (var term in termTFs.Keys)
            {
                if (Terms[term].DocumentIDsByTFs.ContainsKey(documentID))
                {
                    if (Terms[term].DocumentIDsByTFs.Count == 1)
                    {
                        Terms.Remove(term);
                    }
                    else
                    {
                        Terms[term].DocumentIDsByTFs.Remove(documentID);
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

            // Get all the document IDs.
            var totalDocs = new HashSet<uint>();
            foreach (var term in Terms.Values)
            foreach (var docID in term.DocumentIDsByTFs.Keys)
            {
                if (!totalDocs.Contains(docID))
                {
                    totalDocs.Add(docID);
                }
            }

            var totalDocCount = (float)totalDocs.Count;

            foreach (var term in Terms.Keys)
            {
                // How many documents contain the term?
                var docsFound = Terms[term].DocumentIDsByTFs.Count;

                Terms[term].IDF = (float)Math.Log(totalDocCount / docsFound, 2);
            }

            minipulatedSinceLastRecalc = false;
        }

        /// <summary>
        /// Calculates the cosine similarity of the given strings (normally words)
        /// compared to the current collection of Terms.
        /// </summary>
        /// <param name="terms">A collection of tokens (i.e., words) for a given string.</param>
        /// <returns>
        /// A dictionary containing a collection of highest
        /// matching document IDs (the key) with their given similarity (the value).
        /// </returns>
        public Dictionary<uint, float> GetSimilarity(IEnumerable<string> terms, ushort maxDocsToReturn)
        {
            if (minipulatedSinceLastRecalc)
            {
                RecalculateIDFs();
            }

            var queryVector = CalculateQueryTfIdfVector(terms);
            var queryLength = CalculateQueryLength(queryVector);

            // To prevent calculating the similarity for every document,
            // we'll take all the documents which actually contain at least 
            // one of the query's terms.
            var matchingTerms = Terms.Values.Where(x => terms.Any(y => y == x.Value));
            var matchingDocIDs = new HashSet<uint>();
            foreach (var term in matchingTerms)
            foreach (var docID in term.DocumentIDsByTFs.Keys)
            {
                if (!matchingDocIDs.Contains(docID))
                {
                    matchingDocIDs.Add(docID);
                }
            }

            // Reconstruct the documents from our term collection.
            var docs = new Dictionary<uint, List<string>>();
            foreach (var docID in matchingDocIDs)
            {
                docs[docID] = GetDocument(docID);
            }

            // Calculate the Euclidean lengths of the documents.
            var docLengths = new Dictionary<uint, float>();
            foreach (var docID in docs.Keys)
            {
                docLengths[docID] = CalculateDocumentLength(docID, docs[docID]);
            }

            // FINALLY, phew! We made it this far. So now we can
            // actually calculate the cosine similarity for the documents.
            var docSimilarities = new Dictionary<uint, float>();
            foreach (var docID in docs.Keys)
            {
                var sim = 0D;

                foreach (var term in queryVector.Keys)
                {
                    if (docs[docID].Contains(term))
                    { //        query tf-idf   x   the term's idf   x    the term's tf
                        sim += queryVector[term] * (Terms[term].IDF * Terms[term].DocumentIDsByTFs[docID]);
                    }
                }

                docSimilarities[docID] = (float)sim / Math.Max((queryLength * docLengths[docID]), 1);
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

        private float CalculateDocumentLength(uint docID, List<string> terms)
        {
            var len = 0D;

            foreach (var term in terms)
            {
                if (Terms.ContainsKey(term))
                {
                    len += Terms[term].IDF * Terms[term].DocumentIDsByTFs[docID] *
                           Terms[term].IDF * Terms[term].DocumentIDsByTFs[docID];
                }
            }

            return (float)Math.Sqrt(len);
        }

        private float CalculateQueryLength(Dictionary<string, float> queryVector)
        {
            var len = 0D;

            foreach (var tfidf in queryVector.Values)
            {
                len += tfidf * tfidf;
            }

            return (float)Math.Sqrt(len);
        }

        private Dictionary<string, float> CalculateQueryTfIdfVector(IEnumerable<string> terms)
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

            var tfIdf = new Dictionary<string, float>();

            foreach (var term in tf.Keys)
            {
                if (Terms.ContainsKey(term))
                {
                    tfIdf[term] = (maxFrec / tf[term]) * Terms[term].IDF;
                }
            }

            return tfIdf;
        }

        private List<string> GetDocument(uint docID)
        {
            var terms = new List<string>();

            foreach (var term in Terms.Values)
            {
                if (term.DocumentIDsByTFs.Keys.Contains(docID))
                {
                    for (var i = 0; i < term.DocumentIDsByTFs[docID]; i++)
                    {
                        terms.Add(term.Value);
                    }
                }
            }

            return terms;
        }
    }
}
