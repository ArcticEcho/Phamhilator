using System.Collections.Generic;
using System.Net;
using System.Linq;
using System.Text.RegularExpressions;
using JsonFx.Json;
using System;



namespace Yamhilator
{
    public static class LinkUnshortifier
    {
        private static readonly Regex shortLink = new Regex(@"(?is)^https?://(goo\.gl|bit\.ly|tinyurl\.com|ow\.ly|tiny\.cc|bit\.do|po\.st|bigly\.us|t\.co|r\.im|cli\.gs|short\.ie|kl\.am|idek\.net|i\.gd|hex\.io)/\w*$", RegexOptions.Compiled | RegexOptions.CultureInvariant);



        public static bool IsShortLink(string url)
        {
            return !String.IsNullOrEmpty(url) && shortLink.IsMatch(url.Trim());
        }

        public static string UnshortifyLink(string url)
        {
            if (!IsShortLink(url)) { return url; }

            var trimmed = url.Trim();
            var res = new WebClient().DownloadString("http://urlex.org/json/" + trimmed);
            var data = new JsonReader().Read<Dictionary<string, string>>(res);

            return data.Values.First();
        }
    }
}
