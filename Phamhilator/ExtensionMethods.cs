using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json;



namespace Phamhilator
{
    public static class ExtensionMethods
    {
        public static FilterType GetCorrespondingWhiteFilter(this FilterType input)
        {
            return (FilterType)Enum.Parse(typeof(FilterType), input.ToString().Replace("Black", "White"));
        }

        public static FilterType GetCorrespondingBlackFilter(this FilterType input)
        {
            return (FilterType)Enum.Parse(typeof(FilterType), input.ToString().Replace("White", "Black"));
        }

        public static bool IsBlackFilter(this FilterType input)
        {
            return (int)input < 99;
        }

        public static bool Contains(this HashSet<Term> input, Regex term, string site = "")
        {
            return input.Count != 0 && input.Contains(new Term(FilterType.AnswerBlackLQ, term, 0, site));
        }

        public static void WriteTerm(this HashSet<Term> terms, FilterType filter, Regex oldTerm, Regex newTerm, string site = "", float newScore = 0)
        {
            if (String.IsNullOrEmpty(oldTerm.ToString()) && String.IsNullOrEmpty(newTerm.ToString())) { throw new Exception("oldTerm and newTerm can not both be empty."); }

            var file = String.IsNullOrEmpty(site) ? DirectoryTools.GetFilterFile(filter) : Path.Combine(DirectoryTools.GetFilterFile(filter), site, "Terms.txt");

            if (!File.Exists(file))
            {
                if (!Directory.Exists(Directory.GetParent(file).FullName))
                {
                    Directory.CreateDirectory(Directory.GetParent(file).FullName);
                }

                File.Create(file).Dispose();
            }
            
            if (String.IsNullOrEmpty(oldTerm.ToString())) // Add new term.
            {
                terms.Add(new Term(filter, newTerm, newScore, site));
            }
            else if (String.IsNullOrEmpty(newTerm.ToString())) // Remove old term.
            {
                terms.Remove(terms.GetRealTerm(oldTerm, site));
            }
            else // Edit existing term.
            {
                var realTerm = terms.GetRealTerm(oldTerm, site);

                terms.Remove(realTerm);
                terms.Add(new Term(filter, newTerm, realTerm.Score, realTerm.Site, realTerm.IsAuto, realTerm.TPCount, realTerm.FPCount, realTerm.CaughtCount));
            }

            File.WriteAllText(file, JsonConvert.SerializeObject(terms.ToJsonTerms(), Formatting.Indented));
        }

        public static void WriteScore(this HashSet<Term> terms, FilterType filter, Regex term, float newScore, string site = "")
        {
            if (String.IsNullOrEmpty(term.ToString())) { throw new ArgumentException("Can not be empty.", "term"); }

            var file = String.IsNullOrEmpty(site) ? DirectoryTools.GetFilterFile(filter) : Path.Combine(DirectoryTools.GetFilterFile(filter), site, "Terms.txt");
            var realTerm = terms.GetRealTerm(term, site);

            if (!File.Exists(file))
            {
                if (!Directory.Exists(Directory.GetParent(file).FullName))
                {
                    Directory.CreateDirectory(Directory.GetParent(file).FullName);
                }

                File.Create(file).Dispose();
            }

            terms.Remove(realTerm);
            terms.Add(new Term(filter, realTerm.Regex, newScore, realTerm.Site, realTerm.IsAuto, realTerm.TPCount, realTerm.FPCount, realTerm.CaughtCount));

            File.WriteAllText(file, JsonConvert.SerializeObject(terms.ToJsonTerms(), Formatting.Indented));
        }

        public static void WriteAuto(this HashSet<Term> terms, FilterType filter, Regex term, bool isAuto, string site = "")
        {
            if (String.IsNullOrEmpty(term.ToString())) { throw new ArgumentException("Can not be empty.", "term"); }

            var file = String.IsNullOrEmpty(site) ? DirectoryTools.GetFilterFile(filter) : Path.Combine(DirectoryTools.GetFilterFile(filter), site, "Terms.txt");
            var realTerm = terms.GetRealTerm(term, site);

            terms.Remove(realTerm);
            terms.Add(new Term(filter, realTerm.Regex, realTerm.Score, realTerm.Site, isAuto, realTerm.TPCount, realTerm.FPCount, realTerm.CaughtCount));

            File.WriteAllText(file, JsonConvert.SerializeObject(terms.ToJsonTerms(), Formatting.Indented));
        }

        public static Term GetRealTerm(this HashSet<Term> terms, Regex term, string site = "")
        {
            if (String.IsNullOrEmpty(site))
            {
                foreach (var t in terms)
                {
                    if (t.Regex.ToString() == term.ToString())
                    {
                        return t;
                    }
                }
            }
            else
            {
                foreach (var t in terms)
                {
                    if (t.Equals(term, site))
                    {
                        return t;
                    }
                }
            }

            throw new KeyNotFoundException();
        }

        public static bool IsFlakRegex(this Regex regex)
        {
            try
            {
                var r = new Regex(regex.ToString(), RegexOptions.Compiled, TimeSpan.FromMilliseconds(20));

                r.IsMatch(Properties.Resources.NewRegexPayloadAlphaNumSpec);
                r.IsMatch(Properties.Resources.NewRegexPayloadAlphaNum);
                r.IsMatch(Properties.Resources.NewRegexPayloadAlphaSpec);
                r.IsMatch(Properties.Resources.NewRegexPayloadNumSpec);
                r.IsMatch(Properties.Resources.NewRegexPayloadAlpha);
                r.IsMatch(Properties.Resources.NewRegexPayloadNum);
                r.IsMatch(Properties.Resources.NewRegexPayloadSpec);
            }
            catch (Exception)
            {
                return true;
            }

            return false;
        }

        public static Term ToTerm(this JsonTerm input, FilterType filter)
        {
            return new Term(filter, new Regex(input.Regex, RegexOptions.Compiled), input.Score, input.Site, input.IsAuto, input.TPCount, input.FPCount, input.CaughtCount);
        }

        public static JsonTerm ToJsonTerm(this Term input)
        {
            return new JsonTerm
            {
                Regex = input.Regex.ToString(),
                Score = input.Score,
                Site = input.Site,
                IsAuto = input.IsAuto,
                FPCount = (int)input.FPCount,
                TPCount = (int)input.TPCount,
                CaughtCount = (int)input.CaughtCount
            };
        }

        public static JsonTerm[] ToJsonTerms(this ICollection<Term> input)
        {
            var jsonTerms = new JsonTerm[input.Count];

            for (var i = 0; i < input.Count; i++)
            {
                jsonTerms[i] = input.ElementAt(i).ToJsonTerm();
            }

            return jsonTerms;
        }

        public static LogTerm[] ToLogTerms(this ICollection<Term> input)
        {
            var logTerms = new LogTerm[input.Count];

            for (var i = 0; i < input.Count; i++)
            {
                var term = input.ElementAt(i);

                logTerms[i] = new LogTerm
                {
                    Regex = term.Regex.ToString(),
                    Score = term.Score,
                    Site = term.Site,
                    IsAuto = term.IsAuto,
                    FPCount = (int)term.FPCount,
                    TPCount = (int)term.TPCount,
                    CaughtCount = (int)term.CaughtCount,
                    Type = term.Type
                };
            }

            return logTerms;
        }
    }
}
