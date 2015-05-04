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
        //TODO: Use listener socket + sender socket (yet to be implemented) classes for communication, rather than reimplementing each socket.

        //private bool disposed;
        //private IPEndPoint targetEP;

        //# region Private listening fields.

        //private readonly ManualResetEvent listenerThreadDeadMRE = new ManualResetEvent(false);
        //private UdpClient listener;
        //private EndPoint endPoint = new IPEndPoint(new IPAddress(new byte[] { 0, 0, 0, 0 }), 60000);
        //private Thread listenerThread;

        //# endregion

        //# region Private transmitting fields.

        //private static UdpClient broadcastSocket;
        //private static bool shutdown;
        //private static uint dataSent;

        //# endregion

        //# region Public properties.

        //public EventManager<ClientSocketEventType> EventManager { get; private set; }

        //public ulong TotalDataReceived { get; private set; }

        //public ulong TotalDataSent { get; private set; }

        //# endregion



        //public YamClientLocal(char sender)
        //{
        //    if ("PG".Contains(Char.ToUpperInvariant(sender))) { throw new ArgumentException("Invalid sender char selected. Supported chars include: 'P' and 'G'.", "sender"); }

        //    EventManager = new EventManager<ClientSocketEventType>(ClientSocketEventType.InternalException);

        //    // Initialise listener.
        //    listener = new UdpClient();
        //    listener.ExclusiveAddressUse = false;
        //    listener.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
        //    listener.Client.Bind(LocalSocketIPEndPoints.YamToAll);
        //    listener.JoinMulticastGroup(LocalSocketIPEndPoints.MulticastAddress);
        //    listenerThread = new Thread(Listen) { IsBackground = true };
        //    listenerThread.Start();

        //    // Initialise sender.
        //    targetEP = GetTargetEP(sender);
        //    broadcastSocket = new UdpClient();
        //    broadcastSocket.JoinMulticastGroup(LocalSocketIPEndPoints.MulticastAddress);
        //}

        //~YamClientLocal()
        //{
        //    if (!disposed)
        //    {
        //        Dispose();
        //    }
        //}



        //public void Dispose()
        //{
        //    if (disposed) { return; }

        //    disposed = true;

        //    broadcastSocket.Close();
        //    broadcastSocket.Client.Dispose();
        //    listenerThreadDeadMRE.WaitOne();
        //    listenerThreadDeadMRE.Dispose();
        //    listener.Close();
        //    listener.Client.Dispose();

        //    GC.SuppressFinalize(this);
        //}

        //public void SendData(string messageType, object objData)
        //{
        //    if (String.IsNullOrEmpty(messageType)) { throw new ArgumentException("'messageType' cannot be null or empty.", "messageType"); }
        //    if (objData == null) { throw new ArgumentNullException("objData"); }
        //    if (disposed) { return; }

        //    var json = JsonConvert.SerializeObject(objData, Formatting.Indented);
        //    var bytes = Encoding.BigEndianUnicode.GetBytes(messageType + json);
        //    TotalDataSent += (uint)broadcastSocket.Send(bytes, bytes.Length, targetEP);
        //}



        //private IPEndPoint GetTargetEP(char sender)
        //{
        //    var sndrUp = Char.ToUpperInvariant(sender);
        //    switch (sndrUp)
        //    {
        //        case 'P':
        //        {
        //            return LocalSocketIPEndPoints.PhamToYam;
        //        }
        //        case 'G':
        //        {
        //            return LocalSocketIPEndPoints.GhamToYam;
        //        }
        //        default:
        //        {
        //            throw new NotSupportedException();
        //        }
        //    }
        //}

        //private void Listen()
        //{
        //    while (!disposed)
        //    {
        //        var bytes = new byte[0];
        //        var ep = new IPEndPoint(0, 0);

        //        try
        //        {
        //            bytes = listener.Receive(ref ep);
        //            TotalDataReceived += (uint)bytes.Length;
                
        //            var data = Encoding.BigEndianUnicode.GetString(bytes);

        //            if (data.Length < 3) { continue; }

        //            var payload = data.Remove(0, 3);
        //            switch (Char.ToUpperInvariant(data[1]))
        //            {
        //                case 'Q': // Received a question from Yam.
        //                {
        //                    var q = JsonConvert.DeserializeObject<Question>(payload);
        //                    EventManager.CallListeners(ClientSocketEventType.Question, q);
        //                    break;
        //                }
        //                case 'A': // Received an answer from Yam.
        //                {
        //                    var a = JsonConvert.DeserializeObject<Answer>(payload);
        //                    EventManager.CallListeners(ClientSocketEventType.Answer, a);
        //                    break;
        //                }
        //                case 'C': // Received a command from Yam.
        //                {
        //                    if (!String.IsNullOrEmpty(payload))
        //                    {
        //                        EventManager.CallListeners(ClientSocketEventType.Command, payload);
        //                    }
        //                    break;
        //                }
        //                case 'D': // Received misc. data from Yam.
        //                {
        //                    if (!String.IsNullOrEmpty(payload))
        //                    {
        //                        EventManager.CallListeners(ClientSocketEventType.Data, payload);
        //                    }
        //                    break;
        //                }
        //                case 'F': // Received a requested file from Yam.
        //                {
        //                    if (!String.IsNullOrEmpty(payload))
        //                    {
        //                        EventManager.CallListeners(ClientSocketEventType.File, payload);
        //                    }
        //                    break;
        //                }
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            EventManager.CallListeners(ClientSocketEventType.InternalException, ex);
        //        }
        //    }

        //    listenerThreadDeadMRE.Set();
        //}
    }
}
