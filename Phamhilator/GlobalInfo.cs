using System;
using System.Collections.Generic;
using Phamhilator.Filters;



namespace Phamhilator
{
	public static class GlobalInfo
	{
		public static readonly Dictionary<int, MessageInfo> PostedReports = new Dictionary<int, MessageInfo>();
		public static readonly IgnoreFilters.BadUsername IgnoreName = new IgnoreFilters.BadUsername();
		public static readonly IgnoreFilters.Offensive IgnoreOff = new IgnoreFilters.Offensive();
		public static readonly IgnoreFilters.Spam IgnoreSpam = new IgnoreFilters.Spam();
		public static readonly IgnoreFilters.LQ IgnoreLQ = new IgnoreFilters.LQ();	
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
