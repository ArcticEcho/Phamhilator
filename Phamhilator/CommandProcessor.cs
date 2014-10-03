using System;
using System.Linq;
using System.Text.RegularExpressions;



namespace Phamhilator
{
	public static class CommandProcessor
	{
		private static MessageInfo message;
		private static string commandLower = "";



		public static string ExacuteCommand(MessageInfo input)
		{
			string command;

			if (input.Body.StartsWith(">>"))
			{
				command = input.Body.Remove(0, 2).TrimStart();
			}
			else if (input.Body.ToLowerInvariant().StartsWith("@" + GlobalInfo.BotUsername.ToLowerInvariant()) && GlobalInfo.PostedReports.ContainsKey(input.RepliesToMessageID))
			{
				command = input.Body.Remove(0, 5).TrimStart();
			}
			else
			{
				return "";
			}

			commandLower = command.ToLowerInvariant();

			var user = input.AuthorID;

			if (IsNormalUserCommand())
			{
				try
				{
					return NormalUserCommands();
				}
				catch (Exception)
				{
					return "`Error executing command.`";
				}
			}

			if (IsPrivilegedUserCommand())
			{
				message = input;

				if (!UserAccess.CommandAccessUsers.Contains(user) && !UserAccess.Owners.Contains(user))
				{
					return "`Access denied.`";
				}

				try
				{
					return PrivilegedUserCommands(command);
				}
				catch (Exception)
				{
					return "`Error executing command.`";
				}		
			}
			
			if (IsOwnerCommand())
			{
				if (!UserAccess.Owners.Contains(user))
				{
					return "`Access denied.`";
				}
				
				try
				{
					return OwnerCommand(command);
				}
				catch (Exception)
				{
					return "`Error executing command.`";
				}
			}

			return "`Command not recognised.`";
		}



		private static bool IsOwnerCommand()
		{
			return commandLower.StartsWith("add user") ||
				   commandLower.StartsWith("start") ||
				   commandLower.StartsWith("pause") ||
				   commandLower.StartsWith("threshold");
		}

		private static string OwnerCommand(string command)
		{
			if (commandLower.StartsWith("add user"))
			{
				return AddUser(command);
			}

			if (commandLower == "start")
			{
				return StartBot();
			}

			if (commandLower == "pause")
			{
				return PauseBot();
			}

			if (commandLower.StartsWith("threshold"))
			{
				return SetAccuracyThreshold(command);
			}

			return "`Command not recognised.`";
		}


		private static bool IsPrivilegedUserCommand()
		{
			return commandLower == "fp" || commandLower == "false" || commandLower == "false pos" || commandLower == "false positive" ||
				   commandLower == "tp" || commandLower == "true" || commandLower == "true pos" || commandLower == "true positive" ||
				   commandLower == "clean" || commandLower == "sanitise" || commandLower == "sanitize" ||
				   commandLower.StartsWith("bremove term") ||
				   commandLower.StartsWith("badd term") ||
				   commandLower.StartsWith("wremove term") ||
				   commandLower.StartsWith("wadd term") ||
				   commandLower.StartsWith("add tag") ||
				   commandLower.StartsWith("remove tag");
		}

		private static string PrivilegedUserCommands(string command)
		{
			if (commandLower == "fp" || commandLower == "false" || commandLower == "false pos" || commandLower == "false positive")
			{
				return FalsePositive();
			}

			if (commandLower == "tp" || commandLower == "true" || commandLower == "true pos" || commandLower == "true positive")
			{
				return TruePositive();
			}

			if (commandLower == "clean" || commandLower == "sanitise" || commandLower == "sanitize")
			{
				return CleanPost();
			}

			if (commandLower.StartsWith("bremove term"))
			{
				return RemoveBlackTerm(command);
			}

			if (commandLower.StartsWith("badd term"))
			{
				return AddBlackTerm(command);
			}

			if (commandLower.StartsWith("wremove term"))
			{
				return RemoveWhiteTerm(command);
			}

			if (commandLower.StartsWith("wadd term"))
			{
				return AddWhiteTerm(command);
			}

			if (commandLower.StartsWith("add tag"))
			{
				return AddTag(command);
			}

			if (commandLower.StartsWith("remove tag"))
			{
				return RemoveTag(command);
			}

			return "`Command not recognised.`";
		}


		private static bool IsNormalUserCommand()
		{
			return commandLower == "stats" || commandLower == "info" || commandLower == "help" || commandLower == "commands";
		}

