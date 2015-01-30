using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using ChatExchangeDotNet;



namespace Yhamhilator
{
    public class Program
    {
        private static Client chatClient;



        private static void Main(string[] args)
        {
            TryLogin();

            Console.Write("Joining room(s)...");

            var primaryRoom = chatClient.JoinRoom("http://chat.meta.stackexchange.com/rooms/773/low-quality-posts-hq");

            Console.Write("done.");

            primaryRoom.PostMessage("`Yamhilator started.`");
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
    }
}
