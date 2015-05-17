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
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Phamhilator.Yam.UI
{
    public class RemoteServer : IDisposable
    {
        private readonly TcpListener listener;
        private bool disposed;

        public const string ApiKeysDataKey = "Registered API keys";
        public delegate void ClientEventHandler(RemoteClient client);
        public event ClientEventHandler ClientConnected;
        public event ClientEventHandler ClientDisconnected;

        public ConcurrentDictionary<string, string> ApiKeys { get; private set; }

        public List<RemoteClient> Clients { get; private set; }

        public ulong TotalDataSent { get; private set; }



        public RemoteServer()
        {
            if (DataManager.DataExists("Yam", ApiKeysDataKey))
            {
                var data = DataManager.LoadData("Yam", ApiKeysDataKey);
                var keys = data.Split(new[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
                ApiKeys = new ConcurrentDictionary<string, string>();

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
            }
            else
            {
                ApiKeys = new ConcurrentDictionary<string, string>();
            }

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

            Clients = Clients.Where(c =>
            {
                if (!c.Socket.Connected)
                {
                    c.Socket.Close();
                    if (ClientDisconnected != null) { ClientDisconnected(c); }
                    return false;
                }
                return true;
            }).ToList();

            for (var i = 0; i < Clients.Count; i++)
            {
                try
                {
                    var json = JsonConvert.SerializeObject(data);
                    var bytes = Encoding.UTF8.GetBytes(json);
                    var size = bytes.Length;
                    Clients[i].Socket.GetStream().Write(bytes, 0, size);
                    TotalDataSent += (ulong)size;
                }
                catch (Exception) { }
            }
        }



        private void ListenLoop()
        {
            while (!disposed)
            {
                var socket = listener.AcceptTcpClient();
                var apiKey = new byte[32];
                var owner = "";

                try
                {
                    socket.GetStream().Read(apiKey, 0, 32);
                }
                catch (Exception) { }

                owner = CheckApiKey(apiKey);

                if (String.IsNullOrEmpty(owner))
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
                    Owner = owner
                };
                var mre = new ManualResetEvent(false);
                Task.Run(() =>
                {
                    while (!socket.Connected)
                    {
                        Thread.Sleep(100);
                    }
                    mre.Set();
                });
                mre.WaitOne();

                Clients.Add(client);
                if (ClientConnected == null) { continue; }
                ClientConnected(client);
            }
        }

        public string CheckApiKey(byte[] clientKey)
        {
            foreach (var realKey in ApiKeys)
            {
                byte[] realKeyHashed;

                using (var sha = new SHA256Managed())
                {
                    var realKeyBytes = Encoding.UTF8.GetBytes(realKey.Key);
                    realKeyHashed = sha.ComputeHash(realKeyBytes);
                }

                if (clientKey.Length != realKeyHashed.Length) { continue; }

                for (var i = 0; i < realKeyHashed.Length; i++)
                {
                    if (realKeyHashed[i] != clientKey[i])
                    {
                        continue;
                    }
                    else if (i == realKeyHashed.Length - 1)
                    {
                        return realKey.Value;
                    }
                }
            }
            return null;
        }
    }
}