		private static string NormalUserCommands()
		{
			if (commandLower == "stats" || commandLower == "info")
			{
				return "`Owners: " + GlobalInfo.Owners + ". Total terms: " + GlobalInfo.TermCount + ". Accuracy threshold: " + GlobalInfo.AccuracyThreshold + "%. Posts caught over last 7 days: " + GlobalInfo.PostsCaught + ". Uptime: " + (DateTime.UtcNow - GlobalInfo.UpTime) + ".`";
			}

			if (commandLower == "help" || commandLower == "commands")
			{
				return "`See` [`here`](https://github.com/ArcticWinter/Phamhilator/blob/master/Phamhilator/Readme%20-%20Chat%20Commands.md) `for a full list of commands.`";
			}

			return "`Command not recognised.`";
		}


		// Privileged user commands.


		private static string AddBlackTerm(string command)
		{
			var addCommand = command.Substring(command.IndexOf(" ", command.IndexOf(" ", StringComparison.Ordinal) + 1, StringComparison.Ordinal) + 1);

			if (addCommand.StartsWith("off") || addCommand.StartsWith("spam") || addCommand.StartsWith("lq") || addCommand.StartsWith("name"))
			{
				Regex term;

				if (addCommand.StartsWith("off"))
				{
					term = new Regex(addCommand.Remove(0, 4));

					if (GlobalInfo.QBlackOff.Terms.ContainsTerm(term)) { return "`Term already exists.`"; }

					GlobalInfo.QBlackOff.AddTerm(term);
				}

				if (addCommand.StartsWith("spam"))
				{
					term = new Regex(addCommand.Remove(0, 5));

					if (GlobalInfo.QBlackSpam.Terms.ContainsTerm(term)) { return "`Term already exists.`"; }

					GlobalInfo.QBlackSpam.AddTerm(term);
				}

				if (addCommand.StartsWith("lq"))
				{
					term = new Regex(addCommand.Remove(0, 3));

					if (GlobalInfo.QBlackLQ.Terms.ContainsTerm(term)) { return "`Term already exists.`"; }

					GlobalInfo.QBlackLQ.AddTerm(term);
				}

				if (addCommand.StartsWith("name"))
				{
					term = new Regex(addCommand.Remove(0, 5));

					if (GlobalInfo.QBlackName.Terms.ContainsTerm(term)) { return "`Term already exists.`"; }

					GlobalInfo.QBlackName.AddTerm(term);
				}

				return "`Term added.`";
			}

			return "`Command not recognised.`";
		}

		private static string RemoveBlackTerm(string command)
		{
			var removeCommand = command.Substring(command.IndexOf(" ", command.IndexOf(" ", StringComparison.Ordinal) + 1, StringComparison.Ordinal) + 1);

			if (removeCommand.StartsWith("off") || removeCommand.StartsWith("spam") || removeCommand.StartsWith("lq") || removeCommand.StartsWith("name"))
			{
				Regex term;

				if (removeCommand.StartsWith("off"))
				{
					term = new Regex(removeCommand.Remove(0, 4));

					if (!GlobalInfo.QBlackOff.Terms.ContainsTerm(term)) { return "`Term does not exist.`"; }

					GlobalInfo.QBlackOff.RemoveTerm(term);
				}

				if (removeCommand.StartsWith("spam"))
				{
					term = new Regex(removeCommand.Remove(0, 5));

					if (!GlobalInfo.QBlackSpam.Terms.ContainsTerm(term)) { return "`Term does not exist.`"; }

					GlobalInfo.QBlackSpam.RemoveTerm(term);
				}

				if (removeCommand.StartsWith("lq"))
				{
					term = new Regex(removeCommand.Remove(0, 3));

					if (!GlobalInfo.QBlackLQ.Terms.ContainsTerm(term)) { return "`Term does not exist.`"; }

					GlobalInfo.QBlackLQ.RemoveTerm( term);
				}

				if (removeCommand.StartsWith("name"))
				{
					term = new Regex(removeCommand.Remove(0, 5));

					if (!GlobalInfo.QBlackName.Terms.ContainsTerm(term)) { return "`Term does not exist.`"; }

					GlobalInfo.QBlackName.RemoveTerm(term);
				}

				return "`Term removed.`";
			}

			return "`Command not recognised.`";
		}


