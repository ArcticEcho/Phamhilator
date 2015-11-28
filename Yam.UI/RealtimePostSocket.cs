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
        public delegate void OnActiveAnswerEventHandler(Answer a);
        public delegate void OnExceptionEventHandler(Exception ex);
        public event OnActiveQuestionEventHandler OnActiveQuestion;
        public event OnActiveAnswerEventHandler OnActiveAnswer;
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
            // SE site IDs (may come in handy later on).
            // 415,520,139,477,540,11,41,118,463,89,514,528,532,320,126,375,308,502,147,565,431,435,371,304,605,196,597,391,571,363,419,563,65,281,557,182,220,553,530,591,135,583,595,97,481,546,471,500,299,53,269,467,79,253,174,607,567,587,324,73,156,162,455,524,257,555,336,593,312,479,403,387,548,69,504,4,248,224,367,601,240,615,496,498,228,93,277,518,265,61,216,151,379,475,131,200,208,559,204,447,489,469,122,451,459,186,49,2,232,295,212,244,536,512,508,353,411,101,1,526,609,573,3,483,85,395,423,114,516,273,485,106,102,599,170,34,45,427,603,110,579,166,613,581
            socket.OnOpen += (o, oo) => socket.Send("155-questions-active");
            socket.OnMessage += (o, message) =>
            {
                if (OnActiveQuestion == null && OnActiveAnswer == null) { return; }

                try
                {
                    Task.Factory.StartNew(() =>
                    {
                        var question = PostFetcher.GetQuestion(message);
                        var answer = PostFetcher.GetLatestAnswer(question);

                        if (OnActiveQuestion != null && answer == null)
                        {
                            OnActiveQuestion(question);
                            return;
                        }

                        if (OnActiveAnswer != null)
                        {
                            OnActiveAnswer(answer);
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
