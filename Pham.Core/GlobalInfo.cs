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





//using System;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.Globalization;
//using System.IO;
//using ChatExchangeDotNet;

//namespace Phamhilator.Pham.Core
//{
//    public static partial class GlobalInfo
//    {
//        private static string commitHash;
//        private static string commitFormatted;
//        private readonly static List<User> owners = new List<User>();

//        #region Filters

//        public static readonly Dictionary<FilterType, BlackFilter> BlackFilters = new Dictionary<FilterType, BlackFilter>()
//        {
//            { FilterType.QuestionTitleBlackName, new BlackFilter(FilterType.QuestionTitleBlackName) },
//            { FilterType.QuestionTitleBlackOff, new BlackFilter(FilterType.QuestionTitleBlackOff) },
//            { FilterType.QuestionTitleBlackSpam, new BlackFilter(FilterType.QuestionTitleBlackSpam) },
//            { FilterType.QuestionTitleBlackLQ, new BlackFilter(FilterType.QuestionTitleBlackLQ) },

//            { FilterType.QuestionBodyBlackSpam, new BlackFilter(FilterType.QuestionBodyBlackSpam) },
//            { FilterType.QuestionBodyBlackLQ, new BlackFilter(FilterType.QuestionBodyBlackLQ) },
//            { FilterType.QuestionBodyBlackOff, new BlackFilter(FilterType.QuestionBodyBlackOff) },

//            { FilterType.AnswerBlackSpam, new BlackFilter(FilterType.AnswerBlackSpam) },
//            { FilterType.AnswerBlackLQ, new BlackFilter(FilterType.AnswerBlackLQ) },
//            { FilterType.AnswerBlackOff, new BlackFilter(FilterType.AnswerBlackOff) },
//            { FilterType.AnswerBlackName, new BlackFilter(FilterType.AnswerBlackName) }
//        };

//        public static readonly Dictionary<FilterType, WhiteFilter> WhiteFilters = new Dictionary<FilterType, WhiteFilter>()
//        {
//            { FilterType.QuestionTitleWhiteName, new WhiteFilter(FilterType.QuestionTitleWhiteName) },
//            { FilterType.QuestionTitleWhiteOff, new WhiteFilter(FilterType.QuestionTitleWhiteOff) },
//            { FilterType.QuestionTitleWhiteSpam, new WhiteFilter(FilterType.QuestionTitleWhiteSpam) },
//            { FilterType.QuestionTitleWhiteLQ, new WhiteFilter(FilterType.QuestionTitleWhiteLQ) },

//            { FilterType.QuestionBodyWhiteSpam, new WhiteFilter(FilterType.QuestionBodyWhiteSpam) },
//            { FilterType.QuestionBodyWhiteLQ, new WhiteFilter(FilterType.QuestionBodyWhiteLQ) },
//            { FilterType.QuestionBodyWhiteOff, new WhiteFilter(FilterType.QuestionBodyWhiteOff) },

//            { FilterType.AnswerWhiteSpam, new WhiteFilter(FilterType.AnswerWhiteSpam) },
//            { FilterType.AnswerWhiteLQ, new WhiteFilter(FilterType.AnswerWhiteLQ) },
//            { FilterType.AnswerWhiteOff, new WhiteFilter(FilterType.AnswerWhiteOff) },
//            { FilterType.AnswerWhiteName, new WhiteFilter(FilterType.AnswerWhiteName) }
//        };

//        #endregion

//        public readonly static Dictionary<int, MessageInfo> PostedReports = new Dictionary<int, MessageInfo>(); // Message ID, actual message.
//        public readonly static BadTagDefinitions BadTagDefinitions = new BadTagDefinitions();
//        public readonly static List<Spammer> Spammers = new List<Spammer>();
//        public readonly static ReportLog Log = new ReportLog();
//        public readonly static Pham Core = new Pham();
//        public static Room PrimaryRoom;
//        public static Client ChatClient;
//        public static int PostsCaught;
//        public static DateTime UpTime;
//        public static bool BotRunning;
//        public static bool Shutdown;

//        public static bool FullScanEnabled
//        {
//            get
//            {
//                return Boolean.Parse(File.ReadAllText(DirectoryTools.GetEnableFullScanFile()));
//            }

