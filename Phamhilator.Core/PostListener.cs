using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Phamhilator.Core
{
    public class PostListener : IDisposable
    {
        private Socket listener;
        private EndPoint endPoint = new IPEndPoint(new IPAddress(new byte[] { 0, 0, 0, 0 }), 60000);
        private Thread listenerThread;
        private uint dataReceived;
        private bool dispose;
        private bool disposed;

        public delegate void OnActiveQuestionEventHandler(Question q);
        public delegate void OnActiveAnswerEventHandler(Answer a);
        public event OnActiveQuestionEventHandler OnActiveQuestion;
        public event OnActiveAnswerEventHandler OnActiveAnswer;



        public PostListener()
        {
            listener = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            listener.Bind(endPoint);

            listenerThread = new Thread(Listen) { IsBackground = true };
            listenerThread.Start();
        }

        ~PostListener()
        {
            if (!disposed)
            {
                Dispose();
            }
        }



        public void Dispose()
        {
            if (disposed) { return; }

            dispose = true;

            GC.SuppressFinalize(this);

            while (listenerThread.IsAlive) { Thread.Sleep(100); }

            disposed = true;
        }



        private void Listen()
        {
            while (!dispose)
            {
                var bytes = new byte[50000];
                EndPoint ep = new IPEndPoint(0, 0);

                try
                {
                    dataReceived += (uint)listener.ReceiveFrom(bytes, ref ep);
                }
                catch (Exception) { }

                var data = Encoding.UTF8.GetString(bytes);
                var json = data.Remove(0, 3);
                if (data.StartsWith("<Q>"))
                {
                    var q = new JsonFx.Json.JsonReader().Read<Question>(json);
                    if (q != null && OnActiveQuestion != null)
                    {
                        OnActiveQuestion(q);
                    }
                }
                else if (data.StartsWith("<A>"))
                {
                    var a = new JsonFx.Json.JsonReader().Read<Answer>(json);
                    if (a != null && OnActiveAnswer != null)
                    {
                        OnActiveAnswer(a);
                    }
                }
            }
        }
    }
}
