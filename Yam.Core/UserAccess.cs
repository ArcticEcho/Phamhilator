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
using System.Text;
using ChatExchangeDotNet;

namespace Phamhilator.Yam.Core
{
    public class UserAccess
    {
        private const string dataKey = "Authorised Users";
        private static readonly List<User> owners = new List<User>();
        private readonly LocalRequestClient client;

        public List<int> AuthorisedUsers { get; private set; }

        static public List<User> Owners
        {
            get
            {
                lock (owners)
                {
                    if (owners.Count == 0) { PopulateOwners("meta.stackexchange.com", 773); }
                    return owners;
                }
            }
        }

        static public string OwnerNames
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



        public UserAccess(ref LocalRequestClient client)
        {
            if (client == null) { throw new ArgumentNullException("client"); }

            this.client = client;
            PopulateAuthorisedUsers();
        }



        public void AddAuthorisedUser(int id)
        {
            AuthorisedUsers.Add(id);

            var sb = new StringBuilder();

            foreach (var i in AuthorisedUsers)
            {
                sb.Append(i);
                sb.Append("\r\n");
            }

            client.UpdateData("Yam", dataKey, sb.ToString().Trim());
        }



        private void PopulateAuthorisedUsers()
        {
            AuthorisedUsers = new List<int>();

            if (!client.DataExists("Yam", dataKey)) { return; }

            var data = client.RequestData("Yam", dataKey);
            var idsStr = data.Split(new[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var id in idsStr)
            {
                AuthorisedUsers.Add(int.Parse(id));
            }
        }

        static private void PopulateOwners(string host, int roomID)
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
