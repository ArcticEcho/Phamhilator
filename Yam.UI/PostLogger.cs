using System;
using System.Collections.Concurrent;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Phamhilator.Yam.Core;

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
        /// The interval at which to flush the log to disk (in seconds; default set to 120).
        /// </summary>
        public static int UpdateInterval { get; set; }



        public static void InitialiseLogger()
        {
            if (DataManager.DataExists("Yam", dataManagerLogKey))
            {
                var bytes = DataManager.LoadRawData("Yam", dataManagerLogKey);
                var uncompData = UncompressData(bytes);
                var json = Encoding.UTF8.GetString(uncompData);
                Log = JsonConvert.DeserializeObject<ConcurrentDictionary<uint, LogEntry>>(json);
            }
            else
            {
                Log = new ConcurrentDictionary<uint, LogEntry>();
            }

            loggerIntervalMre = new ManualResetEvent(false);
            loggerStoppedMre = new ManualResetEvent(false);
            UpdateInterval = 120;
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



        private static void LoggerLoop()
        {
            while (!stop)
            {
                loggerIntervalMre.WaitOne(UpdateInterval * 1000);

                for (uint i = 0; i < Log.Count; i++)
                {
                    if ((DateTime.UtcNow - Log[i].Timestamp).TotalDays > 5)
                    {
                        LogEntry entry;
                        Log.TryRemove(i, out entry);

                        if (EntryRemoved == null) { continue; }
                        EntryRemoved(entry);
                    }
                }

                var json = JsonConvert.SerializeObject(Log);
                var uncompData = Encoding.UTF8.GetBytes(json);
                var compData = CompressData(uncompData);

                LogSizeUncompressed = uncompData.Length;
                LogSizeCompressed = compData.Length;

                DataManager.SaveData("Yam", "Post Log", compData);
            }

            loggerStoppedMre.Set();
        }

        private static byte[] CompressData(byte[] data)
        {
            byte[] compressed;

            using (var compStrm = new MemoryStream())
            {
                using (var zipper = new GZipStream(compStrm, CompressionMode.Compress))
                using (var ms = new MemoryStream(data))
                {
                    ms.CopyTo(zipper);
                }

                compressed = compStrm.ToArray();
            }

            return compressed;
        }

        private static byte[] UncompressData(byte[] data)
        {
            using (var msIn = new MemoryStream(data))
            using (var unzipper = new GZipStream(msIn, CompressionMode.Decompress))
            using (var msOut = new MemoryStream())
            {
                unzipper.CopyTo(msOut);
                return msOut.ToArray();
            }
        }
    }
}
