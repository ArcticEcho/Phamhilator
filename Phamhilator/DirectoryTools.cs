using System;
using System.IO;



namespace Phamhilator
{
	public static class DirectoryTools
	{
		private static readonly string root = /*Directory.GetParent(Directory.GetParent(*/Environment.CurrentDirectory/*).FullName).FullName*/;
		private static readonly string QTBFilterTermsPath = Path.Combine(root, "Question", "Title", "Black Filter Terms");
		private static readonly string QTWFilterTermsPath = Path.Combine(root, "Question", "Title", "White Filter Terms");
		private static readonly string QBBFilterTermsPath = Path.Combine(root, "Question", "Body", "Black Filter Terms");
		private static readonly string QBWFilterTermsPath = Path.Combine(root, "Question", "Body", "White Filter Terms");
		private static readonly string ABFilterTermsPath = Path.Combine(root, "Answer", "Black Filter Terms");
		private static readonly string AWFilterTermsPath = Path.Combine(root, "Answer", "White Filter Terms");



		//public static string GetFilterFile(Filters filter)
		//{
		//	var path = "";

		//	switch (filter)
		//	{
		//		case Filters.AnswerBodyBlackLQ:
		//		{
		//			path = "";

		//			break;
		//		}
		//		case Filters.AnswerBodyBlackName:
		//		{
		//			path = "";

		//			break;
		//		}
		//		case Filters.AnswerBodyBlackOff:
		//		{
		//			path = "";

		//			break;
		//		}
		//		case Filters.AnswerBodyBlackSpam:
		//		{
		//			path = "";

		//			break;
		//		}
		//		case Filters.AnswerBodyWhiteLQ:
		//		{
		//			path = "";

		//			break;
		//		}
		//		case Filters.AnswerBodyWhiteName:
		//		{
		//			path = "";

		//			break;
		//		}
		//		case Filters.AnswerBodyWhiteOff:
		//		{
		//			path = "";

		//			break;
		//		}
		//		case Filters.AnswerBodyWhiteSpam:
		//		{
		//			path = "";

		//			break;
		//		}
		//		case Filters.QuestionBodyBlackLQ:
		//		{
		//			path = "";

		//			break;
		//		}
		//		case Filters.QuestionBodyBlackOff:
		//		{
		//			path = "";

		//			break;
		//		}
		//		case Filters.QuestionBodyBlackSpam:
		//		{
		//			path = "";

		//			break;
		//		}
		//		case Filters.QuestionBodyWhiteLQ:
		//		{
		//			path = "";

		//			break;
		//		}
		//		case Filters.QuestionBodyWhiteOff:
		//		{
		//			path = "";

		//			break;
		//		}
		//		case Filters.QuestionBodyWhiteSpam:
		//		{
		//			path = "";

		//			break;
		//		}
		//		case Filters.QuestionTitleBlackLQ:
		//		{
		//			path = "";

		//			break;
		//		}
		//		case Filters.QuestionTitleBlackName:
		//		{
		//			path = "";

		//			break;
		//		}
		//		case Filters.QuestionTitleBlackOff:
		//		{
		//			path = "";

		//			break;
		//		}
		//		case Filters.QuestionTitleBlackSpam:
		//		{
		//			path = "";

		//			break;
		//		}
		//		case Filters.QuestionTitleWhiteLQ:
		//		{
		//			path = "";

		//			break;
		//		}
		//		case Filters.QuestionTitleWhiteName:
		//		{
		//			path = "";

		//			break;
		//		}
		//		case Filters.QuestionTitleWhiteOff:
		//		{
		//			path = "";

		//			break;
		//		}
		//		case Filters.QuestionTitleWhiteSpam:
		//		{
		//			path = "";

		//			break;
		//		}
		//	}
		//}

		public static string GetQTBOffTermsFile()
		{
			var path = Path.Combine(QTBFilterTermsPath, "Offensive Terms.txt");

			if (!Directory.Exists(QTBFilterTermsPath))
			{
				Directory.CreateDirectory(QTBFilterTermsPath);
			}

			if (!File.Exists(path))
			{
				File.Create(path).Dispose();
			}

			return path;
		}

