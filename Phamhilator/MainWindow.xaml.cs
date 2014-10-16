﻿using System;
using System.Linq;
using System.Net;
using System.Windows;
using Microsoft.Win32;
using System.Threading;
using System.Reflection;
using System.Diagnostics;
using System.Globalization;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Collections.Generic;



namespace Phamhilator
{
	public partial class MainWindow
	{
		private bool die;
		private int refreshRate = 10000; // In milliseconds.
		private readonly HashSet<int> spammers = new HashSet<int>();
		private static readonly List<string> previouslyFoundPosts = new List<string>();
		private Thread commandListenerThread;
		private Thread postCatcherThread;
		private Thread postRefresherThread;
		private DateTime requestedDieTime;



		public MainWindow()
		{
			InitializeComponent();

			GlobalInfo.ChatWb = chatWb;
			GlobalInfo.AnnounceWb = announceWb;

			SwitchToIE9();

			HideScriptErrors(realtimeWb, true);
			HideScriptErrors(chatWb, true);
			HideScriptErrors(announceWb, true);

			BotKiller();

			KillListener();

			PostRefresher();

			PostCatcher();

			CommandListener();
		}



		private void PostCatcher()
		{
			var postSuccessMessage = false;
			dynamic doc = null;
			string html;

			postCatcherThread = new Thread(() =>
			{
				for (var i = 1; i < 5; i++)
				{
					try
					{
						do
						{
							Thread.Sleep(refreshRate / 2);
						} while (!GlobalInfo.BotRunning);

						if (i != 1)
						{
							postSuccessMessage = true;
						}

						while (!GlobalInfo.Exit)
						{
							Dispatcher.Invoke(() => doc = realtimeWb.Document);

							try
							{
								html = doc.documentElement.InnerHtml;
							}
							catch (Exception)
							{
								continue;
							}

							if (html.IndexOf("DIV class=metaInfo", StringComparison.Ordinal) == -1)
							{
								continue;
							}

							var posts = GetAllPosts(html);

							if (posts.Length != 0)
							{
								CheckPosts(posts);
							}

							do
							{
								Thread.Sleep(refreshRate / 2);
							} while (!GlobalInfo.BotRunning);

							if (postSuccessMessage)
							{
								GlobalInfo.MessagePoster.MessageQueue.Add(new MessageInfo { Body = "`Restart successful!`" }, GlobalInfo.ChatRoomID);

								postSuccessMessage = false;
							}
						}
					}
					catch (Exception)
					{
						if (!GlobalInfo.Exit)
						{
							Thread.Sleep(2000);

							if (i == 4)
							{
								GlobalInfo.MessagePoster.MessageQueue.Add(new MessageInfo { Body = "`Warning: 3 attempts to restart post catcher thread have failed. Now shutting down...`" }, GlobalInfo.ChatRoomID);

								GlobalInfo.Exit = true;
							}
							else
							{
								GlobalInfo.MessagePoster.MessageQueue.Add(new MessageInfo { Body = "`Warning: post catcher thread has died. Attempting to restart...`" }, GlobalInfo.ChatRoomID);
							}
						}
					}

					if (GlobalInfo.Exit) { break; }
				}
			}) { Priority = ThreadPriority.Lowest };

			postCatcherThread.Start();
		}

		private void PostRefresher()
		{
			var postSuccessMessage = false;

			postRefresherThread = new Thread(() =>
			{
				for (var i = 1; i < 5; i++)
				{
					try
					{
						do
						{
							Thread.Sleep(refreshRate);
						} while (!GlobalInfo.BotRunning);

						if (i != 1)
						{
							postSuccessMessage = true;
						}

						while (!GlobalInfo.Exit)
						{
							Dispatcher.Invoke(() => realtimeWb.Refresh());

							do
							{
								Thread.Sleep(refreshRate);
							} while (!GlobalInfo.BotRunning);

							if (postSuccessMessage)
							{
								GlobalInfo.MessagePoster.MessageQueue.Add(new MessageInfo { Body = "`Restart successful!`" }, GlobalInfo.ChatRoomID);

								postSuccessMessage = false;
							}
						}
					}
					catch (Exception)
					{
						if (!GlobalInfo.Exit)
						{
							Thread.Sleep(1000);

							if (i == 4)
							{
								GlobalInfo.MessagePoster.MessageQueue.Add(new MessageInfo { Body = "`Warning: 3 attempts to restart post refresher thread have failed. Now shutting down...`" }, GlobalInfo.ChatRoomID);

								GlobalInfo.Exit = true;
							}
							else
							{
								GlobalInfo.MessagePoster.MessageQueue.Add(new MessageInfo { Body = "`Warning: post refresher thread has died. Attempting to restart...`" }, GlobalInfo.ChatRoomID);
							}
						}
					}

					if (GlobalInfo.Exit) { break; }
				}
			}) { Priority = ThreadPriority.Lowest };

			postRefresherThread.Start();
		}

