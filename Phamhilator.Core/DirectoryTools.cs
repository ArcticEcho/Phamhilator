using System;
using System.IO;



namespace Phamhilator.Core
{
    public static class DirectoryTools
    {
        private static readonly string root = Path.GetDirectoryName(new Uri(System.Reflection.Assembly.GetExecutingAssembly().CodeBase).LocalPath);
        private static readonly string configPath = Path.Combine(root, "Config");



        public static string GetFilterFile(FilterConfig filter)
        {
            var path = root;

            if (filter.Class.IsQuestion())
            {
                path = Path.Combine(path, "Question");

                if (filter.Class.IsQuestionTitle())
                {
                    path = Path.Combine(path, "Title");
                    path = AddFilterType(path, filter.Type);
                }
                else
                {
                    path = Path.Combine(path, "Body");
                    path = AddFilterType(path, filter.Type);
                }
            }
            else
            {
                path = Path.Combine(path, "Answer");
                path = AddFilterType(path, filter.Type);
            }

            if (filter.Type == FilterType.Black)
            {
                path = AddBlackFilterSubclass(path, filter.Class);
            }
            else
            {
                path = AddWhiteFilterSubclass(path, filter.Class);
            }

            return path;
        }

        # region Misc files

        public static string GetBadTagsFolder()
        {
            var path = Path.Combine(root, "Bad Tags");

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            return path;
        }

        public static string GetLogFile()
        {
            var path = Path.Combine(configPath, "Log.txt");

            if (!File.Exists(path))
            {
                File.Create(path).Dispose();
            }

            return path;
        }

        public static string GetPrivUsersFile()
        {
            var path = Path.Combine(configPath, "Priv Users.txt");

            if (!File.Exists(path))
            {
                File.Create(path).Dispose();
            }

            return path;
        }

        public static string GetAccuracyThresholdFile()
        {
            var path = Path.Combine(configPath, "Accuracy Threshold.txt");

            if (!File.Exists(path))
            {
                File.WriteAllText(path, "15"); // Default threshold = 15%
            }

            return path;
        }

        public static string GetFullScanFile()
        {
            var path = Path.Combine(configPath, "Full Scan.txt");

            if (!File.Exists(path))
            {
                File.WriteAllText(path, "false"); // Disabled by default.
            }

            return path;
        }

        public static string GetBannedUsersFile()
        {
            return Path.Combine(configPath, "Banned Users.txt");
        }

        public static string GetStatusFile()
        {
            var path = Path.Combine(configPath, "Status.txt");

            if (!File.Exists(path))
            {
                File.WriteAllText(path, "Beta");
            }

            return path;
        }

        public static string GetTotalCheckedPostsFile()
        {
            var path = Path.Combine(configPath, "Stats", "Total Checked Posts.txt");

            if (!Directory.Exists(Path.Combine(configPath, "Stats")))
            {
                Directory.CreateDirectory(Path.Combine(configPath, "Stats"));
            }

            if (!File.Exists(path))
            {
                File.WriteAllText(path, "0");
            }

            return path;
        }

        public static string GetTotalTPCountFile()
        {
            var path = Path.Combine(configPath, "Stats", "Total TP Count.txt");

            if (!Directory.Exists(Path.Combine(configPath, "Stats")))
            {
                Directory.CreateDirectory(Path.Combine(configPath, "Stats"));
            }

            if (!File.Exists(path))
            {
                File.WriteAllText(path, "0");
            }

            return path;
        }

        public static string GetTotalFPCountFile()
        {
            var path = Path.Combine(configPath, "Stats", "Total FP Count.txt");

            if (!Directory.Exists(Path.Combine(configPath, "Stats")))
            {
                Directory.CreateDirectory(Path.Combine(configPath, "Stats"));
            }

            if (!File.Exists(path))
            {
                File.WriteAllText(path, "0");
            }

            return path;
        }

        public static string GetCredsFile()
        {
            var path = Path.Combine(configPath, "Creds.txt");

            if (!File.Exists(path))
            {
                File.Create(path).Dispose();
            }

            return path;
        }

        # endregion



        private static string AddFilterType(string path, FilterType type)
        {
            if (type == FilterType.Black)
            {
                return Path.Combine(path, "Black Filter Terms");
            }
            
            return Path.Combine(path, "White Filter Terms");
        }

        private static string AddBlackFilterSubclass(string path, FilterClass classification)
        {
            switch (classification.ToPostType())
            {
                case PostType.LowQuality:
                {
                    return Path.Combine(path, "LQ Terms.txt");
                }
                case PostType.BadUsername:
                {
                    return Path.Combine(path, "Bad Username Terms.txt");
                }
                case PostType.Offensive:
                {
                    return Path.Combine(path, "Offensive Terms.txt");
                }
                case PostType.Spam:
                {
                    return Path.Combine(path, "Spam Terms.txt");
                }
                default:
                {
                    throw new NotSupportedException();
                }
            }
        }

        private static string AddWhiteFilterSubclass(string path, FilterClass classification)
        {
            switch (classification.ToPostType())
            {
                case PostType.LowQuality:
                {
                    return Path.Combine(path, "LQ");
                }
                case PostType.BadUsername:
                {
                    return Path.Combine(path, "Bad Username");
                }
                case PostType.Offensive:
                {
                    return Path.Combine(path, "Offensive");
                }
                case PostType.Spam:
                {
                    return Path.Combine(path, "Spam");
                }
                default:
                {
                    throw new NotSupportedException();
                }
            }
        }
    }
}
