using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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
		private bool quietMode;
		private bool refreshBadTags;
		private bool startMonitoring;
		private bool catchBadTag = true;
		private bool firstStart = true;
		private bool catchSpam = true;
		private bool catchOff = true;
		private bool catchLQ = true;
		private readonly DateTime twentyTen = new DateTime(2010, 01, 01);
		private readonly List<Post> postedMessages = new List<Post>();
		private readonly HashSet<int> spammers = new HashSet<int>();
		private readonly string previouslyPostMessagesPath = DirectoryTools.GetPostPersitenceFile();



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
					Thread.Sleep(12000);
				} while (!startMonitoring);

				while (!exit)
				{
					Dispatcher.Invoke(() => realtimeWb.Refresh());

					do
					{
						Thread.Sleep(12000);
					} while (!startMonitoring);
				}
			}).Start();

			new Thread(() =>
			{
				do
				{
					Thread.Sleep(6000);
				} while (!startMonitoring);

				HookupListener();

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
						Thread.Sleep(6000);
					} while (!startMonitoring);
				}
			}).Start();
		}



		private void HookupListener()
		{
//			Dispatcher.Invoke(() =>
//			{
//				chatWb.InvokeScript("eval", new object[] { @"
//                $.post('/chats/' + '" + roomId + @"' + '/events', { since: 0, mode: 'Messages', msgCount: 1, fkey: fkey().fkey }).success(
//                function (eve)
//                {
//                    console.log(eve.time);
//
//                    $.post('/ws-auth', { roomid: " + roomId + @" , fkey: fkey().fkey }).success(
//                    function (au)
//                    {
//                        console.log(au);
//
//                        var ws = new WebSocket(au.url + '?l=' + eve.time.toString());
//
//                        ws.onmessage = function (e)
//                        {
//                            var fld = 'r' + " + roomId + @", roomevent = JSON.parse(e.data)[fld], ce;
//
//                            if (roomevent && roomevent.e)
//                            {
//                                ce = roomevent.e;
//
//                                var element = document.getElementById('chatMessages');
//
//                                if (element == null)
//                                {
//                                    var ele = document.createElement('p');
//                                    ele.id = 'chatMessages';
//
//                                    var node = document.createTextNode(ce[0].user_name + ' -=- ' + ce[0].content);
//                                    ele.appendChild(node);
//
//                                    document.body.insertBefore(ele, document.body.firstChild);
//                                }
//                                else
//                               {
//                                    element.innerHTML = ce[0].content;
//                               }
//
//                               console.log(ce);
//                           }
//                       };
//                   ws.onerror = function (e) { console.log(e); };
//               });
//           });" 
//				});
//			});

			new Thread(() =>
			{
				while (!exit)
				{		
					Thread.Sleep(500);

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

					if (!html.Contains("chatMessages")) { continue; }

					var startIndex = html.IndexOf("username owner") + 14;
					var endIndex = html.IndexOf("</p>", startIndex);

					var t = html.Substring(startIndex, endIndex - startIndex);

					CommandProcessor.ExacuteCommand(t);
				}
			}).Start();
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
			if (refreshBadTags) { refreshBadTags = false; }

			foreach (var post in posts.Where(p => postedMessages.All(pp => pp.Title != p.Title)))
			{
				var info = PostChecker.CheckPost(post, refreshBadTags);
				var message = (!info.InaccuracyPossible ? "" : " (possible)") + ": " + FormatTags(info.BadTags) + "[" + post.Title + "](" + post.URL + "), by [" + post.AuthorName + "](" + post.AuthorLink + "), on `" + post.Site + "`.";

				if (SpamAbuseDetected(post))
				{
					Task.Factory.StartNew(() => PostMessage("[Spammer abuse](" + post.URL + ")."));
					AddPost(post);

					continue;
				}

				switch (info.Type)
				{
					case PostType.Offensive:
					{
						if (!catchOff) { break; }

						if (quietMode)
						{
							Task.Factory.StartNew(() => PostMessage("[Offensive](" + post.URL + ")" + (!info.InaccuracyPossible ? "." : " possible.")));
							AddPost(post);
						}
						else
						{
							Task.Factory.StartNew(() => PostMessage("**Offensive**" + message));
							AddPost(post);
						}

						break;
					}

					case PostType.BadUsername:
					{
						if (!catchOff) { break; }

						if (quietMode)
						{
							Task.Factory.StartNew(() => PostMessage("[Bad Username](" + post.URL + ")" + (!info.InaccuracyPossible ? "." : " possible.")));
							AddPost(post);
						}
						else
						{
							Task.Factory.StartNew(() => PostMessage("**Bad Username**" + message));
							AddPost(post);
						}

						break;
					}

					case PostType.BadTagUsed:
					{
						if (!catchBadTag) { break; }

						if (quietMode)
						{
							Task.Factory.StartNew(() => PostMessage("[Bad Tag Used](" + post.URL + ")" + (!info.InaccuracyPossible ? "." : " possible.")));
							AddPost(post);
						}
						else
						{
							Task.Factory.StartNew(() => PostMessage("**Bad Tag Used**" + message));
							AddPost(post);
						}

						break;
					}

					case PostType.LowQuality:
					{
						if (!catchLQ) { break; }

						if (quietMode)
						{
							Task.Factory.StartNew(() => PostMessage("[Low Quality](" + post.URL + ")" + (!info.InaccuracyPossible ? "." : " possible.")));
							AddPost(post);
						}
						else
						{
							Task.Factory.StartNew(() => PostMessage("**Low Quality**" + message));
							AddPost(post);
						}

						break;
					}

					case PostType.Spam:
					{
						if (!catchSpam) { break; }

						if (quietMode)
						{
							Task.Factory.StartNew(() => PostMessage("[Spam](" + post.URL + ")" + (!info.InaccuracyPossible ? "." : " possible.")));
							AddPost(post);
						}
						else
						{
							Task.Factory.StartNew(() => PostMessage("**Spam**" + message));
							AddPost(post);
						}

						break;
					}
				}
			}
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

			if (delay >= 20) { return; }

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

			if (File.Exists(previouslyPostMessagesPath))
			{
				File.AppendAllText(previouslyPostMessagesPath, (DateTime.Now - twentyTen).TotalMinutes + "]" + post.Title + "\n");
			}
			else
			{
				File.WriteAllText(previouslyPostMessagesPath, (DateTime.Now - twentyTen).TotalMinutes + "]" + post.Title + "\n");	
			}
		}

		private void PopulatePostedMessages()
		{
			if (!File.Exists(previouslyPostMessagesPath)) { return; }

			var titles = new List<string>(File.ReadAllText(previouslyPostMessagesPath).Split('\n'));
			var date = 0.0;

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
			}

			File.WriteAllText(previouslyPostMessagesPath, "");

			foreach (var post in titles)
			{
				File.AppendAllText(previouslyPostMessagesPath, post + "\n");
			}
		}

		private string FormatTags(IEnumerable<string> tags)
		{
			var result = "";

			foreach (var tag in tags)
			{
				result += "`[" + tag + "]` ";
			}

			return result;
		}

		private bool SpamAbuseDetected(Post post)
		{
			if (IsDefaultUsername(post.AuthorName) && postedMessages[0].AuthorName != null && IsDefaultUsername(postedMessages[0].AuthorName))
			{
				var username0Id = int.Parse(post.AuthorName.Remove(0, 4));
				var username1Id = int.Parse(postedMessages[0].AuthorName.Remove(0, 4));

				if (username0Id < username1Id + 5 || spammers.Contains(username0Id))
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
				startMonitoring = true;

				b.Content = "Pause Monitoring";

				if (System.Diagnostics.Debugger.IsAttached && firstStart)
				{
					PostMessage("`Phamhilator™ started (debug mode)...`");
				}
				else
				{
					PostMessage("`Phamhilator™ started...`");
				}
			}
			else
			{
				startMonitoring = false;

				b.Content = "Start Monitoring";

				PostMessage("`Phamhilator™ paused...`");
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

		private void Button_Click_3(object sender, RoutedEventArgs e)
		{
			var b = (Button)sender;

			if ((string)b.Content == "Enable Quiet Mode")
			{
				quietMode = true;
				b.Content = "Disable Quiet Mode";

				PostMessage("`Quiet mode enabled.`");
			}
			else
			{
				quietMode = false;
				b.Content = "Enable Quiet Mode";

				PostMessage("`Quiet mode disabled.`");
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
			refreshBadTags = true;

			PostMessage("`Bad Tag definitions updated.`");
		}
	}
}