		private void CommandListener()
		{
			var postSuccessMessage = false;
			var lastChatMessage = new MessageInfo();
			var lastAnnounceMessage = new MessageInfo();

			commandListenerThread = new Thread(() =>
			{
				for (var i = 1; i < 5; i++)
				{
					try
					{
						do
						{
							Thread.Sleep(refreshRate / 2);
						} while (!GlobalInfo.BotRunning);

						if (i != 1)
						{
							postSuccessMessage = true;
						}

						while (!GlobalInfo.Exit)
						{
							Thread.Sleep(333);

							var chatMessage = GetLatestChatMessage();
							var announceMessage = GetLatestAnnounceMessage();

							CheckExecuteCommand(chatMessage, ref lastChatMessage);
							CheckExecuteCommand(announceMessage, ref lastAnnounceMessage);
							
							if (postSuccessMessage)
							{
								GlobalInfo.MessagePoster.MessageQueue.Add(new MessageInfo { Body = "`Restart successful!`" }, GlobalInfo.ChatRoomID);

								postSuccessMessage = false;
							}
						}
					}
					catch (Exception)
					{
						if (!GlobalInfo.Exit)
						{
							Thread.Sleep(3000);

							if (i == 4)
							{
								GlobalInfo.MessagePoster.MessageQueue.Add(new MessageInfo { Body = "`Warning: 3 attempts to restart command listener thread have failed. Now shutting down...`" }, GlobalInfo.ChatRoomID);

								GlobalInfo.Exit = true;
							}
							else
							{
								GlobalInfo.MessagePoster.MessageQueue.Add(new MessageInfo { Body = "`Warning: command listener thread has died. Attempting to restart...`" }, GlobalInfo.ChatRoomID);
							}
						}
					}

					if (GlobalInfo.Exit) { break; }		
				}
			}) { Priority = ThreadPriority.Lowest };

			commandListenerThread.Start();
		}

		private void KillListener()
		{
			var lastChatMessage = new MessageInfo();

			new Thread(() =>
			{
				do
				{
					Thread.Sleep(refreshRate / 2);
				} while (!GlobalInfo.BotRunning);

				while (!GlobalInfo.Exit)
				{
					Thread.Sleep(333);

					var chatMessage = GetLatestChatMessage();

					if (chatMessage.MessageID == lastChatMessage.MessageID || chatMessage.Body != ">>kill-it-with-no-regrets-for-sure")
					{
						lastChatMessage = chatMessage;

						continue;
					}

					if (UserAccess.Owners.Contains(chatMessage.AuthorID))
					{
						GlobalInfo.MessagePoster.MessageQueue.Add(new MessageInfo { Body = "`Killing...`" }, GlobalInfo.ChatRoomID);

						GlobalInfo.Exit = true;
						die = true;
						requestedDieTime = DateTime.UtcNow;
					}
					else
					{
						lastChatMessage = new MessageInfo { MessageID = chatMessage.MessageID };

						GlobalInfo.MessagePoster.MessageQueue.Add(new MessageInfo { Body = "`Access denied.`" }, GlobalInfo.ChatRoomID);
					}
				}
			}) { Priority = ThreadPriority.Lowest }.Start();		
		}

