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
