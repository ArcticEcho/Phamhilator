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
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Phamhilator.Yam.Core;

namespace Phamhilator.Yam.UI
{
    internal static class IPFetcher
    {
        private static Regex ipPattern = new Regex(@"\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}", RegexOptions.Compiled | RegexOptions.CultureInvariant);



        public static string FetchIP()
        {
            var html = new StringDownloader().DownloadString("https://www.google.co.uk/search?&q=what+is+my+ip&oq=what+is+my+ip");
            var ip = html.Substring(0, html.IndexOf("Your public IP address"));

            ip = ip.Remove(0, ip.Length - 150);
            ip = ipPattern.Match(ip).Value;

            return ip;
        }
    }
}