		private static string AddWhiteTerm(string command)
		{
			var addCommand = command.Substring(command.IndexOf(" ", command.IndexOf(" ", StringComparison.Ordinal) + 1, StringComparison.Ordinal) + 1);

			if (addCommand.StartsWith("off") || addCommand.StartsWith("spam") || addCommand.StartsWith("lq") || addCommand.StartsWith("name"))
			{
				Regex term;
				string site;

				if (addCommand.StartsWith("off"))
				{
					addCommand = addCommand.Remove(0, 4);

					if (addCommand.IndexOf(" ", StringComparison.Ordinal) == -1) { return "`Command not recognised.`"; }

					term = new Regex(addCommand.Remove(0, addCommand.IndexOf(" ", StringComparison.Ordinal) + 1));
					site = addCommand.Substring(0, addCommand.IndexOf(" ", StringComparison.Ordinal));

					if (GlobalInfo.QWhiteOff.Terms.ContainsKey(site) && GlobalInfo.QWhiteOff.Terms[site].ContainsTerm(term)) { return "`Ignore term already exists.`"; }

					GlobalInfo.QWhiteOff.AddTerm(term, site);
				}

				if (addCommand.StartsWith("spam"))
				{
					addCommand = addCommand.Remove(0, 5);

					if (addCommand.IndexOf(" ", StringComparison.Ordinal) == -1) { return "`Command not recognised.`"; }

					term = new Regex(addCommand.Remove(0, addCommand.IndexOf(" ", StringComparison.Ordinal) + 1));
					site = addCommand.Substring(0, addCommand.IndexOf(" ", StringComparison.Ordinal));

					if (GlobalInfo.QWhiteSpam.Terms.ContainsKey(site) && GlobalInfo.QWhiteSpam.Terms[site].ContainsTerm(term)) { return "`Ignore term already exists.`"; }

					GlobalInfo.QWhiteSpam.AddTerm(term, site);
				}

				if (addCommand.StartsWith("lq"))
				{
					addCommand = addCommand.Remove(0, 3);

					if (addCommand.IndexOf(" ", StringComparison.Ordinal) == -1) { return "`Command not recognised.`"; }

					term = new Regex(addCommand.Remove(0, addCommand.IndexOf(" ", StringComparison.Ordinal) + 1));
					site = addCommand.Substring(0, addCommand.IndexOf(" ", StringComparison.Ordinal));

					if (GlobalInfo.QWhiteLQ.Terms.ContainsKey(site) && GlobalInfo.QWhiteLQ.Terms[site].ContainsTerm(term)) { return "`Ignore term already exists.`"; }

					GlobalInfo.QWhiteSpam.AddTerm(term, site);
				}

				if (addCommand.StartsWith("name"))
				{
					addCommand = addCommand.Remove(0, 5);

					if (addCommand.IndexOf(" ", StringComparison.Ordinal) == -1) { return "`Command not recognised.`"; }

					term = new Regex(addCommand.Remove(0, addCommand.IndexOf(" ", StringComparison.Ordinal) + 1));
					site = addCommand.Substring(0, addCommand.IndexOf(" ", StringComparison.Ordinal));

					if (GlobalInfo.QWhiteName.Terms.ContainsKey(site) && GlobalInfo.QWhiteName.Terms[site].ContainsTerm(term)) { return "`Ignore term already exists.`"; }

					GlobalInfo.QWhiteName.AddTerm(term, site);
				}

				return "`Ignore term added.`";
			}

			return "`Command not recognised.`";
		}

