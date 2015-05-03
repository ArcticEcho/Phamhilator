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
using System.IO;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using System.Security.Cryptography;



namespace Phamhilator.Core
{
    public class BannedUsers
    {
        private readonly Random r = new Random();
        private readonly UserAccess userAccess;

        public bool SystemIsClear
        {
            get
            {
                return File.Exists(DirectoryTools.GetBannedUsersFile());
            }
        }


        public BannedUsers(UserAccess userAccess)
        {
            this.userAccess = userAccess;
        }



        public bool AddUser(string ID)
        {
            if (!SystemIsClear || !ID.All(Char.IsDigit) || userAccess.Owners.Any(user => user.ID == int.Parse(ID))) { return false; }

            var ii = r.Next(1001);

            for (var i = 0; i < ii; i++) { r.Next(); }

            var hash = HashID(ID);
            var data = new List<byte>(File.ReadAllBytes(DirectoryTools.GetBannedUsersFile()));

            data.AddRange(GetRandomBytes());

            data.AddRange(hash);

            data.AddRange(GetRandomBytes());

            File.WriteAllBytes(DirectoryTools.GetBannedUsersFile(), data.ToArray());

            File.SetCreationTime(DirectoryTools.GetBannedUsersFile(), new DateTime(1970, 1, 1, 1, 1, 1, 1));
            File.SetLastAccessTime(DirectoryTools.GetBannedUsersFile(), new DateTime(1970, 1, 1, 1, 1, 1, 1));
            File.SetLastWriteTime(DirectoryTools.GetBannedUsersFile(), new DateTime(1970, 1, 1, 1, 1, 1, 1));

            return true;
        }

        public bool IsUserBanned(string ID)
        {
            if (!SystemIsClear || !ID.All(Char.IsDigit)) { return true; }

            var hash = HashID(ID);
            var data = File.ReadAllBytes(DirectoryTools.GetBannedUsersFile()).ToList();

            for (var i = 0; i < data.Count - 64; i++)
            {
                var currentHash = new byte[64];

                data.CopyTo(i, currentHash, 0, 64);

                if (HashIsMatch(currentHash, hash))
                {
                    return true;
                }
            }

            return false;
        }



        private byte[] HashID(string ID)
        {
            using (var sha = new SHA512Managed())
            {
                var bytes = Encoding.UTF8.GetBytes(ID + GetPepper());

                return sha.ComputeHash(bytes);
            }
        }

        private byte[] GetRandomBytes()
        {
            var bytes = new byte[r.Next(1025)];

            r.NextBytes(bytes);

            return bytes;
        }

        private bool HashIsMatch(byte[] a, byte[] b)
        {
            if (a.Length != b.Length) { return false; }

            for (var i = 0; i < a.Length; i++)
            {
                if (a[i] != b[i]) { return false; }
            }

            return true;
        }

        private byte[] GetPepper()
        {
            return Encoding.UTF8.GetBytes("Phamhilator.Core");
        }
    }
}
