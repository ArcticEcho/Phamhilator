using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using JsonFx.Json;



namespace Phamhilator
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
                data = new JsonReader().Read<List<JsonTerm>>(File.ReadAllText(path));
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
