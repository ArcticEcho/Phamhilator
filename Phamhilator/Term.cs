using System.IO;
using System.Text.RegularExpressions;
using System;
using Newtonsoft.Json;



namespace Phamhilator
{
    public class Term
    {
        private readonly string file;
        private readonly bool isBlack;
        private float tpCount;
        private float fpCount;
        private float caughtCount;

        public FilterType Type { get; private set; }
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

                if (isBlack)
                {
                    if (!GlobalInfo.BlackFilters[Type].Terms.Contains(this)) { throw new Exception("Can only set TPCount if this Term is within the specified filter."); }

                    GlobalInfo.BlackFilters[Type].Terms.Remove(this);
                    GlobalInfo.BlackFilters[Type].Terms.Add(new Term(Type, Regex, Score, Site, IsAuto, value, FPCount, CaughtCount));

                    json = JsonConvert.SerializeObject(GlobalInfo.BlackFilters[Type].Terms.ToTempTerms(), Formatting.Indented);
                }
                else
                {
                    if (!GlobalInfo.WhiteFilters[Type].Terms.Contains(this)) { throw new Exception("Can only set TPCount if this Term is within the specified filter."); }

                    GlobalInfo.WhiteFilters[Type].Terms.Remove(this);
                    GlobalInfo.WhiteFilters[Type].Terms.Add(new Term(Type, Regex, Score, Site, IsAuto, value, FPCount, CaughtCount));

                    json = JsonConvert.SerializeObject(GlobalInfo.WhiteFilters[Type].Terms.ToTempTerms(), Formatting.Indented);
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

                if (isBlack)
                {
                    if (!GlobalInfo.BlackFilters[Type].Terms.Contains(this)) { throw new Exception("Can only set FPCount if this Term is within the specified filter."); }

                    GlobalInfo.BlackFilters[Type].Terms.Remove(this);
                    GlobalInfo.BlackFilters[Type].Terms.Add(new Term(Type, Regex, Score, Site, IsAuto, TPCount, value, CaughtCount));

                    json = JsonConvert.SerializeObject(GlobalInfo.BlackFilters[Type].Terms.ToTempTerms(), Formatting.Indented);
                }
                else
                {
                    if (!GlobalInfo.WhiteFilters[Type].Terms.Contains(this)) { throw new Exception("Can only set FPCount if this Term is within the specified filter."); }

                    GlobalInfo.WhiteFilters[Type].Terms.Remove(this);
                    GlobalInfo.WhiteFilters[Type].Terms.Add(new Term(Type, Regex, Score, Site, IsAuto, TPCount, value, CaughtCount));

                    json = JsonConvert.SerializeObject(GlobalInfo.WhiteFilters[Type].Terms.ToTempTerms(), Formatting.Indented);
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

                if (isBlack)
                {
                    if (!GlobalInfo.BlackFilters[Type].Terms.Contains(this)) { throw new Exception("Can only set CaughtCount if this Term is within the specified filter."); }

                    GlobalInfo.BlackFilters[Type].Terms.Remove(this);
                    GlobalInfo.BlackFilters[Type].Terms.Add(new Term(Type, Regex, Score, Site, IsAuto, TPCount, FPCount, value));

                    json = JsonConvert.SerializeObject(GlobalInfo.BlackFilters[Type].Terms.ToTempTerms(), Formatting.Indented);
                }
                else
                {
                    if (!GlobalInfo.WhiteFilters[Type].Terms.Contains(this)) { throw new Exception("Can only set CaughtCount if this Term is within the specified filter."); }

                    GlobalInfo.WhiteFilters[Type].Terms.Remove(this);
                    GlobalInfo.WhiteFilters[Type].Terms.Add(new Term(Type, Regex, Score, Site, IsAuto, TPCount, FPCount, value));

                    json = JsonConvert.SerializeObject(GlobalInfo.WhiteFilters[Type].Terms.ToTempTerms(), Formatting.Indented);
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

                return (TPCount * CaughtCount / (FPCount + TPCount)) / (GlobalInfo.Stats.TotalCheckedPosts - GlobalInfo.Stats.TotalTPCount * GlobalInfo.Stats.TotalCheckedPosts / (GlobalInfo.Stats.TotalTPCount + GlobalInfo.Stats.TotalFPCount));			
            }
        }

        public float Specificity
        {
            get
            {
                // Loving formulated by Jan Dvorak (http://stackoverflow.com/users/499214/jan-dvorak).

                return 1 - (FPCount * CaughtCount / (TPCount + FPCount)) / (GlobalInfo.Stats.TotalCheckedPosts - GlobalInfo.Stats.TotalTPCount * GlobalInfo.Stats.TotalCheckedPosts / (GlobalInfo.Stats.TotalTPCount + GlobalInfo.Stats.TotalFPCount));
            }
        }



        public Term(FilterType type, Regex regex, float score, string site = "", bool isAuto = false, float tpCount = 0, float fpCount = 0, float caughtCount = 0)
        {
            if (regex == null) { throw new ArgumentNullException("regex"); }

            file = String.IsNullOrEmpty(site) ? DirectoryTools.GetFilterFile(type) : Path.Combine(DirectoryTools.GetFilterFile(type), site, "Terms.txt");
            isBlack = (int)type < 100;

            Type = type;
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