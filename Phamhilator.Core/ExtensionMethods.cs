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
using ChatExchangeDotNet;
using JsonFx.Json;

namespace Phamhilator.Pham.Core
{
    public static class ExtensionMethods
    {
        public static bool IsAuthorOwner(this Message input)
        {
            return Config.UserAccess.Owners.Any(user => user.ID == input.AuthorID);
        }

        public static bool IsAuthorPrivUser(this Message input)
        {
            return Config.UserAccess.PrivUsers.Any(user => user == input.AuthorID) || input.IsAuthorOwner();
        }

        public static bool IsQuestion(this FilterClass classification)
        {
            return classification.ToString().StartsWith("Question");
        }

        public static bool IsQuestionTitle(this FilterClass classification)
        {
            return classification.ToString().StartsWith("QuestionTitle");
        }

        public static bool IsName(this FilterClass classification)
        {
            return classification.ToString().EndsWith("Name");
        }

        public static QuestionAnalysis ToQuestionAnalysis(this PostAnalysis input)
        {
            return input == null ? null : new QuestionAnalysis 
            { 
                Accuracy = input.Accuracy, 
                AutoTermsFound = input.AutoTermsFound, 
                BlackTermsFound = input.BlackTermsFound,
                FiltersUsed = input.FiltersUsed,
                Type = input.Type,
                WhiteTermsFound = input.WhiteTermsFound 
            };
        }

        public static PostType ToPostType(this FilterClass input)
        {
            var trimmed = input.ToString().Replace("Answer", "").Replace("QuestionTitle", "").Replace("QuestionBody", "");

            switch (trimmed)
            {
                case "LQ":
                {
                    return PostType.LowQuality;
                }
                case "Off":
                {
                    return PostType.Offensive;
                }
                case "Spam":
                {
                    return PostType.Spam;
                }
                case "Name":
                {
                    return PostType.BadUsername;
                }
                default:
                {
                    throw new NotSupportedException();
                }
            }
        }

        public static bool Contains(this HashSet<Term> input, Regex term, string site = "")
        {
            return input.Count != 0 && input.Any(t => t.Regex.ToString() == term.ToString() && (String.IsNullOrEmpty(site) ? true : t.Site == site));
        }

        public static void WriteTerm(this HashSet<Term> terms, FilterConfig filter, Regex oldTerm, Regex newTerm, string site = "", float newScore = 0)
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

            File.WriteAllText(file, new JsonWriter().Write(terms.ToJsonTerms()));
        }

        public static void WriteScore(this HashSet<Term> terms, FilterConfig filter, Regex term, float newScore, string site = "")
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

            File.WriteAllText(file, new JsonWriter().Write(terms.ToJsonTerms()));
        }

        public static void WriteAuto(this HashSet<Term> terms, FilterConfig filter, Regex term, bool isAuto, string site = "")
        {
            if (String.IsNullOrEmpty(term.ToString())) { throw new ArgumentException("Can not be empty.", "term"); }

            var file = String.IsNullOrEmpty(site) ? DirectoryTools.GetFilterFile(filter) : Path.Combine(DirectoryTools.GetFilterFile(filter), site, "Terms.txt");
            var realTerm = terms.GetRealTerm(term, site);

            terms.Remove(realTerm);
            terms.Add(new Term(filter, realTerm.Regex, realTerm.Score, realTerm.Site, isAuto, realTerm.TPCount, realTerm.FPCount, realTerm.CaughtCount));

            File.WriteAllText(file, new JsonWriter().Write(terms.ToJsonTerms()));
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
                r.IsMatch(Properties.Resources.NewRegexPayloadRealData);
            }
            catch (Exception)
            {
                return true;
            }

            return false;
        }

        public static Term ToTerm(this JsonTerm input, FilterConfig filter)
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
                    Type = term.FilterConfig.Class
                };
            }

            return logTerms;
        }
    }
}
