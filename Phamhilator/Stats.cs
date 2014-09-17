using System;
using Phamhilator.Filters;



namespace Phamhilator
{
	public static class Stats
	{
		public static int PostsCaught;
		public static DateTime UpTime;
		public const string Owners = "Sam, Unihedron & ProgramFOX";
		public static bool BotRunning;

		public static int TermCount
		{
			get
			{
				return Offensive.Terms.Count + Spam.Terms.Count + LQ.Terms.Count + BadUsername.Terms.Count + IgnoreFilterTerms.TermCount;
			}
		}
	}
}
