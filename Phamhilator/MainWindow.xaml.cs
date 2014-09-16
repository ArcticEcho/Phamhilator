using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;



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
		private readonly DateTime twentyTen = new DateTime(2010, 01, 01);
		private readonly List<Post> postedMessages = new List<Post>();
		private readonly HashSet<int> spammers = new HashSet<int>();
		private readonly string previouslyPostMessagesPath = DirectoryTools.GetPostPersitenceFile();
		private KeyValuePair<string, string> lastCommand = new KeyValuePair<string, string>();



		public MainWindow()
		{
			InitializeComponent();

			HideScriptErrors(realtimeWb, true);
			HideScriptErrors(chatWb, true);

			PopulatePostedMessages();

			new Thread(() =>
			{
				do
				{
					Thread.Sleep(refreshRate);
				} while (!Stats.BotRunning);

				while (!exit)
				{
					Dispatcher.Invoke(() => realtimeWb.Refresh());

					do
					{
						Thread.Sleep(refreshRate);
					} while (!Stats.BotRunning);
				}
			}).Start();

			new Thread(() =>
			{
				do
				{
					Thread.Sleep(refreshRate / 2 );
				} while (!Stats.BotRunning);

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

					if (!html.Contains("DIV class=metaInfo")) { continue; }	

					var posts = GetAllPosts(html);

					CheckPosts(posts);

					do
					{
						Thread.Sleep(refreshRate / 2);
					} while (!Stats.BotRunning);
				}
			}) { Priority = ThreadPriority.Lowest }.Start();
		}



		private void StartListener()
		{
			new Thread(() =>
			{
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

					if ((message.Key == lastCommand.Key && message.Value == lastCommand.Value) || (!message.Value.StartsWith("&gt;&gt;") && !message.Value.ToLowerInvariant().StartsWith("@sam")))
					{
						lastCommand = new KeyValuePair<string, string>(message.Key, message.Value);

						continue; 
					}

					var commandMessage = CommandProcessor.ExacuteCommand(message);

					if (commandMessage != "")
					{
						PostMessage(commandMessage);
					}

					lastCommand = new KeyValuePair<string,string>(message.Key, message.Value);
				}
			}) { Priority = ThreadPriority.Lowest }.Start();
		}

		private IEnumerable<Post> GetAllPosts(string html)
		{
			var posts = new List<Post>();

			html = html.Substring(html.IndexOf("<BR class=cbo>", StringComparison.Ordinal));

			while (html.Length > 10000)
			{
				var postURL = HTMLScraper.GetURL(html);

				posts.Add(new Post
				{
					URL = postURL,
					Title = HTMLScraper.GetTitle(html),
					AuthorLink = HTMLScraper.GetAuthorLink(html),
					AuthorName = HTMLScraper.GetAuthorName(html),
					Site = HTMLScraper.GetSite(postURL),
					Tags = HTMLScraper.GetTags(html)
				});

				var startIndex = html.IndexOf("question-container realtime-question", 100, StringComparison.Ordinal);

				html = html.Substring(startIndex);
			}

			return posts;
		}

		private void CheckPosts(IEnumerable<Post> posts)
		{
			foreach (var post in posts.Where(p => postedMessages.All(pp => pp.Title != p.Title)))
			{
				var info = PostChecker.CheckPost(post);
				var message = (info.Type == PostType.BadTagUsed ? "" : " (" + Math.Round(info.Accuracy, 1) + "%)") + ": " + FormatTags(info.BadTags) + "[" + post.Title + "](" + post.URL + "), by [" + post.AuthorName + "](" + post.AuthorLink + "), on `" + post.Site + "`.";

				if (SpamAbuseDetected(post))
				{
					PostMessage("[Spammer abuse](" + post.URL + ").");
					AddPost(post);

					continue;
				}

				switch (info.Type)
				{
					case PostType.Offensive:
					{
						if (!catchOff) { break; }

						PostMessage("**Offensive**" + message);
						AddPost(post);
						
						break;
					}

					case PostType.BadUsername:
					{
						if (!catchOff) { break; }

						PostMessage("**Bad Username**" + message);
						AddPost(post);
						

						break;
					}

					case PostType.BadTagUsed:
					{
						if (!catchBadTag) { break; }

						PostMessage("**Bad Tag Used**" + message);
						AddPost(post);

						break;
					}

					case PostType.LowQuality:
					{
						if (!catchLQ) { break; }

						PostMessage("**Low Quality**" + message);
						AddPost(post);

						break;
					}

					case PostType.Spam:
					{
						if (!catchSpam) { break; }

						PostMessage("**Spam**" + message);
						AddPost(post);	

						break;
					}
				}
			}
	
			//if (refreshBadTags) { refreshBadTags = false; }
		}

		private void PostMessage(string message, int consecutiveMessageCount = 0)
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

			if (!error) { return; }

			consecutiveMessageCount++;

			var delay = (int)Math.Min((4.1484 * Math.Log(consecutiveMessageCount) + 1.02242), 20) * 1000;

			if (delay >= 20000) { return; }

			Thread.Sleep(delay);

			PostMessage(message, consecutiveMessageCount);
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

		private void AddPost(Post post)
		{
			if (postedMessages.Count == 0)
			{
				postedMessages.Add(post);
			}
			else
			{
				postedMessages.Insert(0, post);
			}

			File.AppendAllText(previouslyPostMessagesPath, (DateTime.Now - twentyTen).TotalMinutes + "]" + post.Title + "\n");
			Stats.PostsCaught++;
		}

		private void PopulatePostedMessages()
		{
			if (!File.Exists(previouslyPostMessagesPath)) { return; }

			var titles = new List<string>(File.ReadAllText(previouslyPostMessagesPath).Split('\n'));
			double date;

			for (var i = 0; i < titles.Count; i++)
			{
				var dateString = titles[i].Split(']')[0].Trim();

				if (dateString == "")
				{
					continue;
				}

				date = double.Parse(dateString);

				if ((DateTime.Now - twentyTen).TotalMinutes - date > 2880)
				{
					titles.Remove(titles[i]);

					continue;
				}

				postedMessages.Add(new Post { Title = titles[i].Split(']')[1].Trim() });
				Stats.PostsCaught++;
			}

			File.WriteAllText(previouslyPostMessagesPath, "");

			foreach (var post in titles)
			{
				if (!String.IsNullOrEmpty(post))
				{
					File.AppendAllText(previouslyPostMessagesPath, post + "\n");
				}
			}
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
			if (postedMessages[0].AuthorName != null && IsDefaultUsername(post.AuthorName) && IsDefaultUsername(postedMessages[0].AuthorName))
			{
				var username0Id = int.Parse(post.AuthorName.Remove(0, 4));
				var username1Id = int.Parse(postedMessages[0].AuthorName.Remove(0, 4));

				if ((username0Id > username1Id + 5 && username0Id < username1Id) || spammers.Contains(username0Id))
				{
					PostMessage("`Spammer abuse detected.`");

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



		// ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ UI Events ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ 



		private void Button_Click(object sender, RoutedEventArgs e)
		{
			chatWb.Source = new Uri("http://chat.meta.stackexchange.com/rooms/773/room-for-low-quality-posts");

			GetRoomId();
		}

		private void Button_Click_1(object sender, RoutedEventArgs e)
		{
			chatWb.Source = new Uri("http://chat.meta.stackexchange.com/rooms/89/tavern-on-the-meta");

			GetRoomId();
		}

		private void Button_Click_2(object sender, RoutedEventArgs e)
		{
			var b = ((Button)sender);

			if ((string)b.Content == "Start Monitoring")
			{
				Stats.BotRunning = true;

				b.Content = "Pause Monitoring";

				if (firstStart)
				{
					if (System.Diagnostics.Debugger.IsAttached)
					{
						PostMessage("`Phamhilator™ started (debug mode).`");
					}
					else
					{
						PostMessage("`Phamhilator™ started.`");
					}

					firstStart = false;
					Stats.UpTime = DateTime.UtcNow;
				}
				else
				{
					PostMessage("`Phamhilator™ started.`");
				}	
			}
			else
			{
				Stats.BotRunning = false;

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

		private void MetroWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
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

		private void Button_Click_4(object sender, RoutedEventArgs e)
		{
			//refreshBadTags = true;

			//PostMessage("`Bad Tag Definitions updated.`");
		}

		private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			refreshRate = 20000 - (int)e.NewValue;
		}
	}
}
