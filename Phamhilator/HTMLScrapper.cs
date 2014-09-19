using System;
using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;



namespace Phamhilator
{
	public static class HTMLScraper
	{
		private static readonly Regex escapeChars = new Regex(@"([_*\\`\[\]])");



		public static string GetURL(string html)
		{
			var startIndex = html.IndexOf("href=", StringComparison.Ordinal) + 6;
			var endIndex = html.IndexOf("\">", startIndex, StringComparison.Ordinal);

			return html.Substring(startIndex, endIndex - startIndex).Trim();
		}

		public static string GetTitle(string html)
		{
			var startIndex = html.IndexOf("href=", StringComparison.Ordinal) + 6;
			startIndex = html.IndexOf("href=", startIndex, StringComparison.Ordinal);
			startIndex = html.IndexOf("\">", startIndex, StringComparison.Ordinal) + 2;

			var endIndex = html.IndexOf("</A></H2>", startIndex, StringComparison.Ordinal);

			return escapeChars.Replace(WebUtility.HtmlDecode(html.Substring(startIndex, endIndex - startIndex).Trim()), "");
		}

		public static string GetAuthorLink(string html)
		{
			var startIndex = html.IndexOf("owner realtime-owner", StringComparison.Ordinal) + 21;
			startIndex = html.IndexOf("href=", startIndex, StringComparison.Ordinal) + 6;

			var endIndex = html.IndexOf("\">", startIndex, StringComparison.Ordinal);

			return html.Substring(startIndex, endIndex - startIndex).Trim();
		}

		public static string GetAuthorName(string html)
		{
			var startIndex = html.IndexOf("owner realtime-owner", StringComparison.Ordinal) + 21;
			startIndex = html.IndexOf("href=", startIndex, StringComparison.Ordinal) + 6;
			startIndex = html.IndexOf("\">", startIndex, StringComparison.Ordinal) + 2;

			var endIndex = html.IndexOf("</A>", startIndex, StringComparison.Ordinal);

			return escapeChars.Replace(html.Substring(startIndex, endIndex - startIndex).Trim(), "");
		}

		public static int GetMessageIDByReportTitle(string html, string reportTitle)
		{
			var decoded = WebUtility.HtmlDecode(html);

			var startIndex = decoded.LastIndexOf(reportTitle, StringComparison.Ordinal) - 350;

			startIndex = decoded.IndexOf("click for message actions", startIndex, StringComparison.Ordinal) + 53;

			var t = decoded.Substring(startIndex, decoded.IndexOf("#", startIndex, StringComparison.Ordinal) - startIndex);

			return int.Parse(t);
		}

		public static string GetSite(string postURL)
		{
			var siteEndIndex = postURL.IndexOf("/", 7, StringComparison.Ordinal) - 7;

			return postURL.Substring(7, siteEndIndex).Trim();
		}

		public static List<string> GetTags(string html)
		{
			var tags = new List<string>();

			var startIndex = html.IndexOf("realtime-tags", StringComparison.Ordinal);
			var endIndex = html.IndexOf("</SPAN>", startIndex, StringComparison.Ordinal);

			while (true)
			{
				startIndex = html.IndexOf("href=", startIndex + 1, StringComparison.Ordinal);

				if (startIndex != -1 && startIndex < endIndex)
				{
					var start = html.IndexOf("\">", startIndex, StringComparison.Ordinal) + 2;
					var end = html.IndexOf("</A>", start, StringComparison.Ordinal);

					var result = html.Substring(start, end - start).Trim().ToLowerInvariant();

					tags.Add(result);
				}
				else
				{
					break;
				}
			}

			return tags;
		}

		public static MessageInfo GetLastChatMessage(string html)
		{
			var startIndex = html.LastIndexOf("signature user-", StringComparison.Ordinal) + 15;
				
			// Get user ID.

			var authorID = html.Substring(startIndex, (html.IndexOf(@"/", startIndex, StringComparison.Ordinal) - 8) - startIndex);

			// Get message.

			startIndex = html.LastIndexOf("<DIV class=content>", StringComparison.Ordinal) + 19;

			var message = WebUtility.HtmlDecode(html.Substring(startIndex, html.IndexOf("</DIV>", startIndex, StringComparison.Ordinal) - startIndex));//.Replace(@"<SPAN class=mention>", "").Replace("</SPAN>", "");

			var info = new MessageInfo 
			{ 
				Body = message.Replace(@"<SPAN class=mention>", "").Replace("</SPAN>", ""), 
				RepliesToMessageID = GetMessageReplyID(html, message), 
				AuthorID = int.Parse(authorID),
				MessageID = GetLastestMessageID(html)
			};

			if (info.RepliesToMessageID != -1 && GlobalInfo.PostedReports.ContainsKey(info.RepliesToMessageID))
			{
				info.Report = GlobalInfo.PostedReports[info.RepliesToMessageID].Report;
				info.Post = GlobalInfo.PostedReports[info.RepliesToMessageID].Post;
			}

			return info;
		}



		private static int GetLastestMessageID(string html)
		{
			var startIndex = html.LastIndexOf("click for message actions", StringComparison.Ordinal) + 53;

			return int.Parse(html.Substring(startIndex, html.IndexOf("#", startIndex, StringComparison.Ordinal) - startIndex));
		}

		private static int GetMessageReplyID(string html, string messageContent)
		{
			var messageIndex = html.LastIndexOf(messageContent, StringComparison.Ordinal);

			if (messageIndex == -1)
			{
				return -1;
			}

			var infoIndex = html.IndexOf("class=reply-info", messageIndex - 190, StringComparison.Ordinal);

			if (messageIndex - infoIndex > 190 || infoIndex == -1) // Message isn't a replay
			{
				return -1;
			}

			infoIndex = html.IndexOf(@"/message/", infoIndex, StringComparison.Ordinal) + 9;

			return int.Parse(html.Substring(infoIndex, html.IndexOf("#", infoIndex, StringComparison.Ordinal) - infoIndex));
		}
	}
}
