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
    public class GlobalTfIdfRecorder
    {
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



        public void AddTerm(string term, uint docID)
        {
            if (string.IsNullOrWhiteSpace(term))
            {
                throw new ArgumentException("'term' can not be null, empty or entirely whitespace.", "term");
            }

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

        public void RemoveTerm(string term, ushort count = 1)
        {
            if (string.IsNullOrWhiteSpace(term))
            {
                throw new ArgumentException("'term' can not be null, empty or entirely whitespace.", "term");
            }
            if (count == 0)
            {
                throw new ArgumentOutOfRangeException("count", "'count' must be more than 0.");
            }
            if (!Terms.ContainsKey(term)) throw new KeyNotFoundException();

            if (Terms[term].TF >= count)
            {
                Terms.Remove(term);
            }
            else
            {
                Terms[term].TF -= count;
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
        }
    }
}
