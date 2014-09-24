using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;



namespace Phamhilator
{
	public partial class MainWindow
	{
		private int roomId;
		private bool exit;
		private bool firstStart = true;
		private int refreshRate = 10000; // In milliseconds.
		private readonly HashSet<int> spammers = new HashSet<int>();
		private MessageInfo lastCommand = new MessageInfo();
		private static readonly List<string> previouslyFoundPosts = new List<string>();



		public MainWindow()
		{
			InitializeComponent();

			SwitchToIE9();

			HideScriptErrors(realtimeWb, true);
			HideScriptErrors(chatWb, true);

			PostRefresher();

			PostCatcher();

			CommandListener();
		}



		private void PostCatcher()
		{
			var postSuccessMessage = false;
			dynamic doc = null;
			string html;

			new Thread(() =>
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

							if (html.IndexOf("DIV class=metaInfo", StringComparison.Ordinal) == -1) { continue; }

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
								PostMessage("`Restart successful!`");

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
								PostMessage("`Warning: 3 attempts to restart the post catcher thread have failed. Now shutting down...`");

								exit = true;
							}
							else
							{
								PostMessage("`Warning: post catcher thread has died. Attempting to restart...`");
							}
						}
					}

					if (exit) { return; }
				}		
			}) { Priority = ThreadPriority.Lowest }.Start();
		}

		private void PostRefresher()
		{
			var postSuccessMessage = false;

			new Thread(() =>
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
								PostMessage("`Restart successful!`");

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
								PostMessage("`Warning: 3 attempts to restart the post refresher thread have failed. Now shutting down...`");

								exit = true;
							}
							else
							{
								PostMessage("`Warning: post refresher thread has died. Attempting to restart...`");
							}
						}
					}

					if (exit) { return; }
				}
			}) { Priority = ThreadPriority.Lowest }.Start();
		}

		private void CommandListener()
		{
			var postSuccessMessage = false;
			dynamic doc = null;
			string html;

			new Thread(() =>
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

							Dispatcher.Invoke(() => doc = chatWb.Document);

							try
							{
								html = doc.documentElement.InnerHtml;
							}
							catch (Exception)
							{
								continue;
							}

							if (html.IndexOf("username owner", StringComparison.Ordinal) == -1) { continue; }

							var message = HTMLScraper.GetLastChatMessage(html);

							if (message.MessageID == lastCommand.MessageID || (!message.Body.StartsWith(">>") && !message.Body.ToLowerInvariant().StartsWith("@" + GlobalInfo.BotUsername.ToLowerInvariant())))
							{
								lastCommand = message;

								continue;
							}

							var commandMessage = CommandProcessor.ExacuteCommand(message);

							if (commandMessage != "")
							{
								PostMessage(commandMessage);
							}

							lastCommand = new MessageInfo { MessageID = message.MessageID };

							while (!GlobalInfo.BotRunning)
							{
								Thread.Sleep(1000);
							} 
							
							if (postSuccessMessage)
							{
								PostMessage("`Restart successful!`");

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
								PostMessage("`Warning: 3 attempts to restart the command listener thread have failed. Now shutting down...`");

								exit = true;
							}
							else
							{
								PostMessage("`Warning: command listener thread has died. Attempting to restart...`");
							}
						}
					}

					if (exit) { return; }		
				}
			}) { Priority = ThreadPriority.Lowest }.Start();
		}

		private Post[] GetAllPosts(string html)
		{
			var posts = new List<Post>();

			html = html.Substring(html.IndexOf("<BR class=cbo>", StringComparison.Ordinal));

			while (html.Length > 10000)
			{
				var postURL = HTMLScraper.GetURL(html);

				var post = new Post
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

		private void CheckPosts(IEnumerable<Post> posts)
		{
			PostAnalysis info;
			string message;

			foreach (var post in posts.Where(p => PostPersistence.Messages.All(pp => pp.URL != p.URL)))
			{
				info = PostChecker.CheckPost(post);
				message = GetReportMessage(info, post);

				if (info.Accuracy <= GlobalInfo.AccuracyThreshold) { continue; }

				//if (SpamAbuseDetected(post))
				//{
				//	PostMessage("[Spammer abuse](" + post.URL + ").");
				//	PostPersistence.AddPost(post);

				//	return;
				//}

				switch (info.Type)
				{
					case PostType.Offensive:
					{
						PostMessage("**Offensive**" + message, post, info);
						PostPersistence.AddPost(post);

						break;
					}

					case PostType.BadUsername:
					{
						PostMessage("**Bad Username**" + message, post, info);
						PostPersistence.AddPost(post);

						break;
					}

					case PostType.BadTagUsed:
					{
						PostMessage("**Bad Tag Used**" + message);
						PostPersistence.AddPost(post);

						break;
					}

					case PostType.LowQuality:
					{
						PostMessage("**Low Quality**" + message, post, info);
						PostPersistence.AddPost(post);

						break;
					}

					case PostType.Spam:
					{
						PostMessage("**Spam**" + message, post, info);
						PostPersistence.AddPost(post);

						break;
					}
				}
			}
		}

		private string GetReportMessage(PostAnalysis info, Post post)
		{
			if (info.Type == PostType.BadTagUsed)
			{
				return ": " + FormatTags(info.BadTags) + "| [" + post.Title + "](" + post.URL + "), by [" + post.AuthorName + "](" + post.AuthorLink + "), on `" + post.Site + "`.";
			}

			return " (" + Math.Round(info.Accuracy, 1) + "%)" + ": [" + post.Title + "](" + post.URL + "), by [" + post.AuthorName + "](" + post.AuthorLink + "), on `" + post.Site + "`.";
		}

		private void PostMessage(string message, Post post = null, PostAnalysis report = null/* int consecutiveMessageCount = 0*/)
		{
			if (roomId == 0)
			{
				GetRoomId();
			}

			var error = false;
			
			Dispatcher.Invoke(() =>
			{
				try
				{
					chatWb.InvokeScript("eval", new object[]
					{
						"$.post('/chats/" + roomId + "/messages/new', { text: '" + message + "', fkey: fkey().fkey });"
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

				Dispatcher.Invoke(() => doc = chatWb.Document);

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
				GlobalInfo.PostedReports.Add(id, new MessageInfo{ Post = post, Report = report });
			}		

			//consecutiveMessageCount++;

			//var delay = (int)(4.1484 * Math.Log(consecutiveMessageCount) + 1.02242) * 1000;

			//if (consecutiveMessageCount >= 20) { return; }

			//Thread.Sleep(delay);

			//PostMessage(message, consecutiveMessageCount);
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

		private void GetRoomId()
		{
			Dispatcher.Invoke(() =>
			{
				try
				{
					var startIndex = chatWb.Source.AbsolutePath.IndexOf("rooms/", StringComparison.Ordinal) + 6;
					var endIndex = chatWb.Source.AbsolutePath.IndexOf("/", startIndex + 1, StringComparison.Ordinal);

					var id = chatWb.Source.AbsolutePath.Substring(startIndex, endIndex - startIndex);

					if (!id.All(Char.IsDigit)) { return; }

					roomId = int.Parse(id);
				}
				catch (Exception)
				{

				}			
			});
		}

		private string FormatTags(Dictionary<string, string> tags)
		{
			var result = new StringBuilder();

			foreach (var tag in tags)
			{
				if (tag.Value == "")
				{
					result.Append("`[" + tag.Key + "]` ");
				}
				else
				{
					result.Append("[`[" + tag.Key + "]`](" + tag.Value + ") ");
				}
			}

			return result.ToString();
		}

		private bool SpamAbuseDetected(Post post)
		{
			if (PostPersistence.Messages[0].AuthorName != null && IsDefaultUsername(post.AuthorName) && IsDefaultUsername(PostPersistence.Messages[0].AuthorName))
			{
				var username0Id = int.Parse(post.AuthorName.Remove(0, 4));
				var username1Id = int.Parse(PostPersistence.Messages[0].AuthorName.Remove(0, 4));

				if ((username0Id > username1Id + 5 && username0Id < username1Id) || spammers.Contains(username0Id))
				{
					spammers.Add(username0Id);

					return true;
				}
			}

			return false;
		}

		private bool IsDefaultUsername(string username)
		{
			return username.Contains("user") && username.Remove(0, 4).All(Char.IsDigit);
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

			GetRoomId();
		}

		private void Button_Click_1(object sender, RoutedEventArgs e)
		{
			chatWb.Source = new Uri("http://chat.meta.stackexchange.com/rooms/651/sandbox");

			GetRoomId();
		}

		private void Button_Click_2(object sender, RoutedEventArgs e)
		{
			var b = ((Button)sender);

			if ((string)b.Content == "Start Monitoring")
			{
				GlobalInfo.BotRunning = true;

				b.Content = "Pause Monitoring";

				if (firstStart)
				{
					if (Debugger.IsAttached)
					{
						PostMessage("`Phamhilator™ started (debug mode).`");
					}
					else
					{
						PostMessage("`Phamhilator™ started.`");
					}

					firstStart = false;
					GlobalInfo.UpTime = DateTime.UtcNow;
				}
				else
				{
					PostMessage("`Phamhilator™ started.`");
				}	
			}
			else
			{
				GlobalInfo.BotRunning = false;

				b.Content = "Start Monitoring";

				PostMessage("`Phamhilator™ paused.`");
			}
		}

		private void MetroWindow_Closing(object sender, CancelEventArgs e)
		{
			if (roomId == 0) { return; }

			e.Cancel = true;

			PostMessage("`Phamhilator™ stopped.`");

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
