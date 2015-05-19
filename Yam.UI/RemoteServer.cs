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
        public delegate void ClientEventHandler(RemoteClient client);
        public delegate void LogQueryReceivedEventHandler(RemoteClient client, RemoteLogRequest request);
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
                try
                {
                    var json = JsonSerializer.SerializeToString(post);
                    var bytes = Encoding.UTF8.GetBytes(json);

                    if (client.Key.ConnectionRequest.EnableEncryption)
                    {
                        bytes = DataUtilities.AseEncrypt(bytes, client.Key.ConnectionRequest.ApiKey);
                    }
                    if (client.Key.ConnectionRequest.EnableCompression)
                    {
                        bytes = DataUtilities.GZipCompress(bytes);
                    }

                    var size = bytes.Length;
                    client.Key.Socket.GetStream().Write(bytes, 0, size);
                    client.Key.TotalDataUploaded += size;
                }
                catch (IOException) { }
            }
        }

        public void SendLogEntries(RemoteClient client, HashSet<LogEntry> entries)
        {
            if (client == null) { throw new ArgumentNullException("client"); }
            if (entries == null) { throw new ArgumentNullException("entries"); }

            var json = JsonSerializer.SerializeToString(entries);
            var bytes = Encoding.UTF8.GetBytes(json);

            if (client.ConnectionRequest.EnableEncryption)
            {
                bytes = DataUtilities.AseEncrypt(bytes, client.ConnectionRequest.ApiKey);
            }
            if (client.ConnectionRequest.EnableCompression)
            {
                bytes = DataUtilities.GZipCompress(bytes);
            }

            try
            {
                var size = bytes.Length;
                client.Socket.GetStream().Write(bytes, 0, size);
                client.TotalDataUploaded += size;
            }
            catch (Exception) { }
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
                catch (Exception)
                {
                    //TODO: Log something?
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
                catch (Exception)
                {
                    //TODO: Log something?
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

            owner = CheckApiKey(req.ApiKey);

            if (String.IsNullOrEmpty(owner))
            {
                try
                {
                    var message = Encoding.UTF8.GetBytes("Invalid API key.");
                    socket.GetStream().Write(message, 0, message.Length);
                    socket.Close();
                }
                catch (Exception) { }
                return null;
            }

            socket.Client.SendBufferSize = 256000;

            var client = new RemoteClient
            {
                Socket = socket,
                ConnectionRequest = req,
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

            while (!disposed && client.Socket.Connected)
            {
                mre.WaitOne(TimeSpan.FromMilliseconds(1000));
            }

            client.Socket.Close();

            object temp;
            RealtimePostClients.TryRemove(client, out temp);
            if (RealtimePostClientDisconnected != null) { RealtimePostClientDisconnected(client); }
        }

        private void HandleLogQueryClient(RemoteClient client)
        {
            var mre = new ManualResetEvent(false);

            while (!disposed && client.Socket.Connected)
            {
                if (client.Socket.Available == 0)
                {
                    mre.WaitOne(TimeSpan.FromMilliseconds(333));
                    continue;
                }

                var data = new byte[client.Socket.Available];
                try
                {
                    client.Socket.GetStream().Read(data, 0, client.Socket.Available);
                }
                catch (Exception) { continue; }
                client.TotalDataDownloaded += client.Socket.Available;

                if (client.ConnectionRequest.EnableCompression)
                {
                    data = DataUtilities.GZipDecompress(data);
                }
                if (client.ConnectionRequest.EnableEncryption)
                {
                    data = DataUtilities.AseDecrypt(data, client.ConnectionRequest.ApiKey);
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

        private string CheckApiKey(byte[] clientKey)
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
