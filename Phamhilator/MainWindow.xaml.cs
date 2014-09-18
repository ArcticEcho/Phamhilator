using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
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
		private bool catchBadTag = true;
		private bool firstStart = true;
		private bool catchSpam = true;
		private bool catchOff = true;
		private bool catchLQ = true;
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

			new Thread(() =>
			{
				do
				{
					Thread.Sleep(refreshRate);
				} while (!GlobalInfo.BotRunning);

				while (!exit)
				{
					Dispatcher.Invoke(() => realtimeWb.Refresh());

					do
					{
						Thread.Sleep(refreshRate);
					} while (!GlobalInfo.BotRunning);
				}
			}).Start();

			new Thread(() =>
			{
				do
				{
					Thread.Sleep(refreshRate / 2);
				} while (!GlobalInfo.BotRunning);

				StartListener();

				while (!exit)
				{
					dynamic doc = null;
					string html;

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

					if (posts.Count != 0)
					{
						CheckPosts(posts);
					}

					do
					{
						Thread.Sleep(refreshRate / 2);
					} while (!GlobalInfo.BotRunning);
				}
			}) { Priority = ThreadPriority.Lowest }.Start();
		}



		private void StartListener()
		{
			new Thread(() =>
			{
				while (!GlobalInfo.BotRunning)
				{
					Thread.Sleep(1000);
				}

				while (!exit)
				{
					Thread.Sleep(333);

					dynamic doc = null;
					string html;

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

					if (message.MessageID == lastCommand.MessageID ||
						!GlobalInfo.PostedReports.ContainsKey(message.ReplyMessageID) || 
						(!message.Body.StartsWith(">>") && !message.Body.ToLowerInvariant().StartsWith("@sam")))
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
				}
			}) { Priority = ThreadPriority.Lowest }.Start();
		}

		private List<Post> GetAllPosts(string html)
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

				if (!previouslyFoundPosts.Contains(post.Title))
				{
					posts.Add(post);
					previouslyFoundPosts.Add(post.Title);
				}

				if (previouslyFoundPosts.Count > 500)
				{
					previouslyFoundPosts.RemoveAt(29);
				}

				var startIndex = html.IndexOf("question-container realtime-question", 100, StringComparison.Ordinal);

				html = html.Substring(startIndex);
			}

			return posts;
		}

		private void CheckPosts(IEnumerable<Post> posts)
		{
			//Parallel.ForEach(posts.Where(p => PostPersistence.Messages.All(pp => pp.Title != p.Title)), post =>
			foreach (var post in posts.Where(p => PostPersistence.Messages.All(pp => pp.Title != p.Title)))
			{
				var info = PostChecker.CheckPost(post);
				string message;

				if (post.Score != int.MinValue)
				{
					message = " (" + Math.Round(info.Accuracy, 1) + "%)" + ": " + FormatTags(info.BadTags) + "[" + post.Title + "](" + post.URL + "), score " + post.Score + ", by [" + post.AuthorName + "](" + post.AuthorLink + "), on `" + post.Site + "`.";
				}
				else if (info.Type == PostType.BadTagUsed)
				{
					message = ": " + FormatTags(info.BadTags) + "[" + post.Title + "](" + post.URL + "), by [" + post.AuthorName + "](" + post.AuthorLink + "), on `" + post.Site + "`.";
				} 
				else
				{
					message = " (" + Math.Round(info.Accuracy, 1) + "%)" + ": " + FormatTags(info.BadTags) + "[" + post.Title + "](" + post.URL + "), by [" + post.AuthorName + "](" + post.AuthorLink + "), on `" + post.Site + "`.";
				}

				//if (float.IsNaN(info.Accuracy) && info.Type != PostType.BadTagUsed)
				//{
				//	return;
				//}

				if (SpamAbuseDetected(post))
				{
					PostMessage("[Spammer abuse](" + post.URL + ").");
					PostPersistence.AddPost(post);

					return;
				}

				switch (info.Type)
				{
					case PostType.Offensive:
					{
						if (!catchOff) { break; }

						PostMessage("**Offensive**" + message, post, info);
						PostPersistence.AddPost(post);

						break;
					}

					case PostType.BadUsername:
					{
						if (!catchOff) { break; }

						PostMessage("**Bad Username**" + message, post, info);
						PostPersistence.AddPost(post);

						break;
					}

					case PostType.BadTagUsed:
					{
						if (!catchBadTag) { break; }

						PostMessage("**Bad Tag Used**" + message);
						PostPersistence.AddPost(post);

						break;
					}

					case PostType.LowQuality:
					{
						if (!catchLQ) { break; }

						PostMessage("**Low Quality**" + message, post, info);
						PostPersistence.AddPost(post);

						break;
					}

					case PostType.Spam:
					{
						if (!catchSpam) { break; }

						PostMessage("**Spam**" + message, post, info);
						PostPersistence.AddPost(post);

						break;
					}
				}
			}
		}

		private void PostMessage(string message, Post post = null, PostTypeInfo report = null/* int consecutiveMessageCount = 0*/)
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

			if (error || report == null) { return; }

			Thread.Sleep(5000);

			dynamic doc = null;
			var html = "";

			var title = WebUtility.HtmlEncode(post.Title);

			while (html.IndexOf(title, StringComparison.Ordinal) == -1)
			{
				Dispatcher.Invoke(() => doc = chatWb.Document);

				try
				{
					html = doc.documentElement.InnerHtml;
				}
				catch (Exception)
				{
					return;
				}

				Thread.Sleep(5000);
			}		

			var id = HTMLScraper.GetMessageIDByReportTitle(html, title);

			GlobalInfo.PostedReports.Add(id, new MessageInfo{ Post = post, Report = report });

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

					var t = chatWb.Source.AbsolutePath.Substring(startIndex, endIndex - startIndex);

					if (!t.All(Char.IsDigit)) { return; }

					roomId = int.Parse(t);
				}
				catch (Exception)
				{

				}			
			});
		}

		private string FormatTags(IEnumerable<string> tags)
		{
			var result = new StringBuilder();

			foreach (var tag in tags)
			{
				result.Append("`[" + tag + "]` ");
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
			var entryLabel = "Phamhilator.exe";
			var osInfo = Environment.OSVersion;

			var version = osInfo.Version.Major.ToString() + '.' + osInfo.Version.Minor;
			var editFlag = (uint)((version == "6.2") ? 0x2710 : 0x2328); // 6.2 = Windows 8 and therefore IE10

			var existingSubKey = Registry.LocalMachine.OpenSubKey(installkey, false); // readonly key

			if (existingSubKey.GetValue(entryLabel) == null)
			{
				existingSubKey = Registry.LocalMachine.OpenSubKey(installkey, true); // writable key
				existingSubKey.SetValue(entryLabel, unchecked((int)editFlag), RegistryValueKind.DWord);
			}
		}

		private bool IsSimilar(string a, string b)
		{
			//var diffAllowance = 0.08;
			//var aTotal = 0.0;
			//var bTotal = 0.0;

			//for (var i = 0; i < a.Length; i++)
			//{
			//	aTotal += a[i];
			//}

			//for (var i = 0; i < b.Length; i++)
			//{
			//	bTotal += b[i];
			//}

			var t = false;

			//if (aTotal > bTotal)
			//{
			//	t = ((aTotal - bTotal) / Math.Max(aTotal, bTotal)) > diffAllowance;
			//}
			//else
			//{
			//	t = ((bTotal - aTotal) / Math.Max(aTotal, bTotal)) > diffAllowance;
			//}

			return t;
		}



		// ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ UI Events ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ 



		private void Button_Click(object sender, RoutedEventArgs e)
		{
			chatWb.Source = new Uri("http://chat.meta.stackexchange.com/rooms/773/room-for-low-quality-posts");

			GetRoomId();
		}

		//private void Button_Click_1(object sender, RoutedEventArgs e)
		//{
		//	chatWb.Source = new Uri("http://chat.meta.stackexchange.com/rooms/89/tavern-on-the-meta");

		//	GetRoomId();
		//}

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

		private void catchOffCb_Checked(object sender, RoutedEventArgs e)
		{
			catchOff = true;

			if (roomId != 0)
			{
				PostMessage("`Offensive reports enabled.`");
			}
		}

		private void catchOffCb_Unchecked(object sender, RoutedEventArgs e)
		{
			catchOff = false;

			if (roomId != 0)
			{
				PostMessage("`Offensive reports disabled.`");
			}
		}

		private void catchLQCb_Checked(object sender, RoutedEventArgs e)
		{
			catchLQ = true;

			if (roomId != 0)
			{
				PostMessage("`Low Quality reports enabled.`");
			}
		}

		private void catchLQCb_Unchecked(object sender, RoutedEventArgs e)
		{
			catchLQ = false;

			if (roomId != 0)
			{
				PostMessage("`Low Quality reports disabled.`");
			}
		}

		private void catchSpamCb_Checked(object sender, RoutedEventArgs e)
		{
			catchSpam = true;

			if (roomId != 0)
			{
				PostMessage("`Spam reports enabled.`");
			}
		}

		private void catchSpamCb_Unchecked(object sender, RoutedEventArgs e)
		{
			catchSpam = false;

			if (roomId != 0)
			{
				PostMessage("`Spam reports disabled.`");
			}
		}

		private void CheckBox_Checked(object sender, RoutedEventArgs e)
		{
			catchBadTag = true;

			if (roomId != 0)
			{
				PostMessage("`Bad Tag Used reports enabled.`");
			}
		}

		private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
		{
			catchBadTag = false;

			if (roomId != 0)
			{
				PostMessage("`Bad Tags Used reports disabled.`");
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
