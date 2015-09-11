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
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;
using ChatExchangeDotNet;
using Phamhilator.Yam.Core;
using ServiceStack.Text;

namespace Phamhilator.Yam.UI
{
    using System.Text.RegularExpressions;
    using RequestType = LocalRequest.RequestType;

    public class Program
    {
        private static readonly ManualResetEvent shutdownMre = new ManualResetEvent(false);
        private static readonly Regex logSearchReg = new Regex("(?i)(site:|title:|body:|score:|createdAfter:|createdBefore:|authorName:|authorRep:|authorNetworkID:)", RegexOptions.Compiled | RegexOptions.CultureInvariant);
        private static string apiKeySenderEmail;
        private static string apiKeySenderPwd;
        private static string apiKeySenderHost;
        private static Client chatClient;
        private static AuthorisedUsers authUsers;
        private static Room hq;
        private static Room socvr;
        private static RealtimePostSocket postSocket;
        private static LocalServer locServer;
        private static RemoteServer remServer;
        private static DateTime startTime;
        private static uint yamErrorCount;
        private static uint phamErrorCount;
        private static uint ghamErrorCount;



        private static void Main(string[] args)
        {
            Console.Title = "Yam v2";
            GetApiKeyEmailCreds();
            TryLogin();
            Console.Write("Joining chat room(s)...");
            JoinRooms();
            Console.Write("done.\nInitialising config data...");
            InitialiseConfigData();
            Console.Write("done.\nInitialising log...");
            PostLogger.InitialiseLogger();
            Console.Write("done.\nStarting server...");
            InitialiseLocalServer();
            InitialiseRemoteServer();
#if DEBUG
            Console.Write("done.\nYam v2 started (debug), press Q to exit.\n");
            hq.PostMessageFast("`Yam v2 started` (**`debug`**)`.`");
            socvr.PostMessageFast("`Yam v2 started` (**`debug`**)`.`");
#else
            Console.Write("done.\nYam v2 started, press Q to exit.\n");
            hq.PostMessageFast("`Yam v2 started.`");
            socvr.PostMessageFast("`Yam v2 started.`");
#endif
            startTime = DateTime.UtcNow;

            Task.Run(() =>
            {
                while (true)
                {
                    if (Char.ToLowerInvariant(Console.ReadKey(true).KeyChar) == 'q')
                    {
                        Console.WriteLine("Stopping...");
                        shutdownMre.Set();
                        return;
                    }
                }
            });

            shutdownMre.WaitOne();

            shutdownMre.Dispose();
            postSocket.Close();
            postSocket.Dispose();
            locServer.Dispose();
            remServer.Dispose();
            PostLogger.StopLogger();
            hq.PostMessageFast("`Shutdown successful.`");
            socvr.PostMessageFast("`Shutdown successful.`");
            hq.Leave();
            socvr.Leave();
            chatClient.Dispose();
        }

        private static void GetApiKeyEmailCreds()
        {
            Console.WriteLine("Please enter your API key email credentials.\n");

            Console.Write("Email: ");
            apiKeySenderEmail = Console.ReadLine();

            Console.Write("Password: ");
            apiKeySenderPwd = Console.ReadLine();

            var domain = apiKeySenderEmail.Split('@')[1].ToLowerInvariant();
            switch(domain)
            {
                case "gmail.com":
                {
                    apiKeySenderHost = "smtp.gmail.com";
                    return;
                }
                case "outlook.com":
                {
                    apiKeySenderHost = "smtp-mail.outlook.com";
                    return;
                }
                //TODO: Add more later.
            }
        }

        private static void TryLogin()
        {
            var success = false;
            while (true)
            {
                Console.WriteLine("\nPlease enter your Stack Exchange OpenID credentials.\n");

                Console.Write("Email: ");
                var email = Console.ReadLine();

                Console.Write("Password: ");
                var password = Console.ReadLine();

                try
                {
                    Console.Write("\nAuthenticating...");
                    chatClient = new Client(email, password);
                    Console.WriteLine("login successful!");
                    success = true;
                }
                catch (Exception)
                {
                    Console.WriteLine("failed to login.");
                }
                Thread.Sleep(3000);
                Console.Clear();
                if (success) { return; }
            }
        }

        private static void JoinRooms()
        {
            hq = chatClient.JoinRoom("http://chat.meta.stackexchange.com/rooms/773/");
            hq.IgnoreOwnEvents = true;
            hq.StripMention = true;
            hq.EventManager.ConnectListener(EventType.UserMentioned, new Action<Message>(m => HandleChatCommand(hq, m)));

            socvr = chatClient.JoinRoom("http://chat.stackoverflow.com/rooms/41570/");//("http://chat.stackoverflow.com/rooms/68414");//
            socvr.IgnoreOwnEvents = true;
            socvr.StripMention = true;
            socvr.EventManager.ConnectListener(EventType.UserMentioned, new Action<Message>(m => HandleChatCommand(socvr, m)));
        }

