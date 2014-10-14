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
				command = input.Body.Remove(0, GlobalInfo.BotUsername.Length + 1).TrimStart();
			}
			else
			{
				return "";
			}

			commandLower = command.ToLowerInvariant();

			var user = input.AuthorID;

			if (IsNormalUserCommand(commandLower))
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

			if (IsPrivilegedUserCommand(commandLower))
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
			
			if (IsOwnerCommand(commandLower))
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

		public static bool IsValidCommand(string command)
		{
			var commandLower = command.ToLowerInvariant();

			if (commandLower.StartsWith(">>"))
			{
				commandLower = commandLower.Remove(0, 2).TrimStart();
			}
			else if (commandLower.StartsWith("@" + GlobalInfo.BotUsername.ToLowerInvariant()))
			{
				commandLower = commandLower.Remove(0, GlobalInfo.BotUsername.Length + 1).TrimStart();
			}
			else
			{
				return false;
			}

			return IsNormalUserCommand(commandLower) || IsPrivilegedUserCommand(commandLower) || IsOwnerCommand(commandLower);
		}


		private static bool IsOwnerCommand(string command)
		{
			return command.StartsWith("add user") ||
				   command.StartsWith("threshold") ||
				   command == "start" ||
				   command == "pause" ||
				   command == "full scan";
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

			if (commandLower == "full scan")
			{
				return FullScan();
			}

			if (commandLower.StartsWith("threshold"))
			{
				return SetAccuracyThreshold(command);
			}

			return "`Command not recognised.`";
		}


		private static bool IsPrivilegedUserCommand(string command)
		{
			return command == "fp" || command == "fpa" ||
				   command == "tp" || command == "tpa" ||
				   command == "clean" || command == "sanitise" || command == "sanitize" ||
				   command == "del" || command == "delete" || command == "remove" ||
				   command.StartsWith("bqremove term") ||
				   command.StartsWith("bqadd term") ||
				   command.StartsWith("wqremove term") ||
				   command.StartsWith("wqadd term") ||
				   command.StartsWith("baremove term") ||
				   command.StartsWith("baadd term") ||
				   command.StartsWith("waremove term") ||
				   command.StartsWith("waadd term") ||
				   command.StartsWith("add tag") ||
				   command.StartsWith("remove tag");
		}

		private static string PrivilegedUserCommands(string command)
		{
			if (commandLower == "fp" || commandLower == "fpa")
			{
				return FalsePositive();
			}

			if (commandLower == "tp" || commandLower == "tpa")
			{
				return TruePositive();
			}

			if (commandLower == "clean" || commandLower == "sanitise" || commandLower == "sanitize")
			{
				return CleanPost();
			}

			if (commandLower == "del" || commandLower == "delete" || commandLower == "remove")
			{
				return DeletePost();
			}

			if (commandLower.StartsWith("bqremove term"))
			{
				return RemoveQBlackTerm(command);
			}

			if (commandLower.StartsWith("bqadd term"))
			{
				return AddQBlackTerm(command);
			}

			if (commandLower.StartsWith("wqremove term"))
			{
				return RemoveQWhiteTerm(command);
			}

			if (commandLower.StartsWith("wqadd term"))
			{
				return AddAWhiteTerm(command);
			}

			if (commandLower.StartsWith("baremove term"))
			{
				return RemoveABlackTerm(command);
			}

			if (commandLower.StartsWith("baadd term"))
			{
				return AddABlackTerm(command);
			}

			if (commandLower.StartsWith("waremove term"))
			{
				return RemoveAWhiteTerm(command);
			}

			if (commandLower.StartsWith("waadd term"))
			{
				return AddQWhiteTerm(command);
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


		private static bool IsNormalUserCommand(string command)
		{
			return command == "stats" || command == "info" || command == "help" || command == "commands";
		}

		private static string NormalUserCommands()
		{
			if (commandLower == "stats" || commandLower == "info")
			{
				return "`Owners: " + GlobalInfo.Owners + ". Total terms: " + GlobalInfo.TermCount + ". Accuracy threshold: " + GlobalInfo.AccuracyThreshold + "%. Full scan enabled: " + GlobalInfo.EnableFullScan + ". Posts caught over last 7 days: " + GlobalInfo.PostsCaught + ". Uptime: " + (DateTime.UtcNow - GlobalInfo.UpTime) + ".`";
			}

			if (commandLower == "help" || commandLower == "commands")
			{
				return "`See` [`here`](https://github.com/ArcticEcho/Phamhilator/wiki/Chat-Commands) `for a full list of commands.`";
			}

			return "`Command not recognised.`";
		}



		// Privileged user commands.



		private static string AddQBlackTerm(string command)
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

		private static string RemoveQBlackTerm(string command)
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

					GlobalInfo.QBlackLQ.RemoveTerm(term);
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


		private static string AddQWhiteTerm(string command)
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

		private static string RemoveQWhiteTerm(string command)
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


		private static string AddABlackTerm(string command)
		{
			var addCommand = command.Substring(command.IndexOf(" ", command.IndexOf(" ", StringComparison.Ordinal) + 1, StringComparison.Ordinal) + 1);

			if (addCommand.StartsWith("off") || addCommand.StartsWith("spam") || addCommand.StartsWith("lq") || addCommand.StartsWith("name"))
			{
				Regex term;

				if (addCommand.StartsWith("off"))
				{
					term = new Regex(addCommand.Remove(0, 4));

					if (GlobalInfo.ABlackOff.Terms.ContainsTerm(term)) { return "`Term already exists.`"; }

					GlobalInfo.ABlackOff.AddTerm(term);
				}

				if (addCommand.StartsWith("spam"))
				{
					term = new Regex(addCommand.Remove(0, 5));

					if (GlobalInfo.ABlackSpam.Terms.ContainsTerm(term)) { return "`Term already exists.`"; }

					GlobalInfo.ABlackSpam.AddTerm(term);
				}

				if (addCommand.StartsWith("lq"))
				{
					term = new Regex(addCommand.Remove(0, 3));

					if (GlobalInfo.ABlackLQ.Terms.ContainsTerm(term)) { return "`Term already exists.`"; }

					GlobalInfo.ABlackLQ.AddTerm(term);
				}

				if (addCommand.StartsWith("name"))
				{
					term = new Regex(addCommand.Remove(0, 5));

					if (GlobalInfo.ABlackName.Terms.ContainsTerm(term)) { return "`Term already exists.`"; }

					GlobalInfo.ABlackName.AddTerm(term);
				}

				return "`Term added.`";
			}

			return "`Command not recognised.`";
		}

		private static string RemoveABlackTerm(string command)
		{
			var removeCommand = command.Substring(command.IndexOf(" ", command.IndexOf(" ", StringComparison.Ordinal) + 1, StringComparison.Ordinal) + 1);

			if (removeCommand.StartsWith("off") || removeCommand.StartsWith("spam") || removeCommand.StartsWith("lq") || removeCommand.StartsWith("name"))
			{
				Regex term;

				if (removeCommand.StartsWith("off"))
				{
					term = new Regex(removeCommand.Remove(0, 4));

					if (!GlobalInfo.ABlackOff.Terms.ContainsTerm(term)) { return "`Term does not exist.`"; }

					GlobalInfo.ABlackOff.RemoveTerm(term);
				}

				if (removeCommand.StartsWith("spam"))
				{
					term = new Regex(removeCommand.Remove(0, 5));

					if (!GlobalInfo.ABlackSpam.Terms.ContainsTerm(term)) { return "`Term does not exist.`"; }

					GlobalInfo.ABlackSpam.RemoveTerm(term);
				}

				if (removeCommand.StartsWith("lq"))
				{
					term = new Regex(removeCommand.Remove(0, 3));

					if (!GlobalInfo.ABlackLQ.Terms.ContainsTerm(term)) { return "`Term does not exist.`"; }

					GlobalInfo.ABlackLQ.RemoveTerm(term);
				}

				if (removeCommand.StartsWith("name"))
				{
					term = new Regex(removeCommand.Remove(0, 5));

					if (!GlobalInfo.ABlackName.Terms.ContainsTerm(term)) { return "`Term does not exist.`"; }

					GlobalInfo.ABlackName.RemoveTerm(term);
				}

				return "`Term removed.`";
			}

			return "`Command not recognised.`";
		}


		private static string AddAWhiteTerm(string command)
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

					if (GlobalInfo.AWhiteOff.Terms.ContainsKey(site) && GlobalInfo.AWhiteOff.Terms[site].ContainsTerm(term)) { return "`Ignore term already exists.`"; }

					GlobalInfo.AWhiteOff.AddTerm(term, site);
				}

				if (addCommand.StartsWith("spam"))
				{
					addCommand = addCommand.Remove(0, 5);

					if (addCommand.IndexOf(" ", StringComparison.Ordinal) == -1) { return "`Command not recognised.`"; }

					term = new Regex(addCommand.Remove(0, addCommand.IndexOf(" ", StringComparison.Ordinal) + 1));
					site = addCommand.Substring(0, addCommand.IndexOf(" ", StringComparison.Ordinal));

					if (GlobalInfo.AWhiteSpam.Terms.ContainsKey(site) && GlobalInfo.AWhiteSpam.Terms[site].ContainsTerm(term)) { return "`Ignore term already exists.`"; }

					GlobalInfo.AWhiteSpam.AddTerm(term, site);
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

		private static string RemoveAWhiteTerm(string command)
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

					if (!GlobalInfo.AWhiteOff.Terms.ContainsKey(site) && !GlobalInfo.AWhiteOff.Terms[site].ContainsTerm(term)) { return "`Ignore term does not exist.`"; }

					GlobalInfo.AWhiteOff.RemoveTerm(term, site);
				}

				if (removeCommand.StartsWith("spam"))
				{
					removeCommand = removeCommand.Remove(0, 5);

					if (removeCommand.IndexOf(" ", StringComparison.Ordinal) == -1) { return "`Command not recognised.`"; }

					term = new Regex(removeCommand.Remove(0, removeCommand.IndexOf(" ", StringComparison.Ordinal) + 1));
					site = removeCommand.Substring(0, removeCommand.IndexOf(" ", StringComparison.Ordinal));

					if (!GlobalInfo.AWhiteSpam.Terms.ContainsKey(site) && !GlobalInfo.AWhiteSpam.Terms[site].ContainsTerm(term)) { return "`Ignore term does not exist.`"; }

					GlobalInfo.AWhiteSpam.RemoveTerm(term, site);
				}

				if (removeCommand.StartsWith("lq"))
				{
					removeCommand = removeCommand.Remove(0, 3);

					if (removeCommand.IndexOf(" ", StringComparison.Ordinal) == -1) { return "`Command not recognised.`"; }

					term = new Regex(removeCommand.Remove(0, removeCommand.IndexOf(" ", StringComparison.Ordinal) + 1));
					site = removeCommand.Substring(0, removeCommand.IndexOf(" ", StringComparison.Ordinal));

					if (!GlobalInfo.AWhiteLQ.Terms.ContainsKey(site) && !GlobalInfo.AWhiteLQ.Terms[site].ContainsTerm(term)) { return "`Ignore term does not exist.`"; }

					GlobalInfo.AWhiteLQ.RemoveTerm(term, site);
				}

				if (removeCommand.StartsWith("name"))
				{
					removeCommand = removeCommand.Remove(0, 5);

					if (removeCommand.IndexOf(" ", StringComparison.Ordinal) == -1) { return "`Command not recognised.`"; }

					term = new Regex(removeCommand.Remove(0, removeCommand.IndexOf(" ", StringComparison.Ordinal) + 1));
					site = removeCommand.Substring(0, removeCommand.IndexOf(" ", StringComparison.Ordinal));

					if (!GlobalInfo.AWhiteName.Terms.ContainsKey(site) && !GlobalInfo.AWhiteName.Terms[site].ContainsTerm(term)) { return "`Ignore term does not exist.`"; }

					GlobalInfo.AWhiteName.RemoveTerm(term, site);
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

		private static string DeletePost()
		{
			var reportID = message.RepliesToMessageID;

			if (GlobalInfo.PostedReports.ContainsKey(reportID))
			{
				var title = GlobalInfo.PostedReports[reportID].Post.Title;

				MessageHandler.DeleteMessage(title, reportID, false);
			}

			return "";
		}


		private static string FalsePositive()
		{
			return message.IsQuestionReport ? FalsePositiveQuestion() : FalsePositiveAnswer();
		}

		private static string FalsePositiveQuestion()
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

					return MessageHandler.DeleteMessage(message.Post.Title, message.RepliesToMessageID) ? "" : message.MessageID + ": `FP acknowledged.`";
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

					return MessageHandler.DeleteMessage(message.Post.Title, message.RepliesToMessageID) ? "" : message.MessageID + ": `FP acknowledged.`";
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

					return MessageHandler.DeleteMessage(message.Post.Title, message.RepliesToMessageID) ? "" : message.MessageID + ": `FP acknowledged.`";
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

					return MessageHandler.DeleteMessage(message.Post.Title, message.RepliesToMessageID) ? "" : message.MessageID + ": `FP acknowledged.`";
				}
			} 
			
			return "`Command not recognised.`";
		}

		private static string FalsePositiveAnswer()
		{
			switch (message.Report.Type)
			{
				case PostType.LowQuality:
				{
					foreach (var term in message.Report.BlackTermsFound)
					{
						if (!GlobalInfo.AWhiteLQ.Terms.ContainsKey(message.Post.Site) || !GlobalInfo.AWhiteLQ.Terms[message.Post.Site].ContainsTerm(term.Key))
						{
							GlobalInfo.AWhiteLQ.AddTerm(term.Key, message.Post.Site, message.Report.BlackTermsFound.Values.Max() / 2);
						}
					}

					foreach (var term in message.Report.WhiteTermsFound)
					{
						GlobalInfo.AWhiteLQ.SetScore(term.Key, message.Post.Site, term.Value + 1);
					}

					return MessageHandler.DeleteMessage(message.Post.Title, message.RepliesToMessageID) ? "" : message.MessageID + ": `FP acknowledged.`";
				}

				case PostType.Offensive:
				{
					foreach (var term in message.Report.BlackTermsFound)
					{
						if (!GlobalInfo.AWhiteOff.Terms.ContainsKey(message.Post.Site) || !GlobalInfo.AWhiteOff.Terms[message.Post.Site].ContainsTerm(term.Key))
						{
							GlobalInfo.AWhiteOff.AddTerm(term.Key, message.Post.Site, message.Report.BlackTermsFound.Values.Max() / 2);
						}
					}

					foreach (var term in message.Report.WhiteTermsFound)
					{
						GlobalInfo.AWhiteOff.SetScore(term.Key, message.Post.Site, term.Value + 1);
					}

					return MessageHandler.DeleteMessage(message.Post.Title, message.RepliesToMessageID) ? "" : message.MessageID + ": `FP acknowledged.`";
				}

				case PostType.Spam:
				{
					foreach (var term in message.Report.BlackTermsFound)
					{
						if (!GlobalInfo.AWhiteSpam.Terms.ContainsKey(message.Post.Site) || !GlobalInfo.AWhiteSpam.Terms[message.Post.Site].ContainsTerm(term.Key))
						{
							GlobalInfo.AWhiteSpam.AddTerm(term.Key, message.Post.Site, message.Report.BlackTermsFound.Values.Max() / 2);
						}
					}

					foreach (var term in message.Report.WhiteTermsFound)
					{
						GlobalInfo.AWhiteSpam.SetScore(term.Key, message.Post.Site, term.Value + 1);
					}

					return MessageHandler.DeleteMessage(message.Post.Title, message.RepliesToMessageID) ? "" : message.MessageID + ": `FP acknowledged.`";
				}

				case PostType.BadUsername:
				{
					foreach (var term in message.Report.BlackTermsFound)
					{
						if (!GlobalInfo.AWhiteName.Terms.ContainsKey(message.Post.Site) || !GlobalInfo.AWhiteName.Terms[message.Post.Site].ContainsTerm(term.Key))
						{
							GlobalInfo.AWhiteName.AddTerm(term.Key, message.Post.Site, message.Report.BlackTermsFound.Values.Max() / 2);
						}
					}

					foreach (var term in message.Report.WhiteTermsFound)
					{
						GlobalInfo.AWhiteName.SetScore(term.Key, message.Post.Site, term.Value + 1);
					}

					return MessageHandler.DeleteMessage(message.Post.Title, message.RepliesToMessageID) ? "" : message.MessageID + ": `FP acknowledged.`";
				}
			}

			return "`Command not recognised.`";
		}


		private static string TruePositive()
		{
			var returnMessage = message.IsQuestionReport ? TruePositiveQuestion() : TruePositiveAnswer();

			if (commandLower == "tpa")
			{
				var reportMessage = GlobalInfo.PostedReports[message.RepliesToMessageID];

				GlobalInfo.MessagePoster.MessageQueue.Add(reportMessage, GlobalInfo.AnnouncerRoomID);
			}
			
			return returnMessage;
		}

		private static string TruePositiveQuestion()
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

					return message.MessageID + ": `TP acknowledged.`";
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

					return message.MessageID + ": `TP acknowledged.`";
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

					return message.MessageID + ": `TP acknowledged.`";
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

					return message.MessageID + ": `TP acknowledged.`";
				}
			}

			return "`Command not recognised.`";
		}

		private static string TruePositiveAnswer()
		{
			switch (message.Report.Type)
			{
				case PostType.LowQuality:
				{
					foreach (var blackTerm in message.Report.BlackTermsFound)
					{
						GlobalInfo.ABlackLQ.SetScore(blackTerm.Key, blackTerm.Value + 1);

						foreach (var site in GlobalInfo.AWhiteLQ.Terms)
						{
							for (var i = 0; i < site.Value.Count; i++)
							{
								var whiteTerm = site.Value.ElementAt(i);

								if (whiteTerm.Key.ToString() == blackTerm.Key.ToString() && site.Key != message.Post.Site)
								{
									var oldWhiteScore = GlobalInfo.AWhiteLQ.GetScore(whiteTerm.Key, site.Key);
									var x = oldWhiteScore / blackTerm.Value;

									GlobalInfo.AWhiteLQ.SetScore(whiteTerm.Key, site.Key, x * (blackTerm.Value + 1));
								}
							}
						}
					}

					return message.MessageID + ": `TP acknowledged.`";
				}

				case PostType.Offensive:
				{
					foreach (var blackTerm in message.Report.BlackTermsFound)
					{
						GlobalInfo.ABlackOff.SetScore(blackTerm.Key, blackTerm.Value + 1);

						foreach (var site in GlobalInfo.AWhiteOff.Terms)
						{
							for (var i = 0; i < site.Value.Count; i++)
							{
								var whiteTerm = site.Value.ElementAt(i);

								if (whiteTerm.Key.ToString() == blackTerm.Key.ToString() && site.Key != message.Post.Site)
								{
									var oldWhiteScore = GlobalInfo.AWhiteOff.GetScore(whiteTerm.Key, site.Key);
									var x = oldWhiteScore / blackTerm.Value;

									GlobalInfo.AWhiteOff.SetScore(whiteTerm.Key, site.Key, x * (blackTerm.Value + 1));
								}
							}
						}
					}

					return message.MessageID + ": `TP acknowledged.`";
				}

				case PostType.Spam:
				{
					foreach (var blackTerm in message.Report.BlackTermsFound)
					{
						GlobalInfo.ABlackSpam.SetScore(blackTerm.Key, blackTerm.Value + 1);

						foreach (var site in GlobalInfo.AWhiteSpam.Terms)
						{
							for (var i = 0; i < site.Value.Count; i++)
							{
								var whiteTerm = site.Value.ElementAt(i);

								if (whiteTerm.Key.ToString() == blackTerm.Key.ToString() && site.Key != message.Post.Site)
								{
									var oldWhiteScore = GlobalInfo.AWhiteSpam.GetScore(whiteTerm.Key, site.Key);
									var x = oldWhiteScore / blackTerm.Value;

									GlobalInfo.AWhiteSpam.SetScore(whiteTerm.Key, site.Key, x * (blackTerm.Value + 1));
								}
							}
						}
					}

					return message.MessageID + ": `TP acknowledged.`";
				}

				case PostType.BadUsername:
				{
					foreach (var blackTerm in message.Report.BlackTermsFound)
					{
						GlobalInfo.ABlackName.SetScore(blackTerm.Key, blackTerm.Value + 1);

						foreach (var site in GlobalInfo.AWhiteName.Terms)
						{
							for (var i = 0; i < site.Value.Count; i++)
							{
								var whiteTerm = site.Value.ElementAt(i);

								if (whiteTerm.Key.ToString() == blackTerm.Key.ToString() && site.Key != message.Post.Site)
								{
									var oldWhiteScore = GlobalInfo.AWhiteName.GetScore(whiteTerm.Key, site.Key);
									var x = oldWhiteScore / blackTerm.Value;

									GlobalInfo.AWhiteName.SetScore(whiteTerm.Key, site.Key, x * (blackTerm.Value + 1));
								}
							}
						}
					}

					return message.MessageID + ": `TP acknowledged.`";
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


		private static string FullScan()
		{
			if (GlobalInfo.EnableFullScan)
			{
				GlobalInfo.EnableFullScan = false;

				return "`Full scan disabled.`";
			}

			GlobalInfo.EnableFullScan = true;

			return "`Full scan enabled.`";
		}


		private static string StartBot()
		{
			GlobalInfo.BotRunning = true;

			return "`Phamhilator™ started.`";
		}

		private static string PauseBot()
		{
			GlobalInfo.BotRunning = false;

			return "`Phamhilator™ paused.`";
		}
	}
}
