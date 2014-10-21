using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Controls;



namespace Phamhilator
{
	public static class GlobalInfo
	{
		private static int chatID;
		private static int announceID;

		#region Filters

		public static readonly QuestionFilters.Title.White.BadUsername QTWName = new QuestionFilters.Title.White.BadUsername();
		public static readonly QuestionFilters.Title.White.Offensive QTWOff = new QuestionFilters.Title.White.Offensive();
		public static readonly QuestionFilters.Title.White.Spam QTWSpam = new QuestionFilters.Title.White.Spam();
		public static readonly QuestionFilters.Title.White.LQ QTWLQ = new QuestionFilters.Title.White.LQ();

		public static readonly QuestionFilters.Title.Black.BadUsername QTBName = new QuestionFilters.Title.Black.BadUsername();
		public static readonly QuestionFilters.Title.Black.Offensive QTBOff = new QuestionFilters.Title.Black.Offensive();
		public static readonly QuestionFilters.Title.Black.Spam QTBSpam = new QuestionFilters.Title.Black.Spam();
		public static readonly QuestionFilters.Title.Black.LQ QTBLQ = new QuestionFilters.Title.Black.LQ();


		public static readonly QuestionFilters.Body.White.Offensive QBWOff = new QuestionFilters.Body.White.Offensive();
		public static readonly QuestionFilters.Body.White.Spam QBWSpam = new QuestionFilters.Body.White.Spam();
		public static readonly QuestionFilters.Body.White.LQ QBWLQ = new QuestionFilters.Body.White.LQ();

		public static readonly QuestionFilters.Body.Black.Offensive QBBOff = new QuestionFilters.Body.Black.Offensive();
		public static readonly QuestionFilters.Body.Black.Spam QBBSpam = new QuestionFilters.Body.Black.Spam();
		public static readonly QuestionFilters.Body.Black.LQ QBBLQ = new QuestionFilters.Body.Black.LQ();


		public static readonly AnswerFilters.Black.BadUsername ABName = new AnswerFilters.Black.BadUsername();
		public static readonly AnswerFilters.Black.Offensive ABOff = new AnswerFilters.Black.Offensive();
		public static readonly AnswerFilters.Black.Spam ABSpam = new AnswerFilters.Black.Spam();
		public static readonly AnswerFilters.Black.LQ ABLQ = new AnswerFilters.Black.LQ();

		public static readonly AnswerFilters.White.BadUsername AWName = new AnswerFilters.White.BadUsername();
		public static readonly AnswerFilters.White.Offensive AWOff = new AnswerFilters.White.Offensive();
		public static readonly AnswerFilters.White.Spam AWSpam = new AnswerFilters.White.Spam();
		public static readonly AnswerFilters.White.LQ AWLQ = new AnswerFilters.White.LQ();

		#endregion

		public const string BotUsername = "pham"; // TODO: change this to the username of your account which the bot will be using.
		public static readonly Dictionary<int, MessageInfo> PostedReports = new Dictionary<int, MessageInfo>(); // Message ID, actual message.
		public static readonly MessageHandler MessagePoster = new MessageHandler();
		public const string Owners = "Sam, Unihedron, Patrick Hofman, Jan Dvorak & ProgramFOX";
		public static WebBrowser ChatWb;
		public static WebBrowser AnnounceWb;
		public static int PostsCaught;
		public static DateTime UpTime;
		public static bool BotRunning;
		public static bool Exit;

		public static bool EnableFullScan
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
				return QTBOff.Terms.Count + QTBSpam.Terms.Count + QTBLQ.Terms.Count + QTBName.Terms.Count +
					   QBBOff.Terms.Count + QBBSpam.Terms.Count + QBBLQ.Terms.Count +
					   ABOff.Terms.Count + ABSpam.Terms.Count + ABLQ.Terms.Count + ABName.Terms.Count +
					   QTWName.Terms.Values.Sum(x => x.Count) + QTWOff.Terms.Values.Sum(x => x.Count) + QTWSpam.Terms.Values.Sum(x => x.Count) + QTWLQ.Terms.Values.Sum(x => x.Count) +
					   QBWOff.Terms.Values.Sum(x => x.Count) + QBWSpam.Terms.Values.Sum(x => x.Count) + QBWLQ.Terms.Values.Sum(x => x.Count) +
					   AWName.Terms.Values.Sum(x => x.Count) + AWOff.Terms.Values.Sum(x => x.Count) + AWSpam.Terms.Values.Sum(x => x.Count) + AWLQ.Terms.Values.Sum(x => x.Count) + 
					   BadTagDefinitions.BadTags.Count;
			}
		}

		public static int ChatRoomID
		{
			get
			{
				if (chatID == 0)
				{
					App.Current.Dispatcher.Invoke(() =>
					{
						try
						{
							var startIndex = ChatWb.Source.AbsolutePath.IndexOf("rooms/", StringComparison.Ordinal) + 6;
							var endIndex = ChatWb.Source.AbsolutePath.IndexOf("/", startIndex + 1, StringComparison.Ordinal);

							var IDString = ChatWb.Source.AbsolutePath.Substring(startIndex, endIndex - startIndex);

							if (!IDString.All(Char.IsDigit)) { return; }

							chatID = int.Parse(IDString);
						}
						catch (Exception)
						{

						}
					});
				}

				return chatID;
			}
		}

		public static int AnnouncerRoomID
		{
			get
			{
				if (announceID == 0)
				{
					App.Current.Dispatcher.Invoke(() =>
					{
						try
						{
							var startIndex = AnnounceWb.Source.AbsolutePath.IndexOf("rooms/", StringComparison.Ordinal) + 6;
							var endIndex = AnnounceWb.Source.AbsolutePath.IndexOf("/", startIndex + 1, StringComparison.Ordinal);

							var IDString = AnnounceWb.Source.AbsolutePath.Substring(startIndex, endIndex - startIndex);

							if (!IDString.All(Char.IsDigit)) { return; }

							announceID = int.Parse(IDString);
						}
						catch (Exception)
						{

						}
					});
				}

				return announceID;
			}
		}

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
