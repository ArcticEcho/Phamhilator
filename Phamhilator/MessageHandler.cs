using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ChatExchangeDotNet;



namespace Phamhilator
{
    public class MessageHandler : IDisposable
    {
        private bool disposed;
        private bool exit;
        private readonly Thread processor;
        private readonly Dictionary<Room, List<DateTime>> lastPostRecord = new Dictionary<Room, List<DateTime>>();

        public delegate void MessagePostedCallBack();
        public List<ChatAction> Queue { get; private set; }



        public MessageHandler()
        {
            Queue = new List<ChatAction>();

            processor = new Thread(ProcessQueue);

            processor.Start();
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

            // Give the thread a chance to exit gracefully.
            Thread.Sleep(400); 

            if (processor.IsAlive)
            {
                // Otherwise kill it with fire!
                processor.Abort();
            }
        }

        public void QueueItem(ChatAction message)
        {
            lock (Queue)
            lock (lastPostRecord)
            {
                if (!lastPostRecord.ContainsKey(message.Room))
                {
                    lastPostRecord.Add(message.Room, new List<DateTime>());
                }

                Queue.Add(message);
            }
        }



        private void ProcessQueue()
        {
            while (!exit)
            {
                while (Queue.Count == 0)
                {
                    Thread.Sleep(200);
                }

                ChatAction nextM;
                double rateLimit;

                lock (Queue)
                lock (lastPostRecord)
                {
                    nextM = Queue[0];
                    rateLimit = CalcRateLimit(lastPostRecord[nextM.Room]);
                }

                if (rateLimit > 0)
                {
                    Thread.Sleep(TimeSpan.FromSeconds(rateLimit /*+ 1*/));
                }

                nextM.Action();

                lock (Queue)
                lock (lastPostRecord)
                {
                    if (lastPostRecord[nextM.Room].Count == 0)
                    {
                        lastPostRecord[nextM.Room].Add(DateTime.UtcNow);
                    }
                    else
                    {
                        lastPostRecord[nextM.Room].Insert(0, DateTime.UtcNow);
                    }

                    Queue.Remove(nextM);
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
