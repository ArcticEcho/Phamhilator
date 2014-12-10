using System;
using System.Collections.Generic;
using System.Text;



namespace Phamhilator
{
	public static class MessageGenerator
	{
		public static string GetQReport(QuestionAnalysis info, Question post)
		{
			var author = String.IsNullOrEmpty(post.AuthorLink) ? post.AuthorName : "[" + post.AuthorName + "](" + post.AuthorLink + ")";
            var title = String.IsNullOrEmpty(post.Title) ? "`Unable to get post excerpt.`" : post.Title;
            //var accuracy = "";
		    var fullScanFailed = "";

		    if (/*info.Accuracy == 0 && */post.PopulateExtraDataFailed)
		    {
		        fullScanFailed = " (FSF)";
		    }

            //if (info.Accuracy != 0 && !post.PopulateExtraDataFailed)
            //{
            //    accuracy = " (" + Math.Round(info.Accuracy, 1) + "%)";
            //}

            //if (info.Accuracy != 0 && post.PopulateExtraDataFailed)
            //{
            //    accuracy = " (" + Math.Round(info.Accuracy, 1) + "%";
            //    fullScanFailed = " FSF)";
            //}

			switch (info.Type)
			{
				case PostType.BadTagUsed:
				{
					return ": " + FormatTags(info.BadTags) + "| [" + title + "](" + post.Url + "), by " + author + ", on `" + post.Site + "`.";
				}

				default:
				{
					return " **Q**" + /*accuracy +*/ fullScanFailed + ": [" + title + "](" + post.Url + "), by " + author + ", on `" + post.Site + "`.";
				}
			}
		}

		public static string GetAReport(AnswerAnalysis info, Answer post)
		{
			var author = String.IsNullOrEmpty(post.AuthorLink) ? post.AuthorName : "[" + post.AuthorName + "](" + post.AuthorLink + ")";
			var title = String.IsNullOrEmpty(post.Title) ? "`Unable to get post excerpt.`" : post.Title;
            //var accuracy = info.Accuracy == 0 ? "" : " (" + Math.Round(info.Accuracy, 1) + "%)";

			return " **A**" + /*accuracy +*/ ": [" + title + "](" + post.Url + "), by " + author + ", on `" + post.Site + "`.";
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
