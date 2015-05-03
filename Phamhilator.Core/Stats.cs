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
using System.Globalization;
using System.IO;



namespace Phamhilator.Core
{
    public static class Stats
    {
        private static readonly HashSet<ReportedUser> reportedUsers = new HashSet<ReportedUser>();

        public static DateTime UpTime { get; set; }

        public static List<Report> PostedReports { get; set; }

        public static int PostsCaught { get; set; }

        public static float TotalCheckedPosts
        {
            get
            {
                return int.Parse(File.ReadAllText(DirectoryTools.GetTotalCheckedPostsFile()), CultureInfo.InvariantCulture);
            }

            set
            {
                File.WriteAllText(DirectoryTools.GetTotalCheckedPostsFile(), value.ToString(CultureInfo.InvariantCulture));
            }
        }

        public static float TotalTPCount
        {
            get
            {
                return int.Parse(File.ReadAllText(DirectoryTools.GetTotalTPCountFile()), CultureInfo.InvariantCulture);
            }

            set
            {
                File.WriteAllText(DirectoryTools.GetTotalTPCountFile(), value.ToString(CultureInfo.InvariantCulture));
            }
        }

        public static float TotalFPCount
        {
            get
            {
                return int.Parse(File.ReadAllText(DirectoryTools.GetTotalFPCountFile()), CultureInfo.InvariantCulture);
            }

            set
            {
                File.WriteAllText(DirectoryTools.GetTotalFPCountFile(), value.ToString(CultureInfo.InvariantCulture));
            }
        }

        public static int TermCount
        {
            get
            {
                var termCount = 0;

                foreach (var filter in Config.BlackFilters.Values)
                {
                    termCount += filter.Terms.Count;
                }

                foreach (var filter in Config.WhiteFilters.Values)
                {
                    termCount += filter.Terms.Count;
                }

                return termCount + Config.BadTags.Tags.Count;
            }
        }

        public static HashSet<ReportedUser> ReportedUsers
        {
            get
            {
                return reportedUsers;
            }
        }
    }
}
