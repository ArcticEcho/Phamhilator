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
