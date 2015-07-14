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
using System.Text.RegularExpressions;

namespace Phamhilator.Yam.Core
{
    public class Cue
    {
        public Regex Pattern { get; set; }
        public CueType Type { get; set; }
        public float Weight { get; set; }
        public int Found { get; set; }
        public int Positive { get; set; }
        public int Negative { get; set; }
        public bool EnglishOnly { get; set; }
        public bool IncludeCode { get; set; }
        public bool IncludeHtml { get; set; }



        public Cue(Regex pattern, CueType type, float weight, int found, int pos, int neg, bool eng, bool code, bool html)
        {
            if (pattern == null) { throw new ArgumentNullException("pattern"); }

            Pattern = pattern;
            Type = type;
            Weight = weight;
            Found = found;
            Positive = pos;
            Negative = neg;
            EnglishOnly = eng;
            IncludeCode = code;
            IncludeHtml = html;
        }



        public override int GetHashCode()
        {
            return Pattern == null ? -1 : Pattern.GetHashCode();
        }
    }
}
