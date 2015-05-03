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
    public class KeyWord
    {
        private string word;
        private float score;

        public string Word { get { return word; } }
        public ClassificationRating SpamRating { get; set; }
        public ClassificationRating OffensiveRating { get; set; }
        public ClassificationRating LowQualityRating { get; set; }



        public KeyWord(string word, ClassificationRating spamRating, ClassificationRating offensiveRating, ClassificationRating lowQualityRating)
        {
            this.word = word;
            SpamRating = spamRating;
            OffensiveRating = offensiveRating;
            LowQualityRating = lowQualityRating;
        }
    }
}
