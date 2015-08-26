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

        public static HashSet<LogEntry> SearchLog(RemoteLogRequest req)
        {
            var fieldLower = req.SearchBy.Trim().ToLowerInvariant();
            var pattern = new Regex(req.SearchPattern, RegexOptions.Compiled | RegexOptions.CultureInvariant);
            var entries = new HashSet<LogEntry>();
            Func<LogEntry, string> getField = null;
            Func<string, bool> search = null;

            switch (fieldLower)
            {
                case "site":
                {
                    getField = new Func<LogEntry, string>(entry => entry.Post.Site);
                    break;
                }
                case "title":
                {
                    getField = new Func<LogEntry, string>(entry => entry.Post.Title);
                    break;
                }
                case "body":
                {
                    getField = new Func<LogEntry, string>(entry => entry.Post.Body);
                    break;
                }
                case "authorname":
                {
                    getField = new Func<LogEntry, string>(entry => entry.Post.AuthorName);
                    break;
                }
                case "authornetworkid":
                {
                    getField = new Func<LogEntry, string>(entry => entry.Post.AuthorNetworkID.ToString());
                    break;
                }
            }

            search = new Func<string, bool>(field => pattern.IsMatch(field));

            bool? fetchQs = null;
            var postType = req.PostType.Trim().ToLowerInvariant();
            if (!string.IsNullOrEmpty(req.PostType))
            {
                if (postType.StartsWith("question"))
                {
                    fetchQs = true;
                }
                else if (postType.StartsWith("answer"))
                {
                    fetchQs = false;
                }
            }

            foreach (var entry in Log.Values)
            {
                if (entries.Count == 100) { break; }
                if (entry.Post.CreationDate < req.StartDate && entry.Post.CreationDate > req.EndDate) { continue; }
                if (search(getField(entry)) && (fetchQs == null || (bool)fetchQs && entry.IsQuestion))
                {
                    entries.Add(entry);
                }
            }

            return entries;
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
