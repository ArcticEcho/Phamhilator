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
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.IO;



namespace Phamhilator.Core
{
    public class ActiveRooms
    {
        private readonly string secRoomsPath = Path.Combine(FilePaths.ConfigDir, "Secondary Rooms.txt");
        private readonly string priRoomPath = Path.Combine(FilePaths.ConfigDir, "Primary Room.txt");
        private string priRoomUrl;
        private List<string> secRoomUrls;

        public string PrimaryRoomUrl
        {
            get
            {
                return File.ReadAllText(priRoomPath);
            }

            set
            {
                File.WriteAllText(priRoomPath, value);
            }
        }

        public ReadOnlyCollection<string> SecondaryRoomUrls 
        { 
            get
            {
                return secRoomUrls.AsReadOnly();
            }

            set
            {
                File.WriteAllLines(secRoomsPath, value);
            }
        }



        public ActiveRooms()
        {
            if (!File.Exists(secRoomsPath) || !File.Exists(priRoomPath))
            {
                var dirPath = Path.GetDirectoryName(priRoomPath);

                if (!Directory.Exists(dirPath))
                {
                    Directory.CreateDirectory(dirPath);
                }

                // Set default to the LQP HQ & the tavern.
                PrimaryRoomUrl = "http://chat.meta.stackexchange.com/rooms/773/low-quality-posts-hq";
                secRoomUrls = new List<string> { "http://chat.meta.stackexchange.com/rooms/89/tavern-on-the-meta" };
            }
            else
            {
                priRoomUrl = File.ReadAllText(priRoomPath);
                secRoomUrls = File.ReadAllLines(secRoomsPath).ToList();
            }
        }
    }
}
