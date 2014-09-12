namespace Phamhilator
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

			wb.Dispose();
		}



		private string GetBody(string html)
		{
			var startIndex = html.IndexOf("<p>", System.StringComparison.Ordinal) + 3;
			var endIndex = html.IndexOf("</div>", startIndex, System.StringComparison.Ordinal) - 11;

			var result = html.Substring(startIndex, endIndex - startIndex);

			return result;
		}

		private int GetScore(string html)
		{
			var startIndex = html.IndexOf("vote-count-post", System.StringComparison.Ordinal) + 18;
			var endIndex = html.IndexOf("</span>", startIndex, System.StringComparison.Ordinal);

			var result = html.Substring(startIndex, endIndex - startIndex);

			return int.Parse(result);
		}
	}
}