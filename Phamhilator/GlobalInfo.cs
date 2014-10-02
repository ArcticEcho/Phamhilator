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
		public const string BotUsername = "pham"; // TODO: change this to the username of your account which the bot will be using.
		public static readonly Dictionary<int, MessageInfo> PostedReports = new Dictionary<int, MessageInfo>(); // Message ID, actual message.
		public static readonly WhiteFilters.BadUsername WhiteName = new WhiteFilters.BadUsername();
		public static readonly WhiteFilters.Offensive WhiteOff = new WhiteFilters.Offensive();
		public static readonly WhiteFilters.Spam WhiteSpam = new WhiteFilters.Spam();
		public static readonly WhiteFilters.LQ WhiteLQ = new WhiteFilters.LQ();
		public static readonly BlackFilters.BadUsername BlackName = new BlackFilters.BadUsername();
		public static readonly BlackFilters.Offensive BlackOff = new BlackFilters.Offensive();
		public static readonly BlackFilters.Spam BlackSpam = new BlackFilters.Spam();
		public static readonly BlackFilters.LQ BlackLQ = new BlackFilters.LQ();
		public static WebBrowser ChatWb;
		public const string Owners = "Sam, Unihedron & ProgramFOX";
		public static int PostsCaught;
		public static DateTime UpTime;
		public static bool BotRunning;

		public static float AccuracyThreshold
		{
			get
			{
				return Single.Parse(File.ReadAllText(DirectoryTools.GetAccuracyThresholdFile()));
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
				return BlackOff.Terms.Count + BlackSpam.Terms.Count + BlackLQ.Terms.Count + BlackName.Terms.Count +
					   WhiteName.Terms.Values.Sum(x => x.Count) + WhiteOff.Terms.Values.Sum(x => x.Count) + WhiteSpam.Terms.Values.Sum(x => x.Count) + WhiteLQ.Terms.Values.Sum(x => x.Count) + 
					   BadTagDefinitions.BadTags.Count;
			}
		}

		public static int RoomID
		{
			get
			{
				var ID = 0;

				App.Current.Dispatcher.Invoke(() =>
				{
					try
					{
						var startIndex = ChatWb.Source.AbsolutePath.IndexOf("rooms/", StringComparison.Ordinal) + 6;
						var endIndex = ChatWb.Source.AbsolutePath.IndexOf("/", startIndex + 1, StringComparison.Ordinal);

						var IDString = ChatWb.Source.AbsolutePath.Substring(startIndex, endIndex - startIndex);

						if (!IDString.All(Char.IsDigit)) { return; }

						ID = int.Parse(IDString);
					}
					catch (Exception)
					{

					}
				});

				return ID;
			}
		}
	}
}
