using System;
using System.Linq;
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
using System.Security;



namespace Phamhilator
{
	public partial class MainWindow
	{
		private int refreshRate = 10000; // In milliseconds.
		private readonly HashSet<int> spammers = new HashSet<int>();
		private static readonly List<string> previouslyFoundPosts = new List<string>();
		private Thread commandListenerThread;
		private Thread postCatcherThread;
		private DateTime requestedDieTime;



		public MainWindow()
		{
			InitializeComponent();

			GlobalInfo.ChatWb = chatWb;
			GlobalInfo.AnnounceWb = announceWb;

			SwitchToIE9();

			HideScriptErrors(chatWb, true);
			HideScriptErrors(announceWb, true);

			KillListener();

			PostCatcher();

			CommandListener();
		}



		private void PostCatcher()
		{
			var postSuccessMessage = false;
			string html;

			postCatcherThread = new Thread(() =>
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
							try
							{
								html = StringDownloader.DownloadString("http://stackexchange.com/questions?tab=realtime");
							}
							catch (Exception)
							{
								return;
							}

							if (html.IndexOf("hot-question-site-icon", StringComparison.Ordinal) == -1)
							{
								return;
							}

							var posts = GetAllPosts(html);

							if (posts.Length != 0)
							{
								Task.Factory.StartNew(() => CheckPosts(posts));
							}

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
							Thread.Sleep(2000);

							if (i == 4)
							{
								GlobalInfo.MessagePoster.MessageQueue.Add(new MessageInfo { Body = "`Warning: 3 attempts to restart the post catcher thread have failed. Now shutting down...`" }, GlobalInfo.ChatRoomID);

								GlobalInfo.Exit = true;
							}
							else
							{
								GlobalInfo.MessagePoster.MessageQueue.Add(new MessageInfo { Body = "`Warning: post catcher thread has died. Attempting to restart...`" }, GlobalInfo.ChatRoomID);
							}
						}
					}

