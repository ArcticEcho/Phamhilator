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
using System.Threading;
using ServiceStack.Text;

namespace Phamhilator.Yam.Core
{
    using RequestType = LocalRequest.RequestType;

    public partial class LocalRequestClient : IDisposable
    {
        private readonly LocalSocketSender sender;
        private readonly LocalSocketListener listener;
        private bool disposed;

        public EventManager<LocalRequest.RequestType> EventManager { get; private set; }

        public ulong TotalDataReceived { get { return listener.TotalDataReceived; } }

        public ulong TotalDataSent { get { return sender.TotalDataSent; } }



        public LocalRequestClient(string callerBot)
        {
            if (String.IsNullOrEmpty(callerBot)) { throw new ArgumentException("callerBot"); }
            var caller = callerBot.ToUpperInvariant();
            if (caller != "GHAM" && caller != "PHAM") { throw new ArgumentException("Invalid 'callerBot' specified. Supported names include: 'PHAM' and 'GHAM'.", "callerBot"); }
            EventManager = new EventManager<RequestType>(RequestType.Exception);
            
            var listenPort = (int)(caller == "PHAM" ? LocalSocketPort.YamToPham : LocalSocketPort.YamToGham);
            var sendPort = (int)(caller == "PHAM" ? LocalSocketPort.PhamToYam : LocalSocketPort.GhamToYam);

            listener = new LocalSocketListener(listenPort);
            EventManager = new EventManager<RequestType>(RequestType.Exception);
            listener.OnMessage += HandleMessage;
            listener.OnException += HandleException;

            sender = new LocalSocketSender(sendPort);
        }

        ~LocalRequestClient()
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

        public string RequestData(string owner, string key)
        {
            var reqId = LocalRequest.GetNewID();
            LocalRequest response = null;
            Action<LocalRequest> dataReceivedAction;
            using (var dataWaitMre = new ManualResetEvent(false))
            {
                dataReceivedAction = new Action<LocalRequest>(r =>
                {
                    if (Guid.Parse((string)r.Options["FullFillReqID"]) == reqId)
                    {
                        response = r;
                        dataWaitMre.Set();
                    }
                });
                var req = new LocalRequest
                {
                    ID = reqId,
                    Type = RequestType.DataManagerRequest,
                    Options = new Dictionary<string, object>
                    {
                        { "DMReqType", "GET" },
                        { "Owner", owner },
                        { "Key", key }
                    }
                };
                EventManager.ConnectListener(RequestType.DataManagerRequest, dataReceivedAction);
                sender.SendData(req);
                dataWaitMre.WaitOne();
            }

            EventManager.DisconnectListener(RequestType.DataManagerRequest, dataReceivedAction);

            return (string)response.Data;
        }

        public void UpdateData(string owner, string key, string data)
        {
            var req = new LocalRequest
            {
                ID = LocalRequest.GetNewID(),
                Type = RequestType.DataManagerRequest,
                Options = new Dictionary<string, object>
                {
                    { "DMReqType", "UPD" },
                    { "Owner", owner },
                    { "Key", key }
                },
                Data = data
            };

            sender.SendData(req);
        }

        public void DeleteData(string owner, string key)
        {
            var req = new LocalRequest
            {
                ID = LocalRequest.GetNewID(),
                Type = RequestType.DataManagerRequest,
                Options = new Dictionary<string, object>
                {
                    { "DMReqType", "DEL" },
                    { "Owner", owner },
                    { "Key", key }
                }
            };

            sender.SendData(req);
        }



        private void HandleMessage(LocalRequest req)
        {
            try
            {
                if (req.Type == RequestType.Answer)
                {
                    var a = JsonSerializer.DeserializeFromString<Answer>(req.Data.ToString());
                    EventManager.CallListeners(req.Type, a);
                    return;
                }

                if (req.Type == RequestType.Question)
                {
                    var q = JsonSerializer.DeserializeFromString<Question>(req.Data.ToString());
                    EventManager.CallListeners(req.Type, q);
                    return;
                }

                EventManager.CallListeners(req.Type, req);
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
        }

        private void HandleException(Exception ex)
        {
            EventManager.CallListeners(RequestType.Exception, new LocalRequest
            {
                ID = LocalRequest.GetNewID(),
                Type = RequestType.Exception,
                Data = ex
            });
        }
    }
}
