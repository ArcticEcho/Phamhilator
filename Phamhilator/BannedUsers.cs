using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;



namespace Phamhilator
{
	public static class BannedUsers
	{
		private static readonly Random r = new Random();



		public static void AddUser(string ID)
		{
			var ii = r.Next(1001);

			for (var i = 0; i < ii; i++) { r.Next(); }

			var hashSalt = HashID(ID);
			var data = File.ReadAllBytes(DirectoryTools.GetBannedUsersFile()).ToList();

			data.AddRange(GetRandomBytes());

			data.AddRange(hashSalt);

			data.AddRange(GetRandomBytes());

			File.WriteAllBytes(DirectoryTools.GetBannedUsersFile(), data.ToArray());
		}

		public static bool IsUserBanned(string ID)
		{
			var data = File.ReadAllBytes(DirectoryTools.GetBannedUsersFile()).ToList();



		}



		private static IEnumerable<byte> HashID(string ID)
		{
			var assembly = AppDomain.CurrentDomain.DomainManager.EntryAssembly;
			var attribute = (GuidAttribute)assembly.GetCustomAttributes(typeof(GuidAttribute), true)[0];
			var pepper = attribute.Value;

			using (var sha = new SHA512Managed())
			{
				var bytes = Encoding.UTF8.GetBytes(ID + pepper);

				return sha.ComputeHash(bytes);
			}
		}

		private static IEnumerable<byte> GetRandomBytes()
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
			var assembly = AppDomain.CurrentDomain.DomainManager.EntryAssembly;
			var attribute = (GuidAttribute)assembly.GetCustomAttributes(typeof(GuidAttribute), true)[0];

			return Encoding.UTF8.GetBytes(attribute.Value);
		}
	}
}
