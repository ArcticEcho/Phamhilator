using System;
using System.Collections.Generic;
using System.Threading;
using ChatExchangeDotNet;



namespace Phamhilator
{
    public class MessageHandler : IDisposable
    {
        private bool disposed;
        private bool exit;
        private readonly Dictionary<Room, Thread> processors;
        /// <summary>
        /// Newest record at index 0.
        /// </summary>
        private readonly Dictionary<Room, List<DateTime>> lastPostRecord;

        public delegate void MessagePostedCallBack();
        public Dictionary<Room, List<ChatAction>> Queue { get; private set; }



        public MessageHandler()
        {
            lastPostRecord = lastPostRecord = new Dictionary<Room, List<DateTime>>();
            processors = new Dictionary<Room, Thread>();
            Queue = new Dictionary<Room, List<ChatAction>>();
        }

        ~MessageHandler()
        {
            if (disposed) { return; }

            Dispose();
        }



        public void Dispose()
        {
            if (disposed) { return; }

            exit = true;
            disposed = true;

            // Give the threads a chance to exit gracefully.
            Thread.Sleep(400);

            foreach (var processor in processors.Values)
            {
                if (processor.IsAlive)
                {
                     //Otherwise kill it with fire!
                    processor.Abort();
                }
            }
        }

        public void QueueItem(ChatAction message)
        {
            lock (Queue)
            lock (lastPostRecord)
            {
                if (!Queue.ContainsKey(message.Room))
                {
                    Queue.Add(message.Room, new List<ChatAction>());
                    lastPostRecord.Add(message.Room, new List<DateTime>());
                    processors.Add(message.Room, new Thread(() => ProcessRoomQueue(message.Room)));
                    processors[message.Room].Start();
                }

                if (lastPostRecord[message.Room].Count > 100)
                {
                    lastPostRecord[message.Room].RemoveAt(0);
                }

                Queue[message.Room].Add(message);
            }
        }



        private void ProcessRoomQueue(Room room)
        {
            while (!exit)
            {
                while (Queue[room].Count == 0)
                {
                    Thread.Sleep(200);
                }

                ChatAction nextM;
                double rateLimit;

                lock (Queue[room])
                lock (lastPostRecord[room])
                {
                    nextM = Queue[room][0];
                    rateLimit = CalcRateLimit(lastPostRecord[room]);
                }

                if (rateLimit > 0)
                {
                    Thread.Sleep(TimeSpan.FromSeconds(rateLimit));
                }

                nextM.Action();

                lock (Queue[room])
                lock (lastPostRecord[room])
                {
                    if (lastPostRecord[room].Count == 0)
                    {
                        lastPostRecord[room].Add(DateTime.UtcNow);
                    }
                    else
                    {
                        lastPostRecord[room].Insert(0, DateTime.UtcNow);
                    }

                    Queue[room].Remove(nextM);
                }
            }
        }

        private static double CalcRateLimit(List<DateTime> messageRecords)
        {
            if (messageRecords == null || messageRecords.Count == 0) { return 0; }

            var limit = 0.0;
            var lastClearIndex = messageRecords.Count - 1;
            var currentThrottleDur = 0.0;

            for (var i = messageRecords.Count - 1; i > -1; i--)
            {
                limit = Limit(lastClearIndex - i);
                var timeSinceClear = (messageRecords[lastClearIndex] - messageRecords[i]).TotalSeconds;

                if (timeSinceClear < limit)
                {
                    currentThrottleDur = limit;

                    continue;
                }

                if (timeSinceClear > limit * 2 || currentThrottleDur - timeSinceClear < 0)
                {
                    lastClearIndex = i;
                }
            }

            limit = Limit(lastClearIndex);

            return Math.Max(limit, 0);
        }

        private static double Limit(double x)
        {
            return Math.Min((4.1484 * Math.Log(x) + 1.02242), 20);
        }
    }
}
