using System;
using System.Threading;
using System.Windows;



namespace Phamhilator
{
	public static class MessageHandler
	{
		public static void PostMessage(string message, Post post = null, PostAnalysis report = null, bool isQuestionReport = true/* int consecutiveMessageCount = 0*/)
		{
			if (GlobalInfo.RoomID == 0) { return; }

			var error = false;

			Application.Current.Dispatcher.Invoke(() =>
			{
				try
				{
					GlobalInfo.ChatWb.InvokeScript("eval", new object[]
					{
						"$.post('/chats/" + GlobalInfo.RoomID + "/messages/new', { text: '" + message + "', fkey: fkey().fkey });"
					});
				}
				catch (Exception)
				{
					error = true;
				}
			});

			if (error || post == null || report == null) { return; }

			Thread.Sleep(3000);

			dynamic doc = null;
			var i = 0;
			var html = "";

			while (html.IndexOf(post.Title, StringComparison.Ordinal) == -1)
			{
				if (i >= 5) { return; }

				Application.Current.Dispatcher.Invoke(() => doc = GlobalInfo.ChatWb.Document);

				try
				{
					html = doc.documentElement.InnerHtml;
				}
				catch (Exception)
				{
					return;
				}

				i++;

				Thread.Sleep(3000);
			}

			var id = HTMLScraper.GetMessageIDByReportTitle(html, post.Title);

			if (!GlobalInfo.PostedReports.ContainsKey(id))
			{
				GlobalInfo.PostedReports.Add(id, new MessageInfo { Post = post, Report = report, Body = message, IsQuestionReport = isQuestionReport });
			}

			//consecutiveMessageCount++;

			//var delay = (int)(4.1484 * Math.Log(consecutiveMessageCount) + 1.02242) * 1000;

			//if (consecutiveMessageCount >= 20) { return; }

			//Thread.Sleep(delay);

			//PostMessage(message, consecutiveMessageCount);
		}

		public static void EditMessage(string newMessage, int messageID)
		{
			Application.Current.Dispatcher.Invoke(() =>
			{
				try
				{
					GlobalInfo.ChatWb.InvokeScript("eval", new object[]
					{
						@"$.post('http://chat.meta.stackexchange.com/messages/" + messageID + "', { text: '" + newMessage + "', fkey: fkey().fkey });"
					});
				}
				catch (Exception)
				{

				}
			});
		}

		public static void DeleteMessage(int messageID)
		{
			Application.Current.Dispatcher.Invoke(() =>
			{
				try
				{
					GlobalInfo.ChatWb.InvokeScript("eval", new object[]
					{
						@"$.post('http://chat.meta.stackexchange.com/messages/" + messageID + "/delete" + "', { fkey: fkey().fkey });"
					});
				}
				catch (Exception)
				{

				}
			});
		}
	}
}
