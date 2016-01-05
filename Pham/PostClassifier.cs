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
using Phamhilator.NLP;
using Phamhilator.Yam.Core;

namespace Phamhilator.Pham.UI
{
    public class PostClassifier : IDisposable
    {
        private readonly PostTermsExtractor modelGen = new PostTermsExtractor();
        private readonly BagOfWords bow;
        private readonly Logger<Term> termLog;
        private readonly ClassificationResults.SuggestedAction action;
        private bool dispose;



        public PostClassifier(string termLogPath, ClassificationResults.SuggestedAction termAction)
        {
            action = termAction;
            termLog = new Logger<Term>(termLogPath);
            bow = new BagOfWords(termLog);
        }

        ~PostClassifier()
        {
            Dispose();
        }



        public void Dispose()
        {
            if (dispose) return;
            dispose = true;

            termLog.Clear();
            foreach (var t in bow.Terms.Values)
            {
                termLog.EnqueueItem(t);
            }
            termLog.Dispose();

            GC.SuppressFinalize(this);
        }

        public ClassificationResults ClassifyPost(Post post)
        {
            var postTermTFs = modelGen.GetTerms(post.Body);
            var simple = ToSimpleTermCollection(postTermTFs);
            var docs = new Dictionary<uint, float>();

            lock (bow)
            {
                docs = bow.GetSimilarity(simple, 5);
            }

            //TODO: This will need some experimentation.
            // Average the similarity results.
            var match = 0F;
            var sims = docs.Values.Where(x => x >= 2 / 3F).ToArray();
            foreach (var s in sims)
            {
                match += s / sims.Length;
            }

            return new ClassificationResults(action, match);
        }

        public void AddPostToModels(Post post)
        {
            if (bow.ContainsDocument(post.ID)) return;

            var postTermTFs = modelGen.GetTerms(post.Body);

            lock (bow)
            {
                bow.AddDocument(post.ID, postTermTFs);
            }
        }

        public void RemovePostFromModels(Post post)
        {
            if (!bow.ContainsDocument(post.ID)) return;

            var postTermTFs = modelGen.GetTerms(post.Body);

            lock (bow)
            {
                bow.RemoveDocument(post.ID, postTermTFs);
            }
        }



        private string[] ToSimpleTermCollection(IDictionary<string, ushort> termTFs)
        {
            var simple = new string[termTFs.Sum(x => x.Value)];
            var i = 0;

            foreach (var term in termTFs)
            for (var k = 0; k < term.Value; k++)
            {
                simple[i] = term.Key;
                i++;
            }

            return simple;
        }
    }
}