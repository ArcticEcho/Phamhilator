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



		public static bool AddUser(string ID)
		{
			if (!Directory.Exists(DirectoryTools.GetBannedUsersDir())) { return false; }

			var ii = r.Next(1001);

			for (var i = 0; i < ii; i++) { r.Next(); }

			var hash = HashID(ID);
			var data = new List<byte>();

			data.AddRange(GetRandomBytes());

			data.AddRange(hash);

			data.AddRange(GetRandomBytes());

			var path = Path.Combine(DirectoryTools.GetBannedUsersDir(), Path.GetRandomFileName() + ".txt");

			File.WriteAllBytes(path, data.ToArray());
			File.SetAttributes(path, FileAttributes.Offline | FileAttributes.Encrypted | FileAttributes.Hidden);
			File.SetCreationTime(path, new DateTime(1970, 1, 1, 1, 1, 1, 1));
			File.SetLastAccessTime(path, new DateTime(1970, 1, 1, 1, 1, 1, 1));
			File.SetLastWriteTime(path, new DateTime(1970, 1, 1, 1, 1, 1, 1));

			return true;
		}

		public static bool IsUserBanned(string ID)
		{
			if (!Directory.Exists(DirectoryTools.GetBannedUsersDir())) { return true; }

			var hash = HashID(ID);
			var files = Directory.EnumerateFiles(DirectoryTools.GetBannedUsersDir());
			var data = files.Select(file => File.ReadAllBytes(file).ToList()).ToList();

			for (var i = 0; i < data.Count; i++)
			{
				var currentData = data[i];

				for (var ii = 0; ii < currentData.Count - 64; ii++)
				{
					var currentHash = new byte[64];

					currentData.CopyTo(ii, currentHash, 0, 64);

					if (HashIsMatch(currentHash, hash))
					{
						return true;
					}
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