		private void BotKiller()
		{
			var postCatcherMessagePosted = false;
			var postRefresherMessagePosted = false;
			var commandListenerMessagePosted = false;

			new Thread(() =>
			{
				while (true)
				{
					Thread.Sleep(1000);

					if (!die) { continue; }

					if (!commandListenerThread.IsAlive && !postRefresherThread.IsAlive && !postCatcherThread.IsAlive && !die) { return; }

					if (!postCatcherMessagePosted)
					{			
						if (!postCatcherThread.IsAlive)
						{
							GlobalInfo.MessagePoster.MessageQueue.Add(new MessageInfo { Body = "`Post catcher thread has died...`" }, GlobalInfo.ChatRoomID);

							postCatcherMessagePosted = true;
						}
						else if ((DateTime.UtcNow - requestedDieTime).TotalMilliseconds > 5000)
						{
							GlobalInfo.MessagePoster.MessageQueue.Add(new MessageInfo { Body = "`Warning: 5 seconds have pasted since kill command was issued and post catcher thread is still alive. Now forcing thread death...`" }, GlobalInfo.ChatRoomID);

							postCatcherThread.Abort();
						}
					}

					if (!postRefresherMessagePosted)
					{
						if (!postRefresherThread.IsAlive)
						{
							GlobalInfo.MessagePoster.MessageQueue.Add(new MessageInfo { Body = "`Post refresher thread has died...`" }, GlobalInfo.ChatRoomID);

							postRefresherMessagePosted = true;
						}
						else if ((DateTime.UtcNow - requestedDieTime).TotalMilliseconds > 5000)
						{
							GlobalInfo.MessagePoster.MessageQueue.Add(new MessageInfo { Body = "`Warning: 5 seconds have pasted since kill command was issued and post refresher thread is still alive. Now forcing thread death...`" }, GlobalInfo.ChatRoomID);

							postRefresherThread.Abort();
						}
					}

					if (!commandListenerMessagePosted)
					{
						if (!commandListenerThread.IsAlive)
						{
							GlobalInfo.MessagePoster.MessageQueue.Add(new MessageInfo { Body = "`Command listener thread has died...`" }, GlobalInfo.ChatRoomID);

							commandListenerMessagePosted = true;
						}
						else if ((DateTime.UtcNow - requestedDieTime).TotalMilliseconds > 5000)
						{
							GlobalInfo.MessagePoster.MessageQueue.Add(new MessageInfo { Body = "`Warning: 5 seconds have pasted since kill command was issued and command listener thread is still alive. Now forcing thread death...`" }, GlobalInfo.ChatRoomID);

							commandListenerThread.Abort();
						}
					}

					if (!commandListenerThread.IsAlive && !postRefresherThread.IsAlive && !postCatcherThread.IsAlive && die)
					{
						while (GlobalInfo.MessagePoster.MessageQueue.Count != 0) { Thread.Sleep(500); }

						GlobalInfo.MessagePoster.MessageQueue.Add(new MessageInfo { Body = "`All threads have died. Kill successful!`" }, GlobalInfo.ChatRoomID);

						Dispatcher.Invoke(() => Application.Current.Shutdown());

						return;
					}
				}
			}) { Priority = ThreadPriority.Lowest }.Start();	
		}

		private MessageInfo GetLatestChatMessage()
		{
			dynamic doc = null;
			string html;

			Dispatcher.Invoke(() => doc = chatWb.Document);

			try
			{
				html = doc.documentElement.InnerHtml;
			}
			catch (Exception)
			{
				return new MessageInfo();
			}

			if (html.Replace("\"", "").IndexOf("class=username", StringComparison.Ordinal) == -1) { return new MessageInfo(); }

			var data = HTMLScraper.GetLastChatMessage(html);

			data.RoomID = GlobalInfo.ChatRoomID;

			return data;
		}

		private MessageInfo GetLatestAnnounceMessage()
		{
			dynamic doc = null;
			string html;

			Dispatcher.Invoke(() => doc = announceWb.Document);

			try
			{
				html = doc.documentElement.InnerHtml;
			}
			catch (Exception)
			{
				return new MessageInfo();
			}

			if (html.Replace("\"", "").IndexOf("class=username", StringComparison.Ordinal) == -1) { return new MessageInfo(); }

			var data = HTMLScraper.GetLastChatMessage(html);

			data.RoomID = GlobalInfo.AnnouncerRoomID;

			return data;
		}

