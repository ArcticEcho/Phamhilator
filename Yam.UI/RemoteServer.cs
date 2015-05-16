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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Phamhilator.Yam.UI
{
    public class RemoteServer : IDisposable
    {
        private readonly ManualResetEvent apiKeysUpdaterMre;
        private readonly TcpListener listener;
        private bool disposed;

        public const string ApiKeysDataKey = "Registered API keys";
        public delegate void ClientEventHandler(RemoteClient client);
        public event ClientEventHandler ClientConnected;

        public ConcurrentDictionary<string, string> ApiKeys { get; private set; }

        public List<RemoteClient> Clients { get; private set; }

        public ulong TotalDataSent { get; private set; }



        public RemoteServer()
        {
            apiKeysUpdaterMre = new ManualResetEvent(false);
            Clients = new List<RemoteClient>();

            listener = new TcpListener(IPAddress.Any, 45010);
            listener.Start();
            Task.Run(() => ListenLoop());
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
                client.Socket.Close();
            }

            GC.SuppressFinalize(this);
        }

        public void SendDataAll(object data)
        {
            if (data == null) { throw new ArgumentNullException("data"); }

            for (var i = 0; i < Clients.Count; i++)
            {
                if (!Clients[i].Socket.Connected)
                {
                    Clients.RemoveAt(i);
                    i--;
                    continue;
                }
                var json = JsonConvert.SerializeObject(data);
                var bytes = Encoding.UTF8.GetBytes(json);
                var size = bytes.Length;
                Clients[i].Socket.GetStream().Write(bytes, 0, size);
                TotalDataSent += (ulong)size;
            }
        }



        private void ListenLoop()
        {
            while (!disposed)
            {
                var socket = listener.AcceptTcpClient();
                var apiKeyBytes = new byte[1024];
                var apiKey = "";

                try
                {
                    socket.GetStream().Read(apiKeyBytes, 0, 1024);
                    apiKey = Encoding.UTF8.GetString(apiKeyBytes).ToLowerInvariant();
                }
                catch (Exception) { }

                if (String.IsNullOrEmpty(apiKey) || !ApiKeys.ContainsKey(apiKey))
                {
                    try
                    {
                        var message = Encoding.UTF8.GetBytes("Invalid API key.");
                        socket.GetStream().Write(message, 0, message.Length);
                        socket.Close();
                    }
                    catch (Exception) { }
                    continue;
                }

                var client = new RemoteClient
                {
                    Socket = socket,
                    ApiKey = apiKey,
                    Owner = ApiKeys[apiKey]
                };

                Clients.Add(client);
                if (ClientConnected == null) { continue; }
                ClientConnected(client);
            }
        }

        private void ApiKeysUpdater()
        {
            while (!disposed)
            {
                if (!DataManager.DataExists("Yam", ApiKeysDataKey)) { continue; }

                var data = DataManager.LoadData("Yam", ApiKeysDataKey);
                var keys = data.Split(new[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);

                foreach (var keyPair in keys)
                {
                    var split = keyPair.Split(':');
                    var owner = split[0];
                    var key = split[1].ToLowerInvariant();

                    if (!String.IsNullOrEmpty(owner) && !String.IsNullOrEmpty(key))
                    {
                        ApiKeys[key] = owner;
                    }
                }

                apiKeysUpdaterMre.WaitOne(TimeSpan.FromMinutes(2));
            }
        }
    }
}
