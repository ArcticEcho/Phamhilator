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
