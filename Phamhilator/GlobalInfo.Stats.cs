using System;
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

		//public static class Stats
		//{
		//	private static readonly FileStream totalCheckedPostsCount = new FileStream(DirectoryTools.GetTotalCheckedPostsFile(), FileMode.Open);
		//	private static readonly FileStream totalTPCount = new FileStream(DirectoryTools.GetTotalTPCountFile(), FileMode.Open);
		//	private static readonly FileStream totalFPCount = new FileStream(DirectoryTools.GetTotalFPCountFile(), FileMode.Open);

		//	public static float TotalCheckedPosts
		//	{
		//		get
		//		{
		//			lock (totalCheckedPostsCount)
		//			{
		//				var bytes = new byte[4];

		//				totalCheckedPostsCount.Read(bytes, 0, 4);

		//				return BitConverter.ToInt32(bytes, 0);
		//			}
		//		}

		//		set
		//		{
		//			lock (totalCheckedPostsCount)
		//			{
		//				totalCheckedPostsCount.Write(BitConverter.GetBytes((int)value), 0, 4);
		//			}
		//		}
		//	}

		//	public static float TotalTPCount
		//	{
		//		get
		//		{
		//			lock (totalTPCount)
		//			{
		//				var bytes = new byte[4];

		//				totalTPCount.Read(bytes, 0, 4);

		//				return BitConverter.ToInt32(bytes, 0);
		//			}
		//		}

		//		set
		//		{
		//			lock (totalTPCount)
		//			{
		//				totalTPCount.Write(BitConverter.GetBytes((int)value), 0, 4);
		//			}
		//		}
		//	}

		//	public static float TotalFPCount
		//	{
		//		get
		//		{
		//			lock (totalFPCount)
		//			{
		//				var bytes = new byte[4];

		//				totalFPCount.Read(bytes, 0, 4);

		//				return BitConverter.ToInt32(bytes, 0);
		//			}
		//		}

		//		set
		//		{
		//			lock (totalFPCount)
		//			{
		//				totalFPCount.Write(BitConverter.GetBytes((int)value), 0, 4);
		//			}
		//		}
		//	}



		//	//~Stats()
		//	//{
		//	//	if (totalCheckedPostsCount != null)
		//	//	{
		//	//		totalCheckedPostsCount.Dispose();
		//	//	}

		//	//	if (totalTPCount != null)
		//	//	{
		//	//		totalTPCount.Dispose();
		//	//	}

		//	//	if (totalFPCount != null)
		//	//	{
		//	//		totalFPCount.Dispose();
		//	//	}
		//	//}
		//}
	}
}
