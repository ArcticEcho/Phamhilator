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
