using System;
using System.IO;



namespace Phamhilator
{
	public static class DirectoryTools
	{
		private static readonly string root = Directory.GetParent(Directory.GetParent(Environment.CurrentDirectory).FullName).FullName;
		private static readonly string QBlackFilterTermsPath = Path.Combine(root, "Question", "Black Filter Terms");
		private static readonly string QWhiteFilterTermsPath = Path.Combine(root, "Question", "White Filter Terms");
		private static readonly string ABlackFilterTermsPath = Path.Combine(root, "Answer", "Black Filter Terms");
		private static readonly string AWhiteFilterTermsPath = Path.Combine(root, "Answer", "White Filter Terms");



		public static string GetQBlackOffensiveTermsFile()
		{
			var path = Path.Combine(QBlackFilterTermsPath, "Offensive Terms.txt");

			if (!Directory.Exists(QBlackFilterTermsPath))
			{
				Directory.CreateDirectory(QBlackFilterTermsPath);
			}

			if (!File.Exists(path))
			{
				File.Create(path).Dispose();
			}

			return path;
		}

		public static string GetQBlackBadUsernameTermsFile()
		{
			var path = Path.Combine(QBlackFilterTermsPath, "Bad Username Terms.txt");

			if (!Directory.Exists(QBlackFilterTermsPath))
			{
				Directory.CreateDirectory(QBlackFilterTermsPath);
			}

			if (!File.Exists(path))
			{
				File.Create(path).Dispose();
			}

			return path;
		}

		public static string GetQBlackLQTermsFile()
		{
			var path = Path.Combine(QBlackFilterTermsPath, "LQ Terms.txt");

			if (!Directory.Exists(QBlackFilterTermsPath))
			{
				Directory.CreateDirectory(QBlackFilterTermsPath);
			}

			if (!File.Exists(path))
			{
				File.Create(path).Dispose();
			}

			return path;
		}

		public static string GetQBlackSpamTermsFile()
		{
			var path = Path.Combine(QBlackFilterTermsPath, "Spam Terms.txt");

			if (!Directory.Exists(QBlackFilterTermsPath))
			{
				Directory.CreateDirectory(QBlackFilterTermsPath);
			}

			if (!File.Exists(path))
			{
				File.Create(path).Dispose();
			}

			return path;
		}


		public static string GetQWhiteOffensiveTermsDir()
		{
			var path = Path.Combine(QWhiteFilterTermsPath, "Offensive");

			if (!Directory.Exists(QWhiteFilterTermsPath))
			{
				Directory.CreateDirectory(QWhiteFilterTermsPath);
			}

			if (!Directory.Exists(path))
			{
				Directory.CreateDirectory(path);
			}

			return path;
		}

		public static string GetQWhiteBadUsernameTermsDir()
		{
			var path = Path.Combine(QWhiteFilterTermsPath, "Bad Username");

			if (!Directory.Exists(QWhiteFilterTermsPath))
			{
				Directory.CreateDirectory(QWhiteFilterTermsPath);
			}

			if (!Directory.Exists(path))
			{
				Directory.CreateDirectory(path);
			}

			return path;
		}

		public static string GetQWhiteLQTermsDir()
		{
			var path = Path.Combine(QWhiteFilterTermsPath, "LQ");

			if (!Directory.Exists(QWhiteFilterTermsPath))
			{
				Directory.CreateDirectory(QWhiteFilterTermsPath);
			}

			if (!Directory.Exists(path))
			{
				Directory.CreateDirectory(path);
			}

			return path;
		}

		public static string GetQWhiteSpamTermsDir()
		{
			var path = Path.Combine(QWhiteFilterTermsPath, "Spam");

			if (!Directory.Exists(QWhiteFilterTermsPath))
			{
				Directory.CreateDirectory(QWhiteFilterTermsPath);
			}

			if (!Directory.Exists(path))
			{
				Directory.CreateDirectory(path);
			}

			return path;
		}


		public static string GetABlackOffensiveTermsFile()
		{
			var path = Path.Combine(ABlackFilterTermsPath, "Offensive Terms.txt");

			if (!Directory.Exists(ABlackFilterTermsPath))
			{
				Directory.CreateDirectory(ABlackFilterTermsPath);
			}

			if (!File.Exists(path))
			{
				File.Create(path).Dispose();
			}

			return path;
		}

