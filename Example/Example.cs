using System;
using System.Threading;
using ChatExchangeDotNet;



namespace Example
{
    public class Example
    {
        static void Main()
        {
            Console.WriteLine("This is a ChatExchange.Net demonstration. Press the 'Q' key to exit...\n\n");

            // Create a client to authenticate the user (which will then allow us to interact with chat).
            var client = new Client(/*"user0000000", */"some-email@address.com", "MySuperStr0ngPa55word");

            // Join a room by specifying its URL (returns a Room object).
            var sandbox = client.JoinRoom("http://chat.meta.stackexchange.com/rooms/651/sandbox");

            // Posts a new message in the room (if successful, returns a Message object, otherwise returns null).
            var myMessage = sandbox.PostMessage("Hello world!");

            // Listen to the NewMessage event for new messages.
            sandbox.NewMessage += message =>
            {
                // Print the new message (with the author's name).
                Console.WriteLine("Author: " + message.AuthorName + "\nMessage: " + message.Content + "\n");

                // If the message contains "3... 2... 1...", post "KA-BOOM!".
                if (message.Content.Contains("3... 2... 1..."))
                {
                    var success = sandbox.PostMessage("**KA-BOOM!**") != null;

                    Console.WriteLine("'KA-BOOM' message successfully posted: " + success);
                }
            };

            // Register to the UserJoined event and post a welcome message when the event occurs.
            sandbox.UserJoind += user =>
            {
                var success = sandbox.PostMessage("Welcome " + user.Name + "!") != null;

                Console.WriteLine("'Welcome' message successfully posted: " + success);
            };

            // Wait for the user to press the "Q" key before we exit.
            while (Char.ToLower(Console.ReadKey(true).KeyChar) != 'q')
            {
                Thread.Sleep(500);
            }
        }
    }
}
