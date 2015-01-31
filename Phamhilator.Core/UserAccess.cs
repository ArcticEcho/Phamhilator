using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using ChatExchangeDotNet;



namespace Phamhilator.Core
{
    public class UserAccess
    {
        private readonly List<User> owners = new List<User>();

        public List<int> PrivUsers  { get; private set; }

        public List<User> Owners
        {
            get
            {
                return owners;
            }
        }

        public string OwnerNames
        {
            get
            {
                var names = "";

                for (var i = 0; i < owners.Count; i++)
                {
                    if (i == owners.Count - 2)
                    {
                        names += owners[i].Name + " & ";
                    }
                    else if (i == owners.Count - 1)
                    {
                        names += owners[i].Name;
                    }
                    else
                    {
                        names += owners[i].Name + ", ";
                    }
                }

                return names;
            }
        }



        public UserAccess(string host, int roomID)
        {
            if (string.IsNullOrEmpty(host)) { throw new ArgumentException("Must not be null or empty.", "host"); }

            PopulatePrivUsers();
            PopulateOwners(host, roomID);
        }



        public void AddPrivUser(int id)
        {
            PrivUsers.Add(id);

            File.AppendAllLines(DirectoryTools.GetPrivUsersFile(), new[] { id.ToString(CultureInfo.InvariantCulture) });
        }



        private void PopulatePrivUsers()
        {
            PrivUsers = new List<int>();

            var users = File.ReadAllLines(DirectoryTools.GetPrivUsersFile());

            foreach (var user in users)
            {
                if (!string.IsNullOrWhiteSpace(user))
                {
                    PrivUsers.Add(int.Parse(user.Trim()));
                }
            }
        }

        private void PopulateOwners(string host, int roomID)
        {
            var sam = new User(host, roomID, 227577);
            var uni = new User(host, roomID, 266094);
            var fox = new User(host, roomID, 229438);
            var jan = new User(host, roomID, 194047);
            var pat = new User(host, roomID, 245360);
            var moo = new User(host, roomID, 202832);

            owners.Add(sam);
            owners.Add(uni);
            owners.Add(fox);
            owners.Add(jan);
            owners.Add(pat);
            owners.Add(moo);
        }
    }
}
