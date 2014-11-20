using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ChatExchangeDotNet;



namespace Phamhilator
{
	public static partial class GlobalInfo
	{
		private static string botUsername = "";
		private static int chatID;
		private static int announceID;

		#region Filters

		public static readonly Dictionary<FilterType, BlackFilter> BlackFilters = new Dictionary<FilterType, BlackFilter>()
		{
			{ FilterType.QuestionTitleBlackName, new BlackFilter(FilterType.QuestionTitleBlackName) },
			{ FilterType.QuestionTitleBlackOff, new BlackFilter(FilterType.QuestionTitleBlackOff) },
			{ FilterType.QuestionTitleBlackSpam, new BlackFilter(FilterType.QuestionTitleBlackSpam) },
			{ FilterType.QuestionTitleBlackLQ, new BlackFilter(FilterType.QuestionTitleBlackLQ) },

			{ FilterType.QuestionBodyBlackSpam, new BlackFilter(FilterType.QuestionBodyBlackSpam) },
			{ FilterType.QuestionBodyBlackLQ, new BlackFilter(FilterType.QuestionBodyBlackLQ) },
			{ FilterType.QuestionBodyBlackOff, new BlackFilter(FilterType.QuestionBodyBlackOff) },

			{ FilterType.AnswerBlackSpam, new BlackFilter(FilterType.AnswerBlackSpam) },
			{ FilterType.AnswerBlackLQ, new BlackFilter(FilterType.AnswerBlackLQ) },
			{ FilterType.AnswerBlackOff, new BlackFilter(FilterType.AnswerBlackOff) },
			{ FilterType.AnswerBlackName, new BlackFilter(FilterType.AnswerBlackName) }
		};

		public static readonly Dictionary<FilterType, WhiteFilter> WhiteFilters = new Dictionary<FilterType, WhiteFilter>()
		{
			{ FilterType.QuestionTitleWhiteName, new WhiteFilter(FilterType.QuestionTitleWhiteName) },
			{ FilterType.QuestionTitleWhiteOff, new WhiteFilter(FilterType.QuestionTitleWhiteOff) },
			{ FilterType.QuestionTitleWhiteSpam, new WhiteFilter(FilterType.QuestionTitleWhiteSpam) },
			{ FilterType.QuestionTitleWhiteLQ, new WhiteFilter(FilterType.QuestionTitleWhiteLQ) },

			{ FilterType.QuestionBodyWhiteSpam, new WhiteFilter(FilterType.QuestionBodyWhiteSpam) },
			{ FilterType.QuestionBodyWhiteLQ, new WhiteFilter(FilterType.QuestionBodyWhiteLQ) },
			{ FilterType.QuestionBodyWhiteOff, new WhiteFilter(FilterType.QuestionBodyWhiteOff) },

			{ FilterType.AnswerWhiteSpam, new WhiteFilter(FilterType.AnswerWhiteSpam) },
			{ FilterType.AnswerWhiteLQ, new WhiteFilter(FilterType.AnswerWhiteLQ) },
			{ FilterType.AnswerWhiteOff, new WhiteFilter(FilterType.AnswerWhiteOff) },
			{ FilterType.AnswerWhiteName, new WhiteFilter(FilterType.AnswerWhiteName) }
		};

		#endregion

		public static readonly Dictionary<int, MessageInfo> PostedReports = new Dictionary<int, MessageInfo>(); // Message ID, actual message.
		//public static readonly MessageHandler MessagePoster = new MessageHandler();
		public const string Owners = "Sam, Unihedron, Patrick Hofman, Jan Dvorak & ProgramFOX";
		//public static WebBrowser ChatWb;
		//public static WebBrowser AnnounceWb;
		public static Room PrimaryRoom;
		public static Client ChatClient;
		public static int PostsCaught;
		public static DateTime UpTime;
		public static bool BotRunning;
		public static bool Exit;

