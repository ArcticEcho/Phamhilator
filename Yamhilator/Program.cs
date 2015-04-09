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
using Phamhilator.Core;



namespace Yamhilator
{
    public class Program
    {
        private static readonly int[] owners = new[] { 227577, 266094, 229438 }; // Sam, Uni & Fox.
        private static Client chatClient;
        private static Room primaryRoom;
        private static RealtimePostSocket postSocket;
        //private static Socket broadcastSocket;
        private static UdpClient broadcastSocket;
        private static bool shutdown;
        private static uint dataSent;
        private static IPAddress multicastaddress = IPAddress.Parse("239.0.0.222");
        private static IPEndPoint remoteep = new IPEndPoint(multicastaddress, 60000);



        private static void Main(string[] args)
        {
            TryLogin();

            Console.Write("Joining room(s)...");

            var primaryRoom = chatClient.JoinRoom("http://chat.meta.stackexchange.com/rooms/773/low-quality-posts-hq");

            Console.Write("done.\nStarting sockets...");

            postSocket = new RealtimePostSocket();
            postSocket.OnActiveQuestion = new Action<Question>(BroadcastQuestion);
            postSocket.OnActiveThreadAnswers = new Action<List<Answer>>(BroadcastAnswers);
            //broadcastSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            //broadcastSocket.Connect(new IPAddress(new byte[] { 255, 255, 255, 255 }), 60000);
            
            broadcastSocket = new UdpClient();
            broadcastSocket.JoinMulticastGroup(multicastaddress);

            postSocket.Connect();

            Console.Write("done.\nYamhilator started (press Q to exit).\n");
            primaryRoom.PostMessage("`Yamhilator started.`");

            while (!shutdown)
            {
                if (Char.ToLowerInvariant(Console.ReadKey(true).KeyChar) == 'q')
                {
                    shutdown = true;
                }
            }

            postSocket.Close();
            postSocket.Dispose();
            //broadcastSocket.Shutdown(SocketShutdown.Both);
            broadcastSocket.Close();
            primaryRoom.PostMessage("`Yamhilator stopped.`");
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
            primaryRoom.IgnoreOwnEvents = false;
            primaryRoom.StripMentionFromMessages = false;
            primaryRoom.EventManager.ConnectListener(EventType.UserMentioned, new Action<Message>(HandleCommand));
        }

        private static void HandleCommand(Message command)
        {
            if (!owners.Contains(command.AuthorID)) { return; }

            if (command.Content.Trim() == "stop")
            {
                primaryRoom.PostMessage("Stopping...");
                shutdown = true;
            }
        }

        private static void BroadcastQuestion(Question q)
        {
            var data = "<Q>" + new JsonFx.Json.JsonWriter().Write(q);
            var bytes = Encoding.UTF8.GetBytes(data);
            var bytesSent = broadcastSocket.Send(bytes, bytes.Length, remoteep);
            Console.WriteLine(bytesSent + " bytes sent.");
            dataSent += (uint)bytesSent;
        }

        private static void BroadcastAnswers(List<Answer> answers)
        {
            foreach (var a in answers)
            {
                var data = "<A>" + new JsonFx.Json.JsonWriter().Write(a);
                var bytes = Encoding.UTF8.GetBytes(data);
                var bytesSent = broadcastSocket.Send(bytes, bytes.Length, remoteep);
                Console.WriteLine(bytesSent + " bytes sent.");
                dataSent += (uint)bytesSent;
            }
        }
    }
}
