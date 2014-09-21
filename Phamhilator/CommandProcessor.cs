using System;
using System.Text.RegularExpressions;



namespace Phamhilator
{
	public static class CommandProcessor
	{
		private static MessageInfo message;



		public static string ExacuteCommand(MessageInfo input)
		{
			string command;

			if (input.Body.StartsWith(">>"))
			{
				command = input.Body.Remove(0, 2).TrimStart();
			}
			else if (input.Body.ToLowerInvariant().StartsWith("@sam") && GlobalInfo.PostedReports.ContainsKey(input.RepliesToMessageID))
			{
				command = input.Body.Remove(0, 4).TrimStart();
			}
			else
			{
				return "";
			}

			var user = input.AuthorID;

			if (IsNormalUserCommand(command))
			{
				return NormalUserCommands(command);
			}

			if (IsPrivilegedUserCommand(command))
			{
				message = input;

				if (!UserAccess.CommandAccessUsers.Contains(user) && !UserAccess.Owners.Contains(user))
				{
					return "`Access denied.`"; 
				}

				return PrivilegedUserCommands(command);			
			}
			
			if (IsOwnerCommand(command))
			{
				if (!UserAccess.Owners.Contains(user))
				{
					return "`Access denied.`";
				}

				return OwnerCommand(command);
			}

			return "`Command not recognised.`";
		}



		private static bool IsOwnerCommand(string command)
		{
			return command.StartsWith("add user");
		}

		private static string OwnerCommand(string command)
		{
			if (command.StartsWith("add user"))
			{
				return AddUser(command);
			}

			return "`Command not recognised.`";
		}


		private static bool IsPrivilegedUserCommand(string command)
		{
			return command.StartsWith("fp") || command.StartsWith("false") || command.StartsWith("false pos") || command.StartsWith("false positive") ||
				   command.StartsWith("tp") || command.StartsWith("true") || command.StartsWith("true pos") || command.StartsWith("true positive") ||
				   //command.StartsWith("-1") || command.StartsWith("dv") || command.StartsWith("downvote") ||
				   //command.StartsWith("+1") || command.StartsWith("uv") || command.StartsWith("upvote") ||
			       command.StartsWith("bremove term") ||
			       command.StartsWith("badd term") ||
			       command.StartsWith("wremove term") ||
			       command.StartsWith("wadd term") ||
			       command.StartsWith("add tag") ||
			       command.StartsWith("remove tag") ||
			       command.StartsWith("start") ||
			       command.StartsWith("pause");
		}

		private static string PrivilegedUserCommands(string command)
		{
			if (command.StartsWith("fp") || command.StartsWith("false") || command.StartsWith("false pos") || command.StartsWith("false positive"))
			{
				return FalsePositive();
			}
			
			if (command.StartsWith("tp") || command.StartsWith("true") || command.StartsWith("true pos") || command.StartsWith("true positive"))
			{
				return TruePositive();
			}

			if (command.StartsWith("bremove term"))
			{
				return RemoveBlackTerm(command);
			}

			if (command.StartsWith("badd term"))
			{
				return AddBlackTerm(command);
			}

			if (command.StartsWith("wremove term"))
			{
				return RemoveWhiteTerm(command);
			}

			if (command.StartsWith("wadd term"))
			{
				return AddWhiteTerm(command);
			}

			if (command.StartsWith("start"))
			{
				return StartBot();
			}

			if (command.StartsWith("pause"))
			{
				return PauseBot();
			}

			if (command.StartsWith("add tag"))
			{
				return AddTag(command);
			}

			if (command.StartsWith("remove tag"))
			{
				return RemoveTag(command);
			}

			return "`Command not recognised.`";
		}


		private static bool IsNormalUserCommand(string command)
		{
			return command == "stats" || command == "info" || command == "help" || command == "commands";
		}

		private static string NormalUserCommands(string command)
		{
			if (command == "stats" || command == "info")
			{
				return "`Owners: " + GlobalInfo.Owners + ". Total terms: " + GlobalInfo.TermCount + ". Posts caught over last 2 days: " + GlobalInfo.PostsCaught + ". Uptime: " + (DateTime.UtcNow - GlobalInfo.UpTime) + ".`";
			}

			if (command == "help" || command == "commands")
			{
				return "`See` [`here`](https://github.com/ArcticWinter/Phamhilator/blob/master/Phamhilator/Readme%20-%20Chat%20Commands.md) `for a full list of commands.`";
			}

			return "`Command not recognised.`";
		}


