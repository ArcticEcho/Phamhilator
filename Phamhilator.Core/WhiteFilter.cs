using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using JsonFx.Json;



namespace Phamhilator.Core
{
    public class WhiteFilter
    {
        public HashSet<Term> Terms { get; private set; }

        public FilterConfig Config { get; private set; }



        public WhiteFilter(FilterClass filter)
        {
            Config = new FilterConfig(filter, Phamhilator.Core.FilterType.White);
            Terms = new HashSet<Term>();

            var sites = Directory.EnumerateDirectories(DirectoryTools.GetFilterFile(Config)).ToArray();
            var reader = new JsonReader();

            for (var i = 0; i < sites.Length; i++)
            {
                sites[i] = Path.GetFileName(sites[i]);
            }

            foreach (var site in sites)
            {
                var path = Path.Combine(DirectoryTools.GetFilterFile(Config), site, "Terms.txt");
                List<JsonTerm> data;

                try
                {
                    data = reader.Read<List<JsonTerm>>(File.ReadAllText(path));
                }
                catch (Exception ex)
                {
                    throw new Exception(string.Format("Can't read file '{0}'. Reason: {1}", path, ex.Message), ex);
                }

                foreach (var t in data)
                {
                    t.Site = site;

                    Terms.Add(t.ToTerm(Config));
                }
            }
        }



        public void AddTerm(Term term)
        {
            if (Terms.Contains(term.Regex, term.Site)) { return; }

            Terms.WriteTerm(Config, new Regex(""), term.Regex, term.Site, term.Score);
        }

        public void RemoveTerm(Regex term, string site)
        {
            if (!Terms.Contains(term, site)) { return; }

            Terms.WriteTerm(Config, term, new Regex(""), site);
        }

        public void EditTerm(Regex oldTerm, Regex newTerm, string site)
        {
            if (!Terms.Contains(oldTerm, site)) { return; }

            Terms.WriteTerm(Config, oldTerm, newTerm, site);
        }

        public void SetScore(Term term, float newScore)
        {
            if (!Terms.Contains(term.Regex, term.Site)) { return; }

            Terms.WriteScore(Config, term.Regex, newScore, term.Site);
        }

        public void SetAuto(Regex term, bool isAuto, string site, bool persistence = false)
        {
            if (!Terms.Contains(term, site)) { return; }

            if (persistence)
            {
                Terms.WriteAuto(Config, term, isAuto, site);
            }
            else
            {
                var t = Terms.GetRealTerm(term, site);

                Terms.Remove(t);

                Terms.Add(new Term(Config, t.Regex, t.Score, t.Site, isAuto));
            }
        }
    }
}
