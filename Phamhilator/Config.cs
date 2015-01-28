using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Globalization;
using ChatExchangeDotNet;



namespace Phamhilator
{
    public static class Config
    {
        public static Pham Core { get; internal set; }

        public static Room PrimaryRoom { get; internal set; }

        public static List<Room> SecondaryRooms { get; internal set; }

        public static Dictionary<FilterConfig, WhiteFilter> WhiteFilters { get; internal set; }

        public static Dictionary<FilterConfig, BlackFilter> BlackFilters { get; internal set; }

        public static BadTags BadTags { get; internal set; }

        public static UserAccess UserAccess { get; internal set; }

        public static BannedUsers BannedUsers { get; internal set; }

        public static ReportLog Log { get; internal set; }

        public static bool IsRunning { get; internal set; }

        public static bool Shutdown { get; internal set; }

        public static bool FullScanEnabled
        {
            get
            {
                return Boolean.Parse(File.ReadAllText(DirectoryTools.GetFullScanFile()));
            }

            internal set
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

            internal set
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

            internal set
            {
                File.WriteAllText(DirectoryTools.GetStatusFile(), value);
            }
        }
    }
}