		private static string AddBlackTerm(string command)
		{
			var addCommand = command.Substring(command.IndexOf(" ", command.IndexOf(" ", StringComparison.Ordinal) + 1, StringComparison.Ordinal) + 1);

			if (addCommand.StartsWith("off") || addCommand.StartsWith("spam") || addCommand.StartsWith("lq") || addCommand.StartsWith("name"))
			{
				Regex term;

				if (addCommand.StartsWith("off"))
				{
					term = new Regex(addCommand.Remove(0, 4));

					if (GlobalInfo.BlackOff.Terms.ContainsTerm(term)) { return "`Term already exists.`"; }

					GlobalInfo.BlackOff.AddTerm(term);
				}

				if (addCommand.StartsWith("spam"))
				{
					term = new Regex(addCommand.Remove(0, 5));

					if (GlobalInfo.BlackSpam.Terms.ContainsTerm(term)) { return "`Term already exists.`"; }

					GlobalInfo.BlackSpam.AddTerm(term);
				}

				if (addCommand.StartsWith("lq"))
				{
					term = new Regex(addCommand.Remove(0, 3));

					if (GlobalInfo.BlackLQ.Terms.ContainsTerm(term)) { return "`Term already exists.`"; }

					GlobalInfo.BlackLQ.AddTerm(term);
				}

				if (addCommand.StartsWith("name"))
				{
					term = new Regex(addCommand.Remove(0, 5));

					if (GlobalInfo.BlackName.Terms.ContainsTerm(term)) { return "`Term already exists.`"; }

					GlobalInfo.BlackName.AddTerm(term);
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

					if (!GlobalInfo.BlackOff.Terms.ContainsTerm(term)) { return "`Term does not exist.`"; }

					GlobalInfo.BlackOff.RemoveTerm(term);
				}

				if (removeCommand.StartsWith("spam"))
				{
					term = new Regex(removeCommand.Remove(0, 5));

					if (!GlobalInfo.BlackSpam.Terms.ContainsTerm(term)) { return "`Term does not exist.`"; }

					GlobalInfo.BlackSpam.RemoveTerm(term);
				}

				if (removeCommand.StartsWith("lq"))
				{
					term = new Regex(removeCommand.Remove(0, 3));

					if (!GlobalInfo.BlackLQ.Terms.ContainsTerm(term)) { return "`Term does not exist.`"; }

					GlobalInfo.BlackLQ.RemoveTerm( term);
				}

				if (removeCommand.StartsWith("name"))
				{
					term = new Regex(removeCommand.Remove(0, 5));

					if (!GlobalInfo.BlackName.Terms.ContainsTerm(term)) { return "`Term does not exist.`"; }

					GlobalInfo.BlackName.RemoveTerm(term);
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

					if (GlobalInfo.WhiteOff.Terms.ContainsKey(site) && GlobalInfo.WhiteOff.Terms[site].ContainsTerm(term)) { return "`Ignore term already exists.`"; }

					GlobalInfo.WhiteOff.AddTerm(term, site);
				}

				if (addCommand.StartsWith("spam"))
				{
					addCommand = addCommand.Remove(0, 5);

					if (addCommand.IndexOf(" ", StringComparison.Ordinal) == -1) { return "`Command not recognised.`"; }

					term = new Regex(addCommand.Remove(0, addCommand.IndexOf(" ", StringComparison.Ordinal) + 1));
					site = addCommand.Substring(0, addCommand.IndexOf(" ", StringComparison.Ordinal));

					if (GlobalInfo.WhiteSpam.Terms.ContainsKey(site) && GlobalInfo.WhiteSpam.Terms[site].ContainsTerm(term)) { return "`Ignore term already exists.`"; }

					GlobalInfo.WhiteSpam.AddTerm(term, site);
				}

				if (addCommand.StartsWith("lq"))
				{
					addCommand = addCommand.Remove(0, 3);

					if (addCommand.IndexOf(" ", StringComparison.Ordinal) == -1) { return "`Command not recognised.`"; }

					term = new Regex(addCommand.Remove(0, addCommand.IndexOf(" ", StringComparison.Ordinal) + 1));
					site = addCommand.Substring(0, addCommand.IndexOf(" ", StringComparison.Ordinal));

					if (GlobalInfo.WhiteLQ.Terms.ContainsKey(site) && GlobalInfo.WhiteLQ.Terms[site].ContainsTerm(term)) { return "`Ignore term already exists.`"; }

					GlobalInfo.WhiteSpam.AddTerm(term, site);
				}

				if (addCommand.StartsWith("name"))
				{
					addCommand = addCommand.Remove(0, 5);

					if (addCommand.IndexOf(" ", StringComparison.Ordinal) == -1) { return "`Command not recognised.`"; }

					term = new Regex(addCommand.Remove(0, addCommand.IndexOf(" ", StringComparison.Ordinal) + 1));
					site = addCommand.Substring(0, addCommand.IndexOf(" ", StringComparison.Ordinal));

					if (GlobalInfo.WhiteName.Terms.ContainsKey(site) && GlobalInfo.WhiteName.Terms[site].ContainsTerm(term)) { return "`Ignore term already exists.`"; }

					GlobalInfo.WhiteName.AddTerm(term, site);
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

					if (!GlobalInfo.WhiteOff.Terms.ContainsKey(site) && !GlobalInfo.WhiteOff.Terms[site].ContainsTerm(term)) { return "`Ignore term does not exist.`"; }

					GlobalInfo.WhiteOff.RemoveTerm(term, site);
				}

				if (removeCommand.StartsWith("spam"))
				{
					removeCommand = removeCommand.Remove(0, 5);

					if (removeCommand.IndexOf(" ", StringComparison.Ordinal) == -1) { return "`Command not recognised.`"; }

					term = new Regex(removeCommand.Remove(0, removeCommand.IndexOf(" ", StringComparison.Ordinal) + 1));
					site = removeCommand.Substring(0, removeCommand.IndexOf(" ", StringComparison.Ordinal));

					if (!GlobalInfo.WhiteSpam.Terms.ContainsKey(site) && !GlobalInfo.WhiteSpam.Terms[site].ContainsTerm(term)) { return "`Ignore term does not exist.`"; }

					GlobalInfo.WhiteSpam.RemoveTerm(term, site);
				}

				if (removeCommand.StartsWith("lq"))
				{
					removeCommand = removeCommand.Remove(0, 3);

					if (removeCommand.IndexOf(" ", StringComparison.Ordinal) == -1) { return "`Command not recognised.`"; }

					term = new Regex(removeCommand.Remove(0, removeCommand.IndexOf(" ", StringComparison.Ordinal) + 1));
					site = removeCommand.Substring(0, removeCommand.IndexOf(" ", StringComparison.Ordinal));

					if (!GlobalInfo.WhiteLQ.Terms.ContainsKey(site) && !GlobalInfo.WhiteLQ.Terms[site].ContainsTerm(term)) { return "`Ignore term does not exist.`"; }

					GlobalInfo.WhiteLQ.RemoveTerm(term, site);
				}

				if (removeCommand.StartsWith("name"))
				{
					removeCommand = removeCommand.Remove(0, 5);

					if (removeCommand.IndexOf(" ", StringComparison.Ordinal) == -1) { return "`Command not recognised.`"; }

					term = new Regex(removeCommand.Remove(0, removeCommand.IndexOf(" ", StringComparison.Ordinal) + 1));
					site = removeCommand.Substring(0, removeCommand.IndexOf(" ", StringComparison.Ordinal));

					if (!GlobalInfo.WhiteName.Terms.ContainsKey(site) && !GlobalInfo.WhiteName.Terms[site].ContainsTerm(term)) { return "`Ignore term does not exist.`"; }

					GlobalInfo.WhiteName.RemoveTerm(term, site);
				}

				return "`Ignore Term removed.`";
			}

			return "`Command not recognised.`";
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


		private static string AddTag(string command)
		{
			var tagCommand = command.Remove(0, command.IndexOf("tag", StringComparison.Ordinal) + 4);

			var site = tagCommand.Substring(0, tagCommand.IndexOf(" ", StringComparison.Ordinal));
			var tag = tagCommand.Remove(0, tagCommand.IndexOf(" ", StringComparison.Ordinal) + 1);

			if (BadTagDefinitions.BadTags.ContainsKey(site) && BadTagDefinitions.BadTags[site].Contains(tag)) { return "`Tag already exists.`"; }

			BadTagDefinitions.AddTag(site, tag);

			return "`Tag added.`";
		}

		private static string RemoveTag(string command)
		{
			var tagCommand = command.Remove(0, command.IndexOf("tag", StringComparison.Ordinal) + 4);

			var site = tagCommand.Substring(0, tagCommand.IndexOf(" ", StringComparison.Ordinal));
			var tag = tagCommand.Remove(0, tagCommand.IndexOf(" ", StringComparison.Ordinal) + 1);

			if (BadTagDefinitions.BadTags.ContainsKey(site))
			{
				if (BadTagDefinitions.BadTags[site].Contains(tag))
				{
					BadTagDefinitions.RemoveTag(site, tag);

					return "`Tag removed.`";
				}

				return "`Tag does not exist.`";
			}

			return "`Site does not exist.`";
		}


		private static string FalsePositive()
		{
			switch (message.Report.Type)
			{
				case PostType.LowQuality:
				{
					foreach (var term in message.Report.TermsFound)
					{
						if (GlobalInfo.WhiteLQ.Terms.ContainsKey(message.Post.Site) && GlobalInfo.WhiteLQ.Terms[message.Post.Site].ContainsTerm(term))
						{
							GlobalInfo.WhiteLQ.SetScore(term, message.Post.Site, GlobalInfo.WhiteLQ.GetScore(term, message.Post.Site) + 1);
						}

						if (GlobalInfo.BlackLQ.Terms.ContainsTerm(term))
						{
							if (!GlobalInfo.WhiteLQ.Terms.ContainsKey(message.Post.Site) || !GlobalInfo.WhiteLQ.Terms[message.Post.Site].ContainsTerm(term))
							{
								GlobalInfo.WhiteLQ.AddTerm(term, message.Post.Site, 5);
							}
						}
					}

					return "`FP for message " + message.RepliesToMessageID + " acknowledged.`";
				}

				case PostType.Offensive:
				{

					foreach (var term in message.Report.TermsFound)
					{
						if (GlobalInfo.WhiteOff.Terms.ContainsKey(message.Post.Site) && GlobalInfo.WhiteOff.Terms[message.Post.Site].ContainsTerm(term))
						{
							GlobalInfo.WhiteOff.SetScore(term, message.Post.Site, GlobalInfo.WhiteOff.GetScore(term, message.Post.Site) + 1);
						}

						if (GlobalInfo.BlackOff.Terms.ContainsTerm(term))
						{
							if (!GlobalInfo.WhiteOff.Terms.ContainsKey(message.Post.Site) || !GlobalInfo.WhiteOff.Terms[message.Post.Site].ContainsTerm(term))
							{
								GlobalInfo.WhiteOff.AddTerm(term, message.Post.Site, 5);
							}
						}
					}

					return "`FP for message " + message.RepliesToMessageID + " acknowledged.`";
				}

				case PostType.Spam:
				{
					foreach (var term in message.Report.TermsFound)
					{
						if (GlobalInfo.WhiteSpam.Terms.ContainsKey(message.Post.Site) && GlobalInfo.WhiteSpam.Terms[message.Post.Site].ContainsTerm(term))
						{
							GlobalInfo.WhiteSpam.SetScore(term, message.Post.Site, GlobalInfo.WhiteSpam.GetScore(term, message.Post.Site) + 1);
						}

						if (GlobalInfo.BlackSpam.Terms.ContainsTerm(term))
						{
							if (!GlobalInfo.WhiteSpam.Terms.ContainsKey(message.Post.Site) || !GlobalInfo.WhiteSpam.Terms[message.Post.Site].ContainsTerm(term))
							{
								GlobalInfo.WhiteSpam.AddTerm(term, message.Post.Site, 5);
							}
						}			
					}

					return "`FP for message " + message.RepliesToMessageID + " acknowledged.`";
				}

				case PostType.BadUsername:
				{
					foreach (var term in message.Report.TermsFound)
					{
						if (GlobalInfo.WhiteName.Terms.ContainsKey(message.Post.Site) && GlobalInfo.WhiteName.Terms[message.Post.Site].ContainsTerm(term))
						{
							GlobalInfo.WhiteName.SetScore(term, message.Post.Site, GlobalInfo.WhiteName.GetScore(term, message.Post.Site) + 1);
						}

						if (GlobalInfo.BlackName.Terms.ContainsTerm(term))
						{
							if (!GlobalInfo.WhiteName.Terms.ContainsKey(message.Post.Site) || !GlobalInfo.WhiteName.Terms[message.Post.Site].ContainsTerm(term))
							{
								GlobalInfo.WhiteName.AddTerm(term, message.Post.Site, 5);
							}
						}
					}

					return "`FP for message " + message.RepliesToMessageID + " acknowledged.`";
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
					foreach (var term in message.Report.TermsFound)
					{
						if (GlobalInfo.BlackLQ.Terms.ContainsTerm(term))
						{
							GlobalInfo.BlackLQ.SetScore(term, GlobalInfo.BlackLQ.GetScore(term) + 1);
						}

						if (GlobalInfo.WhiteLQ.Terms.ContainsKey(message.Post.Site) && GlobalInfo.WhiteLQ.Terms[message.Post.Site].ContainsTerm(term))
						{
							GlobalInfo.WhiteLQ.SetScore(term, message.Post.Site, GlobalInfo.WhiteLQ.GetScore(term, message.Post.Site) - 1);
						}
					}

					return "`TP for message " + message.RepliesToMessageID + " acknowledged.`";
				}

				case PostType.Offensive:
				{
					foreach (var term in message.Report.TermsFound)
					{
						if (GlobalInfo.BlackOff.Terms.ContainsTerm(term))
						{
							GlobalInfo.BlackOff.SetScore(term, GlobalInfo.BlackOff.GetScore(term) + 1);
						}

						if (GlobalInfo.WhiteOff.Terms.ContainsKey(message.Post.Site) && GlobalInfo.WhiteOff.Terms[message.Post.Site].ContainsTerm(term))
						{
							GlobalInfo.WhiteOff.SetScore(term, message.Post.Site, GlobalInfo.WhiteOff.GetScore(term, message.Post.Site) - 1);
						}
					}

					return "`TP for message " + message.RepliesToMessageID + " acknowledged.`";
				}

				case PostType.Spam:
				{
					foreach (var term in message.Report.TermsFound)
					{
						if (GlobalInfo.BlackSpam.Terms.ContainsTerm(term))
						{
							GlobalInfo.BlackSpam.SetScore(term, GlobalInfo.BlackSpam.GetScore(term) + 1);
						}

						if (GlobalInfo.WhiteSpam.Terms.ContainsKey(message.Post.Site) && GlobalInfo.WhiteSpam.Terms[message.Post.Site].ContainsTerm(term))
						{
							GlobalInfo.WhiteSpam.SetScore(term, message.Post.Site, GlobalInfo.WhiteSpam.GetScore(term, message.Post.Site) - 1);
						}
					}

					return "`TP for message " + message.RepliesToMessageID + " acknowledged.`";
				}

				case PostType.BadUsername:
				{
					foreach (var term in message.Report.TermsFound)
					{
						if (GlobalInfo.BlackName.Terms.ContainsTerm(term))
						{
							GlobalInfo.BlackName.SetScore(term, GlobalInfo.BlackName.GetScore(term) + 1);
						}

						if (GlobalInfo.WhiteName.Terms.ContainsKey(message.Post.Site) && GlobalInfo.WhiteName.Terms[message.Post.Site].ContainsTerm(term))
						{
							GlobalInfo.WhiteName.SetScore(term, message.Post.Site, GlobalInfo.WhiteName.GetScore(term, message.Post.Site) - 1);
						}
					}

					return "`TP for message " + message.RepliesToMessageID + " acknowledged.`";
				}
			}

			return "`Command not recognised.`";
		}


		private static string AddUser(string command)
		{
			var id = command.Replace("add user", "").Trim();

			UserAccess.AddUser(int.Parse(id));

			return "`User added.`";
		}
	}
}
