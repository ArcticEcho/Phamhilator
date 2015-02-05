using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;



namespace Phamhilator.Core
{
    static class LinkStripper
    {
        private const RegexOptions regOpt = RegexOptions.Compiled | RegexOptions.CultureInvariant;
        private static readonly Regex linksRegex = new Regex(".*<a.*?href=\"|\".*", regOpt);
        private static readonly Regex linkStripperRegex = new Regex(@"(?<!https?:|/)/.*", regOpt);



        public static HashSet<string> GetLinks(string html, bool keepTLD = true, bool keepProtocol = false)
        {
            var links = linksRegex.Replace(html, "\n").Split('\n').Distinct().Where(l => !String.IsNullOrEmpty(l));
            var trimmedLinks = new HashSet<string>();

            foreach (var link in links)
            {
                var trimmedLink = linkStripperRegex.Replace(link, "");

                if (!keepTLD)
                {
                    trimmedLink = trimmedLink.Remove(trimmedLink.LastIndexOf('.'));
                }

                if (!keepProtocol)
                {
                    trimmedLink = trimmedLink.Remove(0, trimmedLink.IndexOf("//") + 2);
                }

                trimmedLinks.Add(trimmedLink);
            }

            return trimmedLinks;
        }
    }
}