		private static string RemoveWhiteTerm(string command)
		{
			var removeCommand = command.Substring(command.IndexOf(" ", command.IndexOf(" ", StringComparison.Ordinal) + 1, StringComparison.Ordinal) + 1);

			if (removeCommand.StartsWith("off") || removeCommand.StartsWith("spam") || removeCommand.StartsWith("lq") || removeCommand.StartsWith("name"))
			{
				Regex term;
				string site;

				if (removeCommand.StartsWith("off"))
				{
					removeCommand = removeCommand.Remove(0, 4);

					if (removeCommand.IndexOf(" ", StringComparison.Ordinal) == -1) { return "`Command not recognised.`"; }

					term = new Regex(removeCommand.Remove(0, removeCommand.IndexOf(" ", StringComparison.Ordinal) + 1));
					site = removeCommand.Substring(0, removeCommand.IndexOf(" ", StringComparison.Ordinal));

					if (!GlobalInfo.QWhiteOff.Terms.ContainsKey(site) && !GlobalInfo.QWhiteOff.Terms[site].ContainsTerm(term)) { return "`Ignore term does not exist.`"; }

					GlobalInfo.QWhiteOff.RemoveTerm(term, site);
				}

				if (removeCommand.StartsWith("spam"))
				{
					removeCommand = removeCommand.Remove(0, 5);

					if (removeCommand.IndexOf(" ", StringComparison.Ordinal) == -1) { return "`Command not recognised.`"; }

					term = new Regex(removeCommand.Remove(0, removeCommand.IndexOf(" ", StringComparison.Ordinal) + 1));
					site = removeCommand.Substring(0, removeCommand.IndexOf(" ", StringComparison.Ordinal));

					if (!GlobalInfo.QWhiteSpam.Terms.ContainsKey(site) && !GlobalInfo.QWhiteSpam.Terms[site].ContainsTerm(term)) { return "`Ignore term does not exist.`"; }

					GlobalInfo.QWhiteSpam.RemoveTerm(term, site);
				}

				if (removeCommand.StartsWith("lq"))
				{
					removeCommand = removeCommand.Remove(0, 3);

					if (removeCommand.IndexOf(" ", StringComparison.Ordinal) == -1) { return "`Command not recognised.`"; }

					term = new Regex(removeCommand.Remove(0, removeCommand.IndexOf(" ", StringComparison.Ordinal) + 1));
					site = removeCommand.Substring(0, removeCommand.IndexOf(" ", StringComparison.Ordinal));

					if (!GlobalInfo.QWhiteLQ.Terms.ContainsKey(site) && !GlobalInfo.QWhiteLQ.Terms[site].ContainsTerm(term)) { return "`Ignore term does not exist.`"; }

					GlobalInfo.QWhiteLQ.RemoveTerm(term, site);
				}

				if (removeCommand.StartsWith("name"))
				{
					removeCommand = removeCommand.Remove(0, 5);

					if (removeCommand.IndexOf(" ", StringComparison.Ordinal) == -1) { return "`Command not recognised.`"; }

					term = new Regex(removeCommand.Remove(0, removeCommand.IndexOf(" ", StringComparison.Ordinal) + 1));
					site = removeCommand.Substring(0, removeCommand.IndexOf(" ", StringComparison.Ordinal));

					if (!GlobalInfo.QWhiteName.Terms.ContainsKey(site) && !GlobalInfo.QWhiteName.Terms[site].ContainsTerm(term)) { return "`Ignore term does not exist.`"; }

					GlobalInfo.QWhiteName.RemoveTerm(term, site);
				}

				return "`Ignore Term removed.`";
			}

			return "`Command not recognised.`";
		}


		private static string AddTag(string command)
		{
			var tagCommand = command.Remove(0, command.IndexOf("tag", StringComparison.Ordinal) + 4);

			if (tagCommand.Count(c => c == ' ') != 1 && tagCommand.Count(c => c == ' ') != 3) { return "`Command not recognised.`"; }

			var site = tagCommand.Substring(0, tagCommand.IndexOf(" ", StringComparison.Ordinal));
			var metaPost = "";
			string tag; 

			if (tagCommand.IndexOf("href", StringComparison.Ordinal) != -1)
			{
				tag = tagCommand.Substring(site.Length + 1, tagCommand.IndexOf(" ", site.Length + 1, StringComparison.Ordinal) - 1 - site.Length);

				var startIndex = tagCommand.IndexOf("href", StringComparison.Ordinal) + 6;
				var endIndex = tagCommand.LastIndexOf("\">", StringComparison.Ordinal);

				metaPost = tagCommand.Substring(startIndex, endIndex - startIndex);
			}
			else
			{
				tag = tagCommand.Remove(0, tagCommand.IndexOf(" ", StringComparison.Ordinal) + 1);
			}

			if (BadTagDefinitions.BadTags.ContainsKey(site) && BadTagDefinitions.BadTags[site].ContainsKey(tag)) { return "`Tag already exists.`"; }

			BadTagDefinitions.AddTag(site, tag, metaPost);

			return "`Tag added.`";
		}

		private static string RemoveTag(string command)
		{
			var tagCommand = command.Remove(0, command.IndexOf("tag", StringComparison.Ordinal) + 4);

			if (tagCommand.Count(c => c == ' ') != 1) { return "`Command not recognised.`"; }

			var site = tagCommand.Substring(0, tagCommand.IndexOf(" ", StringComparison.Ordinal));
			var tag = tagCommand.Remove(0, tagCommand.IndexOf(" ", StringComparison.Ordinal) + 1);

			if (BadTagDefinitions.BadTags.ContainsKey(site))
			{
				if (BadTagDefinitions.BadTags[site].ContainsKey(tag))
				{
					BadTagDefinitions.RemoveTag(site, tag);

					return "`Tag removed.`";
				}

				return "`Tag does not exist.`";
			}

			return "`Site does not exist.`";
		}


