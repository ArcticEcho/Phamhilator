using System;
using System.Collections.Generic;



namespace Phamhilator
{
	public class Question : Post
	{
		private bool extraDataPopulated;
		private string body = "";
		private int score;
		private int authorRep;
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
				using (var wb = new WebDownload())
				{
					var html = wb.DownloadString(URL);

					body = HTMLScraper.GetQuestionBody(html);
					score = HTMLScraper.GetQuestionScore(html);
					authorRep = HTMLScraper.GetQuestionAuthorRep(html);

					var answerCount = HTMLScraper.GetAnswerCount(html);
					var currentHTML = html;

					for (var i = 0; i < answerCount; i++)
					{
						currentHTML = currentHTML.Remove(0, currentHTML.IndexOf("<div id=\"answer-", 50, StringComparison.Ordinal));

						answers.Add(GetAnswer(currentHTML));
					}				
				}		
			}
			catch (Exception)
			{

			}
			
			extraDataPopulated = true;
		}

		private Answer GetAnswer(string html)
		{
			var aBody = HTMLScraper.GetAnswerBody(html);

			return new Answer
			{
				AuthorLink = HTMLScraper.GetAnswerAuthorLink(html, URL), 
				AuthorName = HTMLScraper.GetAnswerAuthorName(html), 
				AuthorRep = HTMLScraper.GetAnswerAuthorRep(html), 
				URL = HTMLScraper.GetAnswerLink(html, URL), 
				Score = HTMLScraper.GetAnswerScore(html), 
				Body = aBody, 
				Site = Site, 
				Title = StripTags(aBody.Length > 50 ? aBody.Substring(0, 47) + "..." : aBody).Trim()
			};
		}

		private static string StripTags(string source)
		{
			var array = new char[source.Length];
			var arrayIndex = 0;
			var inside = false;

			for (int i = 0; i < source.Length; i++)
			{
				var let = source[i];

				if (let == '<')
				{
					inside = true;

					continue;
				}

				if (let == '>')
				{
					inside = false;

					continue;
				}

				if (!inside)
				{
					array[arrayIndex] = let;
					arrayIndex++;
				}
			}

			return new string(array, 0, arrayIndex);
		}
	}
}