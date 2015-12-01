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
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using ServiceStack.Text;

namespace Phamhilator.Yam.Core
{
    public class LocalSocketListener : IDisposable
    {
        private readonly ManualResetEvent listenerThreadDeadMRE = new ManualResetEvent(false);
        private readonly UdpClient listener;
        private readonly EndPoint endPoint;
        private readonly Thread listenerThread;
        private bool disposed;

        public delegate void OnExceptionEventHandler(Exception ex);
        public delegate void OnMessageEventHandler(LocalRequest request);
        public event OnExceptionEventHandler OnException;
        public event OnMessageEventHandler OnMessage;

        public ulong TotalDataReceived { get; private set; }



        public LocalSocketListener(int port)
        {
            endPoint = new IPEndPoint(IPAddress.Any, port);
            listener = new UdpClient() { ExclusiveAddressUse = false };
            listener.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            listener.Client.Bind(endPoint);
            listener.JoinMulticastGroup(LocalSocketMulticastAddress.Address);
            listenerThread = new Thread(Listen) { IsBackground = true };
            listenerThread.Start();
        }

        ~LocalSocketListener()
        {
            if (disposed) { return; }
            Dispose();
        }



        public void Dispose()
        {
            if (disposed) { return; }
            disposed = true;

            listener.Close();
            listenerThreadDeadMRE.WaitOne();
            listenerThreadDeadMRE.Dispose();

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

                    if (bytes == null || bytes.Length == 0) { continue; }

                    TotalDataReceived += (uint)bytes.Length;
                    var json = Encoding.UTF8.GetString(bytes);
                    var data = JsonSerializer.DeserializeFromString<LocalRequest>(json);

                    if (OnMessage == null || data == null) { continue; }
                    OnMessage(data);
                }
                catch (Exception ex)
                {
                    if (OnException == null) { continue; }
                    OnException(ex);
                }
            }

            listenerThreadDeadMRE.Set();
        }
    }
}
