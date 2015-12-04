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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading;
using ChatExchangeDotNet;
using Phamhilator.Yam.Core;
using ServiceStack.Text;

namespace Phamhilator.Yam.UI
{
    using RequestType = LocalRequest.RequestType;

    public class Program
    {
        private static readonly ManualResetEvent shutdownMre = new ManualResetEvent(false);
        private static string apiKeySenderEmail;
        private static string apiKeySenderPwd;
        private static string apiKeySenderHost;
        private static Client chatClient;
        private static AuthorisedUsers authUsers;
        private static Room socvr;
        private static RealtimePostSocket postSocket;
        private static LocalServer locServer;
        private static RemoteServer remServer;
        private static AppveyorUpdater updater;
        private static DateTime startTime;
        private static bool phamAlive;



        private static void Main(string[] args)
        {
            Console.Title = "Yam v2";
            Console.CancelKeyPress += (o, oo) =>
            {
                oo.Cancel = true;
                shutdownMre.Set();
            };

            Console.Write("Authenticating...");
            InitialiseFromConfig();
            Console.Write("done.\nInitialising updater...");
            InitialiseUpdater();
            Console.Write("done.\nJoining chat room(s)...");
            JoinRooms();
            Console.Write("done.\nStarting server...");
            InitialiseLocalServer();
            InitialiseRemoteServer();

#if DEBUG
            Console.WriteLine("done.\nYam v2 started (debug).");
#else
            Console.WriteLine("done.\nYam v2 started.");
#endif
            startTime = DateTime.UtcNow;

            Console.Write("\nStarting Pham...");
            StartPham();

            shutdownMre.WaitOne();

            Console.Write("Stopping...");

            socvr?.Leave();
            shutdownMre.Dispose();
            postSocket?.Close();
            postSocket?.Dispose();
            locServer?.Dispose();
            remServer?.Dispose();
            authUsers?.Dispose();
            chatClient?.Dispose();

            Console.WriteLine("done.");
        }



        private static void JoinRooms()
        {
            var cr = new ConfigReader();

            socvr = chatClient.JoinRoom(cr.GetSetting("room"));
            socvr.EventManager.ConnectListener(EventType.UserMentioned, new Action<Message>(m => HandleChatCommand(socvr, m)));
        }

        private static void InitialiseFromConfig()
        {
            var cr = new ConfigReader();

            var email = cr.GetSetting("se email");
            var pwd = cr.GetSetting("se pass");

            chatClient = new Client(email, pwd);

            apiKeySenderEmail = cr.GetSetting("api key sender email");
            apiKeySenderPwd = cr.GetSetting("api key sender pass");
            apiKeySenderHost = cr.GetSetting("smtp host");

            authUsers = new AuthorisedUsers();
        }

        private static void InitialiseUpdater()
        {
            var cr = new ConfigReader();
            var tkn = cr.GetSetting("appveyor");

            if (!string.IsNullOrWhiteSpace(tkn))
            {
                updater = new AppveyorUpdater(tkn, "ArcticEcho", "phamhilator");
            }
        }

        private static void InitialiseRemoteServer()
        {
            remServer = new RemoteServer();
            remServer.RealtimePostClientConnected += client =>
            {
                Console.WriteLine("Remote client: " + client.Owner + " has connected (real-time post socket).");
            };
            remServer.RealtimePostClientDisconnected += client =>
            {
                Console.WriteLine("Remote client: " + client.Owner + " has disconnected (real-time post socket).");
            };
        }

        private static void InitialiseLocalServer()
        {
            locServer = new LocalServer();
            locServer.PhamEventManager.ConnectListener(RequestType.Exception, new Action<LocalRequest>(req =>
            {
                Console.WriteLine("Warning, exception thrown from Pham:\n\n" + req.Dump());
            }));
            locServer.PhamEventManager.ConnectListener(RequestType.DataManagerRequest, new Action<LocalRequest>(req =>
            {
                HandleDataManagerRequest(true, req);
            }));
            locServer.PhamEventManager.ConnectListener(RequestType.Info, new Action<LocalRequest>(req =>
            {
                var data = ((string)req?.Data ?? "").Trim().ToUpperInvariant();
                switch (data)
                {
                    case "ALIVE":
                    {
                        break;
                    }
                    case "DEAD":
                    {
                        break;
                    }
                }
            }));

            postSocket = new RealtimePostSocket(true);
            postSocket.OnActiveQuestion += HandleActiveQuestion;
            postSocket.OnActiveAnswer += HandleActiveAnswer;
        }

