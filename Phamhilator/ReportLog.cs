using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;



namespace Phamhilator
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

        public Action<List<LogItem>> EntriesRemovedEvent { get; set; }



        public ReportLog()
        {
            var data = File.ReadAllText(DirectoryTools.GetLogFile());

            if (String.IsNullOrEmpty(data))
            {
                entries = new List<LogItem>();
            }
            else
            {
                entries = Newtonsoft.Json.JsonConvert.DeserializeObject<List<LogItem>>(data);
            }

            GlobalInfo.PostsCaught += entries.Count;

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

            while (writer.IsAlive)
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

            GlobalInfo.PostsCaught++;
        }



        private void UpdateLog()
        {
            var sw = new Stopwatch();

            while (!dispose)
            {
                sw.Start();

                while (sw.Elapsed.TotalMinutes < 5 && !dispose)
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

                File.WriteAllText(DirectoryTools.GetLogFile(), Newtonsoft.Json.JsonConvert.SerializeObject(entries, Newtonsoft.Json.Formatting.Indented));
            }
        }
    }
}
