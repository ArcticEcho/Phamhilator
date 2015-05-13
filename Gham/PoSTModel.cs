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

namespace Phamhilator.Gham
{
    public class PoSTModel
    {
        public ClassificationRating AverageSpamRating
        {
            get
            {
                if (Tags.Length == 0) { return new ClassificationRating(0, 0); }

                var rating = Tags.Sum(x => x.SpamRating.Rating) / Tags.Length;
                var maturity = Tags.Sum(x => x.SpamRating.Maturity) / Tags.Length;

                return new ClassificationRating(rating, maturity);
            }
        }

        public ClassificationRating AverageOffensiveRating
        {
            get
            {
                if (Tags.Length == 0) { return new ClassificationRating(0, 0); }

                var rating = Tags.Sum(x => x.OffensiveRating.Rating) / Tags.Length;
                var maturity = Tags.Sum(x => x.OffensiveRating.Maturity) / Tags.Length;

                return new ClassificationRating(rating, maturity);
            }
        }

        public ClassificationRating AverageLowQualityRating
        {
            get
            {
                if (Tags.Length == 0) { return new ClassificationRating(0, 0); }

                var rating = Tags.Sum(x => x.LowQualityRating.Rating) / Tags.Length;
                var maturity = Tags.Sum(x => x.LowQualityRating.Maturity) / Tags.Length;

                return new ClassificationRating(rating, maturity);
            }
        }

        public PoSTag[] Tags { get; private set; }

        public string ModelID { get; private set; }



        public PoSTModel(PoSTag[] tags, string modelID)
        {
            if (tags == null) { throw new ArgumentNullException("tags"); }
            if (string.IsNullOrEmpty(modelID)) { throw new ArgumentException("'modelID' must not be null or empty.", "modelID"); }

            Tags = tags;
            ModelID = modelID;
        }
    }
}