        private static void StartPham()
        {
            var waitMre = new ManualResetEvent(false);
            var infoCheckAct = new Action<LocalRequest>(req =>
            {
                if (((string)req?.Data ?? "").Trim().ToUpperInvariant() == "ALIVE")
                {
                    waitMre?.Set();
                    phamAlive = true;
                }
            });
            locServer.PhamEventManager.ConnectListener(RequestType.Info, infoCheckAct);
            locServer.SendData(true, new LocalRequest
            {
                ID = LocalRequest.GetNewID(),
                Type = RequestType.Command,
                Data = "STATUS"
            });
            waitMre.WaitOne(TimeSpan.FromSeconds(10));
            waitMre.Reset();

            if (!phamAlive)
            {
                var files = Directory.EnumerateFiles(".");
                var phamExeCrTime = DateTime.MinValue;
                var phamExe = "";

                foreach (var file in files)
                {
                    var name = Path.GetFileName(file);
                    var fileCrTime = new FileInfo(file).CreationTimeUtc;
                    if (name.Contains("Pham") && name.EndsWith(".exe") && fileCrTime > phamExeCrTime)
                    {
                        phamExe = file;
                        phamExeCrTime = fileCrTime;
                    }
                }

                if (string.IsNullOrWhiteSpace(phamExe))
                {
                    Console.WriteLine("skipped (can't find Pham executable).");
                    return;
                }

                if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                {
                    Process.Start(phamExe);
                }
                else
                {
                    try
                    {
                        Process.Start("mono", phamExe);
                    }
                    catch
                    {
                        Console.WriteLine("skipped (unknown error encountered).");
                    }
                }

                waitMre.WaitOne(TimeSpan.FromMinutes(3));
                waitMre.Dispose();

                Console.WriteLine($"{(phamAlive ? "done" : "skipped (timed out)")}.");
            }
            else
            {
                Console.WriteLine("skipped (already running).");
            }

            locServer.PhamEventManager.DisconnectListener(RequestType.Info, infoCheckAct);
        }

        private static void HandleChatCommand(Room room, Message command)
        {
            try
            {
                if (UserAccess.Owners.Any(id => id == command.Author.ID) || command.Author.IsRoomOwner || command.Author.IsMod)
                {
                    var cmdMatches = HandleOwnerCommand(room, command);

                    if (!cmdMatches)
                    {
                        cmdMatches = HandlePrivilegedUserCommand(room, command, true);

                        if (!cmdMatches)
                        {
                            HandleNormalUserCommand(room, command);
                        }
                    }
                }
                else if (authUsers.IDs.Any(id => id == command.Author.ID))
                {
                    var cmdMatches = HandlePrivilegedUserCommand(room, command, false);

                    if (!cmdMatches)
                    {
                        HandleNormalUserCommand(room, command);
                    }
                }
                else
                {
                    HandleNormalUserCommand(room, command);
                }
            }
            catch (Exception ex)
            {
                room.PostReplyFast(command, $"`Unable to execute command: {ex/*.Message*/}`");
            }
        }