//            set
//            {
//                File.WriteAllText(DirectoryTools.GetEnableFullScanFile(), value.ToString(CultureInfo.InvariantCulture));
//            }
//        }

//        public static float AccuracyThreshold
//        {
//            get
//            {
//                return float.Parse(File.ReadAllText(DirectoryTools.GetAccuracyThresholdFile()), CultureInfo.InvariantCulture);
//            }

//            set
//            {
//                File.WriteAllText(DirectoryTools.GetAccuracyThresholdFile(), value.ToString(CultureInfo.InvariantCulture));
//            }
//        }

//        public static int TermCount
//        {
//            get
//            {
//                var termCount = 0;

//                foreach (var filter in BlackFilters.Values)
//                {
//                    termCount += filter.Terms.Count;
//                }

//                foreach (var filter in WhiteFilters.Values)
//                {
//                    termCount += filter.Terms.Count;
//                }

//                return termCount + BadTagDefinitions.BadTags.Count;
//            }
//        }

//        public static string Status
//        {
//            get
//            {
//                return File.ReadAllText(DirectoryTools.GetStatusFile());
//            }

//            set
//            {
//                File.WriteAllText(DirectoryTools.GetStatusFile(), value);
//            }
//        }

//        public static string CommitFormatted
//        {
//            get
//            {
//                UpdateCommitInfo();

//                return commitFormatted;
//            }
//        }

//        public static string CommitHash
//        {
//            get
//            {
//                UpdateCommitInfo();

//                return commitHash;
//            }
//        }

//        public static string OwnerNames
//        {
//            get
//            {
//                if (owners.Count == 0)
//                {
//                    PopulateOwners();
//                }

//                var names = "";

//                for (var i = 0; i < owners.Count; i++)
//                {
//                    if (i == owners.Count - 2)
//                    {
//                        names += owners[i].Name + " & ";
//                    }
//                    else if (i == owners.Count - 1)
//                    {
//                        names += owners[i].Name;
//                    }
//                    else
//                    {
//                        names += owners[i].Name + ", ";
//                    }
//                }

//                return names;
//            }
//        }

//        public static List<User> Owners
//        {
//            get
//            {
//                if (owners.Count == 0)
//                {
//                    PopulateOwners();
//                }

//                return owners;
//            }
//        }



//        private static void PopulateOwners()
//        {
//            var sam = PrimaryRoom.GetUser(227577);
//            var uni = PrimaryRoom.GetUser(266094);
//            var fox = PrimaryRoom.GetUser(229438);
//            var jan = PrimaryRoom.GetUser(194047);
//            var pat = PrimaryRoom.GetUser(245360);
//            var moo = PrimaryRoom.GetUser(202832);

//            owners.Add(sam);
//            owners.Add(uni);
//            owners.Add(fox);
//            owners.Add(jan);
//            owners.Add(pat);
//            owners.Add(moo);
//        }

//        private static void UpdateCommitInfo()
//        {
//            //string output;

//            //using (var p = new Process())
//            //{
//            //    p.StartInfo = new ProcessStartInfo
//            //    {
//            //        FileName = "git",
//            //        Arguments = "log --pretty=format:\"[`%h` *`(%s by %cn)`*]\" -n 1",
//            //        WorkingDirectory = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).FullName,
//            //        UseShellExecute = false,
//            //        RedirectStandardOutput = true,
//            //        CreateNoWindow = true
//            //    };

//            //    p.Start();

//            //    output = p.StandardOutput.ReadToEnd();

//            //    p.WaitForExit();
//            //}



//            //var psi = new ProcessStartInfo
//            //{
//            //    FileName = "cmd.exe",
//            //    Arguments = "/C git log --pretty=format:\"[`%h` *`(%s by %cn)`*]\" -n 1",
//            //    WorkingDirectory = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).FullName,
//            //    UseShellExecute = false,
//            //    RedirectStandardOutput = true,
//            //    CreateNoWindow = true
//            //};

//            //var output = Process.Start(psi).StandardOutput.ReadToEnd();

//            //commitFormatted = output;
//            //commitHash = output.Split('`')[1];
//        }
//    }
//}