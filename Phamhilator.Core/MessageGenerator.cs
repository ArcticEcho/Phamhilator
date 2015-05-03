/*
 * Phamhilator. A .Net based bot network catching spam/low quality posts for Stack Exchange.
 * Copyright © 2015, ArcticEcho.
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */





using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using ChatExchangeDotNet;

namespace Phamhilator.Pham.Core
{
    public static class ReportMessageGenerator
    {
        // TODO: Parse user badges


        private const string squareDescLink = "http://chat.meta.stackexchange.com/transcript/message/2998326";
        private static readonly Regex tpaReportRegex = new Regex(@"\s\(\d{1,3}(\.\d)?\%\)\:\s\[", RegexOptions.Compiled | RegexOptions.CultureInvariant);

        public static string GetQReport(QuestionAnalysis info, Question post)
        {
            if (info == null || post == null) { return null; }

            var name = PostFetcher.ChatEscapeString(post.AuthorName, "");
            var author = String.IsNullOrEmpty(post.AuthorLink) ? name : "[" + name + "](" + post.AuthorLink;
            var title = String.IsNullOrEmpty(post.Title) ? "`Unable to get post excerpt.`" : PostFetcher.ChatEscapeString(post.Title, "");
            var accuracy = "";
            var fullScanFailed = "";
            var postScore = "";

            accuracy = " (" + Math.Round(info.Accuracy, 1) + "%)";
            author = "[" + (post.AuthorRep >= 20 ? "■" : "□") + "](" + squareDescLink + ") " + author + " \"Rep: " + post.AuthorRep + "\")";
            postScore = " \"Score: " + post.Score + "\")";

            switch (info.Type)
            {
                case PostType.BadTagUsed:
                {
                    return InsertReportType(": " + FormatTags(info.BadTags) + "| [" + title + "](" + post.Url + postScore + ", by " + author + ", on `" + post.Site + "`.", info);
                }

                default:
                {
                    return InsertReportType(" **Q**" + accuracy + fullScanFailed + ": [" + title + "](" + post.Url + postScore + ", by " + author + ", on `" + post.Site + "`.", info);
                }
            }
        }

        public static string GetPostReport(PostAnalysis info, Post post, bool isQuestion = false)
        {
            if (info == null || post == null) { return null; }

            var name = PostFetcher.ChatEscapeString(post.AuthorName, "");
            var author = "[" + (post.AuthorRep >= 20 ? "■" : "□") + "](" + squareDescLink + ") " + (String.IsNullOrEmpty(post.AuthorLink) ? name : "[" + name + "](" + post.AuthorLink + " \"Rep: " + post.AuthorRep + "\")");
            var title = String.IsNullOrEmpty(post.Title) ? "`Unable to get post excerpt.`" : PostFetcher.ChatEscapeString(post.Title, " ");
            var accuracy = " (" + Math.Round(info.Accuracy, 1) + "%)";

            return InsertReportType((isQuestion ? " **Q**" : " **A**") + accuracy + ": [" + title + "](" + post.Url + " \"Score: " + post.Score + "\"), by " + author + ", on `" + post.Site + "`.", info);
        }

        public static string GetSecondaryRoomTpaReport(string reportContent, Message tpaMessage)
        {
            var tpaMessageLink = "http://chat." + tpaMessage.Host + "/transcript/message/" + tpaMessage.ID;
            return tpaReportRegex.Replace(reportContent, " ([`TPA`'d by " + tpaMessage.AuthorName + "](" + tpaMessageLink + ")): [");
        }

        public static string GetPrimaryRoomTpaReport(string reportContent)
        {
            return reportContent.Remove(reportContent.Length - 1) + " ***TPA Acknowledged***.";
        }

        public static string GetFPdReport(string reportContent)
        {
            return "---" + reportContent + "---";
        }

        public static string GetTPdReport(string reportContent)
        {
            return reportContent.Remove(reportContent.Length - 1) + " ***TP Acknowledged***.";
        }



        private static string InsertReportType(string reportMessage, PostAnalysis info)
        {
            switch (info.Type)
            {
                case PostType.Offensive:
                {
                    return "**Offensive**" + reportMessage;
                }

                case PostType.BadUsername:
                {
                    return "**Bad Username**" + reportMessage;
                }

                case PostType.BadTagUsed:
                {
                    return "**Bad Tag(s) Used**" + reportMessage;
                }

                case PostType.LowQuality:
                {
                    return "**Low Quality**" + reportMessage;
                }

                case PostType.Spam:
                {
                    return "**Spam**" + reportMessage;
                }
            }

            return reportMessage;
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
