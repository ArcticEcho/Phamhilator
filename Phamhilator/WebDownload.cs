using System;
using System.Net;



namespace Phamhilator
{
    public class WebDownload : WebClient
    {
        protected override WebRequest GetWebRequest(Uri uri)
        {
            var w = base.GetWebRequest(uri);

            w.Timeout = 10000; // 10 secs
            Encoding = System.Text.Encoding.UTF8;
            Proxy = null;
            ServicePointManager.DefaultConnectionLimit = int.MaxValue;

            return w;
        }
    }
}
