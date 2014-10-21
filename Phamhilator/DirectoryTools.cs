using System;
using System.IO;



namespace Phamhilator
{
	public static class DirectoryTools
	{
		private static readonly string root = Environment.CurrentDirectory;
		private static readonly string QTBFilterTermsPath = Path.Combine(root, "Question", "Title", "Black Filter Terms");
		private static readonly string QTWFilterTermsPath = Path.Combine(root, "Question", "Title", "White Filter Terms");
		private static readonly string QBBFilterTermsPath = Path.Combine(root, "Question", "Body", "Black Filter Terms");
		private static readonly string QBWFilterTermsPath = Path.Combine(root, "Question", "Body", "White Filter Terms");
		private static readonly string ABFilterTermsPath = Path.Combine(root, "Answer", "Black Filter Terms");
		private static readonly string AWFilterTermsPath = Path.Combine(root, "Answer", "White Filter Terms");



		public static string GetFilterFile(Filters filter)
		{
			var path = "";

			switch (filter)
			{
				case Filters.AnswerBlackLQ:
				{
					path = Path.Combine(ABFilterTermsPath, "LQ Terms.txt");

					break;
				}
				case Filters.AnswerBlackName:
				{
					path = Path.Combine(ABFilterTermsPath, "Bad Username Terms.txt");

					break;
				}
				case Filters.AnswerBlackOff:
				{
					path = Path.Combine(ABFilterTermsPath, "Offensive Terms.txt");

					break;
				}
				case Filters.AnswerBlackSpam:
				{
					path = Path.Combine(ABFilterTermsPath, "Spam Terms.txt");

					break;
				}
				case Filters.AnswerWhiteLQ:
				{
					path = Path.Combine(AWFilterTermsPath, "LQ");

					break;
				}
				case Filters.AnswerWhiteName:
				{
					path = Path.Combine(AWFilterTermsPath, "Bad Username");

					break;
				}
				case Filters.AnswerWhiteOff:
				{
					path = Path.Combine(AWFilterTermsPath, "Offensive");

					break;
				}
				case Filters.AnswerWhiteSpam:
				{
					path = Path.Combine(AWFilterTermsPath, "Spam");

					break;
				}
				case Filters.QuestionBodyBlackLQ:
				{
					path = Path.Combine(QBBFilterTermsPath, "LQ Terms.txt");

					break;
				}
				case Filters.QuestionBodyBlackOff:
				{
					path = Path.Combine(QBBFilterTermsPath, "Offensive Terms.txt");

					break;
				}
				case Filters.QuestionBodyBlackSpam:
				{
					path = Path.Combine(QBBFilterTermsPath, "Spam Terms.txt");

					break;
				}
				case Filters.QuestionBodyWhiteLQ:
				{
					path = Path.Combine(QBWFilterTermsPath, "LQ");

					break;
				}
				case Filters.QuestionBodyWhiteOff:
				{
					path = Path.Combine(QBWFilterTermsPath, "Offensive");

					break;
				}
				case Filters.QuestionBodyWhiteSpam:
				{
					path = Path.Combine(QBWFilterTermsPath, "Spam");

					break;
				}
				case Filters.QuestionTitleBlackLQ:
				{
					path = Path.Combine(QTBFilterTermsPath, "LQ Terms.txt");

					break;
				}
				case Filters.QuestionTitleBlackName:
				{
					path = Path.Combine(QTBFilterTermsPath, "Bad Username Terms.txt");

					break;
				}
				case Filters.QuestionTitleBlackOff:
				{
					path = Path.Combine(QTBFilterTermsPath, "Offensive Terms.txt");

					break;
				}
				case Filters.QuestionTitleBlackSpam:
				{
					path = Path.Combine(QTBFilterTermsPath, "Spam Terms.txt");

					break;
				}
				case Filters.QuestionTitleWhiteLQ:
				{
					path = Path.Combine(QTWFilterTermsPath, "LQ");

					break;
				}
				case Filters.QuestionTitleWhiteName:
				{
					path = Path.Combine(QTWFilterTermsPath, "Bad Username");

					break;
				}
				case Filters.QuestionTitleWhiteOff:
				{
					path = Path.Combine(QTWFilterTermsPath, "Offensive");

					break;
				}
				case Filters.QuestionTitleWhiteSpam:
				{
					path = Path.Combine(QTWFilterTermsPath, "Spam");

					break;
				}
			}

			if ((int)filter > 99)
			{
				if (!Directory.Exists(Directory.GetParent(path).FullName))
				{
					Directory.CreateDirectory(QTWFilterTermsPath);
				}

				if (!Directory.Exists(path))
				{
					Directory.CreateDirectory(path);
				}
			}
			else
			{
				if (!Directory.Exists(Directory.GetParent(path).FullName))
				{
					Directory.CreateDirectory(QTBFilterTermsPath);
				}

				if (!File.Exists(path))
				{
					File.Create(path).Dispose();
				}
			}

			return path;
		}

		# region Misc files

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

		# endregion
	}
}
