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





using System.IO;
using System.Text.RegularExpressions;
using System;
using Newtonsoft.Json;

namespace Phamhilator.Pham.Core
{
    public class Term
    {
        private readonly string file;
        private float tpCount;
        private float fpCount;
        private float caughtCount;

        public FilterConfig FilterConfig { get; private set; }
        public Regex Regex { get; private set; }
        public bool IsAuto { get; private set; }
        public string Site { get; private set; }
        public float Score { get; private set; }

        public float TPCount
        {
            get
            {
                return tpCount;
            }

            set
            {
                string json;

                if (FilterConfig.Type == FilterType.Black)
                {
                    if (!Config.BlackFilters[FilterConfig].Terms.Contains(this)) { throw new Exception("Can only set TPCount if this Term is within the specified filter."); }

                    Config.BlackFilters[FilterConfig].Terms.Remove(this);
                    Config.BlackFilters[FilterConfig].Terms.Add(new Term(FilterConfig, Regex, Score, Site, IsAuto, value, FPCount, CaughtCount));

                    json = JsonConvert.SerializeObject(Config.BlackFilters[FilterConfig].Terms.ToJsonTerms());
                }
                else
                {
                    if (!Config.WhiteFilters[FilterConfig].Terms.Contains(this)) { throw new Exception("Can only set TPCount if this Term is within the specified filter."); }

                    Config.WhiteFilters[FilterConfig].Terms.Remove(this);
                    Config.WhiteFilters[FilterConfig].Terms.Add(new Term(FilterConfig, Regex, Score, Site, IsAuto, value, FPCount, CaughtCount));

                    json = JsonConvert.SerializeObject(Config.WhiteFilters[FilterConfig].Terms.ToJsonTerms());
                }

                tpCount = value;

                File.WriteAllText(file, json);
            }
        }

        public float FPCount 
        {
            get
            {
                return fpCount;
            }

            set
            {
                string json;

                if (FilterConfig.Type == FilterType.Black)
                {
                    if (!Config.BlackFilters[FilterConfig].Terms.Contains(this)) { throw new Exception("Can only set FPCount if this Term is within the specified filter."); }

                    Config.BlackFilters[FilterConfig].Terms.Remove(this);
                    Config.BlackFilters[FilterConfig].Terms.Add(new Term(FilterConfig, Regex, Score, Site, IsAuto, TPCount, value, CaughtCount));

                    json = JsonConvert.SerializeObject(Config.BlackFilters[FilterConfig].Terms.ToJsonTerms());
                }
                else
                {
                    if (!Config.WhiteFilters[FilterConfig].Terms.Contains(this)) { throw new Exception("Can only set FPCount if this Term is within the specified filter."); }

                    Config.WhiteFilters[FilterConfig].Terms.Remove(this);
                    Config.WhiteFilters[FilterConfig].Terms.Add(new Term(FilterConfig, Regex, Score, Site, IsAuto, TPCount, value, CaughtCount));

                    json = JsonConvert.SerializeObject(Config.WhiteFilters[FilterConfig].Terms.ToJsonTerms());
                } 
                
                fpCount = value;

                File.WriteAllText(file, json);
            }
        }

        public float CaughtCount 
        {
            get
            {
                return caughtCount;
            }

            set
            {
                string json;

                if (FilterConfig.Type == FilterType.Black)
                {
                    if (!Config.BlackFilters[FilterConfig].Terms.Contains(this)) { throw new Exception("Can only set CaughtCount if this Term is within the specified filter."); }

                    Config.BlackFilters[FilterConfig].Terms.Remove(this);
                    Config.BlackFilters[FilterConfig].Terms.Add(new Term(FilterConfig, Regex, Score, Site, IsAuto, TPCount, FPCount, value));

                    json = JsonConvert.SerializeObject(Config.BlackFilters[FilterConfig].Terms.ToJsonTerms());
                }
                else
                {
                    if (!Config.WhiteFilters[FilterConfig].Terms.Contains(this)) { throw new Exception("Can only set CaughtCount if this Term is within the specified filter."); }

                    Config.WhiteFilters[FilterConfig].Terms.Remove(this);
                    Config.WhiteFilters[FilterConfig].Terms.Add(new Term(FilterConfig, Regex, Score, Site, IsAuto, TPCount, FPCount, value));

                    json = JsonConvert.SerializeObject(Config.WhiteFilters[FilterConfig].Terms.ToJsonTerms());
                }

                caughtCount = value;

                File.WriteAllText(file, json);
            }
        }

        public float IgnoredCount
        {
            get
            {
                var igCount = CaughtCount - (TPCount + FPCount);

                return igCount < 0 ? 0 : igCount;
            }
        }

        public float Sensitivity
        {
            get
            {
                // Loving formulated by Jan Dvorak (http://stackoverflow.com/users/499214/jan-dvorak).

                return (TPCount * CaughtCount / (FPCount + TPCount)) / (Stats.TotalCheckedPosts - Stats.TotalTPCount * Stats.TotalCheckedPosts / (Stats.TotalTPCount + Stats.TotalFPCount));
            }
        }

        public float Specificity
        {
            get
            {
                // Loving formulated by Jan Dvorak (http://stackoverflow.com/users/499214/jan-dvorak).

                return 1 - (FPCount * CaughtCount / (TPCount + FPCount)) / (Stats.TotalCheckedPosts - Stats.TotalTPCount * Stats.TotalCheckedPosts / (Stats.TotalTPCount + Stats.TotalFPCount));
            }
        }



        public Term(FilterConfig filterConfig, Regex regex, float score, string site = "", bool isAuto = false, float tpCount = 0, float fpCount = 0, float caughtCount = 0)
        {
            if (regex == null) { throw new ArgumentNullException("regex"); }

            file = String.IsNullOrEmpty(site) ? DirectoryTools.GetFilterFile(filterConfig) : Path.Combine(DirectoryTools.GetFilterFile(filterConfig), site, "Terms.txt");

            FilterConfig = filterConfig;
            Regex = regex;
            Score = score;
            Site = site ?? "";
            IsAuto = isAuto;
            this.tpCount = tpCount;
            this.fpCount = fpCount;
            this.caughtCount = caughtCount;
        }



        public static bool operator ==(Term a, Term b)
        {
            if (ReferenceEquals(a, b)) { return true; }

            if ((object)a == null || (object)b == null) { return false; } // Box args to avoid recursion.

            return a.GetHashCode() == b.GetHashCode();
        }

        public static bool operator !=(Term a, Term b)
        {
            return !(a == b);
        }

        public bool Equals(Term term)
        {
            if (term == null) { return false; }

            return term.GetHashCode() == GetHashCode();
        }

        public bool Equals(Regex regex, string site = "")
        {
            if (String.IsNullOrEmpty(regex.ToString())) { return false; }

            return regex.ToString() == Regex.ToString() && site == Site;
        }

        public override bool Equals(object obj)
        {
            if (obj == null) { return false; }

            if (!(obj is Term)) { return false; }

            return obj.GetHashCode() == GetHashCode();
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return Regex.ToString().GetHashCode() + Site.GetHashCode();
            }
        }
    }
}