using System;
using System.Collections.Generic;



namespace Phamhilator
{
	public class Question
	{
		private List<Answer> answers;
		private bool answersPopulated;
		private bool scoreBodyPopulated;
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
				if (!scoreBodyPopulated)
				{
					PopulateScoreAndBody();
				}

				return score;
			}
		}

		public string Body
		{
			get
			{
				if (!scoreBodyPopulated)
				{
					PopulateScoreAndBody();
				}

				return body;
			}
		}

		public List<Answer> Answers
		{
			get
			{
				if (!answersPopulated)
				{
					PopulateAnswers();
				}

				return answers;
			}
		}



		private void PopulateScoreAndBody()
		{
			try
			{
				var wb = new WebDownload();
				var html = wb.DownloadString(URL);

				body = HTMLScraper.GetQuestionBody(html);
				score = HTMLScraper.GetQuestionScore(html);

				wb.Dispose();

				scoreBodyPopulated = true;
			}
			catch (Exception)
			{
				score = int.MaxValue;
				body = "";
			}
		}

		private void PopulateAnswers()
		{
			
		}
	}
}