        private static void InitialiseConfigData()
        {
            authUsers = new AuthorisedUsers();
        }

        private static void InitialiseRemoteServer()
        {
            remServer = new RemoteServer();
            remServer.RealtimePostClientConnected += client =>
            {
                hq.PostMessage("`Remote client: " + client.Owner + " has connected (real-time post socket).`");
            };
            remServer.RealtimePostClientDisconnected += client =>
            {
                hq.PostMessage("`Remote client: " + client.Owner + " has disconnected (real-time post socket).`");
            };
            remServer.LogQueryClientConnected += client =>
            {
                hq.PostMessage("`Remote client: " + client.Owner + " has connected (log query socket).`");
            };
            remServer.LogQueryClientDisconnected += client =>
            {
                hq.PostMessage("`Remote client: " + client.Owner + " has disconnected (log query socket).`");
            };
            remServer.LogQueryReceived += (client, req) =>
            {
                var entries = PostLogger.SearchLog(req, 250);
                remServer.SendLogEntries(client, entries);
            };
        }

        private static void InitialiseLocalServer()
        {
            locServer = new LocalServer();
            locServer.PhamEventManager.ConnectListener(RequestType.Exception, new Action<LocalRequest>(req =>
            {
                phamErrorCount++;
                hq.PostMessage("Warning, error detected in Pham:\n\n" + req.Dump());
            }));
            locServer.GhamEventManager.ConnectListener(RequestType.Exception, new Action<LocalRequest>(req =>
            {
                ghamErrorCount++;
                hq.PostMessage("Warning, error detected in Gham:\n\n" + req.Dump());
            }));
            locServer.PhamEventManager.ConnectListener(RequestType.DataManagerRequest, new Action<LocalRequest>(req =>
            {
                HandleDataManagerRequest(true, req);
            }));
            locServer.GhamEventManager.ConnectListener(RequestType.DataManagerRequest, new Action<LocalRequest>(req =>
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

            postSocket = new RealtimePostSocket(true);
            postSocket.OnActiveQuestion += HandleActiveQuestion;
            postSocket.OnActiveAnswer += HandleActiveAnswer;
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
                            cmdMatches = HandleNormalUserCommand(room, command);

                            if (!cmdMatches)
                            {
                                room.PostReplyFast(command, "`Command not recognised.`");
                            }
                        }
                    }
                }
                else if (authUsers.IDs.Any(id => id == command.Author.ID))
                {
                    var cmdMatches = HandlePrivilegedUserCommand(room, command, false);

                    if (!cmdMatches)
                    {
                        cmdMatches = HandleNormalUserCommand(room, command);

                        if (!cmdMatches)
                        {
                            room.PostReplyFast(command, "`Command not recognised (at your current access level).`");
                        }
                    }
                }
                else
                {
                    var cmdMatches = HandleNormalUserCommand(room, command);

                    if (!cmdMatches)
                    {
                        room.PostReplyFast(command, "`Command not recognised (at your current access level).`");
                    }
                }
            }
            catch (Exception ex)
            {
                room.PostReplyFast(command, "`Unable to execute command: " + ex.Message + "`");
            }
        }

        private static bool HandleNormalUserCommand(Room room, Message command)
        {
            var cmd = command.Content.Trim().ToUpperInvariant();

            switch (cmd)
            {
                case "STATUS":
                {
                    var hoursAlive = (DateTime.UtcNow - startTime).TotalHours;
                    var getStatus = new Func<int, string>(er => er == 0 ? "Good" : er <= 2 ? "Ok" : "Bad");
                    var getErrorRate = new Func<uint, int>(ec => (int)Math.Round(ec / hoursAlive));
                    var yamErrorRate = getErrorRate(yamErrorCount);   var yamStatus = getStatus(yamErrorRate);
                    var phamErrorRate = getErrorRate(phamErrorCount); var phamStatus = getStatus(phamErrorRate);
                    var ghamErrorRate = getErrorRate(ghamErrorCount); var ghamStatus = getStatus(ghamErrorRate);
                    var statusReport = "    Status report:\n" +
                                        "    Yam:  " + yamStatus + " (" + yamErrorCount + " @ " + yamErrorRate + "/h)\n" +
                                        "    Pham: " + phamStatus + " (" + phamErrorCount + " @ " + phamErrorRate + "/h)\n" +
                                        "    Gham: " + ghamStatus + " (" + ghamErrorCount + " @ " + ghamErrorRate + "/h)";
                    room.PostMessageFast(statusReport);
                    return true;
                }
                case "LOG STATS":
                {
                    var items = PostLogger.Log.Count;
                    var uncomp = PostLogger.LogSizeUncompressed / 1024.0 / 1024;
                    var comp = PostLogger.LogSizeCompressed / 1024.0 / 1024;
                    var compRatio = Math.Round((uncomp / comp) * 100);
                    var dataReport = "    Log report:\n" +
                                        "    Items: " + items + "\n" +
                                        "    Update interval: " + PostLogger.UpdateInterval + "\n" +
                                        "    Size (uncompressed): " + Math.Round(uncomp) + " MiB\n" +
                                        "    Size (compressed): " + Math.Round(comp) + " MiB\n" +
                                        "    Compression %: " + compRatio + "\n";
                    room.PostMessageFast(dataReport);
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
                default:
                {
                    return false;
                }
            }
        }

        private static bool HandlePrivilegedUserCommand(Room room, Message command, bool isOwner)
        {
            if (command.Content.Trim().ToUpperInvariant().StartsWith("SEARCH"))
            {
                HandleChatLogSearchRequest(room, command, isOwner);
            }
            else
            {
                return false;
            }

            return true;
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
            else
            {
                return false;
            }

            return true;
        }

        private static void HandleChatLogSearchRequest(Room room, Message command, bool isOwner)
        {
            var cmd = command.Content.Remove(0, 7);
            var postType = cmd.Substring(0, 3);
            var searchParams = logSearchReg.Split(cmd).Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();
            var req = new RemoteLogRequest
            {
                PostType = postType
            };

            for (var i = 0; i < searchParams.Length; i++)
            {
                if (searchParams[i].ToLowerInvariant() == "site:")
                {
                    req.Site = searchParams[i + 1].Trim();
                    continue;
                }
                if (searchParams[i].ToLowerInvariant() == "title:")
                {
                    req.Title = searchParams[i + 1].Trim();
                    continue;
                }
                if (searchParams[i].ToLowerInvariant() == "body:")
                {
                    req.Body = searchParams[i + 1].Trim();
                    continue;
                }
                if (searchParams[i].ToLowerInvariant() == "score:")
                {
                    req.Score = searchParams[i + 1].Trim();
                    continue;
                }
                if (searchParams[i].ToLowerInvariant() == "createdafter:")
                {
                    req.CreatedAfter = searchParams[i + 1].Trim();
                    continue;
                }
                if (searchParams[i].ToLowerInvariant() == "createdbefore:")
                {
                    req.CreatedBefore = searchParams[i + 1].Trim();
                    continue;
                }
                if (searchParams[i].ToLowerInvariant() == "authorname:")
                {
                    req.AuthorName = searchParams[i + 1].Trim();
                    continue;
                }
                if (searchParams[i].ToLowerInvariant() == "authorrep:")
                {
                    req.AuthorRep = searchParams[i + 1].Trim();
                    continue;
                }
                if (searchParams[i].ToLowerInvariant() == "authornetworkid:")
                {
                    req.AuthorNetworkID = searchParams[i + 1].Trim();
                    continue;
                }
            }

            var entries = PostLogger.SearchLog(req, isOwner ? 500 : 100);

            if (entries.Length == 0)
            {
                room.PostReply(command, "`No entries found.`");
            }
            else
            {
                var entriesDump = entries.Dump().Replace("\n			__type: \"Phamhilator.Yam.Core.Answer, Yam.Core\",", "");
                var link = Hastebin.PostDocument(entriesDump);
                if (entries.Length == 1)
                {
                    room.PostReply(command, "[`1 entry found`](" + link + ")`.`");
                }
                else
                {
                    room.PostReply(command, "[`" + entries.Length + " entries found`](" + link + ")`.`");
                }
            }
        }

        private static void HandleActiveQuestion(Question q)
        {
            PostLogger.EnqueuePost(true, new Post
            {
                Url = q.Url,
                Site = q.Site,
                Title = q.Title,
                Body = q.Body,
                Score = q.Score,
                CreationDate = q.CreationDate,
                AuthorName = q.AuthorName,
                AuthorLink = q.AuthorLink,
                AuthorNetworkID = q.AuthorNetworkID,
                AuthorRep = q.AuthorRep
            });

            var locReq = new LocalRequest { Type = RequestType.Question, Data = q };
            locServer.SendData(true, locReq);
            locServer.SendData(false, locReq);

            remServer.SendPost(q);
        }

        private static void HandleActiveAnswer(Answer a)
        {
            PostLogger.EnqueuePost(false, a);

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
                hq.PostMessage("Detected error in Yam:\n\n" + ex.ToString());
                yamErrorCount++;

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
                yamErrorCount++;
                hq.PostMessage("Detected error in Yam:\n\n" + e.ToString());
            }
        }
    }
}