		private static string CleanPost()
		{
			var reportID = message.RepliesToMessageID;

			if (GlobalInfo.PostedReports.ContainsKey(reportID))
			{
				var oldTitle = GlobalInfo.PostedReports[reportID].Post.Title;
				var newTitle = "";

				foreach (var c in oldTitle)
				{
					if (c == ' ')
					{
						newTitle += ' ';
					}
					else
					{
						newTitle += '*';
					}
				}

				var newMessage = GlobalInfo.PostedReports[reportID].Body.Replace(oldTitle, newTitle);

				MessageHandler.EditMessage(newMessage, reportID);
			}

			return "";
		}


		private static string FalsePositive()
		{
			switch (message.Report.Type)
			{
				case PostType.LowQuality:
				{
					foreach (var term in message.Report.BlackTermsFound)
					{
						if (!GlobalInfo.QWhiteLQ.Terms.ContainsKey(message.Post.Site) || !GlobalInfo.QWhiteLQ.Terms[message.Post.Site].ContainsTerm(term.Key))
						{
							GlobalInfo.QWhiteLQ.AddTerm(term.Key, message.Post.Site, message.Report.BlackTermsFound.Values.Max() / 2);
						}
					}

					foreach (var term in message.Report.WhiteTermsFound)
					{
						GlobalInfo.QWhiteLQ.SetScore(term.Key, message.Post.Site, term.Value + 1);
					}

					MessageHandler.DeleteMessage(message.RepliesToMessageID);

					return "";
				}

				case PostType.Offensive:
				{
					foreach (var term in message.Report.BlackTermsFound)
					{
						if (!GlobalInfo.QWhiteOff.Terms.ContainsKey(message.Post.Site) || !GlobalInfo.QWhiteOff.Terms[message.Post.Site].ContainsTerm(term.Key))
						{
							GlobalInfo.QWhiteOff.AddTerm(term.Key, message.Post.Site, message.Report.BlackTermsFound.Values.Max() / 2);
						}
					}

					foreach (var term in message.Report.WhiteTermsFound)
					{
						GlobalInfo.QWhiteOff.SetScore(term.Key, message.Post.Site, term.Value + 1);
					}

					MessageHandler.DeleteMessage(message.RepliesToMessageID);

					return "";
				}

				case PostType.Spam:
				{
					foreach (var term in message.Report.BlackTermsFound)
					{
						if (!GlobalInfo.QWhiteSpam.Terms.ContainsKey(message.Post.Site) || !GlobalInfo.QWhiteSpam.Terms[message.Post.Site].ContainsTerm(term.Key))
						{
							GlobalInfo.QWhiteSpam.AddTerm(term.Key, message.Post.Site, message.Report.BlackTermsFound.Values.Max() / 2);
						}
					}

					foreach (var term in message.Report.WhiteTermsFound)
					{
						GlobalInfo.QWhiteSpam.SetScore(term.Key, message.Post.Site, term.Value + 1);
					}

					MessageHandler.DeleteMessage(message.RepliesToMessageID);

					return "";
				}

				case PostType.BadUsername:
				{
					foreach (var term in message.Report.BlackTermsFound)
					{
						if (!GlobalInfo.QWhiteName.Terms.ContainsKey(message.Post.Site) || !GlobalInfo.QWhiteName.Terms[message.Post.Site].ContainsTerm(term.Key))
						{
							GlobalInfo.QWhiteName.AddTerm(term.Key, message.Post.Site, message.Report.BlackTermsFound.Values.Max() / 2);
						}
					}

					foreach (var term in message.Report.WhiteTermsFound)
					{
						GlobalInfo.QWhiteName.SetScore(term.Key, message.Post.Site, term.Value + 1);
					}

					MessageHandler.DeleteMessage(message.RepliesToMessageID);

					return "";
				}
			} 
			
			return "`Command not recognised.`";
		}