        private static bool HandleNormalUserCommand(Room room, Message command)
        {
            var cmd = command.Content.Trim().ToUpperInvariant();

            switch (cmd)
            {
                case "ALIVE":
                {
                    var statusReport = $"`Yes, I'm alive ({DateTime.UtcNow - startTime}).`";
                    room.PostMessageFast(statusReport);
                    return true;
                }
                case "LOCAL STATS":
                {
                    var secsAlive = (DateTime.UtcNow - startTime).TotalSeconds;
                    var phamRecTotal = locServer.DataReceivedPham / 1024.0; var phamRecPerSec = phamRecTotal / secsAlive;
                    var phamSentTotal = locServer.DataSentPham / 1024.0;    var phamSentPerSec = phamSentTotal / secsAlive;
                    var ghamRecTotal = locServer.DataReceivedGham / 1024.0; var ghamRecPerSec = ghamRecTotal / secsAlive;
                    var ghamSentTotal = locServer.DataSentGham / 1024.0;    var ghamSentPerSec = ghamSentTotal / secsAlive;
                    var overallTotal = phamRecTotal + phamSentTotal + ghamRecTotal + ghamSentTotal;
                    var overallPerSec = overallTotal / secsAlive;
                    var dataReport = "    Local Yam data report (in KiB):\n" +
                                        "    Total transferred:  " + Math.Round(overallTotal) + " (~" + Math.Round(overallPerSec, 1) + "/s)\n" +
                                        "    Sent to Pham:       " + Math.Round(phamSentTotal) + " (~" + Math.Round(phamSentPerSec, 1) + "/s)\n" +
                                        "    Received from Pham: " + Math.Round(phamRecTotal) + " (~" + Math.Round(phamRecPerSec, 1) + "/s)\n" +
                                        "    Sent to Gham:       " + Math.Round(ghamSentTotal) + " (~" + Math.Round(ghamSentPerSec, 1) + "/s)\n" +
                                        "    Received from Gham: " + Math.Round(ghamRecTotal) + " (~" + Math.Round(ghamRecPerSec, 1) + "/s)";
                    room.PostMessageFast(dataReport);
                    return true;
                }
                case "REMOTE STATS":
                {
                    var secsAlive = (DateTime.UtcNow - startTime).TotalSeconds;
                    var clientCount = remServer.RealtimePostClients.Count;
                    var overallSent = remServer.TotalDataUploaded / 1024.0;
                    var overallSentPerSec = Math.Round(overallSent / secsAlive, 1);
                    var overallRec = remServer.TotalDataDownloaded / 1024.0;
                    var overallRecPerSec = Math.Round(overallRec / secsAlive, 1);
                    var dataReport = "    Remote Yam data report (in KiB):\n" +
                                        "    Total sent:     " + Math.Round(overallSent) + " (~" + overallSentPerSec + "/s)\n" +
                                        "    Total received: " + Math.Round(overallRec) + " (~" + overallRecPerSec + "/s)\n" +
                                        "    Clients (" + clientCount + ")" + ":    " + remServer.ClientsNamesPretty;
                    room.PostMessageFast(dataReport);
                    return true;
                }
                case "COMMANDS":
                {
                    var msg = "`See` [`here`](https://github.com/ArcticEcho/Phamhilator/wiki/Yam-Chat-Commands \"Chat Commands Wiki\")`.`";
                    room.PostReplyFast(command, msg);
                    return true;
                }
                case "VERSION":
                {
                    var msg = $"`My current version is: `{updater.CurrentVersion}.`";
                    room.PostReplyFast(command, msg);
                    return true;
                }
                default:
                {
                    return false;
                }
            }
        }

        private static bool HandlePrivilegedUserCommand(Room room, Message command, bool isOwner)
        {
            return false;
        }

        private static bool HandleOwnerCommand(Room room, Message command)
        {
            var cmd = command.Content.Trim().ToUpperInvariant();

            if (cmd.StartsWith("ADD REMOTE CLIENT"))
            {
                var emailKeyPair = command.Content.Trim().Remove(0, 17).Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                var owner = emailKeyPair[0];
                var email = emailKeyPair[1];
                var key = Guid.NewGuid().ToString().ToLowerInvariant();

                remServer.ApiKeys[key] = owner;
                var otherKeys = DataManager.LoadData("Yam", RemoteServer.ApiKeysDataKey) + "\n";
                DataManager.SaveData("Yam", RemoteServer.ApiKeysDataKey, otherKeys + owner + ":" + key);
                SendApiKeyEmail(command.Author.Name, email, key);

                room.PostReply(command, "`Client successfully added; an email has been sent with the API key.`");
            }
            else if (cmd.StartsWith("ADD USER"))
            {
                var id = 0;

                if (!int.TryParse(new string(cmd.Where(char.IsDigit).ToArray()), out id))
                {
                    room.PostReply(command, "`Please enter a valid user ID..`");
                    return true;
                }

                authUsers.AddUser(id);

                room.PostReply(command, "`User added.`");
            }
            else if (cmd == "STOP")
            {
                room.PostReply(command, "`Stopping...`");
                shutdownMre.Set();
            }
            else if (cmd == "UPDATE")
            {
                UpdateBots(room, command);
            }
            else
            {
                return false;
            }

            return true;
        }