		private void CheckExecuteCommand(MessageInfo message, ref MessageInfo previousMessage)
		{
			var messageLower = message.Body.ToLowerInvariant();

			if (message.MessageID == previousMessage.MessageID || messageLower == ">>kill-it-with-no-regrets-for-sure" || (!CommandProcessor.IsValidCommand(messageLower) && message.RoomID == GlobalInfo.AnnouncerRoomID))
			{
				previousMessage = new MessageInfo { MessageID = message.MessageID };

				return;
			}

			var commandMessage = CommandProcessor.ExacuteCommand(message);

			if (commandMessage != "")
			{
				GlobalInfo.MessagePoster.MessageQueue.Add(new MessageInfo { Body = commandMessage }, message.RoomID);
			}

			previousMessage = new MessageInfo { MessageID = message.MessageID };
		}

		private Question[] GetAllPosts(string html)
		{
			var posts = new List<Question>();

			html = html.Substring(html.IndexOf("<BR class=cbo>", StringComparison.Ordinal));

			while (html.Length > 10000)
			{
				var postURL = HTMLScraper.GetURL(html);

				var post = new Question
				{
					URL = postURL,
					Title = HTMLScraper.GetTitle(html),
					AuthorLink = HTMLScraper.GetAuthorLink(html),
					AuthorName = HTMLScraper.GetAuthorName(html),
					Site = HTMLScraper.GetSite(postURL),
					Tags = HTMLScraper.GetTags(html)
				};

				if (!previouslyFoundPosts.Contains(post.URL))
				{
					posts.Add(post);
					previouslyFoundPosts.Add(post.URL);
				}

				if (previouslyFoundPosts.Count > 500)
				{
					previouslyFoundPosts.RemoveAt(29);
				}

				var startIndex = html.IndexOf("question-container realtime-question", 100, StringComparison.Ordinal);

				html = html.Substring(startIndex);
			}

			return posts.ToArray();
		}

		private void CheckPosts(IEnumerable<Question> questions)
		{
			foreach (var q in questions)
			{
				var info = PostAnalyser.CheckPost(q);

				if (PostPersistence.Messages.All(pp => pp.URL != q.URL))
				{
					PostReport(q, MessageGenerator.GetQReport(info.QResults, q), info.QResults);
				}
				
				foreach (var a in info.AResults.Where(p => PostPersistence.Messages.All(pp => pp.URL != p.Key.URL)))
				{
					PostReport(a.Key, MessageGenerator.GetAReport(a.Value, a.Key), a.Value, false);
				}
			}	
		}

		private void PostReport(Post p, string message, PostAnalysis info, bool isQuestionReport = true)
		{
			if (info.Accuracy <= GlobalInfo.AccuracyThreshold) { return; }

			if (SpamAbuseDetected(p))
			{
				GlobalInfo.MessagePoster.MessageQueue.Add(new MessageInfo { Body = "[Spammer abuse](" + p.URL + ")." }, GlobalInfo.ChatRoomID);
				PostPersistence.AddPost(p);

				return;
			}

			switch (info.Type)
			{
				case PostType.Offensive:
				{
					GlobalInfo.MessagePoster.MessageQueue.Add(new MessageInfo { Body = "**Offensive**" + message, Post = p, Report = info, IsQuestionReport = isQuestionReport }, GlobalInfo.ChatRoomID);
					PostPersistence.AddPost(p);

					break;
				}

				case PostType.BadUsername:
				{
					GlobalInfo.MessagePoster.MessageQueue.Add(new MessageInfo { Body = "**Bad Username**" + message, Post = p, Report = info, IsQuestionReport = isQuestionReport }, GlobalInfo.ChatRoomID);
					PostPersistence.AddPost(p);

					break;
				}

				case PostType.BadTagUsed:
				{
					GlobalInfo.MessagePoster.MessageQueue.Add(new MessageInfo { Body = "**Bad Tag Used**" + message, Post = p, Report = info, IsQuestionReport = isQuestionReport }, GlobalInfo.ChatRoomID);
					PostPersistence.AddPost(p);

					break;
				}

				case PostType.LowQuality:
				{
					GlobalInfo.MessagePoster.MessageQueue.Add(new MessageInfo { Body = "**Low Quality**" + message, Post = p, Report = info, IsQuestionReport = isQuestionReport }, GlobalInfo.ChatRoomID);
					PostPersistence.AddPost(p);

					break;
				}

				case PostType.Spam:
				{
					GlobalInfo.MessagePoster.MessageQueue.Add(new MessageInfo { Body = "**Spam**" + message, Post = p, Report = info, IsQuestionReport = isQuestionReport }, GlobalInfo.ChatRoomID);
					PostPersistence.AddPost(p);

					break;
				}
			}
		}

