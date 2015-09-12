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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Phamhilator.Yam.Core;
using ServiceStack.Text;

namespace Phamhilator.Yam.UI
{
    public static class PostLogger
    {
        private const string dataManagerLogKey = "Post Log";
        private static ManualResetEvent loggerIntervalMre;
        private static ManualResetEvent loggerStoppedMre;
        private static Thread loggerThread;
        private static bool stop;

        public delegate void EntryEventHandler(LogEntry entry);
        public static event EntryEventHandler EntryAdded;
        public static event EntryEventHandler EntryRemoved;

        public static ConcurrentDictionary<uint, LogEntry> Log { get; private set; }

        public static int LogSizeUncompressed { get; private set; }

        public static int LogSizeCompressed { get; private set; }

        /// <summary>
        /// The interval at which to flush the log to disk (default set to 2 minutes).
        /// </summary>
        public static TimeSpan UpdateInterval { get; set; }



        public static void InitialiseLogger()
        {
            if (DataManager.DataExists("Yam", dataManagerLogKey))
            {
                var bytes = DataManager.LoadRawData("Yam", dataManagerLogKey);
                var uncompData = DataUtilities.GZipDecompress(bytes);
                var json = Encoding.UTF8.GetString(uncompData);
                Log = JsonSerializer.DeserializeFromString<ConcurrentDictionary<uint, LogEntry>>(json);
            }
            else
            {
                Log = new ConcurrentDictionary<uint, LogEntry>();
            }

            loggerIntervalMre = new ManualResetEvent(false);
            loggerStoppedMre = new ManualResetEvent(false);
            UpdateInterval = TimeSpan.FromMinutes(2);
            loggerThread = new Thread(LoggerLoop) { IsBackground = true };
            loggerThread.Start();
        }

        public static void StopLogger()
        {
            stop = true;
            loggerIntervalMre.Set();
            loggerStoppedMre.WaitOne();
        }

        public static void EnqueuePost(bool isQuestion, Post post)
        {
            if (post == null) { throw new ArgumentNullException("item"); }

            var entry = new LogEntry
            {
                Post = post,
                IsQuestion = isQuestion,
                Timestamp = DateTime.UtcNow
            };

            if (Log.Values.Contains(entry)) { return; }

            if (Log.Count == 0)
            {
                Log[0] = entry;
            }
            else
            {
                var index = Log.Keys.Max() + 1;
                Log[index] = entry;
            }

            if (EntryAdded == null) { return; }
            EntryAdded(entry);
        }

        public static LogEntry[] SearchLog(RemoteLogRequest req, int maxEntries)
        {
            var regexes = GetRemoteLogReqRegexes(req);
            var entries = new LogEntry[0];

            if (regexes.ContainsKey("Site"))
            {
                entries = Log.Values.Where(entry => regexes["Site"].IsMatch(entry.Post.Site)).ToArray();
            }
            if (regexes.ContainsKey("Title"))
            {
                entries = entries.Where(entry => regexes["Title"].IsMatch(entry.Post.Title)).ToArray();
            }
            if (regexes.ContainsKey("Body"))
            {
                entries = entries.Where(entry => regexes["Body"].IsMatch(entry.Post.Body)).ToArray();
            }
            if (regexes.ContainsKey("AuthorName"))
            {
                entries = entries.Where(entry => regexes["AuthorName"].IsMatch(entry.Post.AuthorName)).ToArray();
            }
            if (regexes.ContainsKey("AuthorNetworkID"))
            {
                entries = entries.Where(entry => regexes["AuthorNetworkID"].IsMatch(entry.Post.AuthorNetworkID.ToString())).ToArray();
            }

            entries = FilterByPostTypeAndTime(entries, req);
            entries = FilterByNumericalProperties(entries, req);

            var trimmed = new HashSet<LogEntry>();
            foreach (var e in entries)
            {
                if (trimmed.Count < maxEntries)
                {
                    trimmed.Add(e);
                }
                else
                {
                    break;
                }
            }

            return trimmed.ToArray();
        }

