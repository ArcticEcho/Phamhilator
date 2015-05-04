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

namespace Phamhilator.Yam.Core
{
    public class LocalUDPSocketListener
    {
        private readonly ManualResetEvent listenerThreadDeadMRE = new ManualResetEvent(false);
        private readonly UdpClient listener;
        private readonly EndPoint endPoint;
        private readonly Thread listenerThread;
        private bool disposed;

        public delegate void OnErrorEventHandler(Exception ex);
        public delegate void OnMessageEventHandler(string message);
        public event OnErrorEventHandler OnError;
        public event OnMessageEventHandler OnMessage;

        public ulong TotalDataReceived { get; private set; }



        public LocalUDPSocketListener(int port)
        {
            endPoint = new IPEndPoint(IPAddress.Any, port);
            listener = new UdpClient() { ExclusiveAddressUse = false };
            listener.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            listener.Client.Bind(endPoint);
            listener.JoinMulticastGroup(LocalSocketIPEndPoints.MulticastAddress);
            listenerThread = new Thread(Listen) { IsBackground = true };
            listenerThread.Start();
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
                    var data = Encoding.BigEndianUnicode.GetString(bytes);

                    if (OnMessage == null) { return; }
                    OnMessage(data);
                }
                catch (Exception ex)
                {
                    if (OnError == null) { return; }
                    OnError(ex);
                }
            }

            listenerThreadDeadMRE.Set();
        }
    }
}
