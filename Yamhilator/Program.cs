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
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using ChatExchangeDotNet;
using System.Net;
using System.Threading;
using Phamhilator.Yam.Core;
using Newtonsoft.Json;

namespace Phamhilator.Yam.UI
{
    using RequestType = LocalRequest.RequestType;

    public class Program
    {
        private static readonly ManualResetEvent shutdownMre = new ManualResetEvent(false);
        private static Client chatClient;
        private static Room primaryRoom;
        private static RealtimePostSocket postSocket;
        private static YamServer server;
        private static DateTime startTime;
        private static uint yamErrorCount;
        private static uint phamErrorCount;
        private static uint ghamErrorCount;



        private static void Main(string[] args)
        {
            Console.Title = "Yam v2";
            TryLogin();
            Console.Write("Joining room(s)...");
            JoinRooms();
            Console.Write("done.\nStarting server...");
            InitialiseServer();
            Console.Write("done.\nYam v2 started (press Q to exit).\n");
            primaryRoom.PostMessage("`Yam v2 started.`");
            startTime = DateTime.UtcNow;

            Task.Run(() =>
            {
                while (true)
                {
                    if (Char.ToLowerInvariant(Console.ReadKey(true).KeyChar) == 'q')
                    {
                        shutdownMre.Set();
                        return;
                    }
                }
            });

            shutdownMre.WaitOne();

            postSocket.Close();
            postSocket.Dispose();
            primaryRoom.PostMessage("`Yam v2 stopped.`");
        }

        private static void TryLogin()
        {
            Console.WriteLine("Please enter your Stack Exchange OpenID credentials.\n");

            while (true)
            {
                Console.Write("Email: ");
                var email = Console.ReadLine();

                Console.Write("Password: ");
                var password = Console.ReadLine();

                try
                {
                    Console.Write("\nAuthenticating...");

                    chatClient = new Client(email, password);

                    Console.WriteLine("login successful!");

                    return;
                }
                catch (Exception)
                {
                    Console.WriteLine("failed to login.");
                }
            }
        }

        private static void JoinRooms()
        {
            Console.Write("Joining primary room...");

            primaryRoom = chatClient.JoinRoom("http://chat.meta.stackexchange.com/rooms/773/low-quality-posts-hq");
            primaryRoom.IgnoreOwnEvents = true;
            primaryRoom.StripMentionFromMessages = true;
            primaryRoom.EventManager.ConnectListener(EventType.UserMentioned, new Action<Message>(HandleChatCommand));
        }

        private static void InitialiseServer()
        {
            postSocket = new RealtimePostSocket(true);
            postSocket.OnActiveQuestion += BroadcastQuestion;
            postSocket.OnActiveThreadAnswers += BroadcastAnswers;

            server = new YamServer();
            server.PhamEventManager.ConnectListener(RequestType.Exception, new Action<Exception>(ex =>
            {
                phamErrorCount++;
                primaryRoom.PostMessage("Warning, error detected in Pham:\n\n" + ex.ToString());
            }));
            server.GhamEventManager.ConnectListener(RequestType.Exception, new Action<Exception>(ex =>
            {
                ghamErrorCount++;
                primaryRoom.PostMessage("Warning, error detected in Gham:\n\n" + ex.ToString());
            }));
            server.PhamEventManager.ConnectListener(RequestType.DataManagerRequest, new Action<LocalRequest>(req =>
            {
                HandleDataManagerRequest(true, req);
            }));
            server.GhamEventManager.ConnectListener(RequestType.DataManagerRequest, new Action<LocalRequest>(req =>
            {
                HandleDataManagerRequest(false, req);
            }));
            //server.PhamEventManager.ConnectListener(RequestType.Info, new Action<LocalRequest>(req =>
            //{
            //    // Do something...
            //}));
            //server.GhamEventManager.ConnectListener(RequestType.Info, new Action<LocalRequest>(req =>
            //{
            //    // Do something...
            //}));
            //server.PhamEventManager.ConnectListener(RequestType.Command, new Action<LocalRequest>(req =>
            //{
            //    // Do something...
            //}));
            //server.GhamEventManager.ConnectListener(RequestType.Command, new Action<LocalRequest>(req =>
            //{
            //    // Do something...
            //}));
        }

