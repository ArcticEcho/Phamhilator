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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ServiceStack.Text;

namespace Phamhilator.Yam.Core
{
    public partial class Logger<T> : IEnumerable<T>, IDisposable
    {
        private readonly ConcurrentDictionary<int, Entry> data = new ConcurrentDictionary<int, Entry>();
        private readonly ManualResetEvent flushMre = new ManualResetEvent(false);
        private readonly ManualResetEvent disposeMre = new ManualResetEvent(false);
        private readonly object lockObj = new object();
        private readonly string logPath;
        private bool dispose;

        public TimeSpan FlushRate { get; }

        public int Count => data.Count;

        internal Action LogFlushed { get; set; }



        public Logger(string logFileName, TimeSpan? flushRate = null)
        {
            FlushRate = flushRate ?? TimeSpan.FromMinutes(60);
            logPath = logFileName;

            if (!File.Exists(logFileName))
            {
                File.Create(logFileName).Dispose();
            }
            else
            {
                var lines = File.ReadLines(logFileName);
                foreach (var line in lines)
                {
                    var entry = JsonSerializer.DeserializeFromString<Entry>(line);

                    data[entry.Data.GetHashCode()] = entry;
                }
            }

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

            flushMre?.Set();
            disposeMre?.WaitOne();
            flushMre?.Dispose();
            disposeMre?.Dispose();

            GC.SuppressFinalize(this);
        }

        public IEnumerator<T> GetEnumerator()
        {
            foreach (var entry in data.Values)
            {
                yield return (T)entry.Data;
            }
        }

        public void EnqueueItem(T item)
        {
            if (item == null) throw new ArgumentNullException("item");
            if (dispose) throw new ObjectDisposedException(GetType().Name);

            var entry = new Entry
            {
                Data = item,
                Timestamp = DateTime.UtcNow
            };

            data[entry.Data.GetHashCode()] = entry;
        }

        public void RemoveItem(T item)
        {
            if (item == null) throw new ArgumentNullException("item");

            Entry temp;
            data.TryRemove(item.GetHashCode(), out temp);
        }

        public void Clear()
        {
            lock (lockObj)
            {
                File.WriteAllText(logPath, "");
                data.Clear();
            }
        }



        private void RemoveItems()
        {
            while (!dispose)
            {
                flushMre.WaitOne(FlushRate);

                var temp = Path.GetTempFileName();

                foreach (var entry in data.Values)
                {
                    var line = JsonSerializer.SerializeToString(entry);

                    File.AppendAllLines(temp, new[] { line });
                }

                lock (lockObj)
                {
                    File.Delete(logPath);
                    File.Move(temp, logPath);
                }

                if (LogFlushed != null)
                {
                    Task.Run(LogFlushed);
                }
            }

            disposeMre.Set();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
