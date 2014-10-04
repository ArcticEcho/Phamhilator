using System;
using System.Collections.Generic;



namespace Phamhilator
{
	public class Question : Post
	{
		private bool extraDataPopulated;
		private string body = "";
		private int score = int.MinValue;
		private int authorRep = int.MaxValue;
		private readonly List<Answer> answers = new List<Answer>();

		public List<string> Tags;

		public int Score
		{
			get
			{
				if (!extraDataPopulated && GlobalInfo.EnableFullScan)
				{
					PopulateExtraData();
				}

				return score;
			}
		}

		public string Body
		{
			get
			{
				if (!extraDataPopulated && GlobalInfo.EnableFullScan)
				{
					PopulateExtraData();
				}

				return body;
			}
		}

		public List<Answer> Answers
		{
			get
			{
				if (!extraDataPopulated && GlobalInfo.EnableFullScan)
				{
					PopulateExtraData();
				}

				return answers;
			}
		}

		public int AuthorRep
		{
			get
			{
				if (!extraDataPopulated && GlobalInfo.EnableFullScan)
				{
					PopulateExtraData();
				}

				return authorRep;
			}
		
		}

		private void PopulateExtraData()
		{
			try
			{
				var wb = new WebDownload();
				var html = wb.DownloadString(URL);

				body = HTMLScraper.GetQuestionBody(html);
				score = HTMLScraper.GetQuestionScore(html);

				wb.Dispose();

				extraDataPopulated = true;
			}
			catch (Exception)
			{
				score = int.MaxValue;
				body = "";
			}
		}
	}
}