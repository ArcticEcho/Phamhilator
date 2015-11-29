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
        private readonly PostModelGenerator modelGen = new PostModelGenerator();
        private readonly Logger<Term> termLog;
        private bool dispose;

        public GlobalTfIdfRecorder TfIdfRecorder { get; private set; }



        public PostClassifier(string termLogPath)
        {
            termLog = new Logger<Term>(termLogPath);
            TfIdfRecorder = new GlobalTfIdfRecorder(termLog);
        }

        ~PostClassifier()
        {
            Dispose();
        }



        public void Dispose()
        {
            if (dispose) return;
            dispose = true;

            UpdateLog();

            GC.SuppressFinalize(this);
        }

        public ClassificationResults ClassifyPost(Post post)
        {
            if (TfIdfRecorder.MinipulatedSinceLastRecalc)
            {
                TfIdfRecorder.RecalculateIDFs();
            }

            var postTermTFs = modelGen.GetModel(post.Body);
            var sim = ToSimpleTermCollection(postTermTFs);

            var docs = TfIdfRecorder.GetSimilarity(sim, 10);

            //TODO: Do stuff with docs.

            return null;
        }

        public void AddPostToModels(Post post)
        {
            var postTermTFs = modelGen.GetModel(post.Body);

            TfIdfRecorder.AddDocument(post.ID, postTermTFs);
        }

        public void RemovePostFromModels(Post post)
        {
            var postTermTFs = modelGen.GetModel(post.Body);

            TfIdfRecorder.RemoveDocument(post.ID, postTermTFs);
        }



        private void UpdateLog()
        {
            termLog.ClearLog();

            termLog.EnqueueItems(TfIdfRecorder.Terms.Values);
        }

        private string[] ToSimpleTermCollection(IDictionary<string, ushort> termTFs)
        {
            var simple = new string[termTFs.Sum(x => x.Value)];
            var i = 0;

            foreach (var term in termTFs)
            {
                for (var k = 0; k < term.Value; k++)
                {
                    simple[i] = term.Key;
                    i++;
                }
            }

            return simple;
        }
    }
}
