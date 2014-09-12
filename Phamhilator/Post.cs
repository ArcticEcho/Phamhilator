using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace Yekoms
{
	public struct Post
	{
		public string Title;
		public string AuthorName;
		public string AuthorLink;
		public string URL;
		public string Site;
		public int Score;
		public string Body;



		public void PopulateScoreAndBody()
		{
			var wb = new System.Net.WebClient();
			var html = wb.DownloadString(URL);

			Body = GetBody(html);
			Score = GetScore(html);
		}



		private string GetBody(string html)
		{
			var startIndex = html.IndexOf("<p>") + 3;
			var endIndex = html.IndexOf("</div>", startIndex) - 11;

			var result = html.Substring(startIndex, endIndex - startIndex);

			return result;
		}

		private int GetScore(string html)
		{
			var startIndex = html.IndexOf("vote-count-post") + 18;
			var endIndex = html.IndexOf("</span>", startIndex);

			var result = html.Substring(startIndex, endIndex - startIndex);

			return int.Parse(result);
		}
	}
}