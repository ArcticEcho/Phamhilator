using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using JsonFx.Json;
using JsonFx.Serialization;



namespace Phamhilator.Core
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
            var reader = new JsonReader();

            if (String.IsNullOrEmpty(data))
            {
                entries = new List<LogItem>();
            }
            else
            {
                entries = reader.Read<List<LogItem>>(data);
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
                }

                if (entriesRemoved.Count != 0 && EntriesRemovedEvent != null)
                {
                    EntriesRemovedEvent(entriesRemoved);
                }

                File.WriteAllText(DirectoryTools.GetLogFile(), new JsonWriter(new DataWriterSettings { PrettyPrint = true } ).Write(entries));
            }
        }
    }
}
