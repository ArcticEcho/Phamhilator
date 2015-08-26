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
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Phamhilator.Yam.Core;
using ServiceStack.Text;

namespace Phamhilator.Yam.UI
{
    public class RemoteServer : IDisposable
    {
        private const int realtimePostPort = 45010;
        private const int logQueryingPort = 45011;
        private readonly ConcurrentDictionary<int, TcpListener> listeners;
        private ConcurrentDictionary<RemoteClient, object> realtimePostClients;
        private ConcurrentDictionary<RemoteClient, object> logQueryClients;
        private bool disposed;

        public const string ApiKeysDataKey = "Registered API keys";
        public delegate void ExceptionEventHandler(Exception ex);
        public delegate void ClientEventHandler(RemoteClient client);
        public delegate void LogQueryReceivedEventHandler(RemoteClient client, RemoteLogRequest request);
        public event ExceptionEventHandler OnException;
        public event ClientEventHandler RealtimePostClientConnected;
        public event ClientEventHandler RealtimePostClientDisconnected;
        public event ClientEventHandler LogQueryClientConnected;
        public event ClientEventHandler LogQueryClientDisconnected;
        public event LogQueryReceivedEventHandler LogQueryReceived;

        public ConcurrentDictionary<string, string> ApiKeys { get; private set; }

        public ConcurrentDictionary<RemoteClient, object> RealtimePostClients { get { return realtimePostClients; } }

        public ConcurrentDictionary<RemoteClient, object> LogQueryClients { get { return logQueryClients; } }

        public long TotalDataUploaded
        {
            get
            {
                var realTimeData = RealtimePostClients.IsEmpty ? 0 : RealtimePostClients.Sum(c => (long)c.Key.TotalDataUploaded);
                var logQueryData = LogQueryClients.IsEmpty ? 0 : LogQueryClients.Sum(c => (long)c.Key.TotalDataUploaded);
                return realTimeData + logQueryData;
            }
        }

        public long TotalDataDownloaded
        {
            get
            {
                var realTimeData = RealtimePostClients.IsEmpty ? 0 : RealtimePostClients.Sum(c => (long)c.Key.TotalDataDownloaded);
                var logQueryData = LogQueryClients.IsEmpty ? 0 : LogQueryClients.Sum(c => (long)c.Key.TotalDataDownloaded);
                return realTimeData + logQueryData;
            }
        }

        public string ClientsNamesPretty
        {
            get
            {
                var names = new List<string>();
                foreach (var client in RealtimePostClients.Keys) { names.Add(client.Owner); }
                foreach (var client in LogQueryClients.Keys) { names.Add(client.Owner); }
                names = names.Distinct().ToList();

                if (names.Count == 0) { return null; }
                if (names.Count == 1) { return names[0]; }
                if (names.Count == 2) { return names[0] + " & " + names[1]; }

                var namesPretty = "";

                for (var i = 0; i < names.Count; i++)
                {
                    if (i == names.Count - 2)
                    {
                        namesPretty += names[i] + " & ";
                    }
                    else if (i == names.Count - 1)
                    {
                        namesPretty += names[i];
                    }
                    else
                    {
                        namesPretty += names[i] + ", ";
                    }
                }

                return namesPretty;
            }
        }



        public RemoteServer()
        {
            listeners = new ConcurrentDictionary<int, TcpListener>();
            realtimePostClients = new ConcurrentDictionary<RemoteClient, object>();
            logQueryClients = new ConcurrentDictionary<RemoteClient, object>();
            ApiKeys = new ConcurrentDictionary<string, string>();

            if (DataManager.DataExists("Yam", ApiKeysDataKey))
            {
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
            }

            // Start real-time post socket listener.
            listeners[realtimePostPort] = new TcpListener(IPAddress.Any, realtimePostPort);
            listeners[realtimePostPort].Start();
            Task.Run(() => RealtimePostListenerLoop());


            // Start log querying socket listener.
            listeners[logQueryingPort] = new TcpListener(IPAddress.Any, logQueryingPort);
            listeners[logQueryingPort].Start();
            Task.Run(() => LogQueryingListenerLoop());
        }

        ~RemoteServer()
        {
            if (disposed) { return; }
            Dispose();
        }



        public void Dispose()
        {
            if (disposed) { return; }
            disposed = true;

            foreach (var listener in listeners.Values)
            {
                listener.Stop();
            }

            foreach (var client in RealtimePostClients.Keys)
            {
                client.Socket.Close();
            }

            GC.SuppressFinalize(this);
        }

        public void SendPost(object post)
        {
            if (post == null) { throw new ArgumentNullException("post"); }

            foreach (var client in RealtimePostClients)
            {
                var json = JsonSerializer.SerializeToString(post);
                var bytes = Encoding.UTF8.GetBytes(json);

                if (client.Key.EnableCompression)
                {
                    bytes = DataUtilities.GZipCompress(bytes);
                }

                var size = bytes.Length;
                client.Key.Socket.Client.SendBufferSize = size;
                client.Key.Socket.GetStream().Write(bytes, 0, size);
                client.Key.TotalDataUploaded += size;
            }
        }

        public void SendLogEntries(RemoteClient client, HashSet<LogEntry> entries)
        {
            if (client == null) { throw new ArgumentNullException("client"); }
            if (entries == null) { throw new ArgumentNullException("entries"); }

            var json = JsonSerializer.SerializeToString(entries);
            var bytes = Encoding.UTF8.GetBytes(json);

            if (client.EnableCompression)
            {
                bytes = DataUtilities.GZipCompress(bytes);
            }

            var size = bytes.Length;
            client.Socket.Client.SendBufferSize = size;
            client.Socket.GetStream().Write(bytes, 0, size);
            client.TotalDataUploaded += size;
        }



