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



namespace Ghamhilator
{
    public class PoSTag
    {
        public string Tag { get; private set; }
        public ClassificationRating SpamRating { get; set; }
        public ClassificationRating OffensiveRating { get; set; }
        public ClassificationRating LowQualityRating { get; set; }
        public HashSet<KeyWord> BlackKeyWords { get; private set; }
        public HashSet<KeyWord> WhiteKeyWords { get; private set; }



        public PoSTag(string tag, ClassificationRating spamRating = null, ClassificationRating offensiveRating = null, ClassificationRating lowQualityRating = null, HashSet<KeyWord> blackWords = null, HashSet<KeyWord> whiteWords = null)
        {
            if (String.IsNullOrEmpty(tag)) { throw new ArgumentException("'tag' must not be null or empty.", "tag"); }

            Tag = tag;
            SpamRating = spamRating ?? new ClassificationRating(1, 1);
            OffensiveRating = offensiveRating ?? new ClassificationRating(1, 1);
            LowQualityRating = lowQualityRating ?? new ClassificationRating(1, 1);
            BlackKeyWords = blackWords ?? new HashSet<KeyWord>();
            WhiteKeyWords = whiteWords ?? new HashSet<KeyWord>();
        }
    }
}
