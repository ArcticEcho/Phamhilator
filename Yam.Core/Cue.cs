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
        public string Pattern { get; private set; }
        public CueType Type { get; private set; }
        public bool EnglishOnly { get; private set; }
        public bool IncludeCode { get; private set; }
        public bool IncludeHtml { get; private set; }
        public float Weight { get; set; }
        public int Found { get; set; }
        public int Positive { get; set; }
        public int Negative { get; set; }



        public Cue(string pattern, CueType type, float weight, int found, int pos, int neg, bool eng, bool code, bool html)
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
            if (Pattern == null || Pattern.ToString() == null) { return -1; }

            return Pattern.ToString().GetHashCode() + Type.GetHashCode();
        }

        public Regex GetRegex()
        {
            return new Regex(Pattern, RegexOptions.Compiled | RegexOptions.CultureInvariant);
        }
    }
}
