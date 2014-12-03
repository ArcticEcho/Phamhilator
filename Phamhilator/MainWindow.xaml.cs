using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Threading;
using System.Diagnostics;
using System.Globalization;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Collections.Generic;
using ChatExchangeDotNet;



namespace Phamhilator
{
	public partial class MainWindow
	{
		private int refreshRate = 10000; // In milliseconds.
		private readonly HashSet<int> spammers = new HashSet<int>();
		private static readonly List<string> previouslyFoundPosts = new List<string>();
		private Thread postCatcherThread;
		private DateTime requestedDieTime;



		public MainWindow()
		{
			try
			{
				AppDomain.CurrentDomain.UnhandledException += GlobalExceptionHandler;

				CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
				CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;

				InitializeComponent();

				var data = File.ReadAllText(DirectoryTools.GetCredsFile());

				if (!String.IsNullOrEmpty(data))
				{
					emailTB.Text = data.Remove(data.IndexOf("¬", StringComparison.InvariantCulture));
					passwordTB.Text = data.Substring(data.IndexOf("¬", StringComparison.InvariantCulture) + 1);
				}

				loginC.Children.Remove(operationC);

				PostPersistence.Initialise();

				PostCatcher();
			}
			catch (Exception ex)
			{
                if (ex is TypeInitializationException && ex.InnerException != null)
                {
                    ex = ex.InnerException;
                }

				Clipboard.SetText(ex.ToString());

				MessageBox.Show(ex + Environment.NewLine + Environment.NewLine + "The above error details have be copied to your clipboard. Now closing Pham...");

				Environment.Exit(Environment.ExitCode);
			}
		}



		private void PostCatcher()
		{
			postCatcherThread = new Thread(() =>
			{
				// Wait for the bot to startup.
				do
				{
					Thread.Sleep(500);
				} while (!GlobalInfo.BotRunning);

				while (!GlobalInfo.Shutdown)
				{
					try
					{						
						string html;

						try
						{
							html = StringDownloader.DownloadString("http://stackexchange.com/questions?tab=realtime");
						}
						catch (Exception ex)
						{
							if (GlobalInfo.DebugMode)
							{
								GlobalInfo.PrimaryRoom.PostMessage(ex.ToString());
							}

							continue;
						}

						if (html.IndexOf("hot-question-site-icon", StringComparison.Ordinal) == -1) { continue; }

						var posts = GetAllPosts(html);

						if (posts.Length != 0)
						{
							CheckPosts(posts);
						}

						var sleepTime = DateTime.UtcNow.AddMilliseconds(refreshRate);

						do
						{
							Thread.Sleep(250);
						} while (DateTime.UtcNow < sleepTime && GlobalInfo.BotRunning && !GlobalInfo.Shutdown);
					}
					catch (Exception)
					{
						Thread.Sleep(2000);
					}
				}
			}) { Priority = ThreadPriority.Lowest };

			postCatcherThread.Start();
		}

		private void KillBot()
		{
			GlobalInfo.Shutdown = true;
			requestedDieTime = DateTime.UtcNow;

			var warningMessagePosted = false;

			while (postCatcherThread.IsAlive)
			{
				Thread.Sleep(250);

				if ((DateTime.UtcNow - requestedDieTime).TotalSeconds > 10 && !warningMessagePosted)
				{
					GlobalInfo.PrimaryRoom.PostMessage("`Warning: 10 seconds have past since the kill command was issued and the post catcher thread is still alive. Now forcing thread death...`");

					warningMessagePosted = true;

					postCatcherThread.Abort();
				}

				if (!postCatcherThread.IsAlive) { break; }
			}

			Thread.Sleep(5000);

			GlobalInfo.PrimaryRoom.PostMessage("`All threads have died. Kill successful!`");

			Thread.Sleep(5000);

			Dispatcher.Invoke(() => Environment.Exit(0));
		}

		private void HandlePrimaryNewMessage(Message message)
		{
			if (message.Content == ">>kill-it-with-no-regrets-for-sure")
			{
				if (UserAccess.Owners.Contains(message.AuthorID))
				{
					GlobalInfo.PrimaryRoom.PostReply(message, "`Killing...`");

					KillBot();
				}
				else
				{
					GlobalInfo.PrimaryRoom.PostReply(message, "`Access denied.`");
				}
			}
			else
			{
				var messages = CommandProcessor.ExacuteCommand(GlobalInfo.PrimaryRoom, message); 
				
				if (messages.Length == 0) { return; }

				foreach (var m in messages.Where(m => !String.IsNullOrEmpty(m.Content)))
				{
				    if (m.Reply)
				    {
                        GlobalInfo.PrimaryRoom.PostReply(message, m.Content);
				    }
				    else
				    {
                        GlobalInfo.PrimaryRoom.PostMessage(m.Content);
				    }
				}
			}
		}