		public static string GetQTBNameTermsFile()
		{
			var path = Path.Combine(QTBFilterTermsPath, "Bad Username Terms.txt");

			if (!Directory.Exists(QTBFilterTermsPath))
			{
				Directory.CreateDirectory(QTBFilterTermsPath);
			}

			if (!File.Exists(path))
			{
				File.Create(path).Dispose();
			}

			return path;
		}

		public static string GetQTBLQTermsFile()
		{
			var path = Path.Combine(QTBFilterTermsPath, "LQ Terms.txt");

			if (!Directory.Exists(QTBFilterTermsPath))
			{
				Directory.CreateDirectory(QTBFilterTermsPath);
			}

			if (!File.Exists(path))
			{
				File.Create(path).Dispose();
			}

			return path;
		}

		public static string GetQTBSpamTermsFile()
		{
			var path = Path.Combine(QTBFilterTermsPath, "Spam Terms.txt");

			if (!Directory.Exists(QTBFilterTermsPath))
			{
				Directory.CreateDirectory(QTBFilterTermsPath);
			}

			if (!File.Exists(path))
			{
				File.Create(path).Dispose();
			}

			return path;
		}


		public static string GetQTWOffTermsDir()
		{
			var path = Path.Combine(QTWFilterTermsPath, "Offensive");

			if (!Directory.Exists(QTWFilterTermsPath))
			{
				Directory.CreateDirectory(QTWFilterTermsPath);
			}

			if (!Directory.Exists(path))
			{
				Directory.CreateDirectory(path);
			}

			return path;
		}

		public static string GetQTWNameTermsDir()
		{
			var path = Path.Combine(QTWFilterTermsPath, "Bad Username");

			if (!Directory.Exists(QTWFilterTermsPath))
			{
				Directory.CreateDirectory(QTWFilterTermsPath);
			}

			if (!Directory.Exists(path))
			{
				Directory.CreateDirectory(path);
			}

			return path;
		}

		public static string GetQTWLQTermsDir()
		{
			var path = Path.Combine(QTWFilterTermsPath, "LQ");

			if (!Directory.Exists(QTWFilterTermsPath))
			{
				Directory.CreateDirectory(QTWFilterTermsPath);
			}

			if (!Directory.Exists(path))
			{
				Directory.CreateDirectory(path);
			}

			return path;
		}

		public static string GetQTWSpamTermsDir()
		{
			var path = Path.Combine(QTWFilterTermsPath, "Spam");

			if (!Directory.Exists(QTWFilterTermsPath))
			{
				Directory.CreateDirectory(QTWFilterTermsPath);
			}

			if (!Directory.Exists(path))
			{
				Directory.CreateDirectory(path);
			}

			return path;
		}


		public static string GetQBBOffTermsFile()
		{
			var path = Path.Combine(QBBFilterTermsPath, "Offensive Terms.txt");

			if (!Directory.Exists(QBBFilterTermsPath))
			{
				Directory.CreateDirectory(QBBFilterTermsPath);
			}

			if (!File.Exists(path))
			{
				File.Create(path).Dispose();
			}

			return path;
		}

		public static string GetQBBNameTermsFile()
		{
			var path = Path.Combine(QBBFilterTermsPath, "Bad Username Terms.txt");

			if (!Directory.Exists(QBBFilterTermsPath))
			{
				Directory.CreateDirectory(QBBFilterTermsPath);
			}

			if (!File.Exists(path))
			{
				File.Create(path).Dispose();
			}

			return path;
		}

		public static string GetQBBLQTermsFile()
		{
			var path = Path.Combine(QBBFilterTermsPath, "LQ Terms.txt");

			if (!Directory.Exists(QBBFilterTermsPath))
			{
				Directory.CreateDirectory(QBBFilterTermsPath);
			}

			if (!File.Exists(path))
			{
				File.Create(path).Dispose();
			}

			return path;
		}

		public static string GetQBBSpamTermsFile()
		{
			var path = Path.Combine(QBBFilterTermsPath, "Spam Terms.txt");

			if (!Directory.Exists(QBBFilterTermsPath))
			{
				Directory.CreateDirectory(QBBFilterTermsPath);
			}

			if (!File.Exists(path))
			{
				File.Create(path).Dispose();
			}

			return path;
		}


