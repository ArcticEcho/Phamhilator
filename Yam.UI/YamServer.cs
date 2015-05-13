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
using Phamhilator.Yam.Core;

namespace Phamhilator.Yam.UI
{
    internal partial class YamServer : IDisposable
    {
        private readonly LocalUDPSocketListener phamListener;
        private readonly LocalUDPSocketListener ghamListener;
        private readonly LocalUDPSocketSender phamSender;
        private readonly LocalUDPSocketSender ghamSender;
        private bool disposed;

        # region Public properties.

        public EventManager<LocalRequest.RequestType> PhamEventManager { get; private set; }

        public EventManager<LocalRequest.RequestType> GhamEventManager { get; private set; }

        /// <summary>
        /// The total number of bytes of data received from Pham.
        /// </summary>
        public ulong DataReceivedPham { get { return phamListener.TotalDataReceived; } }

        /// <summary>
        /// The total number of bytes of data received from Gham.
        /// </summary>
        public ulong DataReceivedGham { get { return ghamListener.TotalDataReceived; } }

        /// <summary>
        /// The total number of bytes of data sent to Pham.
        /// </summary>
        public ulong DataSentPham { get { return phamSender.TotalDataSent; } }

        /// <summary>
        /// The total number of bytes of data sent to Gham.
        /// </summary>
        public ulong DataSentGham { get { return ghamSender.TotalDataSent; } }

        # endregion



        public YamServer()
        {
            // Initialise listeners.
            phamListener = new LocalUDPSocketListener((int)LocalSocketPort.PhamToYam);
            ghamListener = new LocalUDPSocketListener((int)LocalSocketPort.GhamToYam);
            PhamEventManager = new EventManager<LocalRequest.RequestType>(LocalRequest.RequestType.Exception);
            GhamEventManager = new EventManager<LocalRequest.RequestType>(LocalRequest.RequestType.Exception);
            phamListener.OnMessage += r => HandleMessage(true, r);
            ghamListener.OnMessage += r => HandleMessage(false, r);
            phamListener.OnException += ex => HandleException(true, ex);
            ghamListener.OnException += ex => HandleException(false, ex);

            // Initialise senders.
            phamSender = new LocalUDPSocketSender((int)LocalSocketPort.YamToPham);
            ghamSender = new LocalUDPSocketSender((int)LocalSocketPort.YamToGham);
        }

        ~YamServer()
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

            phamListener.Dispose();
            ghamListener.Dispose();
            phamSender.Dispose();
            ghamSender.Dispose();

            GC.SuppressFinalize(this);
        }

        public void SendData(bool toPham, LocalRequest req)
        {
            if (req == null) { throw new ArgumentNullException("objData"); }
            if (disposed) { return; }

            if (toPham)
            {
                phamSender.SendData(req);
            }
            else
            {
                ghamSender.SendData(req);
            }
        }



        private void HandleMessage(bool fromPham, LocalRequest req)
        {
            if (fromPham)
            {
                PhamEventManager.CallListeners(req.Type, req);
            }
            else
            {
                GhamEventManager.CallListeners(req.Type, req);
            }
        }

        private void HandleException(bool phamSocket, Exception ex)
        {
            if (phamSocket)
            {
                PhamEventManager.CallListeners(LocalRequest.RequestType.Exception, ex);
            }
            else
            {
                GhamEventManager.CallListeners(LocalRequest.RequestType.Exception, ex);
            }
        }
    }
}
