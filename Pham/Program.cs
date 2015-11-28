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
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Phamhilator.Yam.Core;
using ChatExchangeDotNet;
using System.Linq;
using System.Collections.Concurrent;

namespace Phamhilator.Pham.UI
{
    public class Program
    {
        private static readonly ConcurrentStack<Post> checkedPosts = new ConcurrentStack<Post>();
        private static readonly ManualResetEvent shutdownMre = new ManualResetEvent(false);
        private static LocalRequestClient yamClient;
        private static Client chatClient;
        private static Room socvr;
        private static UserAccess authUsers;
        private static DateTime startTime;



        static void Main(string[] args)
        {
            Console.Title = "Pham v2";
            Console.CancelKeyPress += (o, oo) => shutdownMre.Set();

            Console.Write("Authenticating...");
            AuthenticateChatClient();
            Console.Write("done.\nJoining chat room...");
            JoinRooms();
            Console.WriteLine("done.\n");

            startTime = DateTime.UtcNow;

#if DEBUG
            Console.WriteLine("Pham v2 started (debug).");
#else
            Console.WriteLine("Pham v2 started.");
#endif

            ConnectYamClientEvents();

            shutdownMre.WaitOne();

            Console.WriteLine("Stopping...");

            socvr.Leave();
            chatClient.Dispose();
            yamClient.Dispose();
        }



        private static void AuthenticateChatClient()
        {
            var cr = new ConfigReader();

            var email = cr.GetSetting("se email");
            var pwd = cr.GetSetting("se pass");

            chatClient = new Client(email, pwd);
        }

        private static void JoinRooms()
        {
            var cr = new ConfigReader();

            socvr = chatClient.JoinRoom(cr.GetSetting("room"));
            socvr.EventManager.ConnectListener(EventType.UserMentioned, new Action<Message>(m => HandleChatCommand(socvr, m)));
        }

        private static void ConnectYamClientEvents()
        {
            yamClient.EventManager.ConnectListener(LocalRequest.RequestType.Question, new Action<Question>(CheckQuestion));

            yamClient.EventManager.ConnectListener(LocalRequest.RequestType.Answer, new Action<Answer>(CheckAnswer));

            yamClient.EventManager.ConnectListener(LocalRequest.RequestType.Exception, new Action<LocalRequest>(ex =>
            {
                yamClient.SendData(new LocalRequest
                {
                    Type = LocalRequest.RequestType.Exception,
                    ID = LocalRequest.GetNewID(),
                    Data = ex.Data.ToString(),
                    Options = ex.Options
                });
            }));
        }

        private static void CheckQuestion(Question q)
        {
            if (checkedPosts.Contains(q) || q.Site != "stackoverflow.com") { return; }
            while (checkedPosts.Count > 1000)
            {
                Post temp;
                checkedPosts.TryPop(out temp);
            }
            checkedPosts.Push(q);

            // TODO: Checking magic.
        }

        private static void CheckAnswer(Answer a)
        {
            if (checkedPosts.Contains(a) || a.Site != "stackoverflow.com") { return; }
            while (checkedPosts.Count > 1000)
            {
                Post temp;
                checkedPosts.TryPop(out temp);
            }
            checkedPosts.Push(a);
            
            //TODO: Checking magic.
        }

        private static void ReportPost(Post post, KeyValuePair<string, double> score)
        {
            var msg = new MessageBuilder();

            msg.AppendText(score.Key, TextFormattingOptions.Bold);
            msg.AppendText(" (" + Math.Round(score.Value, 2) + ") ");
            msg.AppendLink(post.Title, post.Url, "Score: " + post.Score, TextFormattingOptions.None, WhiteSpace.None);
            msg.AppendText(", by ");
            msg.AppendLink(post.AuthorName, post.AuthorLink, "Reputation: " + post.AuthorRep, TextFormattingOptions.None, WhiteSpace.None);
            msg.AppendText(".");

            socvr.PostMessageFast(msg);
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
                else if (authUsers.AuthorisedUsers.Any(id => id == command.Author.ID))
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
            if (command.Content.Trim().ToUpperInvariant() == "THRESHOLD")
            {
                room.PostReplyFast(command, "`Current threshold set to: " + threshold * 100 + "%.`");
                return true;
            }

            return false;
        }

        private static bool HandlePrivilegedUserCommand(Room room, Message command, bool isOwner)
        {
            return false;
        }

        private static bool HandleOwnerCommand(Room room, Message command)
        {
            var cmd = command.Content.Trim().ToUpperInvariant();

            if (cmd.StartsWith("SET THRESHOLD"))
            {
                var newVal = cmd.Remove(0, 14);
                var newThreshold = 0D;

                if (!double.TryParse(newVal, out newThreshold) || newThreshold < 1 || newThreshold > 100)
                {
                    room.PostReply(command, "`Please specify a valid percentage.`");
                    return true;
                }

                threshold = newThreshold / 100;
                yamClient.UpdateData("Pham", thresholdDataManagerKey, threshold.ToString());

                room.PostReply(command, "`Threshold successfully updated.`");
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

    }
}
