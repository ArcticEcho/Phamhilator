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
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Phamhilator.Yam.Core
{
    public class LocalUDPSocketSender : IDisposable
    {
        private readonly UdpClient broadcastSocket;
        private readonly IPEndPoint targetEP;
        private bool disposed;

        public ulong TotalDataSent { get; private set; }



        public LocalUDPSocketSender(int port)
        {
            targetEP = new IPEndPoint(LocalSocketMulticastAddress.Address, port);
            broadcastSocket = new UdpClient();
            broadcastSocket.JoinMulticastGroup(LocalSocketMulticastAddress.Address);
        }

        ~LocalUDPSocketSender()
        {
            if (disposed) { return; }
            Dispose();
        }



        public void Dispose()
        {
            if (disposed) { return; }
            disposed = true;

            broadcastSocket.Close();

            GC.SuppressFinalize(this);
        }

        public void SendData(LocalRequest req)
        {
            if (disposed) { return; }
            if (req == null) { throw new ArgumentNullException("req"); }

            var json = JsonConvert.SerializeObject(req, Formatting.Indented);
            var bytes = Encoding.BigEndianUnicode.GetBytes(json);
            TotalDataSent += (uint)broadcastSocket.Send(bytes, bytes.Length, targetEP);
        }
    }
}
