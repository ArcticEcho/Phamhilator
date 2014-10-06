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



namespace Phamhilator
{
	public partial class MainWindow
	{
		private bool die;
		private bool exit;
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

			SwitchToIE9();

			HideScriptErrors(realtimeWb, true);
			HideScriptErrors(chatWb, true);

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

						while (!exit)
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
								GlobalInfo.MessagePoster.MessageQueue.Add(new MessageInfo { Body = "`Restart successful!`" });

								postSuccessMessage = false;
							}
						}
					}
					catch (Exception)
					{
						if (!exit)
						{
							Thread.Sleep(2000);

							if (i == 4)
							{
								GlobalInfo.MessagePoster.MessageQueue.Add(new MessageInfo { Body = "`Warning: 3 attempts to restart post catcher thread have failed. Now shutting down...`" });

								exit = true;
							}
							else
							{
								GlobalInfo.MessagePoster.MessageQueue.Add(new MessageInfo { Body = "`Warning: post catcher thread has died. Attempting to restart...`" });
							}
						}
					}

					if (exit)
					{
						break;
					}
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

						while (!exit)
						{
							Dispatcher.Invoke(() => realtimeWb.Refresh());

							do
							{
								Thread.Sleep(refreshRate);
							} while (!GlobalInfo.BotRunning);

							if (postSuccessMessage)
							{
								GlobalInfo.MessagePoster.MessageQueue.Add(new MessageInfo { Body = "`Restart successful!`" });

								postSuccessMessage = false;
							}
						}
					}
					catch (Exception)
					{
						if (!exit)
						{
							Thread.Sleep(1000);

							if (i == 4)
							{
								GlobalInfo.MessagePoster.MessageQueue.Add(new MessageInfo { Body = "`Warning: 3 attempts to restart post refresher thread have failed. Now shutting down...`" });

								exit = true;
							}
							else
							{
								GlobalInfo.MessagePoster.MessageQueue.Add(new MessageInfo { Body = "`Warning: post refresher thread has died. Attempting to restart...`" });
							}
						}
					}

					if (exit) { break; }
				}
			}) { Priority = ThreadPriority.Lowest };

			postRefresherThread.Start();
		}

		private void CommandListener()
		{
			var postSuccessMessage = false;
			var lastChatMessage = new MessageInfo();

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

						while (!exit)
						{
							Thread.Sleep(333);

							var message = GetLatestMessage();

							if (message.MessageID == lastChatMessage.MessageID || message.Body == ">>kill-it-with-no-regrets-for-sure" || (!message.Body.StartsWith(">>") && !message.Body.ToLowerInvariant().StartsWith("@" + GlobalInfo.BotUsername.ToLowerInvariant())))
							{
								lastChatMessage = message;

								continue;
							}

							var commandMessage = CommandProcessor.ExacuteCommand(message);

							if (commandMessage != "")
							{
								GlobalInfo.MessagePoster.MessageQueue.Add(new MessageInfo { Body = commandMessage });
							}

							lastChatMessage = new MessageInfo { MessageID = message.MessageID };
							
							if (postSuccessMessage)
							{
								GlobalInfo.MessagePoster.MessageQueue.Add(new MessageInfo { Body = "`Restart successful!`" });

								postSuccessMessage = false;
							}
						}
					}
					catch (Exception)
					{
						if (!exit)
						{
							Thread.Sleep(3000);

							if (i == 4)
							{
								GlobalInfo.MessagePoster.MessageQueue.Add(new MessageInfo { Body = "`Warning: 3 attempts to restart command listener thread have failed. Now shutting down...`" });

								exit = true;
							}
							else
							{
								GlobalInfo.MessagePoster.MessageQueue.Add(new MessageInfo { Body = "`Warning: command listener thread has died. Attempting to restart...`" });
							}
						}
					}

					if (exit) { break; }		
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

				while (!exit)
				{
					Thread.Sleep(333);

					var message = GetLatestMessage();

					if (message.MessageID == lastChatMessage.MessageID || message.Body != ">>kill-it-with-no-regrets-for-sure")
					{
						lastChatMessage = message;

						continue;
					}

					if (UserAccess.Owners.Contains(message.AuthorID))
					{
						GlobalInfo.MessagePoster.MessageQueue.Add(new MessageInfo { Body = "`Killing...`" });

						exit = true;
						die = true;
						requestedDieTime = DateTime.UtcNow;
					}
					else
					{
						lastChatMessage = new MessageInfo { MessageID = message.MessageID };

						GlobalInfo.MessagePoster.MessageQueue.Add(new MessageInfo { Body = "`Access denied.`" });
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

					if (!die)
					{
						continue;
					}

					if (!commandListenerThread.IsAlive && !postRefresherThread.IsAlive && !postCatcherThread.IsAlive && !die)
					{
						return;
					}

					if (!postCatcherMessagePosted)
					{			
						if (!postCatcherThread.IsAlive)
						{
							GlobalInfo.MessagePoster.MessageQueue.Add(new MessageInfo { Body = "`Post catcher thread has died...`" });

							postCatcherMessagePosted = true;
						}
						else if ((DateTime.UtcNow - requestedDieTime).TotalMilliseconds > 5000)
						{
							GlobalInfo.MessagePoster.MessageQueue.Add(new MessageInfo { Body = "`Warning: 5 seconds have pasted since kill command was issued and post catcher thread is still alive. Now forcing thread death...`" });

							postCatcherThread.Abort();
						}
					}

					if (!postRefresherMessagePosted)
					{
						if (!postRefresherThread.IsAlive)
						{
							GlobalInfo.MessagePoster.MessageQueue.Add(new MessageInfo { Body = "`Post refresher thread has died...`" });

							postRefresherMessagePosted = true;
						}
						else if ((DateTime.UtcNow - requestedDieTime).TotalMilliseconds > 5000)
						{
							GlobalInfo.MessagePoster.MessageQueue.Add(new MessageInfo { Body = "`Warning: 5 seconds have pasted since kill command was issued and post refresher thread is still alive. Now forcing thread death...`" });

							postRefresherThread.Abort();
						}
					}

					if (!commandListenerMessagePosted)
					{
						if (!commandListenerThread.IsAlive)
						{
							GlobalInfo.MessagePoster.MessageQueue.Add(new MessageInfo { Body = "`Command listener thread has died...`" });

							commandListenerMessagePosted = true;
						}
						else if ((DateTime.UtcNow - requestedDieTime).TotalMilliseconds > 5000)
						{
							GlobalInfo.MessagePoster.MessageQueue.Add(new MessageInfo { Body = "`Warning: 5 seconds have pasted since kill command was issued and command listener thread is still alive. Now forcing thread death...`" });

							commandListenerThread.Abort();
						}
					}

					if (!commandListenerThread.IsAlive && !postRefresherThread.IsAlive && !postCatcherThread.IsAlive && die)
					{
						GlobalInfo.MessagePoster.MessageQueue.Add(new MessageInfo { Body = "`All threads have died. Kill successful!`" });

						Dispatcher.Invoke(() => Application.Current.Shutdown());

						return;
					}
				}
			}) { Priority = ThreadPriority.Lowest }.Start();	
		}

		private MessageInfo GetLatestMessage()
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

			if (html.IndexOf("username owner", StringComparison.Ordinal) == -1) { return new MessageInfo(); }

			return HTMLScraper.GetLastChatMessage(html);
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
				GlobalInfo.MessagePoster.MessageQueue.Add(new MessageInfo { Body = "[Spammer abuse](" + p.URL + ")." });
				PostPersistence.AddPost(p);

				return;
			}

			switch (info.Type)
			{
				case PostType.Offensive:
				{
					GlobalInfo.MessagePoster.MessageQueue.Add(new MessageInfo { Body = "**Offensive**" + message, Post = p, Report = info, IsQuestionReport = isQuestionReport });
					PostPersistence.AddPost(p);

					break;
				}

				case PostType.BadUsername:
				{
					GlobalInfo.MessagePoster.MessageQueue.Add(new MessageInfo { Body = "**Bad Username**" + message, Post = p, Report = info, IsQuestionReport = isQuestionReport });
					PostPersistence.AddPost(p);

					break;
				}

				case PostType.BadTagUsed:
				{
					GlobalInfo.MessagePoster.MessageQueue.Add(new MessageInfo { Body = "**Bad Tag Used**" + message });
					PostPersistence.AddPost(p);

					break;
				}

				case PostType.LowQuality:
				{
					GlobalInfo.MessagePoster.MessageQueue.Add(new MessageInfo { Body = "**Low Quality**" + message, Post = p, Report = info, IsQuestionReport = isQuestionReport });
					PostPersistence.AddPost(p);

					break;
				}

				case PostType.Spam:
				{
					GlobalInfo.MessagePoster.MessageQueue.Add(new MessageInfo { Body = "**Spam**" + message, Post = p, Report = info, IsQuestionReport = isQuestionReport });
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



		private void Button_Click(object sender, RoutedEventArgs e)
		{
			chatWb.Source = new Uri("http://chat.meta.stackexchange.com/rooms/773/room-for-low-quality-posts");
		}

		private void Button_Click_1(object sender, RoutedEventArgs e)
		{
			chatWb.Source = new Uri("http://chat.meta.stackexchange.com/rooms/651/sandbox");
		}

		private void Button_Click_2(object sender, RoutedEventArgs e)
		{
			var b = ((Button)sender);

			if (!b.IsEnabled) { return; }

			GlobalInfo.BotRunning = true;

			b.IsEnabled = false;
				
			if (Debugger.IsAttached)
			{
				GlobalInfo.MessagePoster.MessageQueue.Add(new MessageInfo { Body = "`Phamhilator™ started (debug mode).`" });
			}
			else
			{
				GlobalInfo.MessagePoster.MessageQueue.Add(new MessageInfo { Body = "`Phamhilator™ started.`" });
			}

			GlobalInfo.UpTime = DateTime.UtcNow;
		}

		private void MetroWindow_Closing(object sender, CancelEventArgs e)
		{
			if (GlobalInfo.RoomID == 0) { return; }

			e.Cancel = true;

			GlobalInfo.MessagePoster.MessageQueue.Add(new MessageInfo { Body = "`Phamhilator™ stopped.`" });

			exit = true;

			Task.Factory.StartNew(() =>
			{
				Thread.Sleep(5000);

				try
				{
					Dispatcher.Invoke(() => Application.Current.Shutdown());
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
