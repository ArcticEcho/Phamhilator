using System;
using System.IO;



namespace Phamhilator
{
	public static class DirectoryTools
	{
		private static readonly string root = Directory.GetParent(Directory.GetParent(Environment.CurrentDirectory).FullName).FullName;
		private static readonly string filterTermsPath = Path.Combine(root, "Filter Terms");



		public static string GetBTDFolder()
		{
			var path = Path.Combine(root, "Bad Tag Definitions");

			if (!Directory.Exists(path))
			{
				Directory.CreateDirectory(path);
			}

			return path;
		}

		public static string GetOffensiveTermsFile()
		{
			var path = Path.Combine(root, "Filter Terms", "Offensive Terms.txt");

			if (!Directory.Exists(filterTermsPath))
			{
				Directory.CreateDirectory(filterTermsPath);
			}

			if (!File.Exists(path))
			{
				File.Create(path).Dispose();
			}

			return path;
		}

		public static string GetBadUsernameTermsFile()
		{
			var path = Path.Combine(root, "Filter Terms", "Bad Username Terms.txt");

			if (!Directory.Exists(filterTermsPath))
			{
				Directory.CreateDirectory(filterTermsPath);
			}

			if (!File.Exists(path))
			{
				File.Create(path).Dispose();
			}

			return path;
		}

		public static string GetLQTermsFile()
		{
			var path = Path.Combine(root, "Filter Terms", "LQ Terms.txt");

			if (!Directory.Exists(filterTermsPath))
			{
				Directory.CreateDirectory(filterTermsPath);
			}

			if (!File.Exists(path))
			{
				File.Create(path).Dispose();
			}

			return path;
		}

		public static string GetSpamTermsFile()
		{
			var path = Path.Combine(root, "Filter Terms", "Spam Terms.txt");

			if (!Directory.Exists(filterTermsPath))
			{
				Directory.CreateDirectory(filterTermsPath);
			}

			if (!File.Exists(path))
			{
				File.Create(path).Dispose();
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
	}
}
