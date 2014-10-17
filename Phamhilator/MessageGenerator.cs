using System;
using System.Collections.Generic;
using System.Text;



namespace Phamhilator
{
	public static class MessageGenerator
	{
		public static string GetQReport(QuestionAnalysis info, Question post)
		{
			switch (info.Type)
			{
				case PostType.BadTagUsed:
				{
					return ": " + FormatTags(info.BadTags) + "| [" + post.Title + "](" + post.URL + "), by [" + post.AuthorName + "](" + post.AuthorLink + "), on `" + post.Site + "`.";
				}

				default:
				{
					return " **Q** (" + Math.Round(info.Accuracy, 1) + "%)" + ": [" + post.Title + "](" + post.URL + "), by [" + post.AuthorName + "](" + post.AuthorLink + "), on `" + post.Site + "`.";
				}
			}
		}

		public static string GetAReport(AnswerAnalysis info, Answer post)
		{
			return " **A** (" + Math.Round(info.Accuracy, 1) + "%)" + ": [" + post.Title + "](" + post.URL + "), by [" + post.AuthorName + "](" + post.AuthorLink + "), on `" + post.Site + "`.";
		}



		private static string FormatTags(Dictionary<string, string> tags)
		{
			var result = new StringBuilder();

			foreach (var tag in tags)
			{
				if (tag.Value == "")
				{
					result.Append("`[" + tag.Key + "]` ");
				}
				else
				{
					result.Append("[`[" + tag.Key + "]`](" + tag.Value + ") ");
				}
			}

			return result.ToString();
		}
	}
}
