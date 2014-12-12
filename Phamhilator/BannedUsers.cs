using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using System.Security.Cryptography;



namespace Phamhilator
{
    public static class BannedUsers
    {
        private static readonly Random r = new Random();

        public static bool SystemIsClear
        {
            get
            {
                return File.Exists(DirectoryTools.GetBannedUsersFile());
            }
        }



        public static bool AddUser(string ID)
        {
            if (!SystemIsClear || !ID.All(Char.IsDigit) || UserAccess.Owners.Contains(int.Parse(ID))) { return false; }

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

        public static bool IsUserBanned(string ID)
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



        private static byte[] HashID(string ID)
        {
            using (var sha = new SHA512Managed())
            {
                var bytes = Encoding.UTF8.GetBytes(ID + GetPepper());

                return sha.ComputeHash(bytes);
            }
        }

        private static byte[] GetRandomBytes()
        {
            var bytes = new byte[r.Next(1025)];

            r.NextBytes(bytes);

            return bytes;
        }

        private static bool HashIsMatch(byte[] a, byte[] b)
        {
            if (a.Length != b.Length) { return false; }

            for (var i = 0; i < a.Length; i++)
            {
                if (a[i] != b[i]) { return false; }
            }

            return true;
        }

        private static byte[] GetPepper()
        {
            return Encoding.UTF8.GetBytes("Phamhilator");
        }
    }
}
