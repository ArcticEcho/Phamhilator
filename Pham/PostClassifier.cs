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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Phamhilator.NLP;
using Phamhilator.Yam.Core;

namespace Phamhilator.Pham.UI
{
    public class PostClassifier
    {
        private readonly PostModelGenerator modelGen = new PostModelGenerator();
        //private BagOfWords bow;

        //public ConcurrentDictionary<int, PostModel> Models { get; } = new ConcurrentDictionary<int, PostModel>();

        //public BagOfWords.WeightMode WeightMethod { get; set; } = BagOfWords.WeightMode.TFIDF;

        public GlobalTfIdfRecorder TFIDFRecorder { get; private set; }



        public ClassificationResults ClassifyPost(Post post)
        {
            //var modelTerms = Models.Values.Select(x => x.Terms);
            //var postTerms = modelGen.GetModel(post.Body);

            //bow = new BagOfWords(postTerms)
            //{
            //    Mode = WeightMethod
            //};

            return null;
        }
    }
}