		//public static string BotUsername
		//{
		//	get
		//	{
		//		if (String.IsNullOrEmpty(botUsername))
		//		{
		//			dynamic doc = null;
		//			string html;

		//			Application.Current.Dispatcher.Invoke(() => doc = ChatWb.Document);

		//			try
		//			{
		//				html = doc.documentElement.InnerHtml;
		//			}
		//			catch (Exception)
		//			{
		//				return "";
		//			}

		//			var startIndex = html.IndexOf("input-area", StringComparison.Ordinal);
		//			startIndex = html.IndexOf("title=", startIndex, StringComparison.Ordinal) + 6;

		//			var endIndex = html.IndexOf(" alt", startIndex, StringComparison.Ordinal);

		//			botUsername = html.Substring(startIndex, endIndex - startIndex).Replace("\"", "").Replace(" ", ""); // Reply names are trimmed without spaces
		//		}

		//		return botUsername;
		//	}
		//}

		public static bool FullScanEnabled
		{
			get
			{
				return Boolean.Parse(File.ReadAllText(DirectoryTools.GetEnableFullScanFile()));
			}

			set
			{
				File.WriteAllText(DirectoryTools.GetEnableFullScanFile(), value.ToString(CultureInfo.InvariantCulture));
			}
		}

		public static float AccuracyThreshold
		{
			get
			{
				return float.Parse(File.ReadAllText(DirectoryTools.GetAccuracyThresholdFile()), CultureInfo.InvariantCulture);
			}

			set
			{
				File.WriteAllText(DirectoryTools.GetAccuracyThresholdFile(), value.ToString(CultureInfo.InvariantCulture));
			}
		}

		public static int TermCount
		{
			get
			{
				var termCount = 0;

				foreach (var filter in BlackFilters.Values)
				{
					termCount += filter.Terms.Count;
				}

				foreach (var filter in WhiteFilters.Values)
				{
					termCount += filter.Terms.Count;
				}

				return termCount + BadTagDefinitions.BadTags.Count;
			}
		}

		//public static int ChatRoomID
		//{
		//	get
		//	{
		//		if (chatID == 0)
		//		{
		//			App.Current.Dispatcher.Invoke(() =>
		//			{
		//				try
		//				{
		//					var startIndex = ChatWb.Source.AbsolutePath.IndexOf("rooms/", StringComparison.Ordinal) + 6;
		//					var endIndex = ChatWb.Source.AbsolutePath.IndexOf("/", startIndex + 1, StringComparison.Ordinal);

		//					var IDString = ChatWb.Source.AbsolutePath.Substring(startIndex, endIndex - startIndex);

		//					if (!IDString.All(Char.IsDigit)) { return; }

		//					chatID = int.Parse(IDString);
		//				}
		//				catch (Exception)
		//				{

		//				}
		//			});
		//		}

		//		return chatID;
		//	}
		//}

		//public static int AnnouncerRoomID
		//{
		//	get
		//	{
		//		if (announceID == 0)
		//		{
		//			App.Current.Dispatcher.Invoke(() =>
		//			{
		//				try
		//				{
		//					var startIndex = AnnounceWb.Source.AbsolutePath.IndexOf("rooms/", StringComparison.Ordinal) + 6;
		//					var endIndex = AnnounceWb.Source.AbsolutePath.IndexOf("/", startIndex + 1, StringComparison.Ordinal);

		//					var IDString = AnnounceWb.Source.AbsolutePath.Substring(startIndex, endIndex - startIndex);

		//					if (!IDString.All(Char.IsDigit)) { return; }

		//					announceID = int.Parse(IDString);
		//				}
		//				catch (Exception)
		//				{

		//				}
		//			});
		//		}

		//		return announceID;
		//	}
		//}

		public static string Status
		{
			get
			{
				return File.ReadAllText(DirectoryTools.GetStatusFile());
			}

			set
			{
				File.WriteAllText(DirectoryTools.GetStatusFile(), value);
			}
		}
	}
}