		private bool SpamAbuseDetected(Post post)
		{
			if (PostPersistence.Messages[0].AuthorName != null && IsDefaultUsername(post.AuthorName) && IsDefaultUsername(PostPersistence.Messages[0].AuthorName))
			{
				var latestUsernameId = int.Parse(post.AuthorName.Remove(0, 4));
				var lastMessageUsernameId = int.Parse(PostPersistence.Messages[0].AuthorName.Remove(0, 4));

				if ((latestUsernameId > lastMessageUsernameId + 8 && latestUsernameId < lastMessageUsernameId) || spammers.Contains(latestUsernameId))
				{
					spammers.Add(latestUsernameId);

					return true;
				}
			}

			return false;
		}

		private bool IsDefaultUsername(string username)
		{
			return username.StartsWith("user") && username.Remove(0, 4).All(Char.IsDigit);
		}


		private void HideScriptErrors(WebBrowser wb, bool hide)
		{
			var fiComWebBrowser = typeof(WebBrowser).GetField("_axIWebBrowser2", BindingFlags.Instance | BindingFlags.NonPublic);
			if (fiComWebBrowser == null) return;
			var objComWebBrowser = fiComWebBrowser.GetValue(wb);
			if (objComWebBrowser == null)
			{
				wb.Loaded += (o, s) => HideScriptErrors(wb, hide);
				return;
			}
			objComWebBrowser.GetType().InvokeMember("Silent", BindingFlags.SetProperty, null, objComWebBrowser, new object[] { hide });
		}

		private void SwitchToIE9()
		{
			const string installkey = @"SOFTWARE\Microsoft\Internet Explorer\Main\FeatureControl\FEATURE_BROWSER_EMULATION";
			const string entryLabel = "Phamhilator.exe";
			var osInfo = Environment.OSVersion;

			var version = osInfo.Version.Major.ToString(CultureInfo.InvariantCulture) + '.' + osInfo.Version.Minor;
			var editFlag = (uint)((version == "6.2") ? 0x2710 : 0x2328); // 6.2 = Windows 8 and therefore IE10

			var existingSubKey = Registry.LocalMachine.OpenSubKey(installkey, false); // readonly key

			if (existingSubKey.GetValue(entryLabel) == null)
			{
				existingSubKey = Registry.LocalMachine.OpenSubKey(installkey, true); // writable key
				existingSubKey.SetValue(entryLabel, unchecked((int)editFlag), RegistryValueKind.DWord);
			}
		}



		// ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ UI Events ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ 



		private void Button_Click_2(object sender, RoutedEventArgs e)
		{
			var b = ((Button)sender);

			if (!b.IsEnabled) { return; }

			GlobalInfo.BotRunning = true;

			b.IsEnabled = false;
				
			if (Debugger.IsAttached)
			{
				GlobalInfo.MessagePoster.MessageQueue.Add(new MessageInfo { Body = "`Phamhilator™ started (debug mode).`" }, GlobalInfo.ChatRoomID);
			}
			else
			{
				GlobalInfo.MessagePoster.MessageQueue.Add(new MessageInfo { Body = "`Phamhilator™ started.`" }, GlobalInfo.ChatRoomID);
			}

			GlobalInfo.UpTime = DateTime.UtcNow;
		}

		private void MetroWindow_Closing(object sender, CancelEventArgs e)
		{
			if (GlobalInfo.ChatRoomID == 0) { return; }

			e.Cancel = true;

			GlobalInfo.MessagePoster.MessageQueue.Add(new MessageInfo { Body = "`Phamhilator™ stopped.`" }, GlobalInfo.ChatRoomID);

			Thread.Sleep(5000);

			GlobalInfo.Exit = true;

			Task.Factory.StartNew(() =>
			{
				Thread.Sleep(5000);

				try
				{
					Dispatcher.Invoke(() => Environment.Exit(0));
				}
				catch (Exception)
				{
					
				}
			});
		}

		private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			var rate = (int)Math.Round(refreshRateS.Minimum + (refreshRateS.Maximum - e.NewValue), 0);

			refreshRate = rate;

			if (refreshRateL != null)
			{
				refreshRateL.Content = rate + " milliseconds";
			}
		}
	}
}
