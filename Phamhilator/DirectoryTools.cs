using System;
using System.IO;



namespace Phamhilator
{
	public static class DirectoryTools
	{
		private static readonly string root = Directory.GetParent(Directory.GetParent(Environment.CurrentDirectory).FullName).FullName;



		public static string GetBTDFolder()
		{
			return Path.Combine(root, "Bad Tag Definitions");
		}

		public static string GetFilterTermsFile()
		{
			return Path.Combine(root, "Filter Terms.txt");
		}

		public static string GetPostPersitenceFile()
		{
			return Path.Combine(root, "Previously Post Messages.txt");
		}
	}
}
