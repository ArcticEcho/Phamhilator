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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using Newtonsoft.Json;

namespace Phamhilator.Pham.Core
{
    public class ReportLog : IDisposable
    {
        private readonly List<LogItem> entries;
        private readonly Thread writer;
        private bool dispose;
        private bool disposed;

        public List<LogItem> Entries
        {
            get
            {
                return entries;
            }
        }

        public Dictionary<string, string> EntryLinks { get; private set; }

        public Action<List<LogItem>> EntriesRemovedEvent { get; set; }



        public ReportLog()
        {
            EntryLinks = new Dictionary<string, string>();

            var data = File.ReadAllText(DirectoryTools.GetLogFile());

            if (String.IsNullOrEmpty(data))
            {
                entries = new List<LogItem>();
            }
            else
            {
                entries = JsonConvert.DeserializeObject<List<LogItem>>(data);
            }

            Stats.PostsCaught += entries.Count;

            writer = new Thread(UpdateLog);
            writer.Start();
        }

        ~ReportLog()
        {
            if (disposed) { return; }

            Dispose();
        }



        public void Dispose()
        {
            if (disposed) { return; }

            dispose = true;

            while (writer != null && writer.IsAlive)
            {
                Thread.Sleep(100);
            }

            disposed = true;
        }

        public void AddEntry(LogItem item)
        {
            lock (entries)
            {
                if (entries.Any(i => i.ReportLink == item.ReportLink)) { return; }

                if (entries.Count == 0)
                {
                    entries.Add(item);
                }
                else
                {
                    entries.Insert(0, item);
                }
            }
        }



        private void UpdateLog()
        {
            var sw = new Stopwatch();

            while (!dispose)
            {
                while (!Config.IsRunning)
                {
                    Thread.Sleep(500);
                }

                sw.Start();

                while (sw.Elapsed.TotalMinutes < 30 && !dispose)
                {
                    Thread.Sleep(1000);
                }

                sw.Reset();

                // Remove week old entries.
                var entriesRemoved = new List<LogItem>();

                lock (entries)
                {
                    for (var i = 0; i < entries.Count; i++)
                    {
                        if ((DateTime.UtcNow - entries[i].TimeStamp).TotalDays > 7)
                        {
                            entriesRemoved.Add(entries[i]);
                            entries.RemoveAt(i);
                            i = 0;
                        }
                    }
                    
                    File.WriteAllText(DirectoryTools.GetLogFile(), JsonConvert.SerializeObject(entries, Formatting.Indented));
                }

                if (entriesRemoved.Count != 0 && EntriesRemovedEvent != null)
                {
                    EntriesRemovedEvent(entriesRemoved);
                }
            }
        }
    }
}
