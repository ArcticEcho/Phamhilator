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
            var credMan = new CredManager();
            bool success;

            if (String.IsNullOrEmpty(credMan.Email) || String.IsNullOrEmpty(credMan.Password))
            {
                success = TryManualLogin(credMan);
            }
            else
            {
                success = TryAutoLogin(credMan);
            }

            if (!success)
            {
                Console.WriteLine("\n\n Press any key to close Yam...");
                Console.ReadKey(true);
                return;
            }

            Console.Write("Joining room(s)...");

            var primaryRoom = chatClient.JoinRoom("http://chat.meta.stackexchange.com/rooms/773/low-quality-posts-hq");

            Console.Write("done.");

            primaryRoom.PostMessage("`Yamhilator started.`");
        }

        private static bool TryManualLogin(CredManager credMan)
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

                    Console.Write("login successful!\nShall I remember your creds? ");

                    try
                    {
                        var remCreds = Console.ReadLine();

                        if (Regex.IsMatch(remCreds, @"(?i)^y(e[sp]|up|)?\s*$"))
                        {
                            credMan.Email = email;
                            credMan.Password = password;

                            Console.WriteLine("Creds successfully remembered!");
                        }
                    }
                    catch (Exception)
                    {
                        credMan.Email = "";
                        credMan.Password = "";

                        Console.WriteLine("Failed to save your creds (creds not remembered).");
                    }

                    return true;
                }
                catch (Exception)
                {
                    Console.WriteLine("failed to login.");

                    return false;
                }
            }
        }

        private static bool TryAutoLogin(CredManager credMan)
        {
            Console.WriteLine("Email: " + credMan.Email);
            Console.WriteLine("Password: " + credMan.Password);
            Console.WriteLine("\nPress the enter key to login...");
            Console.Read();

            try
            {
                Console.Write("Authenticating...");

                chatClient = new Client(credMan.Email, credMan.Password);

                Console.WriteLine("login successful!\nShall I forget your creds?");

                try
                {
                    var clrCreds = Console.ReadLine();

                    if (Regex.IsMatch(clrCreds, @"(?i)^y(e[sp]|up|)?\s*$"))
                    {
                        credMan.Email = "";
                        credMan.Password = "";

                        Console.WriteLine("Creds successfully forgotten!\n");
                    }
                }
                catch (Exception)
                {
                    Console.WriteLine("Failed to forget your creds.\n");
                }
            }
            catch (Exception)
            {
                Console.WriteLine("failed to login.\n");

                return false;
            }

            return true;
        }
    }
}
