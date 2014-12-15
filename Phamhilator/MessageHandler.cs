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
        private Thread processor;
        private bool exit;
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
                    Thread.Sleep(250);
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
                    Thread.Sleep(TimeSpan.FromSeconds(rateLimit + 1));
                }

                nextM.Action();

                //if (nextM.Message.IsReply)
                //{
                //    nextM.Callback(nextM.Room.PostReply(nextM.Message));
                //}

                //nextM.Callback(nextM.Room.PostMessage(nextM.Message));

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

        private double CalcRateLimit(List<DateTime> messageRecords)
        {
            if (messageRecords == null || messageRecords.Count == 0) { return 0; }

            var limit = 0.0;
            var a = messageRecords.Count - 1;
            var b = 0.0;
            var throttled = false;

            for (var i = messageRecords.Count - 1; i > 0; i--)
            {
                limit = Limit(a - i);
                var currentDur = (messageRecords[i] - messageRecords[a]).TotalSeconds;

                if (currentDur < limit && !throttled)
                {
                    throttled = true;

                    b = limit - currentDur;

                    continue;
                }

                if (b - currentDur < 0)
                {
                    throttled = false;

                    a = i;
                }

                if (currentDur > limit && !throttled)
                {
                    throttled = false;
                }

                if (currentDur > limit * 2)
                {
                    a = i;

                    throttled = false;
                }
            }

            limit = Limit(a);

            /*var t = (messageRecords[a] - messageRecords[0]).TotalSeconds;*/
            return /*t -*/ limit;
        }

        private double Limit(double x)
        {
            return Math.Min((4.1484 * Math.Log(x) + 1.02242), 20);
        }
    }
}
