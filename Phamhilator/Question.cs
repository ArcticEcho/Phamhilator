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

		public bool PopulateExtraDataFailed;

		public List<string> Tags;

		public int Score
		{
			get
			{
				if (!extraDataPopulated && GlobalInfo.FullScanEnabled)
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
				if (!extraDataPopulated && GlobalInfo.FullScanEnabled)
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
				if (!extraDataPopulated && GlobalInfo.FullScanEnabled)
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
				if (!extraDataPopulated && GlobalInfo.FullScanEnabled)
				{
					PopulateExtraData();
				}

				return authorRep;
			}
		
		}



        public Question()
        {

        }

        public Question(string url, string title, string site, string authorName, string authorLink, List<string> tags)
        {
            Url = url;
            Title = title;
            Site = site;
            AuthorName = authorName;
            AuthorLink = authorLink;
            Tags = tags;
        }

        public Question(string url, string title, string body, string site, int score, string authorName, string authorLink, int authorRep, List<string> tags)
        {
            extraDataPopulated = true;
            Url = url;
            Title = title;
            this.body = body;
            Site = site;
            this.score = score;
            AuthorName = authorName;
            AuthorLink = authorLink;
            this.authorRep = authorRep;
            Tags = tags;
        }




		public void PopulateExtraData()
		{
			if (extraDataPopulated) { return; }

			try
			{		
				var html = StringDownloader.DownloadString(Url, 15000);

				body = HTMLScraper.GetQuestionBody(html);
				score = HTMLScraper.GetQuestionScore(html);
				authorRep = HTMLScraper.GetQuestionAuthorRep(html);

				var answerCount = HTMLScraper.GetAnswerCount(html);
				var currentHtml = html;

				for (var i = 0; i < answerCount; i++)
				{
					currentHtml = currentHtml.Remove(0, currentHtml.IndexOf("<div id=\"answer-", 50, StringComparison.Ordinal));

					answers.Add(GetAnswer(currentHtml));
				}								
			}
			catch (Exception)
			{
				PopulateExtraDataFailed = true;
			}
			
			extraDataPopulated = true;
		}



		private Answer GetAnswer(string html)
		{
			var aBody = HTMLScraper.GetAnswerBody(html);
            var excerpt = StripTags(aBody).Trim();

		    excerpt = excerpt.Length > 50 ? excerpt.Substring(0, 47) + "..." : excerpt;

		    var aLink = HTMLScraper.GetAnswerAuthorLink(html, Url);
		    var aName = HTMLScraper.GetAnswerAuthorName(html);
		    var aRep = HTMLScraper.GetAnswerAuthorRep(html);
		    var aUrl = HTMLScraper.GetAnswerLink(html, Url);
		    var aScore = HTMLScraper.GetAnswerScore(html);

		    return new Answer(aUrl, excerpt, aBody, Site, aScore, aName, aLink, aRep);
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