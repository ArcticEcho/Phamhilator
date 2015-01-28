using System;
using System.Collections.Generic;
using System.Text;



namespace Phamhilator
{
    public static class MessageGenerator
    {
        public static string GetQReport(QuestionAnalysis info, Question post)
        {
            if (info == null || post == null) { return null; }

            var name = PostFetcher.EscapeString(post.AuthorName, "");
            var author = String.IsNullOrEmpty(post.AuthorLink) ? name : "[" + name + "](" + post.AuthorLink;
            var title = String.IsNullOrEmpty(post.Title) ? "`Unable to get post excerpt.`" : PostFetcher.EscapeString(post.Title, "");
            var accuracy = "";
            var fullScanFailed = "";
            var postScore = "";

            if (post.PopulateExtraDataFailed)
            {
                if (info.Accuracy == 0)
                {
                    fullScanFailed = " (FSF)";
                }
                else
                {
                    accuracy = " (" + Math.Round(info.Accuracy, 1) + "%";
                    fullScanFailed = " FSF)";
                }

                author += ")";
                postScore = ")";
            }
            else
            {
                accuracy = " (" + Math.Round(info.Accuracy, 1) + "%)";
                author += " \"Rep: " + post.AuthorRep + "\")";
                postScore = " \"Score: " + post.Score + "\")";
            }

            switch (info.Type)
            {
                case PostType.BadTagUsed:
                {
                    return ": " + FormatTags(info.BadTags) + "| [" + title + "](" + post.Url + postScore + ", by " + author + ", on `" + post.Site + "`.";
                }

                default:
                {
                    return " **Q**" + accuracy + fullScanFailed + ": [" + title + "](" + post.Url + postScore + ", by " + author + ", on `" + post.Site + "`.";
                }
            }
        }

        public static string GetPostReport(PostAnalysis info, Post post, bool isQuestion = false)
        {
            if (info == null || post == null) { return null; }

            var name = PostFetcher.EscapeString(post.AuthorName, "");
            var author = String.IsNullOrEmpty(post.AuthorLink) ? name : "[" + name + "](" + post.AuthorLink + " \"Rep: " + post.AuthorRep + "\")";
            var title = String.IsNullOrEmpty(post.Title) ? "`Unable to get post excerpt.`" : PostFetcher.EscapeString(post.Title, " ");
            var accuracy = " (" + Math.Round(info.Accuracy, 1) + "%)";

            return (isQuestion ? " **Q**" : " **A**") + accuracy + ": [" + title + "](" + post.Url + " \"Score: " + post.Score + "\"), by " + author + ", on `" + post.Site + "`.";
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
