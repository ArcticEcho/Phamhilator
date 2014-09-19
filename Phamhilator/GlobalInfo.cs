using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Phamhilator.BlackFilters;



namespace Phamhilator
{
	public static class GlobalInfo
	{
		public static readonly Dictionary<int, MessageInfo> PostedReports = new Dictionary<int, MessageInfo>();
		public static readonly WhiteFilters.BadUsername IgnoreName = new WhiteFilters.BadUsername();
		public static readonly WhiteFilters.Offensive IgnoreOff = new WhiteFilters.Offensive();
		public static readonly WhiteFilters.Spam IgnoreSpam = new WhiteFilters.Spam();
		public static readonly WhiteFilters.LQ IgnoreLQ = new WhiteFilters.LQ();	
		public static readonly BadUsername Name = new BadUsername();
		public static readonly Offensive Off = new Offensive();
		public static readonly Spam Spam = new Spam();
		public static readonly LQ LQ = new LQ();
		public static int PostsCaught;
		public static DateTime UpTime;
		public const string Owners = "Sam, Unihedron & ProgramFOX";
		public static bool BotRunning;

		public static int TermCount
		{
			get
			{
				return Off.Terms.Count + Spam.Terms.Count + LQ.Terms.Count + Name.Terms.Count +
					   IgnoreName.Terms.Count + IgnoreOff.Terms.Count + IgnoreSpam.Terms.Count + IgnoreLQ.Terms.Count + 
					   BadTagDefinitions.BadTags.Count;
			}
		}
	}
}