        private static void UpdateBots(Room rm, Message cmd)
        {
            if (updater == null)
            {
                rm.PostReplyFast(cmd, "This feature has been disabled by the host (API key not specified).");
                return;
            }

            var remVer = updater.LatestVersion;

            if (updater.CurrentVersion == remVer)
            {
                rm.PostReplyFast(cmd, "There aren't any updates available at the moment.");
                return;
            }

            rm.PostReplyFast(cmd, $"I've found (and now applying) a new version, `{remVer}`:");
            rm.PostMessageFast($"> {updater.LatestVerMessage}");

            var exes = updater.UpdateAssemblies();

            if (exes != null)
            {
                rm.PostReplyFast(cmd, "Update successful! Now rebooting...");
                locServer.SendData(true, new LocalRequest
                {
                    ID = LocalRequest.GetNewID(),
                    Type = RequestType.Command,
                    Data = "SHUTDOWN"
                });

                var waitMre = new ManualResetEvent(false);
                var act = new Action<LocalRequest>(req =>
                {
                    if (((string)req?.Data ?? "").Trim().ToUpperInvariant() == "DEAD")
                    {
                        waitMre.Set();
                    }
                });
                locServer.PhamEventManager.ConnectListener(RequestType.Info, act);

                waitMre.WaitOne();
                waitMre.Dispose();

                var yamExe = exes.First(x => Path.GetFileName(x).Contains("Yam"));

                if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                {
                    Process.Start(yamExe);
                }
                else
                {
                    Process.Start($"mono {yamExe}");
                }

                shutdownMre.Set();
            }
            else
            {
                rm.PostReplyFast(cmd, "Update failed!");
            }
        }

        private static void HandleActiveQuestion(Question q)
        {
            var locReq = new LocalRequest { Type = RequestType.Question, Data = q };
            locServer.SendData(true, locReq);
            locServer.SendData(false, locReq);

            remServer.SendPost(q);
        }

        private static void HandleActiveAnswer(Answer a)
        {
            var locReq = new LocalRequest { Type = RequestType.Answer, Data = a };
            locServer.SendData(true, locReq);
            locServer.SendData(false, locReq);

            remServer.SendPost(a);
        }

        private static void HandleDataManagerRequest(bool fromPham, LocalRequest req)
        {
            try
            {
                var owner = (string)req.Options["Owner"];
                var key = (string)req.Options["Key"];
                var data = (string)req.Data;

                switch ((string)req.Options["DMReqType"])
                {
                    case "GET":
                    {
                        var requestedData = DataManager.LoadData(owner, key);
                        SendDataManagerResponse(fromPham, req.ID, owner, key, requestedData);
                        return;
                    }
                    case "CHK":
                    {
                        var dataExists = DataManager.DataExists(owner, key);
                        SendDataManagerResponse(fromPham, req.ID, owner, key, dataExists);
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
                        throw new NotSupportedException();
                    }
                }
            }
            catch (Exception ex)
            {
                SendEx(fromPham, ex, new Dictionary<string, object> { { "ReceivedRequest", req } });
            }
        }

        private static void SendDataManagerResponse(bool toPham, Guid fullFillReqID, string owner, string key, object data)
        {
            try
            {
                var response = new LocalRequest
                {
                    ID = LocalRequest.GetNewID(),
                    Type = RequestType.DataManagerRequest,
                    Options = new Dictionary<string, object>
                    {
                        { "FullFillReqID", fullFillReqID },
                        { "Owner", owner },
                        { "Key", key }
                    },
                    Data = data
                };

                locServer.SendData(toPham, response);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error thrown from Yam:\n\n" + ex.ToString());

                // Post back to the listener (prevent the calling thread from hanging).
                var response = new LocalRequest
                {
                    ID = LocalRequest.GetNewID(),
                    Type = RequestType.DataManagerRequest,
                    Options = new Dictionary<string, object>
                    {
                        { "FullFillReqID", fullFillReqID }
                    }
                };
                locServer.SendData(toPham, response);
            }
        }

        private static void SendApiKeyEmail(string acceptedBy, string recipient, string key)
        {
            var ip = IPFetcher.FetchIP();

            using (var client = new SmtpClient())
            {
                client.Port = 587;
                client.Host = apiKeySenderHost;
                client.EnableSsl = true;
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                client.Credentials = new NetworkCredential(apiKeySenderEmail, apiKeySenderPwd);

                using (var mm = new MailMessage(apiKeySenderEmail, recipient)
                {
                    Subject = "Phamhilator API Key",
                    Body = "This is an automated message sent from the Phamhilator Network (upon request from yourself).\n" +
                           "Your application for an API key has been successfully received & accepted by " + acceptedBy + "!\n\n" +
                           "Your API key is: " + key + "\n" +
                           "Our server's IP: " + ip + "\n\n" +
                           "Regards,\nThe Pham Team"
                })
                {
                    client.Send(mm);
                }
            }
        }

        private static void SendEx(bool toPham, Exception ex, Dictionary<string, object> additionalInfo = null)
        {
            try
            {
                locServer.SendData(toPham, new LocalRequest
                {
                    Type = RequestType.Exception,
                    Options = additionalInfo,
                    Data = ex
                });
            }
            catch (Exception e)
            {
                Console.WriteLine("Warning, exception thrown from Yam:\n\n" + e.ToString());
            }
        }
    }
}