					if (GlobalInfo.Exit) { return; }
				}
			}) { Priority = ThreadPriority.Lowest };

			postCatcherThread.Start();
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
							Thread.Sleep(500);
						} while (!GlobalInfo.BotRunning);

						if (i != 1)
						{
							postSuccessMessage = true;
						}

						while (!GlobalInfo.Exit)
						{
							Thread.Sleep(300);

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
								GlobalInfo.MessagePoster.MessageQueue.Add(new MessageInfo { Body = "`Warning: 3 attempts to the restart command listener thread have failed. Now shutting down...`" }, GlobalInfo.ChatRoomID);

								GlobalInfo.Exit = true;
							}
							else
							{
								GlobalInfo.MessagePoster.MessageQueue.Add(new MessageInfo { Body = "`Warning: command listener thread has died. Attempting to restart...`" }, GlobalInfo.ChatRoomID);
							}
						}
					}

					if (GlobalInfo.Exit) { return; }		
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
					Thread.Sleep(500);

					var chatMessage = GetLatestChatMessage();

					if (chatMessage.MessageID == lastChatMessage.MessageID || chatMessage.Body != ">>kill-it-with-no-regrets-for-sure")
					{
						lastChatMessage = chatMessage;

						continue;
					}

					if (UserAccess.Owners.Contains(chatMessage.AuthorID))
					{
						GlobalInfo.MessagePoster.MessageQueue.Add(new MessageInfo { Body = "`Killing...`" }, GlobalInfo.ChatRoomID);
						
						KillBot();
					}
					else
					{
						lastChatMessage = new MessageInfo { MessageID = chatMessage.MessageID };

						GlobalInfo.MessagePoster.MessageQueue.Add(new MessageInfo { Body = "`Access denied.`" }, GlobalInfo.ChatRoomID);
					}
				}
			}) { Priority = ThreadPriority.Lowest }.Start();		
		}

		private void KillBot()
		{
			GlobalInfo.Exit = true;
			requestedDieTime = DateTime.UtcNow;

			var postCatcherMessagePosted = false;
			var postCatcherWarningMessagePosted = false;

			var commandListenerMessagePosted = false;
			var commandListenerWarningMessagePosted = false;

			while (true)
			{
				KillPostCatcher(ref postCatcherMessagePosted, ref postCatcherWarningMessagePosted);

				KillCommandListener(ref commandListenerMessagePosted, ref commandListenerWarningMessagePosted);

				if (!commandListenerThread.IsAlive && !postCatcherThread.IsAlive) { break; }
			}

			while (GlobalInfo.MessagePoster.MessageQueue.Count != 0) { Thread.Sleep(5000); }

			GlobalInfo.MessagePoster.MessageQueue.Add(new MessageInfo { Body = "`All threads have died. Kill successful!`" }, GlobalInfo.ChatRoomID);

			while (GlobalInfo.MessagePoster.MessageQueue.Count != 0) { Thread.Sleep(5000); }

			Thread.Sleep(5000);

			GlobalInfo.MessagePoster.Shutdown();

			Dispatcher.Invoke(() => Environment.Exit(0));
		}

		private void KillPostCatcher(ref bool postCatcherMessagePosted, ref bool postCatcherWarningMessagePosted)
		{
			if (postCatcherMessagePosted) { return; }

			if (!postCatcherThread.IsAlive)
			{
				GlobalInfo.MessagePoster.MessageQueue.Add(new MessageInfo { Body = "`Post catcher thread has died...`" }, GlobalInfo.ChatRoomID);

				postCatcherMessagePosted = true;
			}
			else if ((DateTime.UtcNow - requestedDieTime).TotalSeconds > 10 && !postCatcherWarningMessagePosted)
			{
				GlobalInfo.MessagePoster.MessageQueue.Add(new MessageInfo { Body = "`Warning: 10 seconds have past since the kill command was issued and post catcher thread is still alive. Now forcing thread death...`" }, GlobalInfo.ChatRoomID);

				postCatcherThread.Abort();
			}
		}

		private void KillCommandListener(ref bool commandListenerMessagePosted, ref bool commandListenerWarningMessagePosted)
		{
			if (commandListenerMessagePosted) { return; }

			if (!commandListenerThread.IsAlive)
			{
				GlobalInfo.MessagePoster.MessageQueue.Add(new MessageInfo { Body = "`Command listener thread has died...`" }, GlobalInfo.ChatRoomID);

				commandListenerMessagePosted = true;
			}
			else if ((DateTime.UtcNow - requestedDieTime).TotalSeconds > 10 && !commandListenerWarningMessagePosted)
			{
				GlobalInfo.MessagePoster.MessageQueue.Add(new MessageInfo { Body = "`Warning: 10 seconds have past since the kill command was issued and command listener thread is still alive. Now forcing thread death...`" }, GlobalInfo.ChatRoomID);

				commandListenerThread.Abort();
			}
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

			var commandMessages = CommandProcessor.ExacuteCommand(message);

			foreach (var commandMessage in commandMessages.Where(m => !String.IsNullOrEmpty(m)))
			{
				GlobalInfo.MessagePoster.MessageQueue.Add(new MessageInfo { Body = commandMessage }, message.RoomID);
			}

			previousMessage = new MessageInfo { MessageID = message.MessageID };
		}

		private Question[] GetAllPosts(string html)
		{
			var posts = new List<Question>();

			html = html.Substring(html.IndexOf("<br class=\"cbo\" />", StringComparison.Ordinal));

			while (html.Length > 10000)
			{
				var postURL = HTMLScraper.GetQuestionURL(html);

				var post = new Question
				{
					URL = postURL,
					Title = HTMLScraper.GetQuestionTitle(html),
					AuthorLink = HTMLScraper.GetQuestionAuthorLink(html),
					AuthorName = HTMLScraper.GetQuestionAuthorName(html),
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

			if (GlobalInfo.FullScanEnabled)
			{
				Parallel.ForEach(posts, post => post.PopulateExtraData());
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

			if (fiComWebBrowser == null) { return; }

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

			try
			{
				var existingSubKey = Registry.LocalMachine.OpenSubKey(installkey, false); // readonly key

				if (existingSubKey.GetValue(entryLabel) == null)
				{
					existingSubKey = Registry.LocalMachine.OpenSubKey(installkey, true); // writable key
					existingSubKey.SetValue(entryLabel, unchecked((int)editFlag), RegistryValueKind.DWord);
				}
			}
			catch (SecurityException ex)
			{
				MessageBox.Show(string.Format("Can't write to the registry. Try to run using administrative privileges.\n\nException: {0}", ex.Message), "Phamilator");
			}
		}

		# region UI Events

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
			e.Cancel = true;

			if (!GlobalInfo.BotRunning) { Environment.Exit(0); }

			GlobalInfo.MessagePoster.MessageQueue.Add(new MessageInfo { Body = "`Phamhilator™ stopped.`" }, GlobalInfo.ChatRoomID);

			GlobalInfo.Exit = true;

			Task.Factory.StartNew(() =>
			{
				while (GlobalInfo.MessagePoster.MessageQueue.Count != 0)
				{
					Thread.Sleep(1000);
				}

				GlobalInfo.MessagePoster.Shutdown();

				Dispatcher.Invoke(() => Environment.Exit(0));
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

		# endregion
	}
}
