using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebSocketSharp;



namespace Yamhilator
{
    public class RealtimePostSocket : IDisposable
    {
        private WebSocket socket;
        private bool disposed;

        public Action<Question> OnActiveQuestion { get; set; }
        public Action<List<Answer>> OnActiveThreadAnswers { get; set; }
        public Action<Exception> OnExcption { get; set; }
        public WebSocketState SocketState
        {
            get
            {
                return socket == null ? WebSocketState.Closed : socket.ReadyState;
            }
        }



        public RealtimePostSocket(bool autoConnect = false)
        {
            InitialiseSocket();

            if (autoConnect)
            {
                Connect();
            }
        }

        ~RealtimePostSocket()
        {
            if (!disposed)
            {
                Dispose();
            }
        }



        public void Dispose()
        {
            if (disposed) { return; }

            GC.SuppressFinalize(this);
            Close();
            disposed = true;
        }

        public void Connect()
        {
            if ((SocketState == WebSocketState.Connecting || SocketState == WebSocketState.Closed) && !disposed)
            {
                socket.Connect();
            }
        }

        public void Close()
        {
            if (SocketState == WebSocketState.Open && !disposed)
            {
                socket.Close();
            }
        }



        private void InitialiseSocket()
        {
            socket = new WebSocket("ws://qa.sockets.stackexchange.com");
            socket.OnError += (o, oo) =>
            {
                if (OnExcption != null)
                {
                    OnExcption(oo.Exception);
                }
            };
            socket.OnOpen += (o, oo) => socket.Send("155-questions-active");
            socket.OnMessage += (o, message) =>
            {
                if (OnActiveQuestion == null && OnActiveThreadAnswers == null) { return; }

                try
                {
                    Task.Factory.StartNew(() =>
                    {
                        var question = PostFetcher.GetQuestion(message);

                        if (OnActiveQuestion != null)
                        {
                            OnActiveQuestion(question);
                        }

                        if (OnActiveThreadAnswers != null)
                        {
                            OnActiveThreadAnswers(PostFetcher.GetLatestAnswers(question));
                        }
                    });
                }
                catch (Exception ex)
                {
                    if (OnExcption != null)
                    {
                        OnExcption(ex);
                    }
                }
            };

            socket.OnClose += (o, oo) =>
            {
                if (disposed) { return; }

                InitialiseSocket();
            };
        }
    }
}
