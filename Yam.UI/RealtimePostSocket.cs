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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebSocketSharp;
using Phamhilator.Yam.Core;

namespace Phamhilator.Yam.UI
{
    public class RealtimePostSocket : IDisposable
    {
        private WebSocket socket;
        private bool disposed;

        public delegate void OnActiveQuestionEventHandler(Question q);
        public delegate void OnActiveThreadAnswersEventHandler(List<Answer> a);
        public delegate void OnExceptionEventHandler(Exception ex);
        public event OnActiveQuestionEventHandler OnActiveQuestion;
        public event OnActiveThreadAnswersEventHandler OnActiveThreadAnswers;
        public event OnExceptionEventHandler OnException;



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

            disposed = true;
            Close();
            GC.SuppressFinalize(this);
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
                if (OnException != null)
                {
                    OnException(oo.Exception);
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
                    if (OnException != null)
                    {
                        OnException(ex);
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