		public static string GetQBWOffTermsDir()
		{
			var path = Path.Combine(QBWFilterTermsPath, "Offensive");

			if (!Directory.Exists(QBWFilterTermsPath))
			{
				Directory.CreateDirectory(QBWFilterTermsPath);
			}

			if (!Directory.Exists(path))
			{
				Directory.CreateDirectory(path);
			}

			return path;
		}

		public static string GetQBWNameTermsDir()
		{
			var path = Path.Combine(QBWFilterTermsPath, "Bad Username");

			if (!Directory.Exists(QBWFilterTermsPath))
			{
				Directory.CreateDirectory(QBWFilterTermsPath);
			}

			if (!Directory.Exists(path))
			{
				Directory.CreateDirectory(path);
			}

			return path;
		}

		public static string GetQBWLQTermsDir()
		{
			var path = Path.Combine(QBWFilterTermsPath, "LQ");

			if (!Directory.Exists(QBWFilterTermsPath))
			{
				Directory.CreateDirectory(QBWFilterTermsPath);
			}

			if (!Directory.Exists(path))
			{
				Directory.CreateDirectory(path);
			}

			return path;
		}

		public static string GetQBWSpamTermsDir()
		{
			var path = Path.Combine(QBWFilterTermsPath, "Spam");

			if (!Directory.Exists(QBWFilterTermsPath))
			{
				Directory.CreateDirectory(QBWFilterTermsPath);
			}

			if (!Directory.Exists(path))
			{
				Directory.CreateDirectory(path);
			}

			return path;
		}


		public static string GetABOffTermsFile()
		{
			var path = Path.Combine(ABFilterTermsPath, "Offensive Terms.txt");

			if (!Directory.Exists(ABFilterTermsPath))
			{
				Directory.CreateDirectory(ABFilterTermsPath);
			}

			if (!File.Exists(path))
			{
				File.Create(path).Dispose();
			}

			return path;
		}

		public static string GetABNameTermsFile()
		{
			var path = Path.Combine(ABFilterTermsPath, "Bad Username Terms.txt");

			if (!Directory.Exists(ABFilterTermsPath))
			{
				Directory.CreateDirectory(ABFilterTermsPath);
			}

			if (!File.Exists(path))
			{
				File.Create(path).Dispose();
			}

			return path;
		}

		public static string GetABLQTermsFile()
		{
			var path = Path.Combine(ABFilterTermsPath, "LQ Terms.txt");

			if (!Directory.Exists(ABFilterTermsPath))
			{
				Directory.CreateDirectory(ABFilterTermsPath);
			}

			if (!File.Exists(path))
			{
				File.Create(path).Dispose();
			}

			return path;
		}

		public static string GetABSpamTermsFile()
		{
			var path = Path.Combine(ABFilterTermsPath, "Spam Terms.txt");

			if (!Directory.Exists(ABFilterTermsPath))
			{
				Directory.CreateDirectory(ABFilterTermsPath);
			}

			if (!File.Exists(path))
			{
				File.Create(path).Dispose();
			}

			return path;
		}


		public static string GetAWOffTermsDir()
		{
			var path = Path.Combine(AWFilterTermsPath, "Offensive");

			if (!Directory.Exists(AWFilterTermsPath))
			{
				Directory.CreateDirectory(AWFilterTermsPath);
			}

			if (!Directory.Exists(path))
			{
				Directory.CreateDirectory(path);
			}

			return path;
		}

		public static string GetAWNameTermsDir()
		{
			var path = Path.Combine(AWFilterTermsPath, "Bad Username");

			if (!Directory.Exists(AWFilterTermsPath))
			{
				Directory.CreateDirectory(AWFilterTermsPath);
			}

			if (!Directory.Exists(path))
			{
				Directory.CreateDirectory(path);
			}

			return path;
		}

		public static string GetAWLQTermsDir()
		{
			var path = Path.Combine(AWFilterTermsPath, "LQ");

			if (!Directory.Exists(AWFilterTermsPath))
			{
				Directory.CreateDirectory(AWFilterTermsPath);
			}

			if (!Directory.Exists(path))
			{
				Directory.CreateDirectory(path);
			}

			return path;
		}

		public static string GetAWSpamTermsDir()
		{
			var path = Path.Combine(AWFilterTermsPath, "Spam");

			if (!Directory.Exists(AWFilterTermsPath))
			{
				Directory.CreateDirectory(AWFilterTermsPath);
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
