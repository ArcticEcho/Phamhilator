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





using System.Text;
using System.Linq;
using Phamhilator.Yam.Core;

namespace Phamhilator.Pham.Core
{
    public static class ReportCleaner
    {
        public static string GetCleanReport(int messageID)
        {
            var report = Stats.PostedReports.First(r => r.Message.ID == messageID);

            var oldTitle = PostFetcher.ChatEscapeString(report.Post.Title, " ");
            var newTitle = CensorString(report.Post.Title);

            var oldName = report.Post.AuthorName;
            var newName = CensorString(report.Post.AuthorName);

            return report.Message.Content.Replace(oldTitle, newTitle).Replace(oldName, newName);
        }



        private static string CensorString(string input)
        {
            var censored = new StringBuilder();
            var nonSpaceI = 0;

            for (var i = 0; i < input.Length; i++)
            {
                if (input[i] == ' ')
                {
                    censored.Append(' ');
                }
                else
                {
                    nonSpaceI++;
                    censored.Append(nonSpaceI % 2 == 0 ? '★' : '✩');
                }
            }

            return censored.ToString().Trim();
        }
    }
}
