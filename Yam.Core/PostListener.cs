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
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using JsonFx.Json;

namespace Yam.Core
{
    public class MessageListener : IDisposable
    {
        private readonly ManualResetEvent listenerThreadDeadMRE = new ManualResetEvent(false);
        private readonly JsonReader questionReader = new JsonReader();
        private readonly JsonReader answerReader = new JsonReader();
        private UdpClient listener;
        private EndPoint endPoint = new IPEndPoint(new IPAddress(new byte[] { 0, 0, 0, 0 }), 60000);
        private Thread listenerThread;
        private bool disposed;

        public delegate void OnActiveQuestionEventHandler(Question q);
        public delegate void OnActiveAnswerEventHandler(Answer a);
        public delegate void OnErrorEventHandler(Exception ex);
        public delegate void OnNewCommandEventHandler(string command);
        public event OnActiveQuestionEventHandler OnActiveQuestion;
        public event OnActiveAnswerEventHandler OnActiveAnswer;
        public event OnNewCommandEventHandler OnNewCommand;
        public event OnErrorEventHandler OnError;

        public ulong TotalDataReceived { get; private set; }



        public MessageListener()
        {
            var localEp = new IPEndPoint(IPAddress.Any, 60000);
            var multicastaddress = IPAddress.Parse("239.0.0.222");

            listener = new UdpClient();
            listener.ExclusiveAddressUse = false;
            listener.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            listener.Client.Bind(localEp);
            listener.JoinMulticastGroup(multicastaddress);

            listenerThread = new Thread(Listen) { IsBackground = true };
            listenerThread.Start();
        }

        ~MessageListener()
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

            listenerThreadDeadMRE.WaitOne();
            listenerThreadDeadMRE.Dispose();
            listener.Close();
            listener.Client.Dispose();

            GC.SuppressFinalize(this);
        }



        private void Listen()
        {
            while (!disposed)
            {
                var bytes = new byte[0];
                var ep = new IPEndPoint(0, 0);

                try
                {
                    bytes = listener.Receive(ref ep);
                    TotalDataReceived += (uint)bytes.Length;
                
                    var data = Encoding.UTF8.GetString(bytes);

                    if (data.Length < 3) { continue; }

                    var json = data.Remove(0, 3);
                    if (data.StartsWith("<Q>"))
                    {
                        var q = questionReader.Read<Question>(json);
                        if (q != null && OnActiveQuestion != null)
                        {
                            OnActiveQuestion(q);
                        }
                    }
                    else if (data.StartsWith("<A>"))
                    {
                        var a = answerReader.Read<Answer>(json);
                        if (a != null && OnActiveAnswer != null)
                        {
                            OnActiveAnswer(a);
                        }
                    }
                    else if (data.StartsWith("<C>"))
                    {
                        //TODO: Command message type; implement later.
                    }
                }
                catch (Exception ex)
                {
                    if (OnError != null) { OnError(ex); }
                    continue;
                }
            }

            listenerThreadDeadMRE.Set();
        }
    }
}
