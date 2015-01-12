using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;



namespace Phamhilator
{
    public static class TermCreater
    {
        public static void CreateTerm(FilterType filter, Regex term, string site = "", float newScore = 0)
        {
            if (term == null || String.IsNullOrEmpty(term.ToString())) { throw new ArgumentException("term can not be null or empty.", "term"); }

            var file = String.IsNullOrEmpty(site) ? DirectoryTools.GetFilterFile(filter) : Path.Combine(DirectoryTools.GetFilterFile(filter), site, "Terms.txt");

            if (!File.Exists(file))
            {
                if (!Directory.Exists(Directory.GetParent(file).FullName))
                {
                    Directory.CreateDirectory(Directory.GetParent(file).FullName);
                }

                File.Create(file).Dispose();
            }

            var t = new Term(filter, term, newScore, site);
            
            File.WriteAllText(file, JsonConvert.SerializeObject(t.ToJsonTerm(), Formatting.Indented));
        }
    }
}
