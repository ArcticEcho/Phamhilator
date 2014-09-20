using System;
using System.Collections.Generic;
using System.Linq;



namespace Phamhilator
{
	public static class GlobalInfo
	{
		public static readonly Dictionary<int, MessageInfo> PostedReports = new Dictionary<int, MessageInfo>();
		public static readonly WhiteFilters.BadUsername WhiteName = new WhiteFilters.BadUsername();
		public static readonly WhiteFilters.Offensive WhiteOff = new WhiteFilters.Offensive();
		public static readonly WhiteFilters.Spam WhiteSpam = new WhiteFilters.Spam();
		public static readonly WhiteFilters.LQ WhiteLQ = new WhiteFilters.LQ();
		public static readonly BlackFilters.BadUsername BlackName = new BlackFilters.BadUsername();
		public static readonly BlackFilters.Offensive BlackOff = new BlackFilters.Offensive();
		public static readonly BlackFilters.Spam BlackSpam = new BlackFilters.Spam();
		public static readonly BlackFilters.LQ BlackLQ = new BlackFilters.LQ();
		public static int PostsCaught;
		public static DateTime UpTime;
		public const string Owners = "Sam, Unihedron & ProgramFOX";
		public static bool BotRunning;

		public static int TermCount
		{
			get
			{
				return BlackOff.Terms.Count + BlackSpam.Terms.Count + BlackLQ.Terms.Count + BlackName.Terms.Count +
					   WhiteName.Terms.Values.Sum(x => x.Count) + WhiteOff.Terms.Values.Sum(x => x.Count) + WhiteSpam.Terms.Values.Sum(x => x.Count) + WhiteLQ.Terms.Values.Sum(x => x.Count) + 
					   BadTagDefinitions.BadTags.Count;
			}
		}
	}
}
