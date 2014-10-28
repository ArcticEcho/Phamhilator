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
		private static readonly string configPath = Path.Combine(root, "Config");



		public static string GetFilterFile(FilterType filter)
		{
			var path = "";

			switch (filter)
			{
				case FilterType.AnswerBlackLQ:
				{
					path = Path.Combine(ABFilterTermsPath, "LQ Terms.txt");

					break;
				}
				case FilterType.AnswerBlackName:
				{
					path = Path.Combine(ABFilterTermsPath, "Bad Username Terms.txt");

					break;
				}
				case FilterType.AnswerBlackOff:
				{
					path = Path.Combine(ABFilterTermsPath, "Offensive Terms.txt");

					break;
				}
				case FilterType.AnswerBlackSpam:
				{
					path = Path.Combine(ABFilterTermsPath, "Spam Terms.txt");

					break;
				}
				case FilterType.AnswerWhiteLQ:
				{
					path = Path.Combine(AWFilterTermsPath, "LQ");

					break;
				}
				case FilterType.AnswerWhiteName:
				{
					path = Path.Combine(AWFilterTermsPath, "Bad Username");

					break;
				}
				case FilterType.AnswerWhiteOff:
				{
					path = Path.Combine(AWFilterTermsPath, "Offensive");

					break;
				}
				case FilterType.AnswerWhiteSpam:
				{
					path = Path.Combine(AWFilterTermsPath, "Spam");

					break;
				}
				case FilterType.QuestionBodyBlackLQ:
				{
					path = Path.Combine(QBBFilterTermsPath, "LQ Terms.txt");

					break;
				}
				case FilterType.QuestionBodyBlackOff:
				{
					path = Path.Combine(QBBFilterTermsPath, "Offensive Terms.txt");

					break;
				}
				case FilterType.QuestionBodyBlackSpam:
				{
					path = Path.Combine(QBBFilterTermsPath, "Spam Terms.txt");

					break;
				}
				case FilterType.QuestionBodyWhiteLQ:
				{
					path = Path.Combine(QBWFilterTermsPath, "LQ");

					break;
				}
				case FilterType.QuestionBodyWhiteOff:
				{
					path = Path.Combine(QBWFilterTermsPath, "Offensive");

					break;
				}
				case FilterType.QuestionBodyWhiteSpam:
				{
					path = Path.Combine(QBWFilterTermsPath, "Spam");

					break;
				}
				case FilterType.QuestionTitleBlackLQ:
				{
					path = Path.Combine(QTBFilterTermsPath, "LQ Terms.txt");

					break;
				}
				case FilterType.QuestionTitleBlackName:
				{
					path = Path.Combine(QTBFilterTermsPath, "Bad Username Terms.txt");

					break;
				}
				case FilterType.QuestionTitleBlackOff:
				{
					path = Path.Combine(QTBFilterTermsPath, "Offensive Terms.txt");

					break;
				}
				case FilterType.QuestionTitleBlackSpam:
				{
					path = Path.Combine(QTBFilterTermsPath, "Spam Terms.txt");

					break;
				}
				case FilterType.QuestionTitleWhiteLQ:
				{
					path = Path.Combine(QTWFilterTermsPath, "LQ");

					break;
				}
				case FilterType.QuestionTitleWhiteName:
				{
					path = Path.Combine(QTWFilterTermsPath, "Bad Username");

					break;
				}
				case FilterType.QuestionTitleWhiteOff:
				{
					path = Path.Combine(QTWFilterTermsPath, "Offensive");

					break;
				}
				case FilterType.QuestionTitleWhiteSpam:
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
			var path = Path.Combine(configPath, "Previously Post Messages.txt");

			if (!File.Exists(path))
			{
				File.Create(path).Dispose();
			}

			return path;
		}

		public static string GetCommandAccessUsersFile()
		{
			var path = Path.Combine(configPath, "Command Access Users.txt");

			if (!File.Exists(path))
			{
				File.Create(path).Dispose();
			}

			return path;
		}

		public static string GetAccuracyThresholdFile()
		{
			var path = Path.Combine(configPath, "Accuracy Threshold.txt");

			if (!File.Exists(path))
			{
				File.WriteAllText(path, "15"); // Default threshold = 15%
			}

			return path;
		}

		public static string GetEnableFullScanFile()
		{
			var path = Path.Combine(configPath, "Enable Full Scan.txt");

			if (!File.Exists(path))
			{
				File.WriteAllText(path, "false"); // Disabled by default.
			}

			return path;
		}

		public static string GetBannedUsersFile()
		{
			return Path.Combine(configPath, "Banned Users.txt");
		}

		public static string GetStatusFile()
		{
			var path = Path.Combine(configPath, "Status.txt");

			if (!File.Exists(path))
			{
				File.WriteAllText(path, "Beta");
			}

			return path;
		}

		# endregion
	}
}