        private static Dictionary<string, Regex> GetRemoteLogReqRegexes(RemoteLogRequest req)
        {
            var regexes = new Dictionary<string, Regex>();
            if (req == null) { return regexes; }

            if (!string.IsNullOrWhiteSpace(req.Site))
            {
                if (!req.Site.IsValidRegex())
                {
                    throw new Exception("Invalid regex supplied for search field: Site.");
                }

                var reg = new Regex(req.Site, RegexOptions.Compiled | RegexOptions.CultureInvariant);

                if (reg.IsReDoS())
                {
                    throw new Exception("ReDoS pattern detected for search field: Site.");
                }

                regexes.Add("Site", reg);
            }
            if (!string.IsNullOrWhiteSpace(req.Title))
            {
                if (!req.Title.IsValidRegex())
                {
                    throw new Exception("Invalid regex supplied for search field: Title.");
                }

                var reg = new Regex(req.Title, RegexOptions.Compiled | RegexOptions.CultureInvariant);

                if (reg.IsReDoS())
                {
                    throw new Exception("ReDoS pattern detected for search field: Title.");
                }

                regexes.Add("Title", reg);

            }
            if (!string.IsNullOrWhiteSpace(req.Body))
            {
                if (!req.Body.IsValidRegex())
                {
                    throw new Exception("Invalid regex supplied for search field: Body.");
                }

                var reg = new Regex(req.Body, RegexOptions.Compiled | RegexOptions.CultureInvariant);

                if (reg.IsReDoS())
                {
                    throw new Exception("ReDoS pattern detected for search field: Body.");
                }

                regexes.Add("Body", reg);
            }
            if (!string.IsNullOrWhiteSpace(req.AuthorName))
            {
                if (!req.AuthorName.IsValidRegex())
                {
                    throw new Exception("Invalid regex supplied for search field: AuthorName.");
                }

                var reg = new Regex(req.AuthorName, RegexOptions.Compiled | RegexOptions.CultureInvariant);

                if (reg.IsReDoS())
                {
                    throw new Exception("ReDoS pattern detected for search field: AuthorName.");
                }

                regexes.Add("AuthorName", reg);
            }
            if (!string.IsNullOrWhiteSpace(req.AuthorNetworkID))
            {
                if (!req.AuthorNetworkID.IsValidRegex())
                {
                    throw new Exception("Invalid regex supplied for search field: AuthorNetworkID.");
                }

                var reg = new Regex(req.AuthorNetworkID, RegexOptions.Compiled | RegexOptions.CultureInvariant);

                if (reg.IsReDoS())
                {
                    throw new Exception("ReDoS pattern detected for search field: AuthorNetworkID.");
                }

                regexes.Add("AuthorNetworkID", reg);
            }

            return regexes;
        }

        private static LogEntry[] FilterByNumericalProperties(LogEntry[] entries, RemoteLogRequest req)
        {
            var filtered = entries;

            if (!string.IsNullOrWhiteSpace(req.AuthorRep))
            {
                var predicate = GetNumericalPropertyPredicate(req.AuthorRep);

                filtered = filtered.Where(e => predicate(e.Post.AuthorRep)).ToArray();
            }

            if (!string.IsNullOrWhiteSpace(req.Score))
            {
                var predicate = GetNumericalPropertyPredicate(req.Score);

                filtered = filtered.Where(e => predicate(e.Post.Score)).ToArray();
            }

            return filtered;
        }

        private static Func<int, bool> GetNumericalPropertyPredicate(string expression)
        {
            var ex = expression.Trim();
            var exVal = 0;

            if (string.IsNullOrWhiteSpace(expression) ||
                !int.TryParse(ex.Remove(0, ex[1] == '=' ? 2 : 1), out exVal))
            {
                throw new Exception("Invalid mathematical expression for property: AuthorRep.");
            }

            return new Func<int, bool>(val =>
            {
                if (ex.StartsWith("<="))
                {
                    return val <= exVal;
                }
                if (ex.StartsWith(">="))
                {
                    return val >= exVal;
                }
                if (ex[0] == '<')
                {
                    return val < exVal;
                }
                if (ex[0] == '>')
                {
                    return val > exVal;
                }
                if (char.IsDigit(ex[0]) || ex[0] == '-')
                {
                    return val == exVal;
                }
                return false;
            });
        }

        private static LogEntry[] FilterByPostTypeAndTime(LogEntry[] entries, RemoteLogRequest req)
        {
            bool? fetchQs = null;
            var postType = req.PostType == null ? "" : req.PostType.Trim().ToLowerInvariant();
            if (!string.IsNullOrEmpty(req.PostType))
            {
                if (postType.StartsWith("q"))
                {
                    fetchQs = true;
                }
                else if (postType.StartsWith("a"))
                {
                    fetchQs = false;
                }
            }

            var createdAfter = DateTime.Parse(req.CreatedAfter);
            var createdBefore = DateTime.Parse(req.CreatedBefore);
            var entryAddedAfter = DateTime.Parse(req.EntryAddedAfter);
            var entryAddedBefore = DateTime.Parse(req.EntryAddedBefore);

            var filtered = entries.Where(entry =>
            {
                if ((entry.Post.CreationDate > createdAfter && entry.Post.CreationDate < createdBefore) &&
                   (entry.Timestamp > entryAddedAfter && entry.Timestamp < entryAddedBefore) &&
                   (fetchQs == null || (bool)fetchQs && entry.IsQuestion || !(bool)fetchQs && !entry.IsQuestion))
                {
                    return true;
                }
                return false;
            }).ToArray();

            return filtered;
        }



        private static void LoggerLoop()
        {
            while (!stop)
            {
                loggerIntervalMre.WaitOne(UpdateInterval);

                foreach (var kv in Log)
                {
                    if ((DateTime.UtcNow - kv.Value.Timestamp).TotalDays > 5)
                    {
                        LogEntry entry;
                        Log.TryRemove(kv.Key, out entry);

                        if (EntryRemoved == null) { continue; }
                        EntryRemoved(entry);
                    }
                }

                var json = JsonSerializer.SerializeToString(Log);
                var uncompData = Encoding.UTF8.GetBytes(json);
                var compData = DataUtilities.GZipCompress(uncompData);

                LogSizeUncompressed = uncompData.Length;
                LogSizeCompressed = compData.Length;

                DataManager.SaveData("Yam", "Post Log", compData);
            }

            loggerStoppedMre.Set();
        }
    }
}
