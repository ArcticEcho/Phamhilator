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
using System.Linq;
using System.Text.RegularExpressions;

namespace Phamhilator.NLP
{
    public class PostTermsExtractor
    {
        private const RegexOptions regOpts = RegexOptions.Compiled | RegexOptions.CultureInvariant;
        private readonly Regex CodeBlock = new Regex("(?is)<pre.*?><code>.*?</code></pre>", regOpts);
        private readonly Regex inlineCode = new Regex("(?is)<code>.*?</code>", regOpts);
        private readonly Regex blockQuote = new Regex("(?is)<blockquote.*?></blockquote>", regOpts);
        private readonly Regex link = new Regex("(?is)<a.*?</a>", regOpts);
        private readonly Regex pic = new Regex("(?is)<img.*?>", regOpts);
        private readonly Regex htmlTags = new Regex("(?is)<.*?>", regOpts);
        private readonly Regex modelTags = new Regex(@"\•[A-Z-]*?\•", regOpts);
        private readonly Dictionary<Regex, string> specContractionsRegex;
        private readonly Dictionary<Regex, string> genContractionsRegex;



        public PostTermsExtractor()
        {
            specContractionsRegex = new Dictionary<Regex, string>
            {
                [new Regex(@"\bi'?m\b", regOpts)] = "i am",
                [new Regex(@"\bisn'?t\b", regOpts)] = "is not",
                [new Regex(@"\baren'?t\b", regOpts)] = "are not",
                [new Regex(@"\bwasn'?t\b", regOpts)] = "was not",
                [new Regex(@"\bweren'?t\b", regOpts)] = "were not",
                [new Regex(@"\bhaven'?t\b", regOpts)] = "have not",
                [new Regex(@"\bhasn'?t\b", regOpts)] = "has not",
                [new Regex(@"\bhadn'?t\b", regOpts)] = "had not",
                [new Regex(@"\bwon'?t\b", regOpts)] = "will not",
                [new Regex(@"\bwouldn'?t\b", regOpts)] = "would not",
                [new Regex(@"\bdon'?t\b", regOpts)] = "do not",
                [new Regex(@"\bdoesn'?t\b", regOpts)] = "does not",
                [new Regex(@"\bdidn'?t\b", regOpts)] = "did not",
                [new Regex(@"\bcan(not|'?t)\b", regOpts)] = "can not",
                [new Regex(@"\bcouldn'?t\b", regOpts)] = "could not",
                [new Regex(@"\bshouldn'?t\b", regOpts)] = "should not",
                [new Regex(@"\bmightn'?t\b", regOpts)] = "might not",
                [new Regex(@"\bmustn'?t\b", regOpts)] = "must not",
                [new Regex(@"\bshan'?t\b", regOpts)] = "shall not"
            };

            genContractionsRegex = new Dictionary<Regex, string>()
            {
                [new Regex(@"\b\b([a-z]+)'ll\b", regOpts)] = "$1 will",
                [new Regex(@"\b([a-z]+)'ve\b", regOpts)] = "$1 have",
                [new Regex(@"\b([a-z]+)'d\b", regOpts)] = "$1 would",
                [new Regex(@"\b([a-z]+)'re\b", regOpts)] = "$1 are",
                [new Regex(@"\b([a-z]+)n't\b", regOpts)] = "$1 not",
                [new Regex(@"\b([a-z]+)'s\b", regOpts)] = "$1"
            };
        }



        public Dictionary<string, ushort> GetTerms(string text)
        {
            var prepared = PrepareBody(text);
            var words = prepared.Split(new[]
            {
                '.', ',', ':', ';', '(', ')', '{', '}', '[', ']', '?', '!', '/', '\\', ' ', '\n', '-'
            }, StringSplitOptions.RemoveEmptyEntries);
            var wf = new Dictionary<string, ushort>();

            for (var i = 0; i < words.Length; i++)
            {
                if (words[i].Any(char.IsLetter) &&
                    (words[i].Length > 1 || words[i] == "i" || words[i] == "a"))
                {
                    string w;

                    if (modelTags.IsMatch(words[i]))
                    {
                        w = words[i];
                    }
                    else
                    {
                        w = new string(words[i].Where(char.IsLetterOrDigit).ToArray());
                    }

                    if (wf.ContainsKey(w))
                    {
                        wf[w]++;
                    }
                    else
                    {
                        wf[w] = 1;
                    }
                }
            }

            return wf;
        }



        private string PrepareBody(string text)
        {
            var clean = text.ToLowerInvariant();
            clean = TagChunks(clean);
            clean = htmlTags.Replace(clean, " ");
            clean = ExpandContractions(clean);

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
                    tagged = tagged.Insert(m.Index, " •CB-S• ");
                }
                else if (lines.Length < 26)
                {
                    tagged = tagged.Insert(m.Index, " •CB-M• ");
                }
                else
                {
                    tagged = tagged.Insert(m.Index, " •CB-L• ");
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
                    tagged = tagged.Insert(match.Index, " •IC-S• ");
                }
                else if (code.Length < 26)
                {
                    tagged = tagged.Insert(match.Index, " •IC-M• ");
                }
                else
                {
                    tagged = tagged.Insert(match.Index, " •IC-L• ");
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
                    tagged = tagged.Insert(m.Index, " •BQ-S• ");
                }
                else if (lines.Length < 11)
                {
                    tagged = tagged.Insert(m.Index, " •BQ-M• ");
                }
                else
                {
                    tagged = tagged.Insert(m.Index, " •BQ-L• ");
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

        private string ExpandContractions(string text)
        {
            var expanded = text;

            foreach (var rg in specContractionsRegex)
            {
                expanded = rg.Key.Replace(expanded, rg.Value);
            }

            foreach (var rg in genContractionsRegex)
            {
                expanded = rg.Key.Replace(expanded, rg.Value);
            }

            return expanded;
        }
    }
}
