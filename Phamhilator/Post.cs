using System;
using System.Collections.Generic;



namespace Phamhilator
{
	public class Post
	{
		//private bool scoreBodyPopulated;
		private int score = int.MinValue;
		private string body = "";

		public string Title;
		public string AuthorName;
		public string AuthorLink;
		public string URL;
		public string Site;
		public List<string> Tags;

		public int Score
		{
			get
			{
				//if (!scoreBodyPopulated)
				//{
				//	PopulateScoreAndBody();
				//}

				return score;
			}
		}

		public string Body
		{
			get
			{
				//if (!scoreBodyPopulated)
				//{
				//	PopulateScoreAndBody();
				//}

				return body;
			}
		}



		//private void PopulateScoreAndBody()
		//{
		//	try
		//	{
		//		var wb = new WebDownload();
		//		var html = wb.DownloadString(URL);

		//		body = GetBody(html);
		//		score = GetScore(html);

		//		wb.Dispose();

		//		scoreBodyPopulated = true;
		//	}
		//	catch (Exception)
		//	{
		//		score = int.MinValue;
		//		body = "";
		//	}		
		//}

		//private string GetBody(string html)
		//{
		//	var startIndex = html.IndexOf("<p>", StringComparison.Ordinal) + 3;
		//	var endIndex = html.IndexOf("</div>", startIndex, StringComparison.Ordinal) - 11;

		//	var result = html.Substring(startIndex, endIndex - startIndex);

		//	return result;
		//}

		//private int GetScore(string html)
		//{
		//	var startIndex = html.IndexOf("vote-count-post", StringComparison.Ordinal) + 18;
		//	var endIndex = html.IndexOf("</span>", startIndex, StringComparison.Ordinal);

		//	var result = html.Substring(startIndex, endIndex - startIndex);

		//	return int.Parse(result);
		//}
	}
}