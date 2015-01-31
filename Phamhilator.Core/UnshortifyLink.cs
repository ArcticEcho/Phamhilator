using System.Net;
using System.Text.RegularExpressions;



namespace Phamhilator.Core
{
    public static class UnshortifyLink
    {
        private static readonly Regex shortLink = new Regex(@"(?is)^https?://(goo\.gl|bit\.ly|tinyurl\.com|ow\.ly|tiny\.cc|bit\.do|po\.st|bigly\.us|t\.co|r\.im|cli\.gs|short\.ie|kl\.am|idek\.net|i\.gd|hex\.io)/\w*$", RegexOptions.Compiled | RegexOptions.CultureInvariant);



        public static bool IsShortLink(string url)
        {
            var trimmed = url.Trim();

            return shortLink.IsMatch(trimmed);
        }

        public static string UnshortenLink(string url)
        {
            if (!IsShortLink(url)) { return url; }

            var trimmed = url.Trim();

            var res = new WebClient().DownloadString("http://api.unshort.tk/index.php?u=" + trimmed);

            var longUrl = res.Remove(0, res.IndexOf("\":\"http", System.StringComparison.Ordinal) + 3);

            return longUrl.Substring(0, longUrl.Length - 2).Replace(@"\/", @"/");
        }
    }
}
