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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Phamhilator.Yam.Core;

namespace Phamhilator.Pham.UI
{
    public class LinkClassifier
    {
        private const string dataManagerSpamPhrasesKey = "Link Spam Phrases";
        private const string dataManagerWhiteSitesKey = "White Spam Link Sites";
        private const string dataManagerBlackSitesKey = "Black Spam Link Sites";
        private const RegexOptions regOpts = RegexOptions.Compiled | RegexOptions.CultureInvariant;
        private readonly Regex linkStripper = new Regex(@"(?is)(https?://|www\.)([a-z0-9]{1,}?(\.|\b)){1,4}([\w/?=-]*(\.[a-z]{1,5})?)?", regOpts);
        private readonly Regex ignoredFiles = new Regex(@"\.(png|jpe?g|pdf|exe|msi|zip|css|js)$", regOpts);
        private readonly ConcurrentDictionary<string, LinkClassification> checkedLinks = new ConcurrentDictionary<string, LinkClassification>();
        private readonly HashSet<string> spamPhrases = new HashSet<string>();
        private readonly HashSet<string> blackSites = new HashSet<string>();
        private readonly HashSet<string> whiteSites = new HashSet<string>();

        public int SpamPhrasesCount
        {
            get
            {
                return spamPhrases.Count;
            }
        }




        public LinkClassifier(ref LocalRequestClient yamClient)
        {
            InitialiseCollection(ref yamClient, ref spamPhrases, dataManagerSpamPhrasesKey, true);
            InitialiseCollection(ref yamClient, ref blackSites, dataManagerBlackSitesKey);
            InitialiseCollection(ref yamClient, ref whiteSites, dataManagerWhiteSitesKey);
        }



        public void SyncData(ref LocalRequestClient yamClient)
        {
            var phrasesSb = new StringBuilder();
            var blackSb = new StringBuilder();
            var whiteSb = new StringBuilder();

            foreach (var phrase in spamPhrases)
            {
                phrasesSb.AppendLine(phrase);
            }
            foreach (var site in blackSites)
            {
                blackSb.AppendLine(site);
            }
            foreach (var site in whiteSites)
            {
                whiteSb.AppendLine(site);
            }

            yamClient.UpdateData("Pham", dataManagerSpamPhrasesKey, phrasesSb.ToString());
            yamClient.UpdateData("Pham", dataManagerBlackSitesKey, blackSb.ToString());
            yamClient.UpdateData("Pham", dataManagerWhiteSitesKey, whiteSb.ToString());
        }

        public Dictionary<string, LinkClassification> ClassifyLinks(Post post)
        {
            if (post == null || String.IsNullOrEmpty(post.Body)) { return null; }

            var matches = linkStripper.Matches(post.Body);
            var results = new Dictionary<string, LinkClassification>();

            foreach (Match match in matches)
            {
                if (String.IsNullOrEmpty(match.Value)) { continue; }

                var url = match.Value;
                var stripped = CleanUrl(url).ToLowerInvariant();

                if (results.ContainsKey(stripped)) { continue; }

                if (LinkUnshortifier.IsShortLink(url))
                {
                    url = LinkUnshortifier.UnshortifyLink(url);
                }

                var result = CheckLink(url, stripped);

                results.Add(stripped, result);
            }

            return results;
        }

        public void AddSpamPhrase(string phrase)
        {
            var lower = phrase.Trim().ToLowerInvariant();
            if (spamPhrases.Contains(lower)) { return; }
            spamPhrases.Add(lower);
        }

        public void RemoveSpamPhrase(string phrase)
        {
            var lower = phrase.Trim().ToLowerInvariant();
            if (!spamPhrases.Contains(lower)) { return; }
            spamPhrases.Remove(lower);
        }

        public void AddBlackSite(string url)
        {
            var clean = CleanUrl(url);
            if (blackSites.Contains(clean)) { return; }
            blackSites.Add(clean);
        }

        public void RemoveBlackSite(string url)
        {
            var clean = CleanUrl(url);
            if (!blackSites.Contains(clean)) { return; }
            blackSites.Remove(clean);
        }

