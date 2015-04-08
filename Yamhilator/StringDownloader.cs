using System.IO;
using System.Net;
using System.Text;



namespace Yamhilator
{
    public class StringDownloader
    {
        private readonly int timeout;



        public StringDownloader(int timeoutMilliseconds = 300000) // 5 min default.
        {
            timeout = timeoutMilliseconds;
        }



        public string DownloadString(string url)
        {
            var req = (HttpWebRequest)WebRequest.Create(url);
            req.Timeout = timeout;
            req.Proxy = null;

            using (var res = req.GetResponse())
            using (var stream = res.GetResponseStream())
            using (var sr = new StreamReader(stream, Encoding.UTF8))
            {
                return sr.ReadToEnd();
            }
        }
    }
}
