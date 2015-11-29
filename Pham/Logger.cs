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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ServiceStack.Text;

namespace Phamhilator.Pham.UI
{
    // No clue how this will work, just some ideas.
    // Severity 0: 1 hour
    // Severity 1: 6 hours
    // Severity 2: 12 hours
    // Severity 3: 24 hours

    public partial class Logger<T> : IEnumerable<T>, IDisposable
    {
        private readonly ManualResetEvent itemRemoverMre = new ManualResetEvent(false);
        private readonly HashSet<T> removeItemsQueue = new HashSet<T>();
        private readonly object lockObj = new object();
        private readonly string logPath;
        private bool dispose;

        public TimeSpan LogClearRate { get; }

        public TimeSpan? TimeToLive { get; }

        public int Count { get; private set; }

        public Action<T> ItemRemovedEvent { get; set; }



        public Logger(string logFileName)
        {
            logPath = logFileName;

            InitialiseCount();

            Task.Run(() => RemoveItems());
        }

        public Logger(string logFileName, TimeSpan itemTtl, TimeSpan logClearRate)
        {
            TimeToLive = itemTtl;
            LogClearRate = logClearRate;
            logPath = logFileName;

            InitialiseCount();

            Task.Run(() => RemoveItems());
        }

        ~Logger()
        {
            Dispose();
        }



        public void Dispose()
        {
            if (dispose) return;
            dispose = true;

            itemRemoverMre.Set();

            GC.SuppressFinalize(this);
        }

        public IEnumerator<T> GetEnumerator()
        {
            lock (lockObj)
            {
                var lines = File.ReadLines(logPath);

                foreach (var line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    var entry = JsonSerializer.DeserializeFromString<Entry>(line);
                    var data = (T)entry.Data;

                    if (removeItemsQueue.Contains(data)) continue;

                    yield return data;
                }
            }
        }

        public void EnqueueItem(T item)
        {
            var entry = new Entry
            {
                Data = item,
                Timestamp = DateTime.UtcNow
            };
            var json = JsonSerializer.SerializeToString(entry);

            lock (lockObj)
            {
                File.AppendAllLines(logPath, new[] { json });

                Count++;
            }
        }

        public void EnqueueItems(IEnumerable<T> items)
        {
            lock (lockObj)
            {
                foreach (var item in items)
                {
                    var entry = new Entry
                    {
                        Data = item,
                        Timestamp = DateTime.UtcNow
                    };
                    var json = JsonSerializer.SerializeToString(entry);

                    File.AppendAllLines(logPath, new[] { json });

                    Count++;
                }
            }
        }

        public void RemoveItem(T item)
        {
            if (removeItemsQueue.Contains(item))
            {
                throw new ArgumentException("This item is already queued for removal.", "item");
            }

            lock (lockObj)
            {
                removeItemsQueue.Add(item);
                Count--;
            }
        }

        public void ClearLog()
        {
            lock (lockObj)
            {
                File.WriteAllText(logPath, "");
                removeItemsQueue.Clear();
                Count = 0;
            }
        }



        private void InitialiseCount()
        {
            if (!File.Exists(logPath))
            {
                File.Create(logPath).Dispose();
            }
            else
            {
                lock (lockObj)
                {
                    var lines = File.ReadLines(logPath);

                    foreach (var line in lines)
                    {
                        if (!string.IsNullOrWhiteSpace(line)) Count++;
                    }
                }
            }
        }

        private void RemoveItems()
        {
            var ttl = (TimeSpan)TimeToLive;

            while (!dispose)
            {
                lock (lockObj)
                {
                    var lines = File.ReadLines(logPath);
                    var temp = Path.GetTempFileName();

                    foreach (var line in lines)
                    {
                        if (string.IsNullOrWhiteSpace(line)) continue;

                        var entry = JsonSerializer.DeserializeFromString<Entry>(line);
                        var data = (T)entry.Data;

                        if ((DateTime.UtcNow - entry.Timestamp) < ttl && !removeItemsQueue.Contains(data))
                        {
                            File.AppendAllLines(temp, new[] { line });
                        }
                        else
                        {
                            if (removeItemsQueue.Contains(data))
                            {
                                removeItemsQueue.Remove(data);
                            }

                            if (ItemRemovedEvent == null) continue;

                            ItemRemovedEvent(data);
                        }
                    }

                    File.Delete(logPath);
                    File.Move(temp, logPath);
                }

                itemRemoverMre.WaitOne(LogClearRate);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
