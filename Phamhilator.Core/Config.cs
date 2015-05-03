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
