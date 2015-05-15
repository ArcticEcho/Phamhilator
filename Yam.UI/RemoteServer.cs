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
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Phamhilator.Yam.UI
{
    public class RemoteServer : IDisposable
    {
        private readonly TcpListener listener = new TcpListener(IPAddress.Any, 45010);
        private bool disposed;

        public List<TcpClient> Clients { get; private set; }

        public ulong TotalDataSent { get; private set; }



        public RemoteServer()
        {
            Clients = new List<TcpClient>();

            Task.Run(() => ListenLoop());
            listener.Start();
        }

        ~RemoteServer()
        {
            if (disposed) { return; }
        }



        public void Dispose()
        {
            if (disposed) { return; }
            disposed = true;

            listener.Stop();

            foreach (var client in Clients)
            {
                client.Close();
            }

            GC.SuppressFinalize(this);
        }

        public void SendDataAll(object data)
        {
            if (data == null) { throw new ArgumentNullException("data"); }

            for (var i = 0; i < Clients.Count; i++)
            {
                if (!Clients[i].Connected)
                {
                    Clients.RemoveAt(i);
                    i--;
                    continue;
                }
                var json = JsonConvert.SerializeObject(data);
                var bytes = Encoding.BigEndianUnicode.GetBytes(json);
                var size = bytes.Length;
                Clients[i].GetStream().Write(bytes, 0, size);
                TotalDataSent += (ulong)size;
            }
        }



        private void ListenLoop()
        {
            while (!disposed)
            {
                var client = listener.AcceptTcpClient();
                Clients.Add(client);
            }
        }
    }
}
