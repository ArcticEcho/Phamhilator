using System.Globalization;
using System.IO;



namespace Phamhilator
{
	public static partial class GlobalInfo
	{
		public static class Stats
		{
			public static float TotalCheckedPosts
			{
				get
				{
					return int.Parse(File.ReadAllText(DirectoryTools.GetTotalCheckedPostsFile()), CultureInfo.InvariantCulture);
				}

				set
				{
					File.WriteAllText(DirectoryTools.GetTotalCheckedPostsFile(), value.ToString(CultureInfo.InvariantCulture));
				}
			}

			public static float TotalTPCount
			{
				get
				{
					return int.Parse(File.ReadAllText(DirectoryTools.GetTotalTPCountFile()), CultureInfo.InvariantCulture);
				}

				set
				{
					File.WriteAllText(DirectoryTools.GetTotalTPCountFile(), value.ToString(CultureInfo.InvariantCulture));
				}
			}

			public static float TotalFPCount
			{
				get
				{
					return int.Parse(File.ReadAllText(DirectoryTools.GetTotalFPCountFile()), CultureInfo.InvariantCulture);
				}

				set
				{
					File.WriteAllText(DirectoryTools.GetTotalFPCountFile(), value.ToString(CultureInfo.InvariantCulture));
				}
			}
		}
	}
}