        private static void HandleChatCommand(Message command)
        {
            if (!UserAccess.Owners.Select(u => u.ID).Contains(command.AuthorID)) { return; }

            var cmd = command.Content.Trim().ToUpperInvariant();

            switch (cmd)
            {
                case "STOP":
                {
                    primaryRoom.PostReply(command, "`Stopping...`");
                    shutdownMre.Set();
                    return;
                }
                case "STATUS":
                {
                    var hoursAlive = (DateTime.UtcNow - startTime).TotalHours;
                    var getStatus = new Func<int, string>(er => er == 0 ? "Good" : er <= 3 ? "Ok" : "Bad");
                    var getErrorRate = new Func<uint, int>(ec => (int)Math.Round(ec / hoursAlive));
                    var yamErrorRate = getErrorRate(yamErrorCount);   var yamStatus = getStatus(yamErrorRate);
                    var phamErrorRate = getErrorRate(phamErrorCount); var phamStatus = getStatus(phamErrorRate);
                    var ghamErrorRate = getErrorRate(ghamErrorCount); var ghamStatus = getStatus(ghamErrorRate);
                    var statusReport = "    Status report:\n    \n" +
                                       "    Yam:  " + yamStatus + " (" + yamErrorCount + " @ " + yamErrorRate + "/h)\n" +
                                       "    Pham: " + phamStatus + " (" + phamErrorCount + " @ " + phamErrorRate + "/h)\n" +
                                       "    Gham: " + ghamStatus + " (" + ghamErrorCount + " @ " + ghamErrorRate + "/h)";
                    primaryRoom.PostMessage(statusReport);
                    return;
                }
                case "DATA":
                {
                    var secsAlive = (DateTime.UtcNow - startTime).TotalSeconds;
                    var phamRecTotal = server.DataReceivedPham / 1024.0; var phamRecPerSec = phamRecTotal / secsAlive;
                    var phamSentTotal = server.DataSentPham / 1024.0;    var phamSentPerSec = phamSentTotal / secsAlive;
                    var ghamRecTotal = server.DataReceivedGham / 1024.0; var ghamRecPerSec = ghamRecTotal / secsAlive;
                    var ghamSentTotal = server.DataSentGham / 1024.0;    var ghamSentPerSec = ghamSentTotal / secsAlive;
                    var overallTotal = phamRecTotal + phamSentTotal + ghamRecTotal + ghamSentTotal;
                    var overallPerSec = overallTotal / secsAlive;
                    var dataReport = "    Yam Data report (in KiB):\n" +
                                     "    Total transferred:  " + Math.Round(overallTotal) + " (~" + Math.Round(overallPerSec, 1) + "/s)\n    \n" +
                                     "    Sent to Pham:       " + Math.Round(phamSentTotal) + " (~" + Math.Round(phamSentPerSec, 1) + "/s)\n" +
                                     "    Received from Pham: " + Math.Round(phamRecTotal) + " (~" + Math.Round(phamRecPerSec, 1) + "/s)\n    \n" +
                                     "    Sent to Gham:       " + Math.Round(ghamSentTotal) + " (~" + Math.Round(ghamSentPerSec, 1) + "/s)\n" +
                                     "    Received from Gham: " + Math.Round(ghamRecTotal) + " (~" + Math.Round(ghamRecPerSec, 1) + "/s)";
                    primaryRoom.PostMessage(dataReport);
                    return;
                }
                default:
                {
                    primaryRoom.PostReply(command, "`Command not recognised.`");
                    return;
                }
            }
        }

        private static void BroadcastQuestion(Question q)
        {
            var req = new LocalRequest { Type = LocalRequest.RequestType.Question, Data = q }; 
            server.SendData(true, req);
            server.SendData(false, req);
        }

        private static void BroadcastAnswers(List<Answer> answers)
        {
            foreach (var a in answers)
            {
                var req = new LocalRequest { Type = LocalRequest.RequestType.Answer, Data = a };
                server.SendData(true, req);
                server.SendData(false, req);
            }
        }

        private static void HandleDataManagerRequest(bool fromPham, LocalRequest req)
        {
            var owner = (string)req.Options["Owner"];
            var key = (string)req.Options["Key"];
            var data = GetDataFromDataManagerRequest(fromPham, req);
            if (data == null) { return; }

            switch ((string)req.Options["DMReqType"])
            {
                case "GET":
                {
                    var requestedData = DataManager.LoadData(owner, key);
                    var response = new LocalRequest
                    {
                        ID = LocalRequest.GetNewID(),
                        Type = LocalRequest.RequestType.DataManagerRequest,
                        Options = new Dictionary<string, object>
                        {
                            { "FullFillReqId", req.ID },
                            { "Owner", owner },
                            { "Key", key }
                        },
                        Data = requestedData
                    };

                    try
                    {
                        server.SendData(fromPham, null);
                    }
                    catch (Exception ex)
                    {
                        primaryRoom.PostMessage("Detected error in Yam:\n\n" + ex.ToString());
                        yamErrorCount++;
                    }
                    return;
                }
                case "UPD":
                {
                    DataManager.SaveData(owner, key, data);
                    return;
                }
                case "DEL":
                {
                    DataManager.DeleteData(owner, key);
                    return;
                }
                default:
                {
                    SendEx(fromPham, new NotSupportedException(), new Dictionary<string, object>{ { "ReceivedRequest", req } });
                    return;
                }
            }
        }

        private static byte[] GetDataFromDataManagerRequest(bool fromPham, LocalRequest req)
        {
            if (req.Data is byte[])
            {
                return (byte[])req.Data;
            }
            else if (req.Data is string)
            {
                return Encoding.BigEndianUnicode.GetBytes((string)req.Data);
            }
            else
            {
                SendEx(fromPham, new NotSupportedException(), new Dictionary<string, object> { { "ReceivedRequest", req } });
                return null;
            }
        }

        private static void SendEx(bool toPham, Exception ex, Dictionary<string, object> additionalInfo = null)
        {
            try
            {
                server.SendData(toPham, new LocalRequest
                {
                    Type = LocalRequest.RequestType.Exception,
                    Options = additionalInfo,
                    Data = ex
                });
            }
            catch (Exception e)
            {
                yamErrorCount++;
                primaryRoom.PostMessage("Detected error in Yam:\n\n" + e.ToString());
            }
        }
    }
}
