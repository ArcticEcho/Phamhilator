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

        public delegate void MessagePostedCallBack();
        public Dictionary<Room, List<ChatAction>> Queue { get; private set; }



        public MessageHandler()
        {
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
            {
                if (!Queue.ContainsKey(message.Room))
                {
                    Queue.Add(message.Room, new List<ChatAction>());
                    processors.Add(message.Room, new Thread(() => ProcessRoomQueue(message.Room)));
                    processors[message.Room].Start();
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

                lock (Queue[room])
                {
                    nextM = Queue[room][0];
                }

                nextM.Action();

                lock (Queue[room])
                {
                    Queue[room].Remove(nextM);
                }
            }
        }
    }
}
