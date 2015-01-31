using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Globalization;
using ChatExchangeDotNet;



namespace Phamhilator.Core
{
    public static class Config
    {
        public static Pham Core { get; set; }

        public static Room PrimaryRoom { get; set; }

        public static List<Room> SecondaryRooms { get; set; }

        public static Dictionary<FilterConfig, WhiteFilter> WhiteFilters { get; set; }

        public static Dictionary<FilterConfig, BlackFilter> BlackFilters { get; set; }

        public static BadTags BadTags { get; set; }

        public static UserAccess UserAccess { get; set; }

        public static BannedUsers BannedUsers { get; set; }

        public static ReportLog Log { get; set; }

        public static bool IsRunning { get; set; }

        public static bool Shutdown { get; set; }

        public static bool FullScanEnabled
        {
            get
            {
                return Boolean.Parse(File.ReadAllText(DirectoryTools.GetFullScanFile()));
            }

            set
            {
                File.WriteAllText(DirectoryTools.GetFullScanFile(), value.ToString(CultureInfo.InvariantCulture));
            }
        }

        public static float AccuracyThreshold
        {
            get
            {
                return float.Parse(File.ReadAllText(DirectoryTools.GetAccuracyThresholdFile()), CultureInfo.InvariantCulture);
            }

            set
            {
                File.WriteAllText(DirectoryTools.GetAccuracyThresholdFile(), value.ToString(CultureInfo.InvariantCulture));
            }
        }

        public static string Status
        {
            get
            {
                return File.ReadAllText(DirectoryTools.GetStatusFile());
            }

            set
            {
                File.WriteAllText(DirectoryTools.GetStatusFile(), value);
            }
        }
    }
}
