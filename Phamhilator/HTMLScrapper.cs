using System;
using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;



namespace Phamhilator
{
	public static class HTMLScraper
	{
		private static readonly Regex escapeChars = new Regex(@"[_*\\`\[\]]");



		public static string GetURL(string html)
		{
			var startIndex = html.IndexOf("href=", StringComparison.Ordinal) + 6;
			var endIndex = html.IndexOf("\">", startIndex, StringComparison.Ordinal);

			var url = html.Substring(startIndex, endIndex - startIndex).Trim();

			return url.Substring(0, url.LastIndexOf("/", StringComparison.Ordinal));
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

			var startIndex = decoded.IndexOf(reportTitle, StringComparison.Ordinal) - 350;

			startIndex = decoded.IndexOf("click for message actions", startIndex, StringComparison.Ordinal) + 53;

			var id = decoded.Substring(startIndex, decoded.IndexOf("#", startIndex, StringComparison.Ordinal) - startIndex);

			return int.Parse(id);
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

			var message = WebUtility.HtmlDecode(html.Substring(startIndex, html.IndexOf("</DIV>", startIndex, StringComparison.Ordinal) - startIndex));

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

		public static string GetQuestionBody(string html)
		{
			var startIndex = html.IndexOf("itemprop=\"text\">", StringComparison.Ordinal) + 18;
			var endIndex = html.IndexOf("</div>", startIndex, StringComparison.Ordinal) - 7;

			return WebUtility.HtmlDecode(html.Substring(startIndex, endIndex - startIndex)).Trim();
		}

		public static int GetQuestionScore(string html)
		{
			var startIndex = html.IndexOf("vote-count-post", StringComparison.Ordinal) + 18;
			var endIndex = html.IndexOf("</span>", startIndex, StringComparison.Ordinal);

			var score = html.Substring(startIndex, endIndex - startIndex);

			return int.Parse(score);
		}

		public static int GetQuestionAuthorRep(string html)
		{
			var startIndex = html.IndexOf("asked <", StringComparison.Ordinal);

			if (startIndex == -1) // Check if post is CW.
			{
				return 1;
			}

			var newStartIndex = html.IndexOf("dir=\"ltr\">", startIndex, StringComparison.Ordinal) + 10;

			if (newStartIndex - startIndex > 800 || newStartIndex == -1) // Check if user is anonymous.
			{
				return 1;
			}

			var endIndex = html.IndexOf("<", newStartIndex, StringComparison.Ordinal);

			return ParseRep(html.Substring(newStartIndex, endIndex - newStartIndex));
		}


		public static int GetAnswerCount(string html)
		{
			var count = 0;

			for (var i = 0; i < html.Length - 16; i++)
			{
				if (html.Substring(i, 16) == "<div id=\"answer-")
				{
					count++;
				}
			}

			return count;
		}

		public static int GetAnswerScore(string html)
		{
			var startIndex = html.IndexOf("<div id=\"answer-", StringComparison.Ordinal);
			startIndex = html.IndexOf("vote-count-post", startIndex, StringComparison.Ordinal) + 18;

			var endIndex = html.IndexOf("<", startIndex, StringComparison.Ordinal);

			return int.Parse(html.Substring(startIndex, endIndex - startIndex));
		}

		public static string GetAnswerBody(string html)
		{
			var startIndex = html.IndexOf("<div id=\"answer-", StringComparison.Ordinal);
			startIndex = html.IndexOf("itemprop=\"text\">", startIndex, StringComparison.Ordinal) + 18;

			var endIndex = html.IndexOf("</div>", startIndex, StringComparison.Ordinal);

			return Regex.Replace(escapeChars.Replace(WebUtility.HtmlDecode(html.Substring(startIndex, endIndex - startIndex)), ""), @"\s+", " ").Trim();
		}

		public static string GetAnswerAuthorLink(string html, string questionURL)
		{
			var startIndex = html.IndexOf("<div id=\"answer-", StringComparison.Ordinal);
			startIndex = html.IndexOf("answered <", startIndex, StringComparison.Ordinal);

			if (startIndex == -1) // Check if post is CW.
			{
				return "";
			}

			startIndex = html.IndexOf("user-details", startIndex, StringComparison.Ordinal) + 33;

			var endIndex = html.IndexOf("\"", startIndex, StringComparison.Ordinal);

			return questionURL.Substring(0, questionURL.IndexOf("/", 7, StringComparison.Ordinal)) + html.Substring(startIndex, endIndex - startIndex);
		}

		public static string GetAnswerAuthorName(string html)
		{
			var startIndex = html.IndexOf("<div id=\"answer-", StringComparison.Ordinal);
			startIndex = html.IndexOf("answered <", startIndex, StringComparison.Ordinal);

			if (startIndex == -1) // Check if post is CW.
			{
				return "";
			}

			startIndex = html.IndexOf("user-details", startIndex, StringComparison.Ordinal) + 33;
			startIndex = html.IndexOf(">", startIndex, StringComparison.Ordinal) + 1;

			var endIndex = html.IndexOf("<", startIndex, StringComparison.Ordinal);

			return html.Substring(startIndex, endIndex - startIndex);
		}

		public static string GetAnswerLink(string html, string questionURL)
		{
			var startIndex = html.IndexOf("<div id=\"answer-", StringComparison.Ordinal);
			startIndex = html.IndexOf("post-menu", startIndex, StringComparison.Ordinal) + 20;

			var endIndex = html.IndexOf("\"", startIndex, StringComparison.Ordinal);

			return questionURL.Substring(0, questionURL.IndexOf("/", 7, StringComparison.Ordinal)) + html.Substring(startIndex, endIndex - startIndex);
		}

		public static int GetAnswerAuthorRep(string html)
		{
			var startIndex = html.IndexOf("<div id=\"answer-", StringComparison.Ordinal);
			startIndex = html.IndexOf("answered <", startIndex, StringComparison.Ordinal);

			if (startIndex == -1) // Check if post is CW.
			{
				return 1;
			}

			var newStartIndex = html.IndexOf("reputation score", startIndex, StringComparison.Ordinal);

			if (newStartIndex - startIndex > 800 || newStartIndex == -1) // Check if user is anonymous.
			{
				return 1;
			}

			newStartIndex = html.IndexOf(">", newStartIndex, StringComparison.Ordinal) + 1;

			var endIndex = html.IndexOf("<", newStartIndex, StringComparison.Ordinal);

			return ParseRep(html.Substring(newStartIndex, endIndex - newStartIndex));
		}



		private static int ParseRep(string rep)
		{
			if (String.IsNullOrEmpty(rep))
			{
				return 1;
			}

			if (rep.ToLowerInvariant().Contains("k"))
			{
				if (rep.Contains("."))
				{
					var charsAfterPeriod = rep.Substring(0, rep.IndexOf(".", StringComparison.Ordinal) + 1).Length;
					var e = float.Parse(rep.Replace("k", ""));
					var p = Math.Pow(10, charsAfterPeriod);

					return (int)Math.Round(e * p);
				}

				return (int)float.Parse(rep.ToLowerInvariant().Replace("k", "000"));
			}

			return (int)float.Parse(rep);
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
