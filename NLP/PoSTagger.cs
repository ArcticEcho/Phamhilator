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
using System.Reflection;
using NLP.Properties;
using File = System.IO.File;
using Console = System.Console;



namespace NLP
{
    public class PoSTagger
    {
        private readonly string modelPath;
        private MaxentTagger tagger;
        private Regex tags = new Regex(@"_(C[CD]|DT|EX|FW|IN|JJ[SR]?|LS|MD|NN([PS]|PS)?|P(DT|OS|RP\$?)|R(B[RS]?|P)|SYM|TO|UH|VB[DGNPZ]?|W(DT|P\$?|RB)|[#$(),.:]|\'\'|\`\`)\s", RegexOptions.Compiled | RegexOptions.CultureInvariant);



        public PoSTagger()
        {
            modelPath = Path.Combine(Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath), "wsj-0-18-bidirectional-nodistsim.tagger");

            if (!File.Exists(modelPath))
            {
                File.WriteAllBytes(modelPath, Resources.wsj_0_18_bidirectional_nodistsim);
            }

            tagger = new MaxentTagger(modelPath);
        }

        public string TagString(string input, bool tagsOnly = true)
        {
            var tagged = "";

            try
            {
                tagged = tagger.tagString(input);
            }
            catch (OutOfMemoryException)
            {
                Console.WriteLine("Warning: PoS tagger has ran out of memory, initialising new instance...");
                GC.Collect();
                tagger = new MaxentTagger(modelPath);

                return TagString(input, tagsOnly);
            }

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
