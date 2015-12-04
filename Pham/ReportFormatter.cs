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
using ChatExchangeDotNet;
using Phamhilator.Yam.Core;

namespace Phamhilator.Pham.UI
{
    static class ReportFormatter
    {
        private const string editUrl = "https://github.com/ArcticEcho/Phamhilator/wiki/Pham-Report-Types#edit";
        private const string closeUrl = "https://github.com/ArcticEcho/Phamhilator/wiki/Pham-Report-Types#close";
        private const string deleteUrl = "https://github.com/ArcticEcho/Phamhilator/wiki/Pham-Report-Types#delete";



        public static string FormatReport(Post post, ClassificationResults results)
        {
            if (post == null || results == null ||
                results.Action == ClassificationResults.SuggestedAction.Nothing)
            {
                return null;
            }

            var msg = new MessageBuilder();
            var resData = $"Severity: {Math.Round(results.Severity * 100)}%. " +
                          $"Similarity: {Math.Round(results.Similarity * 100)}%";

            switch (results.Action)
            {
                case ClassificationResults.SuggestedAction.Edit:
                {
                    msg.AppendLink("Edit", editUrl, resData, results.Severity >= 0.75 ?
                        TextFormattingOptions.Bold :
                        TextFormattingOptions.None,
                        WhiteSpace.None);
                    break;
                }
                case ClassificationResults.SuggestedAction.Close:
                {
                    msg.AppendLink("Close", closeUrl, resData, results.Severity >= 0.75 ?
                        TextFormattingOptions.Bold :
                        TextFormattingOptions.None,
                        WhiteSpace.None);
                    break;
                }
                case ClassificationResults.SuggestedAction.Delete:
                {
                    msg.AppendLink("Delete", deleteUrl, resData, results.Severity >= 0.75 ?
                        TextFormattingOptions.Bold :
                        TextFormattingOptions.None,
                        WhiteSpace.None);
                    break;
                }
            }

            msg.AppendText(": ");
            msg.AppendLink(post.Title, post.Url, "Score: " + post.Score, TextFormattingOptions.None, WhiteSpace.None);
            msg.AppendText(", by ");
            msg.AppendLink(post.AuthorName, post.AuthorLink, "Reputation: " + post.AuthorRep, TextFormattingOptions.None, WhiteSpace.None);
            msg.AppendText(".");

            return msg.ToString();
        }
    }
}
