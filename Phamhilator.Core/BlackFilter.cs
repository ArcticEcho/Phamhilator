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
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace Phamhilator.Pham.Core
{
    public class BlackFilter
    {
        public HashSet<Term> Terms { get; private set; }

        public float AverageScore
        {
            get
            {
                return Terms.Count == 0 ? 10 : Terms.Select(t => t.Score).Average();
            }
        }

        public float HighestScore
        {
            get
            {
                return Terms.Count == 0 ? 10 : Terms.Select(t => t.Score).Max();
            }
        }

        //public FilterClass FilterType { get; private set; }

        public FilterConfig Config { get; private set; }



        public BlackFilter(FilterClass filterClass)
        {
            //if ((int)filter > 99) { throw new ArgumentException("Must be a black filter.", "filter"); }

            Config = new FilterConfig(filterClass, FilterType.Black);
            Terms = new HashSet<Term>();

            var path = DirectoryTools.GetFilterFile(Config);
            List<JsonTerm> data;

            try
            {
                data = JsonConvert.DeserializeObject<List<JsonTerm>>(File.ReadAllText(path));
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Can't read file '{0}'. Reason: {1}", path, ex.Message), ex);
            }

            if (data != null)
            {
                foreach (var t in data)
                {
                    Terms.Add(t.ToTerm(Config));
                }
            }
        }



        public void AddTerm(Term term)
        {
            if (Terms.Contains(term.Regex)) { return; }

            Terms.WriteTerm(Config, new Regex(""), term.Regex, "", term.Score);
        }

        public void RemoveTerm(Regex term)
        {
            if (!Terms.Contains(term)) { return; }

            Terms.WriteTerm(Config, term, new Regex(""));
        }

        public void EditTerm(Regex oldTerm, Regex newTerm)
        {
            if (!Terms.Contains(oldTerm)) { return; }

            Terms.WriteTerm(Config, oldTerm, newTerm);
        }

        public void SetScore(Term term, float newScore)
        {
            if (!Terms.Contains(term.Regex)) { return; }

            Terms.WriteScore(Config, term.Regex, newScore);
        }

        public void SetAuto(Regex term, bool isAuto, bool persistence = false)
        {
            if (!Terms.Contains(term)) { return; }

            if (persistence)
            {
                Terms.WriteAuto(Config, term, isAuto);
            }
            else
            {
                var t = Terms.GetRealTerm(term);

                Terms.Remove(t);

                Terms.Add(new Term(Config, t.Regex, t.Score, t.Site, isAuto));
            }
        }
    }
}
