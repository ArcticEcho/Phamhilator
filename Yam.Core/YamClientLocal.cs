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
using Newtonsoft.Json;

namespace Phamhilator.Yam.Core
{
    public partial class YamClientLocal : IDisposable
    {
        private readonly LocalUDPSocketSender sender;
        private readonly LocalUDPSocketListener listener;
        private bool disposed;

        public EventManager<LocalRequest.RequestType> EventManager { get; private set; }

        public ulong TotalDataReceived { get { return listener.TotalDataReceived; } }

        public ulong TotalDataSent { get { return sender.TotalDataSent; } }



        public YamClientLocal(string callerBot)
        {
            if (String.IsNullOrEmpty(callerBot)) { throw new ArgumentException("callerBot"); }
            var caller = callerBot.ToUpperInvariant();
            if (caller != "GHAM" && caller != "PHAM") { throw new ArgumentException("Invalid 'callerBot' specified. Supported names include: 'PHAM' and 'GHAM'.", "callerBot"); }
            EventManager = new EventManager<LocalRequest.RequestType>(LocalRequest.RequestType.Exception);
            
            var listenPort = (int)(caller == "PHAM" ? LocalSocketPort.YamToPham : LocalSocketPort.YamToGham);
            var sendPort = (int)(caller == "PHAM" ? LocalSocketPort.PhamToYam : LocalSocketPort.GhamToYam);

            listener = new LocalUDPSocketListener(listenPort);
            EventManager = new EventManager<LocalRequest.RequestType>(LocalRequest.RequestType.Exception);
            listener.OnMessage += req => EventManager.CallListeners(req.Type, req);
            listener.OnException += ex => EventManager.CallListeners(LocalRequest.RequestType.Exception, ex);

            sender = new LocalUDPSocketSender(sendPort);
        }

        ~YamClientLocal()
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

            listener.Dispose();
            sender.Dispose();

            GC.SuppressFinalize(this);
        }

        public void SendData(LocalRequest req)
        {
            if (disposed) { return; }
            if (req == null) { throw new ArgumentNullException("req"); }

            sender.SendData(req);
        }

        public byte[] RequestData(string owner, string key)
        {
            var reqId = LocalRequest.GetNewID();
            LocalRequest response = null;
            Action<LocalRequest> dataReceivedAction;
            using (var dataWaitMre = new ManualResetEvent(false))
            {
                dataReceivedAction = new Action<LocalRequest>(r =>
                {
                    if ((Guid)r.Options["FullFillReqID"] == reqId)
                    {
                        response = r;
                        dataWaitMre.Set();
                    }
                });
                var req = new LocalRequest
                {
                    ID = reqId,
                    Type = LocalRequest.RequestType.DataManagerRequest,
                    Options = new Dictionary<string, object>
                    {
                        { "DMReqType", "GET" },
                        { "Owner", owner },
                        { "Key", key }
                    }
                };
                EventManager.ConnectListener(LocalRequest.RequestType.DataManagerRequest, dataReceivedAction);
                sender.SendData(req);
                dataWaitMre.WaitOne();
            }

            EventManager.DisconnectListener(LocalRequest.RequestType.DataManagerRequest, dataReceivedAction);

            return (byte[])response.Data;
        }

        public void UpdateData(string owner, string key, byte[] data)
        {

        }

        public void DeleteData(string owner, string key)
        {

        }
    }
}
