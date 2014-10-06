using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;



namespace Phamhilator
{
	public class MessageHandler
	{
		public readonly List<MessageInfo> MessageQueue = new List<MessageInfo>();



		public MessageHandler()
		{
			new Thread(PostMessages) { Priority = ThreadPriority.Lowest }.Start();
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

		public static bool DeleteMessage(int messageID, string reportTitle)
		{
			dynamic doc = null;
			var html = "";

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

			Thread.Sleep(3000); // Wait for message to be deleted.

			try
			{
				Application.Current.Dispatcher.Invoke(() => doc = GlobalInfo.ChatWb.Document);

				html = doc.documentElement.InnerHtml;
			}
			catch (Exception)
			{

			}
			
			return html.IndexOf(reportTitle, StringComparison.Ordinal) == -1;
		}		
		
		
		
		private void PostMessages(/*int consecutiveMessageCount = 0*/)
		{
			var error = false;
			MessageInfo message;

			do
			{
				Thread.Sleep(1000);
			} while (!GlobalInfo.BotRunning);

			while (GlobalInfo.BotRunning)
			{
				Thread.Sleep(1000);

				if (GlobalInfo.RoomID == 0 || MessageQueue.Count == 0) { continue; }

				message = MessageQueue[0];
				error = false;
				
				// Post message.

				Application.Current.Dispatcher.Invoke(() =>
				{
					try
					{
						GlobalInfo.ChatWb.InvokeScript("eval", new object[]
						{
							"$.post('/chats/" + GlobalInfo.RoomID + "/messages/new', { text: '" + message.Body + "', fkey: fkey().fkey });"
						});
					}
					catch (Exception)
					{
						error = true;
					}
				});

				MessageQueue.Remove(message);

				if (error || message.Post == null || message.Report == null) { continue; }

				// Get message ID.

				Thread.Sleep(3000);

				dynamic doc = null;
				var i = 0;
				var html = "";

				while (html.IndexOf(message.Post.Title, StringComparison.Ordinal) == -1)
				{
					if (i > 5) { break; }

					Application.Current.Dispatcher.Invoke(() => doc = GlobalInfo.ChatWb.Document);

					try
					{
						html = doc.documentElement.InnerHtml;
					}
					catch (Exception)
					{
						break;
					}

					i++;

					Thread.Sleep(3000);
				}

				if (i < 5)
				{
					var id = HTMLScraper.GetMessageIDByReportTitle(html, message.Post.Title);

					if (!GlobalInfo.PostedReports.ContainsKey(id))
					{
						GlobalInfo.PostedReports.Add(id, message);
					}
				}		
			}
				

			//consecutiveMessageCount++;

			//var delay = (int)(4.1484 * Math.Log(consecutiveMessageCount) + 1.02242) * 1000;

			//if (consecutiveMessageCount >= 20) { return; }

			//Thread.Sleep(delay);

			//PostMessage(message, consecutiveMessageCount);
		}

	}
}
