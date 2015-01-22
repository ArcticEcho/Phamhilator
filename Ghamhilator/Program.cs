using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChatExchangeDotNet;


namespace Ghamhilator
{
    class Program
    {
        static void Main(string[] args)
        {
            //var credsVerified = false;
            Client client;

            Console.WriteLine("Please enter your Stack Exchange OpenID credentials.\n");

            while (true)
            {
                Console.WriteLine("Username: ");
                var username = Console.ReadLine();

                Console.WriteLine("Password: ");
                var passowrd = Console.ReadLine();

                try
                {
                    client = new Client(username, password);
                    Console.WriteLine("Login successful!");
                    break;
                }
                catch (Exception)
                {
                    Console.WriteLine("Failed to login.\n");
                }
            }

            Console.Write("Joining room(s)...");

            var primaryRoom = client.JoinRoom("http://chat.meta.stackexchange.com/rooms/773/low-quality-posts-hq");

            primaryRoom.PostMessage("`Ghamhilator started.`");




        }
    }
}
