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





using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using edu.stanford.nlp.tagger.maxent; // Yay, ugly Java conventions.

namespace Phamhilator.NLP
{
    public class PosTagger
    {
        private const string modelPath = "english-bidirectional-distsim.tagger";
        private const RegexOptions regOpts = RegexOptions.Compiled | RegexOptions.CultureInvariant;
        private readonly Regex CodeBlock = new Regex("(?is)<pre.*?><code>.*?</code></pre>", regOpts);
        private readonly Regex inlineCode = new Regex("(?is)<code>.*?</code>", regOpts);
        private readonly Regex blockQuote = new Regex("(?is)<blockquote.*?></blockquote>", regOpts);
        private readonly Regex link = new Regex("(?is)<a.*?</a>", regOpts);
        private readonly Regex pic = new Regex("(?is)<img.*?>", regOpts);
        private readonly Regex htmlTags = new Regex("(?is)<.*?>", regOpts);
        private readonly Regex taggedChunks = new Regex("•_[A-Z]+ ([A-Z]+)_[A-Z]+ •_[A-Z]+", regOpts);
        private readonly Regex modelTags = new Regex(@"\S+_(\S+)", regOpts);
        private readonly MaxentTagger tagger;



        public PosTagger()
        {
            if (!File.Exists(modelPath))
            {
                File.WriteAllBytes(modelPath, Properties.Resources.Model);
            }

            tagger = new MaxentTagger(modelPath, new java.util.Properties(), false);
        }



        public Dictionary<string, ushort> GetTags(string text)
        {
            var prepared = PrepareBody(text);
            var tagged = tagger.tagString(prepared);
            var tagsStr = taggedChunks.Replace(tagged, "•$1•");
            var tags = modelTags.Replace(tagsStr, "$1").Split(' ');
            var tf = new Dictionary<string, ushort>();

            foreach (var tag in tags)
            {
                if (string.IsNullOrWhiteSpace(tag)) continue;

                if (tf.ContainsKey(tag))
                {
                    tf[tag]++;
                }
                else
                {
                    tf[tag] = 1;
                }
            }

            return tf;
        }



        private string PrepareBody(string text)
        {
            var clean = text.ToLowerInvariant();
            clean = TagChunks(clean);
            clean = htmlTags.Replace(clean, " ");

            return clean.Trim();
        }

        private string TagChunks(string body)
        {
            var tagged = TagCodeBlocks(body);
            tagged = TagInlineCode(tagged);
            tagged = TagBlockQuotes(tagged);
            tagged = TagPictures(tagged);
            tagged = TagLinks(tagged);

            return tagged;
        }

        private string TagCodeBlocks(string body)
        {
            var tagged = body;
            var m = CodeBlock.Match(tagged);

            while (m.Success)
            {
                var code = tagged.Substring(m.Index, m.Length);
                var lines = code.Split('\n');

                tagged = tagged.Remove(m.Index, m.Length);

                if (lines.Length < 4)
                {
                    tagged = tagged.Insert(m.Index, " •CBS• ");
                }
                else if (lines.Length < 26)
                {
                    tagged = tagged.Insert(m.Index, " •CBM• ");
                }
                else
                {
                    tagged = tagged.Insert(m.Index, " •CBL• ");
                }

                m = CodeBlock.Match(tagged);
            }

            return tagged;
        }

        private string TagInlineCode(string body)
        {
            var tagged = body;
            var m = inlineCode.Matches(tagged);
            var matches = new List<Match>();

            foreach (Match match in m)
            {
                if (matches.Count == 0 || match.Index < matches[0].Index)
                {
                    matches.Add(match);
                }
                else
                {
                    matches.Insert(0, match);
                }
            }

            foreach (var match in matches)
            {
                var code = tagged.Substring(match.Index, match.Length);

                tagged = tagged.Remove(match.Index, match.Length);

                if (code.Length < 6)
                {
                    tagged = tagged.Insert(match.Index, " •ICS• ");
                }
                else if (code.Length < 26)
                {
                    tagged = tagged.Insert(match.Index, " •ICM• ");
                }
                else
                {
                    tagged = tagged.Insert(match.Index, " •ICL• ");
                }
            }

            return tagged;
        }

        private string TagBlockQuotes(string body)
        {
            var tagged = body;
            var m = blockQuote.Match(tagged);

            while (m.Success)
            {
                var quote = tagged.Substring(m.Index, m.Length);
                var lines = quote.Split('\n');

                tagged = tagged.Remove(m.Index, m.Length);

                if (lines.Length < 4)
                {
                    tagged = tagged.Insert(m.Index, " •BQS• ");
                }
                else if (lines.Length < 11)
                {
                    tagged = tagged.Insert(m.Index, " •BQM• ");
                }
                else
                {
                    tagged = tagged.Insert(m.Index, " •BQL• ");
                }

                m = blockQuote.Match(tagged);
            }

            return tagged;
        }

        private string TagLinks(string body)
        {
            var tagged = body;
            var m = link.Match(tagged);

            while (m.Success)
            {
                tagged = tagged.Remove(m.Index, m.Length);
                tagged = tagged.Insert(m.Index, " •L• ");

                m = link.Match(tagged);
            }

            return tagged;
        }

        private string TagPictures(string body)
        {
            var tagged = body;
            var m = pic.Match(tagged);

            while (m.Success)
            {
                tagged = tagged.Remove(m.Index, m.Length);
                tagged = tagged.Insert(m.Index, " •P• ");

                m = pic.Match(tagged);
            }

            return tagged;
        }
    }
}