        private void RealtimePostListenerLoop()
        {
            while (!disposed)
            {
                try
                {
                    var socket = listeners[realtimePostPort].AcceptTcpClient();
                    Task.Run(() =>
                    {
                        var client = AcceptClient(ref realtimePostClients, ref RealtimePostClientConnected, socket);
                        HandleRealtimePostClient(client);
                    });
                }
                catch (Exception ex)
                {
                    if (OnException == null) { continue; }
                    OnException(ex);
                }
            }
        }

        private void LogQueryingListenerLoop()
        {
            while (!disposed)
            {
                try
                {
                    var socket = listeners[logQueryingPort].AcceptTcpClient();
                    Task.Run(() =>
                    {
                        var client = AcceptClient(ref logQueryClients, ref LogQueryClientConnected, socket);
                        HandleLogQueryClient(client);
                    });
                }
                catch (Exception ex)
                {
                    if (OnException == null) { continue; }
                    OnException(ex);
                }
            }
        }

        private RemoteClient AcceptClient(ref ConcurrentDictionary<RemoteClient, object> clients, ref ClientEventHandler connectedEvent, TcpClient socket)
        {
            RemoteClientConnectionRequest req;
            var data = new byte[1024];
            var owner = "";
            try
            {
                socket.GetStream().Read(data, 0, 1024);
                var json = Encoding.UTF8.GetString(data);
                req = JsonSerializer.DeserializeFromString<RemoteClientConnectionRequest>(json);
            }
            catch (Exception)
            {
                socket.Close();
                return null;
            }

            var unhashedKey = "";
            owner = CheckApiKey(req.HashedApiKey, out unhashedKey);

            if (String.IsNullOrEmpty(owner) || clients.Keys.Any(c => c.ApiKey == unhashedKey))
            {
                try
                {
                    var message = Encoding.UTF8.GetBytes("Connection rejected. :p");
                    socket.GetStream().Write(message, 0, message.Length);
                    socket.Close();
                }
                catch (Exception) { }
                return null;
            }

            var client = new RemoteClient
            {
                ApiKey = unhashedKey,
                EnableCompression = req.UseGZip,
                Socket = socket,
                Owner = owner,
                FirstConnected = DateTime.UtcNow
            };

            clients[client] = new object();

            var acptMsg = Encoding.UTF8.GetBytes(@"Connection accepted! \o/");
            socket.GetStream().Write(acptMsg, 0, acptMsg.Length);

            if (connectedEvent != null) { connectedEvent(client); }
            return client;
        }

        private void HandleRealtimePostClient(RemoteClient client)
        {
            var mre = new ManualResetEvent(false);
            var waitTime = TimeSpan.FromMilliseconds(500);
            var lastCheck = DateTime.UtcNow;

            client.Socket.ReceiveBufferSize = 1024;

            while (!disposed && client.Socket.Connected)
            {
                mre.WaitOne(waitTime);

                if ((DateTime.UtcNow - lastCheck).TotalSeconds > 15)
                {
                    try
                    {
                        client.Socket.GetStream().Read(new byte[1], 0, 1);
                    }
                    catch (Exception)
                    {
                        break;
                    }
                }
            }

            client.Socket.Close();

            object temp;
            RealtimePostClients.TryRemove(client, out temp);
            if (RealtimePostClientDisconnected != null) { RealtimePostClientDisconnected(client); }
        }

        private void HandleLogQueryClient(RemoteClient client)
        {
            var mre = new ManualResetEvent(false);
            var waitTime = TimeSpan.FromMilliseconds(500);
            var lastQuery = DateTime.UtcNow;

            client.Socket.ReceiveBufferSize = 1024;

            while (!disposed && client.Socket.Connected)
            {
                if (client.Socket.Available == 0)
                {
                    mre.WaitOne(waitTime);

                    if ((DateTime.UtcNow - lastQuery).TotalSeconds > 15)
                    {
                        try
                        {
                            client.Socket.GetStream().Read(new byte[1], 0, 1);
                        }
                        catch (Exception)
                        {
                            break;
                        }
                    }
                    continue;
                }

                var data = new byte[client.Socket.Available];
                try
                {
                    client.Socket.GetStream().Read(data, 0, client.Socket.Available);
                }
                catch (Exception) { break; }
                client.TotalDataDownloaded += data.Length;
                lastQuery = DateTime.UtcNow;

                if (client.EnableCompression)
                {
                    data = DataUtilities.GZipDecompress(data);
                }

                var json = Encoding.UTF8.GetString(data);
                var req = JsonSerializer.DeserializeFromString<RemoteLogRequest>(json);
                if (LogQueryReceived == null) { continue; }
                LogQueryReceived(client, req);
            }

            client.Socket.Close();

            object temp;
            LogQueryClients.TryRemove(client, out temp);
            if (LogQueryClientDisconnected != null) { LogQueryClientDisconnected(client); }
        }

        private string CheckApiKey(byte[] clientKey, out string unhashedKey)
        {
            unhashedKey = null;
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
                        unhashedKey = realKey.Key;
                        return realKey.Value;
                    }
                }
            }
            return null;
        }
    }
}
