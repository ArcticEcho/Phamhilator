using System.IO;
using System.Net;
using System.Text;



namespace Phamhilator
{
	public static class StringDownloader
	{
		public static string DownloadString(string URL, int timeoutMilliseconds = 300000) // 5 min default.
		{
			var req = (HttpWebRequest)WebRequest.Create(URL);
			req.Timeout = timeoutMilliseconds;
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
