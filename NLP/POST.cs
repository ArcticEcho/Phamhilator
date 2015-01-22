using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.IO;
using java.io;
using java.util;
using edu.stanford.nlp.ling;
using edu.stanford.nlp.tagger.maxent;
using File = System.IO.File;



namespace NLP
{
    public class POST
    {
        private MaxentTagger tagger;
        private Regex tags = new Regex(@"_(C[CD]|DT|EX|FW|IN|JJ[SR]?|LS|MD|NN([PS]|PS)?|P(DT|OS|RP\$?)|R(B[RS]?|P)|SYM|TO|UH|VB[DGNPZ]?|W(DT|P\$?|RB)|[#$(),.:]|\'\'|\`\`)\s", RegexOptions.Compiled | RegexOptions.CultureInvariant);



        public POST()
        {
            var modelPath = Path.Combine(Path.GetDirectoryName(new Uri(System.Reflection.Assembly.GetExecutingAssembly().CodeBase).LocalPath), "wsj-0-18-bidirectional-nodistsim.tagger");

            if (!File.Exists(modelPath))
            {
                File.WriteAllBytes(modelPath, NLP.Properties.Resources.wsj_0_18_bidirectional_nodistsim);
            }

            tagger = new MaxentTagger(modelPath);
        }

        public string TagString(string input, bool tagsOnly = true)
        {
            var tagged = tagger.tagString(input);

            if (tagsOnly)
            {
                var final = "";

                foreach (Match match in tags.Matches(tagged))
                {
                    var tag = match.Value.Remove(0, 1);
                    final += tag;
                }

                return final.TrimEnd();
            }

            return tagged;       
        }
    }
}
