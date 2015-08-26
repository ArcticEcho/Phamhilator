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
using Phamhilator.Yam.Core;

namespace Phamhilator.Pham.UI
{
    public class PostClassifier
    {
        private ModelGenerator modelGen;
        private HashSet<string[]> models;
        private readonly bool dvWorthy;



        public PostClassifier(string[] badPostModels, bool dvWorthyModels)
        {
            modelGen = new ModelGenerator();
            models = new HashSet<string[]>();
            dvWorthy = dvWorthyModels;

            foreach (var model in badPostModels)
            {
                models.Add(model.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries));
            }
        }



        public SuggestedAction ClassifyPost(Post post)
        {
            var postModel = modelGen.GenerateModel(post.Body);
            var highestMatch = -1D;

            foreach (var model in models)
            {
                var score = MatchScore(postModel, model);

                highestMatch = Math.Max(highestMatch, score);
            }

            return highestMatch > (3 / 2D) ? 
                   (dvWorthy ? SuggestedAction.DV : SuggestedAction.CV) :
                   SuggestedAction.Nothing;
        }



        private static double MatchScore(string[] tagsA, string[] tagsB)
        {
            var largeTagsLen = Math.Max(tagsA.Length, tagsB.Length);
            var matchScore = 0D;

            if (tagsA.Length > tagsB.Length)
            {
                for (var i = 0; i < tagsA.Length; i++)
                {
                    var dist = 0;

                    dist = WordDist(tagsA[i], i, tagsB);

                    if (dist == 0)
                    {
                        matchScore += 1D / largeTagsLen;
                        continue;
                    }

                    if (dist < 0)
                    {
                        matchScore -= 1D / largeTagsLen;
                    }
                    else
                    {
                        matchScore -= (1D / largeTagsLen) * (Math.Min(dist, 5D) / 5);
                    }
                }
            }
            else
            {
                for (var i = 0; i < tagsB.Length; i++)
                {
                    var dist = 0;

                    dist = WordDist(tagsB[i], i, tagsA);

                    if (dist == 0)
                    {
                        matchScore += 1D / largeTagsLen;
                        continue;
                    }

                    if (dist < 0)
                    {
                        matchScore -= 1D / largeTagsLen;
                    }
                    else
                    {
                        matchScore -= (1D / tagsB.Length) * (Math.Min(dist, 5D) / 5);
                    }
                }
            }

            return matchScore;
        }

        private static int WordDist(string x, int xIndex, string[] tags)
        {
            var distAhead = -1;
            var distBehind = -1;

            // Search ahead of x.
            for (var i = xIndex; i < tags.Length; i++)
            {
                if (tags[i] == x)
                {
                    distAhead = i - xIndex;
                    break;
                }
            }

            // Search behind of x.
            if (xIndex < tags.Length + 5)
            {
                for (var i = xIndex; i > 0; i--)
                {
                    if (i < tags.Length && tags[i] == x)
                    {
                        distBehind = xIndex - i;
                        break;
                    }
                }
            }

            return Math.Max(distAhead, distBehind);
        }
    }
}
