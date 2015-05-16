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

namespace Phamhilator.Yam.UI
{
    public class RemoteSocketSender : IDisposable
    {
        private readonly TcpClient socket;
        private bool disposed;

        public ulong TotalDataSent { get; private set; }

        public IPEndPoint EndPoint { get; private set; }



        public RemoteSocketSender(string ip, int port)
        {
            if (String.IsNullOrEmpty(ip)) { throw new ArgumentNullException("ip"); }
            socket = new TcpClient(ip, port);
        }

        ~RemoteSocketSender()
        {
            if (disposed) { return; }
            Dispose();
        }



        public void Dispose()
        {
            if (disposed) { return; }
            disposed = true;

            socket.Close();

            GC.SuppressFinalize(this);
        }

        public void SendData(object req)
        {
            if (disposed) { return; }
            if (req == null) { throw new ArgumentNullException("req"); }

            var json = JsonConvert.SerializeObject(req);
            var bytes = Encoding.UTF8.GetBytes(json);
            var size = bytes.Length;
            using (var stream = socket.GetStream())
            {
                stream.Write(bytes, 0, size);
                TotalDataSent += (ulong)size;
            }
        }
    }
}
