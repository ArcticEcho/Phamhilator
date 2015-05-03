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
using System.Globalization;
using System.IO;
using ChatExchangeDotNet;

namespace Phamhilator.Yam.Core
{
    public static class UserAccess
    {
        static private readonly List<User> owners = new List<User>();

        static public List<int> PrivUsers { get; private set; }

        static public List<User> Owners
        {
            get
            {
                return owners;
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



        static UserAccess()
        {
            PopulatePrivUsers();
            PopulateOwners("meta.stackexchange.com", 773); // The LQP HQ.
        }



        static public void AddPrivUser(int id)
        {
            PrivUsers.Add(id);

            File.AppendAllLines(DirectoryTools.GetPrivUsersFile(), new[] { id.ToString(CultureInfo.InvariantCulture) });
        }



        static private void PopulatePrivUsers()
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
