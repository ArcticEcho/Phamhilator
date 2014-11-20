//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading;
//using System.Windows;



//namespace Phamhilator
//{
//	public static class MessageHandler
//	{
//		public static void PostMessage(MessageInfo message, post)
//		{
//			var error = false;
//			MessageInfo message;

//			do
//			{
//				Thread.Sleep(1000);
//			} while (!GlobalInfo.BotRunning);

//			while (!exit)
//			{
//				Thread.Sleep(2000);

//				if (GlobalInfo.AnnouncerRoomID == 0 || MessageQueue.All(m => m.RoomID != GlobalInfo.AnnouncerRoomID)) { continue; }

//				message = MessageQueue.First(m => m.RoomID == GlobalInfo.AnnouncerRoomID);
//				error = false;

//				// Post message.

//				Application.Current.Dispatcher.Invoke(() =>
//				{
//					try
//					{
//						GlobalInfo.AnnounceWb.InvokeScript("eval", new object[]
//						{
//							@"$.post('/chats/" + message.RoomID + "/messages/new', { text: '" + message.Body.Replace("\\", "\\\\") + "', fkey: fkey().fkey });"
//						});
//					}
//					catch (Exception)
//					{
//						error = true;
//					}
//				});

//				MessageQueue.Remove(message);

//				if (error || message.Post == null || message.Report == null) { continue; }

//				// Get message ID.

//				dynamic doc = null;
//				var i = 0;
//				var html = "";
//				int id;

//				while ((id = HTMLScraper.GetMessageIDByPostURL(html, message.Post.URL)) == -1)
//				{
//					if (i > 32) // 8 secs (250 ms * 32).
//					{
//						PostChatMessage("`Failed to get message ID for report: " + message.Post.Title + ".`");

//						break;
//					}

//					Application.Current.Dispatcher.Invoke(() => doc = GlobalInfo.AnnounceWb.Document);

//					try
//					{
//						html = doc.documentElement.InnerHtml;
//					}
//					catch (Exception)
//					{

//					}

//					i++;

//					Thread.Sleep(250);
//				}

//				if (id == -1 || GlobalInfo.PostedReports.ContainsKey(id)) { continue; }

//				GlobalInfo.PostedReports.Add(id, message);
//			}


//			//consecutiveMessageCount++;

//			//var delay = (int)(4.1484 * Math.Log(consecutiveMessageCount) + 1.02242) * 1000;

//			//if (consecutiveMessageCount >= 20) { return; }

//			//Thread.Sleep(delay);

//			//PostMessage(message, consecutiveMessageCount);
//		}
//	}
//}
