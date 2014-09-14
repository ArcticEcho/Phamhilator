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

		public static KeyValuePair<string, string> GetLastChatMessages(string html)
		{
			var startIndex = html.LastIndexOf("username owner", StringComparison.Ordinal);
			var endIndex = html.IndexOf("TABLE id=input-table width", StringComparison.Ordinal);

			startIndex = html.IndexOf("<A class=\\\"signature user", startIndex + 1, StringComparison.Ordinal) + 25;
			startIndex = html.IndexOf("users/", startIndex, StringComparison.Ordinal) + 7;
			startIndex = html.IndexOf("//", startIndex, StringComparison.Ordinal) + 1;
				
			// Get username.

			var username = html.Substring(startIndex, html.IndexOf("\\\">", startIndex, StringComparison.Ordinal));

			// Get message.

			startIndex = html.IndexOf("<DIV class=content>", startIndex) + 20;

			var message = html.Substring(startIndex, html.IndexOf("</DIV>", startIndex));

			return new KeyValuePair<string, string>(username, message);
		}
	}
}