		public static string GetABlackBadUsernameTermsFile()
		{
			var path = Path.Combine(ABlackFilterTermsPath, "Bad Username Terms.txt");

			if (!Directory.Exists(ABlackFilterTermsPath))
			{
				Directory.CreateDirectory(ABlackFilterTermsPath);
			}

			if (!File.Exists(path))
			{
				File.Create(path).Dispose();
			}

			return path;
		}

		public static string GetABlackLQTermsFile()
		{
			var path = Path.Combine(ABlackFilterTermsPath, "LQ Terms.txt");

			if (!Directory.Exists(ABlackFilterTermsPath))
			{
				Directory.CreateDirectory(ABlackFilterTermsPath);
			}

			if (!File.Exists(path))
			{
				File.Create(path).Dispose();
			}

			return path;
		}

		public static string GetABlackSpamTermsFile()
		{
			var path = Path.Combine(ABlackFilterTermsPath, "Spam Terms.txt");

			if (!Directory.Exists(ABlackFilterTermsPath))
			{
				Directory.CreateDirectory(ABlackFilterTermsPath);
			}

			if (!File.Exists(path))
			{
				File.Create(path).Dispose();
			}

			return path;
		}


		public static string GetAWhiteOffensiveTermsDir()
		{
			var path = Path.Combine(AWhiteFilterTermsPath, "Offensive");

			if (!Directory.Exists(AWhiteFilterTermsPath))
			{
				Directory.CreateDirectory(AWhiteFilterTermsPath);
			}

			if (!Directory.Exists(path))
			{
				Directory.CreateDirectory(path);
			}

			return path;
		}

		public static string GetAWhiteBadUsernameTermsDir()
		{
			var path = Path.Combine(AWhiteFilterTermsPath, "Bad Username");

			if (!Directory.Exists(AWhiteFilterTermsPath))
			{
				Directory.CreateDirectory(AWhiteFilterTermsPath);
			}

			if (!Directory.Exists(path))
			{
				Directory.CreateDirectory(path);
			}

			return path;
		}

		public static string GetAWhiteLQTermsDir()
		{
			var path = Path.Combine(AWhiteFilterTermsPath, "LQ");

			if (!Directory.Exists(AWhiteFilterTermsPath))
			{
				Directory.CreateDirectory(AWhiteFilterTermsPath);
			}

			if (!Directory.Exists(path))
			{
				Directory.CreateDirectory(path);
			}

			return path;
		}

		public static string GetAWhiteSpamTermsDir()
		{
			var path = Path.Combine(AWhiteFilterTermsPath, "Spam");

			if (!Directory.Exists(AWhiteFilterTermsPath))
			{
				Directory.CreateDirectory(AWhiteFilterTermsPath);
			}

			if (!Directory.Exists(path))
			{
				Directory.CreateDirectory(path);
			}

			return path;
		}


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


		public static string GetCommandAccessUsersFile()
		{
			var path = Path.Combine(root, "Command Access Users.txt");

			if (!File.Exists(path))
			{
				File.Create(path).Dispose();
			}

			return path;
		}


		public static string GetAccuracyThresholdFile()
		{
			var path = Path.Combine(root, "Accuracy Threshold.txt");

			if (!File.Exists(path))
			{
				File.WriteAllText(path, "15"); // Default threshold = 15%
			}

			return path;
		}


		public static string GetEnableFullScanFile()
		{
			var path = Path.Combine(root, "Enable Full Scan.txt");

			if (!File.Exists(path))
			{
				File.WriteAllText(path, "false"); // Disabled by default.
			}

			return path;
		}


		public static string GetBannedUsersFile()
		{
			var path = Path.Combine(root, "Banned Users.txt");

			if (!File.Exists(path))
			{
				File.WriteAllText(path, "");
			}

			return path;
		}


		public static string GetStatusFile()
		{
			var path = Path.Combine(root, "Status.txt");

			if (!File.Exists(path))
			{
				File.WriteAllText(path, "Beta");
			}

			return path;
		}
	}
}
