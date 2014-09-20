using System;
using System.IO;



namespace Phamhilator
{
	public static class DirectoryTools
	{
		private static readonly string root = Directory.GetParent(Directory.GetParent(Environment.CurrentDirectory).FullName).FullName;
		private static readonly string blackFilterTermsPath = Path.Combine(root, "Black Filter Terms");
		private static readonly string whiteFilterTermsPath = Path.Combine(root, "White Filter Terms");



		public static string GetBTDFolder()
		{
			var path = Path.Combine(root, "Bad Tag Definitions");

			if (!Directory.Exists(path))
			{
				Directory.CreateDirectory(path);
			}

			return path;
		}

		public static string GetPostPersitenceFile()
		{
			var path = Path.Combine(root, "Previously Post Messages.txt");

			if (!File.Exists(path))
			{
				File.Create(path).Dispose();
			}

			return path;
		}


		public static string GetBlackOffensiveTermsFile()
		{
			var path = Path.Combine(blackFilterTermsPath, "Offensive Terms.txt");

			if (!Directory.Exists(blackFilterTermsPath))
			{
				Directory.CreateDirectory(blackFilterTermsPath);
			}

			if (!File.Exists(path))
			{
				File.Create(path).Dispose();
			}

			return path;
		}

		public static string GetBlackBadUsernameTermsFile()
		{
			var path = Path.Combine(blackFilterTermsPath, "Bad Username Terms.txt");

			if (!Directory.Exists(blackFilterTermsPath))
			{
				Directory.CreateDirectory(blackFilterTermsPath);
			}

			if (!File.Exists(path))
			{
				File.Create(path).Dispose();
			}

			return path;
		}

		public static string GetBlackLQTermsFile()
		{
			var path = Path.Combine(blackFilterTermsPath, "LQ Terms.txt");

			if (!Directory.Exists(blackFilterTermsPath))
			{
				Directory.CreateDirectory(blackFilterTermsPath);
			}

			if (!File.Exists(path))
			{
				File.Create(path).Dispose();
			}

			return path;
		}

		public static string GetBlackSpamTermsFile()
		{
			var path = Path.Combine(blackFilterTermsPath, "Spam Terms.txt");

			if (!Directory.Exists(blackFilterTermsPath))
			{
				Directory.CreateDirectory(blackFilterTermsPath);
			}

			if (!File.Exists(path))
			{
				File.Create(path).Dispose();
			}

			return path;
		}


		public static string GetWhiteOffensiveTermsDir()
		{
			var path = Path.Combine(whiteFilterTermsPath, "Offensive");

			if (!Directory.Exists(whiteFilterTermsPath))
			{
				Directory.CreateDirectory(whiteFilterTermsPath);
			}

			if (!Directory.Exists(path))
			{
				Directory.CreateDirectory(path);
			}

			return path;
		}

		public static string GetWhiteBadUsernameTermsDir()
		{
			var path = Path.Combine(whiteFilterTermsPath, "Bad Username");

			if (!Directory.Exists(whiteFilterTermsPath))
			{
				Directory.CreateDirectory(whiteFilterTermsPath);
			}

			if (!Directory.Exists(path))
			{
				Directory.CreateDirectory(path);
			}

			return path;
		}

		public static string GetWhiteLQTermsDir()
		{
			var path = Path.Combine(whiteFilterTermsPath, "LQ");

			if (!Directory.Exists(whiteFilterTermsPath))
			{
				Directory.CreateDirectory(whiteFilterTermsPath);
			}

			if (!Directory.Exists(path))
			{
				Directory.CreateDirectory(path);
			}

			return path;
		}

		public static string GetWhiteSpamTermsDir()
		{
			var path = Path.Combine(whiteFilterTermsPath, "Spam");

			if (!Directory.Exists(whiteFilterTermsPath))
			{
				Directory.CreateDirectory(whiteFilterTermsPath);
			}

			if (!Directory.Exists(path))
			{
				Directory.CreateDirectory(path);
			}

			return path;
		}


		public static string GetCommandAccessUsersFile()
		{
			var path = Path.Combine(root, "Command Access Users.txt");

			if (!File.Exists(path))
			{
				File.Create(path).Dispose();
			}

			return path;
		}
	}
}
