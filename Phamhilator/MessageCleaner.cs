using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace Phamhilator
{
	public static class MessageCleaner
	{
		public static string GetCleanMessage(int messageID)
		{
			var oldTitle = GlobalInfo.PostedReports[messageID].Post.Title;
			var newTitle = CensorString(GlobalInfo.PostedReports[messageID].Post.Title);

			var oldName = GlobalInfo.PostedReports[messageID].Post.AuthorName;
			var newName = CensorString(GlobalInfo.PostedReports[messageID].Post.AuthorName);

			return GlobalInfo.PostedReports[messageID].Body.Replace(oldTitle, newTitle).Replace(oldName, newName);
		}



		private static string CensorString(string input)
		{
			var censored = new StringBuilder();

			foreach (var c in input)
			{
				if (c == ' ')
				{
					censored.Append(' ');
				}
				else
				{
					censored.Append('*');
				}
			}

			return censored.ToString();
		}
	}
}