		private static string TruePositive()
		{
			switch (message.Report.Type)
			{
				case PostType.LowQuality:
				{
					foreach (var blackTerm in message.Report.BlackTermsFound)
					{
						GlobalInfo.QBlackLQ.SetScore(blackTerm.Key, blackTerm.Value + 1);

						foreach (var site in GlobalInfo.QWhiteLQ.Terms)
						{
							for (var i = 0; i < site.Value.Count; i++)
							{
								var whiteTerm = site.Value.ElementAt(i);

								if (whiteTerm.Key.ToString() == blackTerm.Key.ToString() && site.Key != message.Post.Site)
								{
									var oldWhiteScore = GlobalInfo.QWhiteLQ.GetScore(whiteTerm.Key, site.Key);
									var x = oldWhiteScore / blackTerm.Value;

									GlobalInfo.QWhiteLQ.SetScore(whiteTerm.Key, site.Key, x * (blackTerm.Value + 1));
								}
							}
						}
					}		

					return "`TP for message " + message.RepliesToMessageID + " acknowledged.`";
				}

				case PostType.Offensive:
				{
					foreach (var blackTerm in message.Report.BlackTermsFound)
					{
						GlobalInfo.QBlackOff.SetScore(blackTerm.Key, blackTerm.Value + 1);

						foreach (var site in GlobalInfo.QWhiteOff.Terms)
						{
							for (var i = 0; i < site.Value.Count; i++)
							{
								var whiteTerm = site.Value.ElementAt(i);

								if (whiteTerm.Key.ToString() == blackTerm.Key.ToString() && site.Key != message.Post.Site)
								{
									var oldWhiteScore = GlobalInfo.QWhiteOff.GetScore(whiteTerm.Key, site.Key);
									var x = oldWhiteScore / blackTerm.Value;

									GlobalInfo.QWhiteOff.SetScore(whiteTerm.Key, site.Key, x * (blackTerm.Value + 1));
								}
							}
						}
					}

					return "`TP for message " + message.RepliesToMessageID + " acknowledged.`";
				}

				case PostType.Spam:
				{
					foreach (var blackTerm in message.Report.BlackTermsFound)
					{
						GlobalInfo.QBlackSpam.SetScore(blackTerm.Key, blackTerm.Value + 1);

						foreach (var site in GlobalInfo.QWhiteSpam.Terms)
						{
							for (var i = 0; i < site.Value.Count; i++)
							{
								var whiteTerm = site.Value.ElementAt(i);

								if (whiteTerm.Key.ToString() == blackTerm.Key.ToString() && site.Key != message.Post.Site)
								{
									var oldWhiteScore = GlobalInfo.QWhiteSpam.GetScore(whiteTerm.Key, site.Key);
									var x = oldWhiteScore / blackTerm.Value;

									GlobalInfo.QWhiteSpam.SetScore(whiteTerm.Key, site.Key, x * (blackTerm.Value + 1));
								}
							}
						}
					}

					return "`TP for message " + message.RepliesToMessageID + " acknowledged.`";
				}

				case PostType.BadUsername:
				{
					foreach (var blackTerm in message.Report.BlackTermsFound)
					{
						GlobalInfo.QBlackName.SetScore(blackTerm.Key, blackTerm.Value + 1);

						foreach (var site in GlobalInfo.QWhiteName.Terms)
						{
							for (var i = 0; i < site.Value.Count; i++)
							{
								var whiteTerm = site.Value.ElementAt(i);

								if (whiteTerm.Key.ToString() == blackTerm.Key.ToString() && site.Key != message.Post.Site)
								{
									var oldWhiteScore = GlobalInfo.QWhiteName.GetScore(whiteTerm.Key, site.Key);
									var x = oldWhiteScore / blackTerm.Value;

									GlobalInfo.QWhiteName.SetScore(whiteTerm.Key, site.Key, x * (blackTerm.Value + 1));
								}
							}
						}
					}

					return "`TP for message " + message.RepliesToMessageID + " acknowledged.`";
				}
			}

			return "`Command not recognised.`";
		}


		// Owner commands.


		private static string AddUser(string command)
		{
			var id = command.Replace("add user", "").Trim();

			UserAccess.AddUser(int.Parse(id));

			return "`User added.`";
		}


		private static string SetAccuracyThreshold(string command)
		{
			if (command.IndexOf(" ", StringComparison.Ordinal) == -1 || command.All(c => !Char.IsDigit(c))) { return "`Command not recognised.`"; }

			var newLimit = command.Remove(0, 19);

			GlobalInfo.AccuracyThreshold = Single.Parse(newLimit);

			return "`Accuracy threshold updated.`";
		}


		private static string StartBot()
		{
			GlobalInfo.BotRunning = true;

			return "Phamhilator™ started.";
		}

		private static string PauseBot()
		{
			GlobalInfo.BotRunning = false;

			return "Phamhilator™ paused.";
		}
	}
}