        public void AddWhiteSite(string url)
        {
            var clean = CleanUrl(url);
            if (whiteSites.Contains(clean)) { return; }
            whiteSites.Add(clean);
        }

        public void RemoveWhiteSite(string url)
        {
            var clean = CleanUrl(url);
            if (!whiteSites.Contains(clean)) { return; }
            whiteSites.Remove(clean);
        }



        private LinkClassification CheckLink(string url, string cleanUrl)
        {
            if (checkedLinks.ContainsKey(cleanUrl))
            {
                return checkedLinks[cleanUrl];
            }
            if (SiteCollectionContins(blackSites, cleanUrl))
            {
                return new LinkClassification
                {
                    Type = LinkType.Spam,
                    BlackSiteFound = true
                };
            }
            if (SiteCollectionContins(whiteSites, cleanUrl) || ignoredFiles.IsMatch(cleanUrl))
            {
                return new LinkClassification
                {
                    Type = LinkType.Clean,
                    WhiteSiteFound = true
                };
            }

            var html = new StringDownloader().DownloadString(url);
            if (String.IsNullOrEmpty(html)) { return new LinkClassification { Type = LinkType.Clean }; }
            html = html.ToLowerInvariant();
            var classification = new LinkClassification
            {
                Type = LinkType.Clean,
                PhrasesFound = new Dictionary<string,int>()
            };

            foreach (var phrase in spamPhrases)
            {
                var phraseCount = GetPhraseCount(html, phrase);
                if (phraseCount > 0)
                {
                    if (!classification.PhrasesFound.ContainsKey(phrase))
                    {
                        classification.PhrasesFound[phrase] = phraseCount;
                    }
                    else
                    {
                        classification.PhrasesFound[phrase] += phraseCount;
                    }
                }
            }

            if (classification.PhrasesFound.Count > 0)
            {
                classification.Type = LinkType.Spam;
            }

            if (!checkedLinks.ContainsKey(cleanUrl))
            {
                checkedLinks[cleanUrl] = classification;

                if (checkedLinks.Count > 2500)
                {
                    LinkClassification temp;
                    checkedLinks.TryRemove("", out temp);
                }
            }

            return classification;
        }

        private int GetPhraseCount(string html, string phrase)
        {
            var count = 0;
            for (var i = 0; i < html.Length - phrase.Length;)
            {
                var isMatch = false;
                for (var j = 0; j < phrase.Length; j++)
                {
                    if (html[i + j] != phrase[j])
                    {
                        break;
                    }
                    else if (j == phrase.Length - 1)
                    {
                        isMatch = true;
                    }
                }

                if (isMatch)
                {
                    count++;
                    i += phrase.Length;
                }
                else
                {
                    i++;
                }
            }

            return count;
        }

        private bool SiteCollectionContins(HashSet<string> siteList, string url)
        {
            foreach (var site in siteList)
            {
                if (url.StartsWith(site))
                {
                    return true;
                }
            }

            return false;
        }

        private string CleanUrl(string url)
        {
            var clean = url;

            if (clean.StartsWith("http://"))
            {
                clean = clean.Remove(0, 7);
            }
            else if (clean.StartsWith("https://"))
            {
                clean = clean.Remove(0, 8);
            }
            if (clean.EndsWith("/"))
            {
                clean = clean.Substring(0, clean.Length - 1);
            }

            return clean;
        }

        private void InitialiseCollection(ref LocalRequestClient yamClient, ref HashSet<string> collection, string dataManagerKey, bool toLower = false)
        {
            if (!yamClient.DataExists("Pham", dataManagerKey)) { return; }

            var data = yamClient.RequestData("Pham", dataManagerKey);
            var split = data.Split('\n');

            foreach (var item in split)
            {
                if (String.IsNullOrEmpty(item)) { continue; }

                var trimmed = item.Trim();
                if (toLower)
                {
                    collection.Add(trimmed.ToLowerInvariant());
                }
                else
                {
                    collection.Add(trimmed);
                }
            }
        }
    }
}
