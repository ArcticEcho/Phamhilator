using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChatExchangeDotNet;



namespace Phamhilator.Tests
{
    public static class GlobalTestFields
    {
        private static Client chatClient;
        private static Room testRoom;

        public static Client ChatClient
        {
            get
            {
                if (Object.ReferenceEquals(chatClient, null))
                {
                    InitialiseChatClient();
                }

                return chatClient;
            }
        }

        public static Room TestRoom
        {
            get
            {
                if (Object.ReferenceEquals(testRoom, null))
                {
                    InitialiseTestRoom();
                }

                return testRoom;
            }
        }



        private static void InitialiseChatClient()
        {
            if (Object.ReferenceEquals(chatClient, null))
            {
                chatClient = new Client("email", "password"); // TODO: enter your Stack Exchange OpenID creds here.
            }
        }

        private static void InitialiseTestRoom()
        {
            if (Object.ReferenceEquals(chatClient, null))
            {
                InitialiseChatClient();
            }

            testRoom = chatClient.JoinRoom("http://chat.meta.stackexchange.com/rooms/773/low-quality-posts-hq");
        }
    }
}