		private void HandleSecondaryNewMessage(Room room, Message message)
		{
			var messages = CommandProcessor.ExacuteCommand(room, message);

			if (messages.Length == 0) { return; }

			foreach (var m in messages.Where(m => !String.IsNullOrEmpty(m.Content)))
			{
                if (m.Reply)
                {
                    GlobalInfo.PrimaryRoom.PostReply(message, m.Content);
                }
                else
                {
                    GlobalInfo.PrimaryRoom.PostMessage(m.Content);
                }
			}
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

		private void PostReport(Post p, string messageBody, PostAnalysis info, bool isQuestionReport = true)
		{
			if (info.Accuracy <= GlobalInfo.AccuracyThreshold) { return; }

			Message message = null;
			MessageInfo chatMessage = null;

			if (SpamAbuseDetected(p))
			{
				chatMessage = new MessageInfo { Body = "[Spammer abuse detected](" + p.URL + ").", Post = p, Report = info, IsQuestionReport = isQuestionReport, RoomID = GlobalInfo.PrimaryRoom.ID };

				message = GlobalInfo.PrimaryRoom.PostMessage(chatMessage.Body);

				if (message != null)
				{
					PostPersistence.AddPost(p);
					GlobalInfo.PostedReports.Add(message.ID, chatMessage);
				}

				return;
			}

			switch (info.Type)
			{
				case PostType.Offensive:
				{
					chatMessage = new MessageInfo { Body = "**Offensive**" + messageBody, Post = p, Report = info, IsQuestionReport = isQuestionReport, RoomID = GlobalInfo.PrimaryRoom.ID };
					message = GlobalInfo.PrimaryRoom.PostMessage(chatMessage.Body);

					break;
				}

				case PostType.BadUsername:
				{
					chatMessage = new MessageInfo { Body = "**Bad Username**" + messageBody, Post = p, Report = info, IsQuestionReport = isQuestionReport, RoomID = GlobalInfo.PrimaryRoom.ID };
					message = GlobalInfo.PrimaryRoom.PostMessage(chatMessage.Body);

					break;
				}

				case PostType.BadTagUsed:
				{
					chatMessage = new MessageInfo { Body = "**Bad Tag Used**" + messageBody, Post = p, Report = info, IsQuestionReport = isQuestionReport, RoomID = GlobalInfo.PrimaryRoom.ID };
					message = GlobalInfo.PrimaryRoom.PostMessage(chatMessage.Body);

					break;
				}

				case PostType.LowQuality:
				{
					chatMessage = new MessageInfo { Body = "**Low Quality**" + messageBody, Post = p, Report = info, IsQuestionReport = isQuestionReport, RoomID = GlobalInfo.PrimaryRoom.ID };
					message = GlobalInfo.PrimaryRoom.PostMessage(chatMessage.Body);

					break;
				}

				case PostType.Spam:
				{
					chatMessage = new MessageInfo { Body = "**Spam**" + messageBody, Post = p, Report = info, IsQuestionReport = isQuestionReport, RoomID = GlobalInfo.PrimaryRoom.ID };
					message = GlobalInfo.PrimaryRoom.PostMessage(chatMessage.Body);

					break;
				}
			}

			if (message != null)
			{
				PostPersistence.AddPost(p);
				GlobalInfo.PostedReports.Add(message.ID, chatMessage);

				if (info.AutoTermsFound)
				{
					foreach (var room in GlobalInfo.ChatClient.Rooms.Where(r => r.ID != GlobalInfo.PrimaryRoom.ID))
					{
						room.PostMessage(chatMessage.Body);
					}			
				}

				Thread.Sleep(1500); //TODO: Add more efficient rate limiting (either to CE.Ner or Pham).
			}

			GlobalInfo.Stats.TotalCheckedPosts++;
		}

		private bool SpamAbuseDetected(Post post)
		{
			if (PostPersistence.Messages.Count != 0 && PostPersistence.Messages[0].AuthorName != null && IsDefaultUsername(PostPersistence.Messages[0].AuthorName) && IsDefaultUsername(post.AuthorName))
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

		private void GlobalExceptionHandler(object o, UnhandledExceptionEventArgs args)
		{
			Clipboard.SetText(args.ExceptionObject.ToString());

			if (GlobalInfo.DebugMode)
			{
				GlobalInfo.PrimaryRoom.PostMessage(args.ExceptionObject.ToString());
			}

			if (args.IsTerminating)
			{
				MessageBox.Show(args.ExceptionObject + Environment.NewLine + Environment.NewLine + "The above error details have be copied to your clipboard. Now closing Pham...", "Oops...", MessageBoxButton.OK, MessageBoxImage.Error);
			}
			else
			{
				var res = MessageBox.Show(args.ExceptionObject + Environment.NewLine + Environment.NewLine + "The above error details have be copied to your clipboard. Do you wish to continue?", "Oops...", MessageBoxButton.YesNo, MessageBoxImage.Error);

				if (res == MessageBoxResult.No) { Environment.Exit(Environment.ExitCode); }
			}
		}

		# region UI Events

		private void MetroWindow_Closing(object sender, CancelEventArgs e)
		{
			statusL.Content = "Shutting Down Pham...";

			e.Cancel = true;

			if (!GlobalInfo.BotRunning) { Environment.Exit(0); }

			Task.Factory.StartNew(() => GlobalInfo.PrimaryRoom.PostMessage("`Phamhilator™ stopped.`"));

			GlobalInfo.Shutdown = true;

			Thread.Sleep(3000);

			Task.Factory.StartNew(() => Dispatcher.Invoke(() => Environment.Exit(0)));
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

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			if (String.IsNullOrEmpty(emailTB.Text) || String.IsNullOrEmpty(passwordTB.Text)) { return; }
			
			progressBar.IsIndeterminate = true;
			((Button)sender).IsEnabled = false;
			emailTB.IsEnabled = false;
			passwordTB.IsEnabled = false;
            remCredsCB.IsEnabled = false;
            debugCB.IsEnabled = false;
		    loginTitleL.Content = "Logging In...";

			var user = emailTB.Text;
			var pass = passwordTB.Text;
            var remCreds = remCredsCB.IsChecked ?? false;

			Task.Factory.StartNew(() =>
			{
			    try
			    {
			        GlobalInfo.ChatClient = new Client(user, pass);
			    }
			    catch (Exception ex)
			    {
			        Dispatcher.Invoke(() =>
                    {
                        progressBar.IsIndeterminate = false;
                        ((Button)sender).IsEnabled = true;
                        emailTB.IsEnabled = true;
                        passwordTB.IsEnabled = true;
                        remCredsCB.IsEnabled = true;
                        debugCB.IsEnabled = true;
                        statusL.Content = "Could Not Login";
                        Clipboard.SetText(ex.ToString());
			        });

                    return;
			    }

			    if (remCreds)
			    {
				    File.WriteAllText(DirectoryTools.GetCredsFile(), user + "¬" + pass);
			    }
			    else
			    {
				    File.WriteAllText(DirectoryTools.GetCredsFile(), "");
			    }

				GlobalInfo.PrimaryRoom = GlobalInfo.ChatClient.JoinRoom("http://chat.meta.stackexchange.com/rooms/773/low-quality-posts-hq");
				GlobalInfo.PrimaryRoom.NewMessage += HandlePrimaryNewMessage;
				GlobalInfo.PrimaryRoom.MessageEdited += (oldMessage, newMessage) => HandlePrimaryNewMessage(newMessage);
				GlobalInfo.PrimaryRoom.IgnoreOwnEvents = false;

				GlobalInfo.ChatClient.JoinRoom("http://chat.meta.stackexchange.com/rooms/89/tavern-on-the-meta");

				for (var i = 0; i < GlobalInfo.ChatClient.Rooms.Count; i++)
				{
                    var x = i;

                    if (GlobalInfo.ChatClient.Rooms[x].ID == GlobalInfo.PrimaryRoom.ID) { continue; }

                    //try
                    //{
                        GlobalInfo.ChatClient.Rooms[x].NewMessage += message => HandleSecondaryNewMessage(GlobalInfo.ChatClient.Rooms[x], message);
                        GlobalInfo.ChatClient.Rooms[x].MessageEdited += (oldMessage, newMessage) => HandleSecondaryNewMessage(GlobalInfo.ChatClient.Rooms[x], newMessage);
                    //}
                    //catch (ArgumentOutOfRangeException ex)
                    //{
                    //    Trace.WriteLine("TODO: " + ex.Message);
                    //}

                    GlobalInfo.ChatClient.Rooms[x].IgnoreOwnEvents = false;
				}

				Dispatcher.Invoke(() =>
				{
					GlobalInfo.DebugMode = debugCB.IsChecked ?? Debugger.IsAttached;

					transContentC.Content = operationC;

					progressBar.IsIndeterminate = false;
				});
			});
		}

		private void Button_Click_2(object sender, RoutedEventArgs e)
		{
			statusL.Content = "Monitoring Enabled";

			((Button)sender).IsEnabled = false;

			GlobalInfo.BotRunning = true;

			if (Debugger.IsAttached || GlobalInfo.DebugMode)
			{
				GlobalInfo.PrimaryRoom.PostMessage("`Phamhilator™ started (debug mode).`");
			}
			else
			{
				GlobalInfo.PrimaryRoom.PostMessage("`Phamhilator™ started.`");
			}

			GlobalInfo.UpTime = DateTime.UtcNow;
		}

		# endregion
	}
}
