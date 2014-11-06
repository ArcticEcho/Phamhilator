using System;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;



namespace Phamhilator
{
	public static class CommandProcessor
	{
		private static MessageInfo message;
		private static string commandLower = "";
		private static readonly Regex termCommands = new Regex(@"(?i)^(add|del|edit|auto)\-(b|w)\-(a|qb|qt)\-(spam|off|name|lq)(\-p)? ", RegexOptions.Compiled);



		public static string[] ExacuteCommand(MessageInfo input)
		{
			if (BannedUsers.IsUserBanned(input.AuthorID.ToString(CultureInfo.InvariantCulture))) { return new[] { "" }; }

			string command;

			if (input.Body.StartsWith(">>"))
			{
				command = input.Body.Remove(0, 2).TrimStart();
			}
			else if (input.Body.ToLowerInvariant().StartsWith("@" + GlobalInfo.BotUsername.ToLowerInvariant()))
			{
				if (GlobalInfo.PostedReports.ContainsKey(input.RepliesToMessageID))
				{
					command = input.Body.Remove(0, GlobalInfo.BotUsername.Length + 1).TrimStart();
				}
				else
				{
					return new[] { input.RoomID == GlobalInfo.ChatRoomID ? "`Unable to locate message ID.`" : "" };
				}
			}
			else
			{
				return new[] { "" };
			}

			commandLower = command.ToLowerInvariant();

			var user = input.AuthorID;
			message = input;

			if (IsNormalUserCommand(commandLower))
			{
				try
				{
					return NormalUserCommands();
				}
				catch (Exception)
				{
					return new[] { "`Error executing command.`" };
				}
			}

			if (IsPrivilegedUserCommand(commandLower))
			{
				if (!UserAccess.CommandAccessUsers.Contains(user) && !UserAccess.Owners.Contains(user))
				{
					return new[] { "`Access denied.`" };
				}

				try
				{
					return PrivilegedUserCommands(command);
				}
				catch (Exception ex)
				{
					return new[] { "`Error executing command. Reason: " + ex.Message };
				}		
			}
			
			if (IsOwnerCommand(commandLower))
			{
				if (!UserAccess.Owners.Contains(user))
				{
					return new[] { "`Access denied.`" };
				}
				
				try
				{
					return OwnerCommand(command);
				}
				catch (Exception)
				{
					return new[] { "`Error executing command.`" };
				}
			}

			return new[] { "`Command not recognised.`" };
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
			return command.StartsWith("ban user") ||
				   command.StartsWith("add user") ||
				   command.StartsWith("threshold") ||
				   command.StartsWith("set status") ||
				   command == "start" ||
				   command == "pause" ||
				   command == "full scan";
		}

		private static string[] OwnerCommand(string command)
		{
			if (commandLower.StartsWith("add user"))
			{
				return new[] { AddUser(command) };
			}

			if (command.StartsWith("ban user"))
			{
				return new[] { BanUser(command) };
			}

			if (commandLower == "start")
			{
				return new[] { StartBot() };
			}

			if (commandLower == "pause")
			{
				return new[] { PauseBot() };
			}

			if (commandLower == "full scan")
			{
				return new[] { FullScan() };
			}

			if (commandLower.StartsWith("threshold"))
			{
				return new[] { SetAccuracyThreshold(command) };
			}

			if (commandLower.StartsWith("set status"))
			{
				return new[] { SetStatus(command) };
			}

			return new[] { "`Command not recognised.`" };
		}


		private static bool IsPrivilegedUserCommand(string command)
		{
			return command == "fp" || command == "fp why" ||
				   command == "tp" || command == "tpa" || command == "tp why" || command == "tpa why" ||
				   command == "clean" || command == "sanitise" || command == "sanitize" ||
				   command == "del" || command == "delete" || command == "remove" ||
				   command.StartsWith("remove tag") || command.StartsWith("add tag") ||
				   termCommands.IsMatch(command);
		}

		private static string[] PrivilegedUserCommands(string command)
		{
			# region Edit term commands.

			if (commandLower.StartsWith("edit-b-qt"))
			{
				return new[] { EditBQTTerm(command) };
			}

			if (commandLower.StartsWith("edit-w-qt"))
			{
				return new[] { EditWQTTerm(command) };
			}

			if (commandLower.StartsWith("edit-b-qb"))
			{
				return new[] { EditBQBTerm(command) };
			}

			if (commandLower.StartsWith("edit-w-qb"))
			{
				return new[] { EditWQBTerm(command) };
			}

			if (commandLower.StartsWith("edit-b-a"))
			{
				return new[] { EditBATerm(command) };
			}

			if (commandLower.StartsWith("edit-w-a"))
			{
				return new[] { EditWATerm(command) };
			}

			# endregion

			# region QT term commands.

			if (commandLower.StartsWith("del-b-qt"))
			{
				return new[] { RemoveBQTTerm(command) };
			}

			if (commandLower.StartsWith("add-b-qt"))
			{
				return new[] { AddBQTTerm(command) };
			}

			if (commandLower.StartsWith("del-w-qt"))
			{
				return new[] { RemoveWQTTerm(command) };
			}

			if (commandLower.StartsWith("add-w-qt"))
			{
				return new[] { AddWQTTerm(command) };
			}

			#endregion

			#region QB term commands.

			if (commandLower.StartsWith("del-b-qb"))
			{
				return new[] { RemoveBQBTerm(command) };
			}

			if (commandLower.StartsWith("add-b-qb"))
			{
				return new[] { AddBQBTerm(command) };
			}

			if (commandLower.StartsWith("del-w-qb"))
			{
				return new[] { RemoveWQBTerm(command) };
			}

			if (commandLower.StartsWith("add-w-qb"))
			{
				return new[] { AddWQBTerm(command) };
			}

			# endregion

			# region A term commands.

			if (commandLower.StartsWith("del-b-a"))
			{
				return new[] { RemoveBATerm(command) };
			}

			if (commandLower.StartsWith("add-b-a"))
			{
				return new[] { AddBATerm(command) };
			}

			if (commandLower.StartsWith("del-w-a"))
			{
				return new[] { RemoveWATerm(command) };
			}

			if (commandLower.StartsWith("add-w-a"))
			{
				return new[] { AddWATerm(command) };
			}

			# endregion

			# region FP/TP(A) commands.

			if (commandLower == "fp")
			{
				return new[] { FalsePositive() };
			}

			if (commandLower == "fp why")
			{
				return new[] { FalsePositive(), GetTerms() };
			}

			if (commandLower == "tp" || commandLower == "tpa")
			{
				return new[] { TruePositive() };
			}

			if (commandLower == "tp why" || commandLower == "tpa why")
			{
				return new[] { TruePositive(), GetTerms() };
			}

			# endregion

			# region Auto commands

			if (commandLower.StartsWith("auto-b-qt"))
			{
				return new[] { AutoBQTTerm(command) };
			}

			if (commandLower.StartsWith("auto-w-qt"))
			{
				return new[] { AutoWQTTerm(command) };
			}

			if (commandLower.StartsWith("auto-b-qb"))
			{
				return new[] { AutoBQBTerm(command) };
			}

			if (commandLower.StartsWith("auto-w-qb"))
			{
				return new[] { AutoWQBTerm(command) };
			}

			if (commandLower.StartsWith("auto-b-a"))
			{
				return new[] { AutoBATerm(command) };
			}

			if (commandLower.StartsWith("auto-w-a"))
			{
				return new[] { AutoWATerm(command) };
			}

			# endregion

			if (commandLower.StartsWith("add tag"))
			{
				return new[] { AddTag(command) };
			}

			if (commandLower.StartsWith("remove tag"))
			{
				return new[] { RemoveTag(command) };
			}

			if (commandLower == "clean" || commandLower == "sanitise" || commandLower == "sanitize")
			{
				return new[] { CleanPost() };
			}

			if (commandLower == "del" || commandLower == "delete" || commandLower == "remove")
			{
				return new[] { DeletePost() };
			}

			return new[] { "`Command not recognised.`" };
		}


		private static bool IsNormalUserCommand(string command)
		{
			return command == "stats" || command == "info" || command == "help" || 
				   command == "commands" || command == "status" || 
				   command == "terms" || command == "why";
		}

		private static string[] NormalUserCommands()
		{
			if (commandLower == "info")
			{
				return new[] { GetInfo() };
			}

			if (commandLower == "stats")
			{
				return new[] { GetStats() };
			}

			if (commandLower == "help" || commandLower == "commands")
			{
				return new[] { GetHelp() };
			}

			if (commandLower == "status")
			{
				return new[] { GetStatus() };
			}

			if (commandLower == "terms" || commandLower == "why")
			{
				return new[] { GetTerms() };
			}

			return new[] { "`Command not recognised.`" };
		}



		# region Normal user commands.

		private static string GetStatus()
		{
			return "`Current status: " + GlobalInfo.Status + "`.";
		}

		private static string GetHelp()
		{
			return "`See` [`here`](https://github.com/ArcticEcho/Phamhilator/wiki/Chat-Commands) `for a full list of commands.`";
		}

		private static string GetInfo()
		{
			return "[`Phamhilator`](https://github.com/ArcticEcho/Phamhilator/wiki) `is a` [`.NET`](http://en.wikipedia.org/wiki/.NET_Framework)-`based` [`internet bot`](http://en.wikipedia.org/wiki/Internet_bot) `written in` [`C#`](http://stackoverflow.com/questions/tagged/c%23) `which watches over` [`the /realtime tab`](http://stackexchange.com/questions?tab=realtime) `of` [`Stack Exchange`](http://stackexchange.com/)`. Owners: " + GlobalInfo.Owners + ".`";
		}

		private static string GetStats()
		{
			var ignorePercent = Math.Round(((GlobalInfo.Stats.TotalCheckedPosts - (GlobalInfo.Stats.TotalFPCount + GlobalInfo.Stats.TotalTPCount)) / GlobalInfo.Stats.TotalCheckedPosts) * 100, 1);

			return "`Total terms: " + GlobalInfo.TermCount + ". Posts caught: " + GlobalInfo.PostsCaught + " (last 7 days), " + GlobalInfo.Stats.TotalCheckedPosts + " (total). " + "Reports ignored: " + ignorePercent + "% . Uptime: " + (DateTime.UtcNow - GlobalInfo.UpTime) + ".`";
		}

		private static string GetTerms()
		{
			var builder = new StringBuilder("`Term(s) found: ");

			var report = GlobalInfo.PostedReports[message.RepliesToMessageID];

			foreach (var term in report.Report.BlackTermsFound)
			{
				if (term.TPCount + term.FPCount >= 5)
				{
					builder.Append("(Sensitivity: " + Math.Round(term.Sensitivity * 100, 1));
					builder.Append("%. Specificity: " + Math.Round(term.Specificity * 100, 1));
					builder.Append("%. Ignored: " + Math.Round((term.IgnoredCount / term.CaughtCount) * 100, 1));
					builder.Append("%. Score: " + Math.Round(term.Score, 1) + ") " + term.Regex + "   ");
				}
				else
				{
					builder.Append("(Ignored: " + Math.Round((term.IgnoredCount / term.CaughtCount) * 100, 1));
					builder.Append("%. Score: " + Math.Round(term.Score, 1) + ") " + term.Regex + "   ");
				}		
			}

			var stats = builder.ToString().Trim() + "`";

			return ":" + message.MessageID + " " + stats;
		}

		#endregion

		# region Privileged user commands.

		# region Add term commands.

		private static string AddBQTTerm(string command)
		{
			var addCommand = command.Remove(0, 9);
			var term = new Regex(addCommand.Substring(addCommand.IndexOf(' ') + 1), RegexOptions.Compiled);

			if (term.IsFlakRegex()) { return "`ReDoS detected (term not added).`"; }

			switch (addCommand.ToLowerInvariant()[0])
			{
				case 'o':
				{
					if (GlobalInfo.BlackFilters[FilterType.QuestionTitleBlackOff].Terms.Contains(term)) { return "`Blacklist term already exists.`"; }

					GlobalInfo.BlackFilters[FilterType.QuestionTitleBlackOff].AddTerm(new Term(FilterType.QuestionTitleBlackOff, term, GlobalInfo.BlackFilters[FilterType.QuestionTitleBlackOff].AverageScore));

					break;
				}

				case 's':
				{
					if (GlobalInfo.BlackFilters[FilterType.QuestionTitleBlackSpam].Terms.Contains(term)) { return "`Blacklist term already exists.`"; }

					GlobalInfo.BlackFilters[FilterType.QuestionTitleBlackSpam].AddTerm(new Term(FilterType.QuestionTitleBlackSpam, term, GlobalInfo.BlackFilters[FilterType.QuestionTitleBlackSpam].AverageScore));

					break;
				}

				case 'l':
				{
					if (GlobalInfo.BlackFilters[FilterType.QuestionTitleBlackLQ].Terms.Contains(term)) { return "`Blacklist term already exists.`"; }

					GlobalInfo.BlackFilters[FilterType.QuestionTitleBlackLQ].AddTerm(new Term(FilterType.QuestionTitleBlackLQ, term, GlobalInfo.BlackFilters[FilterType.QuestionTitleBlackLQ].AverageScore));

					break;
				}

				case 'n':
				{
					if (GlobalInfo.BlackFilters[FilterType.QuestionTitleBlackName].Terms.Contains(term)) { return "`Blacklist term already exists.`"; }

					GlobalInfo.BlackFilters[FilterType.QuestionTitleBlackName].AddTerm(new Term(FilterType.QuestionTitleBlackName, term, GlobalInfo.BlackFilters[FilterType.QuestionTitleBlackName].AverageScore));

					break;
				}
			}

			return ":" + message.MessageID + " `Blacklist term added.`";
		}

		private static string AddWQTTerm(string command)
		{
			var addCommand = command.Remove(0, 9);
			var firstSpace = command.IndexOf(' ');
			var secondSpace = command.IndexOf(' ', firstSpace + 1);
			var term = new Regex(command.Substring(secondSpace + 1));
			var site = command.Substring(firstSpace + 1, secondSpace - firstSpace - 1);

			if (term.IsFlakRegex()) { return "`ReDoS detected (term not added).`"; }

			switch (addCommand.ToLowerInvariant()[0])
			{
				case 'o':
				{
					if (GlobalInfo.WhiteFilters[FilterType.QuestionTitleWhiteOff].Terms.Contains(term, site)) { return "`Whitelist term already exists.`"; }

					var score = GlobalInfo.WhiteFilters[FilterType.QuestionTitleWhiteOff].Terms.Where(t => t.Site == site).Select(t => t.Score).Average();

					GlobalInfo.WhiteFilters[FilterType.QuestionTitleWhiteOff].AddTerm(new Term(FilterType.QuestionTitleWhiteOff, term, score, site));

					break;
				}

				case 's':
				{
					if (GlobalInfo.WhiteFilters[FilterType.QuestionTitleWhiteSpam].Terms.Contains(term, site)) { return "`Whitelist term already exists.`"; }

					var score = GlobalInfo.WhiteFilters[FilterType.QuestionTitleWhiteSpam].Terms.Where(t => t.Site == site).Select(t => t.Score).Average();

					GlobalInfo.WhiteFilters[FilterType.QuestionTitleWhiteSpam].AddTerm(new Term(FilterType.QuestionTitleWhiteSpam, term, score, site));

					break;
				}

				case 'l':
				{
					if (GlobalInfo.WhiteFilters[FilterType.QuestionTitleWhiteLQ].Terms.Contains(term, site)) { return "`Whitelist term already exists.`"; }

					var score = GlobalInfo.WhiteFilters[FilterType.QuestionTitleWhiteLQ].Terms.Where(t => t.Site == site).Select(t => t.Score).Average();

					GlobalInfo.WhiteFilters[FilterType.QuestionTitleWhiteLQ].AddTerm(new Term(FilterType.QuestionTitleWhiteLQ, term, score, site));

					break;
				}

				case 'n':
				{
					if (GlobalInfo.WhiteFilters[FilterType.QuestionTitleWhiteName].Terms.Contains(term, site)) { return "`Whitelist term already exists.`"; }

					var score = GlobalInfo.WhiteFilters[FilterType.QuestionTitleWhiteName].Terms.Where(t => t.Site == site).Select(t => t.Score).Average();

					GlobalInfo.WhiteFilters[FilterType.QuestionTitleWhiteName].AddTerm(new Term(FilterType.QuestionTitleWhiteName, term, score, site));

					break;
				}
			}

			return ":" + message.MessageID + " `Whitelist term added.`";
		}

		private static string AddBQBTerm(string command)
		{
			var addCommand = command.Remove(0, 9);
			var term = new Regex(addCommand.Substring(addCommand.IndexOf(' ') + 1), RegexOptions.Compiled);

			if (term.IsFlakRegex()) { return "`ReDoS detected (term not added).`"; }

			switch (addCommand.ToLowerInvariant()[0])
			{
				case 'o':
				{
					if (GlobalInfo.BlackFilters[FilterType.QuestionBodyBlackOff].Terms.Contains(term)) { return "`Blacklist term already exists.`"; }

					GlobalInfo.BlackFilters[FilterType.QuestionBodyBlackOff].AddTerm(new Term(FilterType.QuestionBodyBlackOff, term, GlobalInfo.BlackFilters[FilterType.QuestionBodyBlackOff].AverageScore));

					break;
				}

				case 's':
				{
					if (GlobalInfo.BlackFilters[FilterType.QuestionBodyBlackSpam].Terms.Contains(term)) { return "`Blacklist term already exists.`"; }

					GlobalInfo.BlackFilters[FilterType.QuestionBodyBlackSpam].AddTerm(new Term(FilterType.QuestionBodyBlackSpam, term, GlobalInfo.BlackFilters[FilterType.QuestionBodyBlackSpam].AverageScore));

					break;
				}

				case 'l':
				{
					if (GlobalInfo.BlackFilters[FilterType.QuestionBodyBlackLQ].Terms.Contains(term)) { return "`Blacklist term already exists.`"; }

					GlobalInfo.BlackFilters[FilterType.QuestionBodyBlackLQ].AddTerm(new Term(FilterType.QuestionBodyBlackLQ, term, GlobalInfo.BlackFilters[FilterType.QuestionBodyBlackLQ].AverageScore));

					break;
				}
			}

			return ":" + message.MessageID + " `Blacklist term added.`";
		}

		private static string AddWQBTerm(string command)
		{
			var addCommand = command.Remove(0, 9);
			var firstSpace = command.IndexOf(' ');
			var secondSpace = command.IndexOf(' ', firstSpace + 1);
			var term = new Regex(command.Substring(secondSpace + 1));
			var site = command.Substring(firstSpace + 1, secondSpace - firstSpace - 1);

			if (term.IsFlakRegex()) { return "`ReDoS detected (term not added).`"; }

			switch (addCommand.ToLowerInvariant()[0])
			{
				case 'o':
				{
					if (GlobalInfo.WhiteFilters[FilterType.QuestionBodyWhiteOff].Terms.Contains(term, site)) { return "`Whitelist term already exists.`"; }

					var score = GlobalInfo.WhiteFilters[FilterType.QuestionBodyWhiteOff].Terms.Where(t => t.Site == site).Select(t => t.Score).Average();

					GlobalInfo.WhiteFilters[FilterType.QuestionBodyWhiteOff].AddTerm(new Term(FilterType.QuestionBodyWhiteOff, term, score, site));

					break;
				}

				case 's':
				{
					if (GlobalInfo.WhiteFilters[FilterType.QuestionBodyWhiteSpam].Terms.Contains(term, site)) { return "`Whitelist term already exists.`"; }

					var score = GlobalInfo.WhiteFilters[FilterType.QuestionBodyWhiteSpam].Terms.Where(t => t.Site == site).Select(t => t.Score).Average();

					GlobalInfo.WhiteFilters[FilterType.QuestionBodyWhiteSpam].AddTerm(new Term(FilterType.QuestionBodyWhiteSpam, term, score, site));

					break;
				}

				case 'l':
				{
					if (GlobalInfo.WhiteFilters[FilterType.QuestionBodyWhiteLQ].Terms.Contains(term, site)) { return "`Whitelist term already exists.`"; }

					var score = GlobalInfo.WhiteFilters[FilterType.QuestionBodyWhiteLQ].Terms.Where(t => t.Site == site).Select(t => t.Score).Average();

					GlobalInfo.WhiteFilters[FilterType.QuestionBodyWhiteLQ].AddTerm(new Term(FilterType.QuestionBodyWhiteLQ, term, score, site));

					break;
				}
			}

			return ":" + message.MessageID + " `Whitelist term added.`";
		}

		private static string AddBATerm(string command)
		{
			var addCommand = command.Remove(0, 8);
			var term = new Regex(addCommand.Substring(addCommand.IndexOf(' ') + 1), RegexOptions.Compiled);

			if (term.IsFlakRegex()) { return "`ReDoS detected (term not added).`"; }

			switch (addCommand.ToLowerInvariant()[0])
			{
				case 'o':
				{
					if (GlobalInfo.BlackFilters[FilterType.AnswerBlackOff].Terms.Contains(term)) { return "`Blacklist term already exists.`"; }

					GlobalInfo.BlackFilters[FilterType.AnswerBlackOff].AddTerm(new Term(FilterType.AnswerBlackOff, term, GlobalInfo.BlackFilters[FilterType.AnswerBlackOff].AverageScore));

					break;
				}

				case 's':
				{
					if (GlobalInfo.BlackFilters[FilterType.AnswerBlackSpam].Terms.Contains(term)) { return "`Blacklist term already exists.`"; }

					GlobalInfo.BlackFilters[FilterType.AnswerBlackSpam].AddTerm(new Term(FilterType.AnswerBlackSpam, term, GlobalInfo.BlackFilters[FilterType.AnswerBlackSpam].AverageScore));

					break;
				}

				case 'l':
				{
					if (GlobalInfo.BlackFilters[FilterType.AnswerBlackLQ].Terms.Contains(term)) { return "`Blacklist term already exists.`"; }

					GlobalInfo.BlackFilters[FilterType.AnswerBlackLQ].AddTerm(new Term(FilterType.AnswerBlackLQ, term, GlobalInfo.BlackFilters[FilterType.AnswerBlackLQ].AverageScore));

					break;
				}

				case 'n':
				{
					if (GlobalInfo.BlackFilters[FilterType.AnswerBlackName].Terms.Contains(term)) { return "`Blacklist term already exists.`"; }

					GlobalInfo.BlackFilters[FilterType.AnswerBlackName].AddTerm(new Term(FilterType.AnswerBlackName, term, GlobalInfo.BlackFilters[FilterType.AnswerBlackName].AverageScore));

					break;
				}
			}

			return ":" + message.MessageID + " `Blacklist term added.`";
		}

		private static string AddWATerm(string command)
		{
			var addCommand = command.Substring(0, 8);
			var firstSpace = command.IndexOf(' ');
			var secondSpace = command.IndexOf(' ', firstSpace + 1);
			var term = new Regex(command.Substring(secondSpace + 1));
			var site = command.Substring(firstSpace + 1, secondSpace - firstSpace - 1);

			if (term.IsFlakRegex()) { return "`ReDoS detected (term not added).`"; }

			switch (addCommand.ToLowerInvariant()[0])
			{
				case 'o':
				{
					if (GlobalInfo.WhiteFilters[FilterType.AnswerWhiteOff].Terms.Contains(term, site)) { return "`Whitelist term already exists.`"; }

					var score = GlobalInfo.WhiteFilters[FilterType.AnswerWhiteOff].Terms.Where(t => t.Site == site).Select(t => t.Score).Average();

					GlobalInfo.WhiteFilters[FilterType.AnswerWhiteOff].AddTerm(new Term(FilterType.AnswerWhiteOff, term, score, site));

					break;
				}

				case 's':
				{
					if (GlobalInfo.WhiteFilters[FilterType.AnswerWhiteSpam].Terms.Contains(term, site)) { return "`Whitelist term already exists.`"; }

					var score = GlobalInfo.WhiteFilters[FilterType.AnswerWhiteSpam].Terms.Where(t => t.Site == site).Select(t => t.Score).Average();

					GlobalInfo.WhiteFilters[FilterType.AnswerWhiteSpam].AddTerm(new Term(FilterType.AnswerWhiteSpam, term, score, site));

					break;
				}

				case 'l':
				{
					if (GlobalInfo.WhiteFilters[FilterType.AnswerWhiteLQ].Terms.Contains(term, site)) { return "`Whitelist term already exists.`"; }

					var score = GlobalInfo.WhiteFilters[FilterType.AnswerWhiteLQ].Terms.Where(t => t.Site == site).Select(t => t.Score).Average();

					GlobalInfo.WhiteFilters[FilterType.AnswerWhiteLQ].AddTerm(new Term(FilterType.AnswerWhiteLQ, term, score, site));

					break;
				}

				case 'n':
				{
					if (GlobalInfo.WhiteFilters[FilterType.AnswerWhiteName].Terms.Contains(term, site)) { return "`Whitelist term already exists.`"; }

					var score = GlobalInfo.WhiteFilters[FilterType.AnswerWhiteName].Terms.Where(t => t.Site == site).Select(t => t.Score).Average();

					GlobalInfo.WhiteFilters[FilterType.AnswerWhiteName].AddTerm(new Term(FilterType.AnswerWhiteName, term, score, site));

					break;
				}
			}

			return ":" + message.MessageID + " `Whitelist term added.`";
		}

		# endregion

		# region Remove term commands.

		private static string RemoveBQTTerm(string command)
		{
			var removeCommand = command.Remove(0, 9);
			var term = new Regex(removeCommand.Substring(removeCommand.IndexOf(' ') + 1), RegexOptions.Compiled);

			switch (removeCommand.ToLowerInvariant()[0])
			{
				case 'o':
				{
					if (!GlobalInfo.BlackFilters[FilterType.QuestionTitleBlackOff].Terms.Contains(term)) { return "`Blacklist term does not exist.`"; }

					GlobalInfo.BlackFilters[FilterType.QuestionTitleBlackOff].RemoveTerm(term);

					break;
				}

				case 's':
				{
					if (!GlobalInfo.BlackFilters[FilterType.QuestionTitleBlackSpam].Terms.Contains(term)) { return "`Blacklist term does not exist.`"; }

					GlobalInfo.BlackFilters[FilterType.QuestionTitleBlackSpam].RemoveTerm(term);

					break;
				}

				case 'l':
				{
					if (!GlobalInfo.BlackFilters[FilterType.QuestionTitleBlackLQ].Terms.Contains(term)) { return "`Blacklist term does not exist.`"; }

					GlobalInfo.BlackFilters[FilterType.QuestionTitleBlackLQ].RemoveTerm(term);

					break;
				}

				case 'n':
				{
					if (!GlobalInfo.BlackFilters[FilterType.QuestionTitleBlackName].Terms.Contains(term)) { return "`Blacklist term does not exist.`"; }

					GlobalInfo.BlackFilters[FilterType.QuestionTitleBlackName].RemoveTerm(term);

					break;
				}
			}

			return ":" + message.MessageID + " `Blacklist term removed.`";
		}

		private static string RemoveWQTTerm(string command)
		{
			var removeCommand = command.Remove(0, 9);
			var firstSpace = command.IndexOf(' ');
			var secondSpace = command.IndexOf(' ', firstSpace + 1);
			var term = new Regex(command.Substring(secondSpace + 1));
			var site = command.Substring(firstSpace + 1, secondSpace - firstSpace - 1);

			switch (removeCommand.ToLowerInvariant()[0])
			{
				case 'o':
				{
					if (!GlobalInfo.WhiteFilters[FilterType.QuestionTitleWhiteOff].Terms.Contains(term, site)) { return "`Whitelist term does not exist.`"; }

					GlobalInfo.WhiteFilters[FilterType.QuestionTitleWhiteOff].RemoveTerm(new Term(FilterType.QuestionTitleWhiteOff, term, 0, site));

					break;
				}

				case 's':
				{
					if (!GlobalInfo.WhiteFilters[FilterType.QuestionTitleWhiteSpam].Terms.Contains(term, site)) { return "`Whitelist term does not exist.`"; }

					GlobalInfo.WhiteFilters[FilterType.QuestionTitleWhiteSpam].RemoveTerm(new Term(FilterType.QuestionTitleWhiteSpam, term, 0, site));

					break;
				}

				case 'l':
				{
					if (!GlobalInfo.WhiteFilters[FilterType.QuestionTitleWhiteLQ].Terms.Contains(term, site)) { return "`Whitelist term does not exist.`"; }

					GlobalInfo.WhiteFilters[FilterType.QuestionTitleWhiteLQ].RemoveTerm(new Term(FilterType.QuestionTitleWhiteLQ, term, 0, site));

					break;
				}
				
				case 'n':
				{
					if (!GlobalInfo.WhiteFilters[FilterType.QuestionTitleWhiteName].Terms.Contains(term, site)) { return "`Whitelist term does not exist.`"; }

					GlobalInfo.WhiteFilters[FilterType.QuestionTitleWhiteName].RemoveTerm(new Term(FilterType.QuestionTitleWhiteName, term, 0, site));

					break;
				}
			}

			return ":" + message.MessageID + " `Whitelist term removed.`";
		}

		private static string RemoveBQBTerm(string command)
		{
			var removeCommand = command.Remove(0, 9);
			var term = new Regex(removeCommand.Substring(removeCommand.IndexOf(' ') + 1), RegexOptions.Compiled);

			switch (removeCommand.ToLowerInvariant()[0])
			{
				case 'o':
				{
					if (!GlobalInfo.BlackFilters[FilterType.QuestionBodyBlackOff].Terms.Contains(term)) { return "`Blacklist term does not exist.`"; }

					GlobalInfo.BlackFilters[FilterType.QuestionBodyBlackOff].RemoveTerm(term);

					break;
				}

				case 's':
				{
					if (!GlobalInfo.BlackFilters[FilterType.QuestionBodyBlackSpam].Terms.Contains(term)) { return "`Blacklist term does not exist.`"; }

					GlobalInfo.BlackFilters[FilterType.QuestionBodyBlackSpam].RemoveTerm(term);

					break;
				}

				case 'l':
				{
					if (!GlobalInfo.BlackFilters[FilterType.QuestionBodyBlackLQ].Terms.Contains(term)) { return "`Blacklist term does not exist.`"; }

					GlobalInfo.BlackFilters[FilterType.QuestionBodyBlackLQ].RemoveTerm(term);

					break;
				}
			}

			return ":" + message.MessageID + " `Blacklist term removed.`";
		}

		private static string RemoveWQBTerm(string command)
		{
			var removeCommand = command.Remove(0, 9);
			var firstSpace = command.IndexOf(' ');
			var secondSpace = command.IndexOf(' ', firstSpace + 1);
			var term = new Regex(command.Substring(secondSpace + 1));
			var site = command.Substring(firstSpace + 1, secondSpace - firstSpace - 1);

			switch (removeCommand.ToLowerInvariant()[0])
			{
				case 'o':
				{
					if (!GlobalInfo.WhiteFilters[FilterType.QuestionBodyWhiteOff].Terms.Contains(term, site)) { return "`Whitelist term does not exist.`"; }

					GlobalInfo.WhiteFilters[FilterType.QuestionBodyWhiteOff].RemoveTerm(new Term(FilterType.QuestionBodyWhiteOff, term, 0, site));

					break;
				}

				case 's':
				{
					if (!GlobalInfo.WhiteFilters[FilterType.QuestionBodyWhiteSpam].Terms.Contains(term, site)) { return "`Whitelist term does not exist.`"; }

					GlobalInfo.WhiteFilters[FilterType.QuestionBodyWhiteSpam].RemoveTerm(new Term(FilterType.QuestionBodyWhiteSpam, term, 0, site));

					break;
				}

				case 'l':
				{
					if (!GlobalInfo.WhiteFilters[FilterType.QuestionBodyWhiteLQ].Terms.Contains(term, site)) { return "`Whitelist term does not exist.`"; }

					GlobalInfo.WhiteFilters[FilterType.QuestionBodyWhiteLQ].RemoveTerm(new Term(FilterType.QuestionBodyWhiteLQ, term, 0, site));

					break;
				}
			}

			return ":" + message.MessageID + " `Whitelist term removed.`";
		}

		private static string RemoveBATerm(string command)
		{
			var removeCommand = command.Remove(0, 8);
			var term = new Regex(removeCommand.Substring(removeCommand.IndexOf(' ') + 1), RegexOptions.Compiled);

			switch (removeCommand.ToLowerInvariant()[0])
			{
				case 'o':
				{
					if (!GlobalInfo.BlackFilters[FilterType.AnswerBlackOff].Terms.Contains(term)) { return "`Blacklist term does not exist.`"; }

					GlobalInfo.BlackFilters[FilterType.AnswerBlackOff].RemoveTerm(term);

					break;
				}

				case 's':
				{
					if (!GlobalInfo.BlackFilters[FilterType.AnswerBlackSpam].Terms.Contains(term)) { return "`Blacklist term does not exist.`"; }

					GlobalInfo.BlackFilters[FilterType.AnswerBlackSpam].RemoveTerm(term);

					break;
				}

				case 'l':
				{
					if (!GlobalInfo.BlackFilters[FilterType.AnswerBlackLQ].Terms.Contains(term)) { return "`Blacklist term does not exist.`"; }

					GlobalInfo.BlackFilters[FilterType.AnswerBlackLQ].RemoveTerm(term);

					break;
				}

				case 'n':
				{
					if (!GlobalInfo.BlackFilters[FilterType.AnswerBlackName].Terms.Contains(term)) { return "`Blacklist term does not exist.`"; }

					GlobalInfo.BlackFilters[FilterType.AnswerBlackName].RemoveTerm(term);

					break;
				}
			}

			return ":" + message.MessageID + " `Blacklist term removed.`";
		}

		private static string RemoveWATerm(string command)
		{
			var removeCommand = command.Remove(0, 8);
			var firstSpace = command.IndexOf(' ');
			var secondSpace = command.IndexOf(' ', firstSpace + 1);
			var term = new Regex(command.Substring(secondSpace + 1));
			var site = command.Substring(firstSpace + 1, secondSpace - firstSpace - 1);

			switch (removeCommand.ToLowerInvariant()[0])
			{
				case 'o':
				{
					if (!GlobalInfo.WhiteFilters[FilterType.AnswerWhiteOff].Terms.Contains(term)) { return "`Whitelist term does not exist.`"; }

					GlobalInfo.WhiteFilters[FilterType.AnswerWhiteOff].RemoveTerm(new Term(FilterType.AnswerWhiteOff, term, 0, site));

					break;
				}

				case 's':
				{
					if (!GlobalInfo.WhiteFilters[FilterType.AnswerWhiteSpam].Terms.Contains(term)) { return "`Whitelist term does not exist.`"; }

					GlobalInfo.WhiteFilters[FilterType.AnswerWhiteSpam].RemoveTerm(new Term(FilterType.AnswerWhiteSpam, term, 0, site));

					break;
				}

				case 'l':
				{
					if (!GlobalInfo.WhiteFilters[FilterType.AnswerWhiteLQ].Terms.Contains(term)) { return "`Whitelist term does not exist.`"; }

					GlobalInfo.WhiteFilters[FilterType.AnswerWhiteLQ].RemoveTerm(new Term(FilterType.AnswerWhiteLQ, term, 0, site));

					break;
				}

				case 'n':
				{
					if (!GlobalInfo.WhiteFilters[FilterType.AnswerWhiteName].Terms.Contains(term)) { return "`Whitelist term does not exist.`"; }

					GlobalInfo.WhiteFilters[FilterType.AnswerWhiteName].RemoveTerm(new Term(FilterType.AnswerWhiteName, term, 0, site));

					break;
				}
			}

			return ":" + message.MessageID + " `Whitelist term removed.`";
		}

		# endregion

		# region Edit term commands.

		private static string EditBQTTerm(string command)
		{
			var editCommand = command.Remove(0, 10);

			if (!editCommand.StartsWith("off") && !editCommand.StartsWith("spam") && !editCommand.StartsWith("lq") && !editCommand.StartsWith("name")) { return "`Command not recognised.`"; }

			var startIndex = command.IndexOf(' ') + 1;
			var delimiterIndex = command.IndexOf("¬¬¬", StringComparison.Ordinal);
			var oldTerm = new Regex(command.Substring(startIndex, delimiterIndex - startIndex));
			var newTerm = new Regex(command.Remove(0, delimiterIndex + 3), RegexOptions.Compiled);

			if (newTerm.IsFlakRegex()) { return "`ReDoS detected (term not updated).`"; }

			switch (editCommand.ToLowerInvariant()[0])
			{
				case 'o':
				{
					if (!GlobalInfo.BlackFilters[FilterType.QuestionTitleBlackOff].Terms.Contains(oldTerm)) { return "`Blacklist term does not exist.`"; }

					GlobalInfo.BlackFilters[FilterType.QuestionTitleBlackOff].EditTerm(oldTerm, newTerm);

					break;
				}

				case 's':
				{
					if (!GlobalInfo.BlackFilters[FilterType.QuestionTitleBlackSpam].Terms.Contains(oldTerm)) { return "`Blacklist term does not exist.`"; }

					GlobalInfo.BlackFilters[FilterType.QuestionTitleBlackSpam].EditTerm(oldTerm, newTerm);

					break;
				}

				case 'l':
				{
					if (!GlobalInfo.BlackFilters[FilterType.QuestionTitleBlackLQ].Terms.Contains(oldTerm)) { return "`Blacklist term does not exist.`"; }

					GlobalInfo.BlackFilters[FilterType.QuestionTitleBlackLQ].EditTerm(oldTerm, newTerm);

					break;
				}

				case 'n':
				{
					if (!GlobalInfo.BlackFilters[FilterType.QuestionTitleBlackName].Terms.Contains(oldTerm)) { return "`Blacklist term does not exist.`"; }

					GlobalInfo.BlackFilters[FilterType.QuestionTitleBlackName].EditTerm(oldTerm, newTerm);

					break;
				}
			}

			return ":" + message.MessageID + " `Term updated.`";
		}

		private static string EditBQBTerm(string command)
		{
			var editCommand = command.Remove(0, 10);

			if (!editCommand.StartsWith("off") && !editCommand.StartsWith("spam") && !editCommand.StartsWith("lq")) { return "`Command not recognised.`"; }

			var startIndex = command.IndexOf(' ') + 1;
			var delimiterIndex = command.IndexOf("¬¬¬", StringComparison.Ordinal);
			var oldTerm = new Regex(command.Substring(startIndex, delimiterIndex - startIndex));
			var newTerm = new Regex(command.Remove(0, delimiterIndex + 3), RegexOptions.Compiled);

			if (newTerm.IsFlakRegex()) { return "`ReDoS detected (term not updated).`"; }

			switch (editCommand.ToLowerInvariant()[0])
			{
				case 'o':
				{
					if (!GlobalInfo.BlackFilters[FilterType.QuestionBodyBlackOff].Terms.Contains(oldTerm)) { return "`Blacklist term does not exist.`"; }

					GlobalInfo.BlackFilters[FilterType.QuestionBodyBlackOff].EditTerm(oldTerm, newTerm);

					break;
				}

				case 's':
				{
					if (!GlobalInfo.BlackFilters[FilterType.QuestionBodyBlackSpam].Terms.Contains(oldTerm)) { return "`Blacklist term does not exist.`"; }

					GlobalInfo.BlackFilters[FilterType.QuestionBodyBlackSpam].EditTerm(oldTerm, newTerm);

					break;
				}

				case 'l':
				{
					if (!GlobalInfo.BlackFilters[FilterType.QuestionBodyBlackLQ].Terms.Contains(oldTerm)) { return "`Blacklist term does not exist.`"; }

					GlobalInfo.BlackFilters[FilterType.QuestionBodyBlackLQ].EditTerm(oldTerm, newTerm);

					break;
				}
			}

			return ":" + message.MessageID + " `Term updated.`";
		}

		private static string EditWQTTerm(string command)
		{
			var editCommand = command.Remove(0, 10);

			if (!editCommand.StartsWith("off") && !editCommand.StartsWith("spam") && !editCommand.StartsWith("lq") && !editCommand.StartsWith("name")) { return "`Command not recognised.`"; }

			var firstSpace = command.IndexOf(' ');
			var secondSpace = command.IndexOf(' ', firstSpace + 1);
			var delimiter = command.IndexOf("¬¬¬", StringComparison.Ordinal);

			var site = command.Substring(firstSpace + 1, secondSpace - firstSpace - 1);
			var oldTerm = new Regex(command.Substring(secondSpace + 1, delimiter - secondSpace - 1));
			var newTerm = new Regex(command.Remove(0, delimiter + 3), RegexOptions.Compiled);

			if (newTerm.IsFlakRegex()) { return "`ReDoS detected (term not updated).`"; }

			switch (editCommand.ToLowerInvariant()[0])
			{
				case 'o':
				{
					if (!GlobalInfo.WhiteFilters[FilterType.QuestionTitleWhiteOff].Terms.Contains(oldTerm, site)) { return "`Whitelist term does not exist.`"; }

					GlobalInfo.WhiteFilters[FilterType.QuestionTitleWhiteOff].EditTerm(oldTerm, newTerm, site);

					break;
				}

				case 's':
				{
					if (!GlobalInfo.WhiteFilters[FilterType.QuestionTitleWhiteSpam].Terms.Contains(oldTerm, site)) { return "`Whitelist term does not exist.`"; }

					GlobalInfo.WhiteFilters[FilterType.QuestionTitleWhiteSpam].EditTerm(oldTerm, newTerm, site);

					break;
				}

				case 'l':
				{
					if (!GlobalInfo.WhiteFilters[FilterType.QuestionTitleWhiteLQ].Terms.Contains(oldTerm, site)) { return "`Whitelist term does not exist.`"; }

					GlobalInfo.WhiteFilters[FilterType.QuestionTitleWhiteLQ].EditTerm(oldTerm, newTerm, site);

					break;
				}

				case 'n':
				{
					if (!GlobalInfo.WhiteFilters[FilterType.QuestionTitleWhiteName].Terms.Contains(oldTerm, site)) { return "`Whitelist term does not exist.`"; }

					GlobalInfo.WhiteFilters[FilterType.QuestionTitleWhiteName].EditTerm(oldTerm, newTerm, site);

					break;
				}
			}

			return ":" + message.MessageID + " `Term updated.`";
		}

		private static string EditWQBTerm(string command)
		{
			var editCommand = command.Remove(0, 10);

			if (!editCommand.StartsWith("off") && !editCommand.StartsWith("spam") && !editCommand.StartsWith("lq")) { return "`Command not recognised.`"; }

			var firstSpace = command.IndexOf(' ');
			var secondSpace = command.IndexOf(' ', firstSpace + 1);
			var delimiter = command.IndexOf("¬¬¬", StringComparison.Ordinal);

			var site = command.Substring(firstSpace + 1, secondSpace - firstSpace - 1);
			var oldTerm = new Regex(command.Substring(secondSpace + 1, delimiter - secondSpace - 1));
			var newTerm = new Regex(command.Remove(0, delimiter + 3), RegexOptions.Compiled);

			if (newTerm.IsFlakRegex()) { return "`ReDoS detected (term not updated).`"; }

			switch (editCommand.ToLowerInvariant()[0])
			{
				case 'o':
				{
					if (!GlobalInfo.WhiteFilters[FilterType.QuestionBodyWhiteOff].Terms.Contains(oldTerm, site)) { return "`Whitelist term does not exist.`"; }

					GlobalInfo.WhiteFilters[FilterType.QuestionBodyWhiteOff].EditTerm(oldTerm, newTerm, site);

					break;
				}

				case 's':
				{
					if (!GlobalInfo.WhiteFilters[FilterType.QuestionBodyWhiteSpam].Terms.Contains(oldTerm, site)) { return "`Whitelist term does not exist.`"; }

					GlobalInfo.WhiteFilters[FilterType.QuestionBodyWhiteSpam].EditTerm(oldTerm, newTerm, site);

					break;
				}

				case 'l':
				{
					if (!GlobalInfo.WhiteFilters[FilterType.QuestionBodyWhiteLQ].Terms.Contains(oldTerm, site)) { return "`Whitelist term does not exist.`"; }

					GlobalInfo.WhiteFilters[FilterType.QuestionBodyWhiteLQ].EditTerm(oldTerm, newTerm, site);

					break;
				}
			}

			return ":" + message.MessageID + " `Term updated.`";
		}

		private static string EditBATerm(string command)
		{
			var editCommand = command.Remove(0, 9);

			if (!editCommand.StartsWith("off") && !editCommand.StartsWith("spam") && !editCommand.StartsWith("lq") && !editCommand.StartsWith("name")) { return "`Command not recognised.`"; }

			var startIndex = command.IndexOf(' ') + 1;
			var delimiterIndex = command.IndexOf("¬¬¬", StringComparison.Ordinal);
			var oldTerm = new Regex(command.Substring(startIndex, delimiterIndex - startIndex));
			var newTerm = new Regex(command.Remove(0, delimiterIndex + 3), RegexOptions.Compiled);

			if (newTerm.IsFlakRegex()) { return "`ReDoS detected (term not updated).`"; }

			switch (editCommand.ToLowerInvariant()[0])
			{
				case 'o':
				{
					if (!GlobalInfo.BlackFilters[FilterType.AnswerBlackOff].Terms.Contains(oldTerm)) { return "`Blacklist term does not exist.`"; }

					GlobalInfo.BlackFilters[FilterType.AnswerBlackOff].EditTerm(oldTerm, newTerm);

					break;
				}

				case 's':
				{
					if (!GlobalInfo.BlackFilters[FilterType.AnswerBlackSpam].Terms.Contains(oldTerm)) { return "`Blacklist term does not exist.`"; }

					GlobalInfo.BlackFilters[FilterType.AnswerBlackSpam].EditTerm(oldTerm, newTerm);

					break;
				}

				case 'l':
				{
					if (!GlobalInfo.BlackFilters[FilterType.AnswerBlackLQ].Terms.Contains(oldTerm)) { return "`Blacklist term does not exist.`"; }

					GlobalInfo.BlackFilters[FilterType.AnswerBlackLQ].EditTerm(oldTerm, newTerm);

					break;
				}

				case 'n':
				{
					if (!GlobalInfo.BlackFilters[FilterType.AnswerBlackName].Terms.Contains(oldTerm)) { return "`Blacklist term does not exist.`"; }

					GlobalInfo.BlackFilters[FilterType.AnswerBlackName].EditTerm(oldTerm, newTerm);

					break;
				}
			}

			return ":" + message.MessageID + " `Term updated.`";
		}

		private static string EditWATerm(string command)
		{
			var editCommand = command.Remove(0, 9);

			if (!editCommand.StartsWith("off") && !editCommand.StartsWith("spam") && !editCommand.StartsWith("lq") && !editCommand.StartsWith("name")) { return "`Command not recognised.`"; }

			var firstSpace = command.IndexOf(' ');
			var secondSpace = command.IndexOf(' ', firstSpace + 1);
			var delimiter = command.IndexOf("¬¬¬", StringComparison.Ordinal);

			var site = command.Substring(firstSpace + 1, secondSpace - firstSpace - 1);
			var oldTerm = new Regex(command.Substring(secondSpace + 1, delimiter - secondSpace - 1));
			var newTerm = new Regex(command.Remove(0, delimiter + 3), RegexOptions.Compiled);

			if (newTerm.IsFlakRegex()) { return "`ReDoS detected (term not updated).`"; }

			switch (editCommand.ToLowerInvariant()[0])
			{
				case 'o':
				{
					if (!GlobalInfo.WhiteFilters[FilterType.AnswerWhiteOff].Terms.Contains(oldTerm, site)) { return "`Whitelist term does not exist.`"; }

					GlobalInfo.WhiteFilters[FilterType.AnswerWhiteOff].EditTerm(oldTerm, newTerm, site);

					break;
				}

				case 's':
				{
					if (!GlobalInfo.WhiteFilters[FilterType.AnswerWhiteSpam].Terms.Contains(oldTerm, site)) { return "`Whitelist term does not exist.`"; }

					GlobalInfo.WhiteFilters[FilterType.AnswerWhiteSpam].EditTerm(oldTerm, newTerm, site);

					break;
				}

				case 'l':
				{
					if (!GlobalInfo.WhiteFilters[FilterType.AnswerWhiteLQ].Terms.Contains(oldTerm, site)) { return "`Whitelist term does not exist.`"; }

					GlobalInfo.WhiteFilters[FilterType.AnswerWhiteLQ].EditTerm(oldTerm, newTerm, site);

					break;
				}

				case 'n':
				{
					if (!GlobalInfo.WhiteFilters[FilterType.AnswerWhiteName].Terms.Contains(oldTerm, site)) { return "`Whitelist term does not exist.`"; }

					GlobalInfo.WhiteFilters[FilterType.AnswerWhiteName].EditTerm(oldTerm, newTerm, site);

					break;
				}
			}

			return ":" + message.MessageID + " `Term updated.`";
		}

		# endregion

		# region FP/TP(A) commands

		private static string FalsePositive()
		{
			return message.Report.Type == PostType.BadTagUsed ? "" : RegisterFalsePositive();
		}

		private static string RegisterFalsePositive()
		{
			GlobalInfo.Stats.TotalFPCount++;

			var newWhiteTermScore = message.Report.BlackTermsFound.Select(t => t.Score).Max() / 2;

			foreach (var filter in message.Report.FiltersUsed)
			{
				if ((int)filter > 99) // White filter
				{
					for (var i = 0; i < message.Report.WhiteTermsFound.Count; i++) // Do NOT change to foreach. We're (indirectly) modifying the collection.
					{
						var term = message.Report.WhiteTermsFound.ElementAt(i);

						if (term.Site == message.Post.Site)
						{
							GlobalInfo.WhiteFilters[filter].SetScore(term, term.Score + 1);
						}
					}
				}
				else // Black filter
				{
					foreach (var term in message.Report.BlackTermsFound)
					{
						term.FPCount++;

						var corFilter = filter.GetCorrespondingWhiteFilter();

						if (GlobalInfo.WhiteFilters[corFilter].Terms.All(tt => tt.Site != term.Site && tt.Regex.ToString() != term.Regex.ToString()))
						{
							GlobalInfo.WhiteFilters[corFilter].AddTerm(new Term(corFilter, term.Regex, newWhiteTermScore, message.Post.Site));
						}
					}
				}
			}

			return MessageHandler.DeleteMessage(message.Post.URL, message.RepliesToMessageID) ? "" : ":" + message.MessageID + " `FP acknowledged.`";
		}


		private static string TruePositive()
		{
			if (commandLower.StartsWith("tpa"))
			{
				var reportMessage = GlobalInfo.PostedReports[message.RepliesToMessageID];

				reportMessage.RoomID = GlobalInfo.AnnouncerRoomID;

				if (reportMessage.Report.Type == PostType.Offensive)
				{
					reportMessage.Body = MessageCleaner.GetCleanMessage(message.RepliesToMessageID);
				}

				GlobalInfo.MessagePoster.MessageQueue.Add(reportMessage);			
			}

			if (message.Report.Type == PostType.BadTagUsed) { return ""; }

			var returnMessage = RegisterTruePositive();

			return commandLower.StartsWith("tpa") ? "" : returnMessage;
		}

		private static string RegisterTruePositive()
		{
			GlobalInfo.Stats.TotalTPCount++;

			foreach (var filter in message.Report.FiltersUsed.Where(filter => (int)filter < 100)) // Make sure we only get black filters.
			foreach (var blackTerm in message.Report.BlackTermsFound.Where(blackTerm => GlobalInfo.BlackFilters[filter].Terms.Contains(blackTerm)))
			{
				GlobalInfo.BlackFilters[filter].SetScore(blackTerm, blackTerm.Score + 1);

				blackTerm.TPCount++;

				for (var i = 0; i < GlobalInfo.WhiteFilters[filter.GetCorrespondingWhiteFilter()].Terms.Count; i++) // Do NOT change to foreach. We're (indirectly) modifying the collection.
				{
					var whiteTerm = GlobalInfo.WhiteFilters[filter.GetCorrespondingWhiteFilter()].Terms.ElementAt(i);

					if (whiteTerm.Regex.ToString() != blackTerm.Regex.ToString() || whiteTerm.Site == message.Post.Site) { continue; }

					var x = whiteTerm.Score / blackTerm.Score;

					GlobalInfo.WhiteFilters[filter.GetCorrespondingWhiteFilter()].SetScore(whiteTerm, x * (blackTerm.Score + 1));
				}
			}

			return ":" + message.MessageID + " `TP acknowledged.`";
		}

		# endregion

		# region Auto commands

		private static string AutoBQTTerm(string command)
		{
			var editCommand = command.Remove(0, 10);

			if (!editCommand.StartsWith("off") && !editCommand.StartsWith("spam") && !editCommand.StartsWith("lq") && !editCommand.StartsWith("name")) { return "`Command not recognised.`"; }

			var startIndex = command.IndexOf(' ') + 1;
			var persistence = command[startIndex - 2] == 'p' || command[startIndex - 2] == 'P';
			var term = new Regex(command.Substring(startIndex, command.Length - startIndex));

			if (editCommand.StartsWith("off"))
			{
				if (!GlobalInfo.BlackFilters[FilterType.QuestionTitleBlackOff].Terms.Contains(term)) { return "`Blacklist term does not exist.`"; }

				var isAuto = GlobalInfo.BlackFilters[FilterType.QuestionTitleBlackOff].Terms.First(t => t.Regex.ToString() == term.ToString()).IsAuto;

				GlobalInfo.BlackFilters[FilterType.QuestionTitleBlackOff].SetAuto(term, !isAuto, persistence);

				return ":" + message.MessageID + " `Auto toggled (now " + !isAuto + ").`";
			}

			if (editCommand.StartsWith("spam"))
			{
				if (!GlobalInfo.BlackFilters[FilterType.QuestionTitleBlackSpam].Terms.Contains(term)) { return "`Blacklist term does not exist.`"; }

				var isAuto = GlobalInfo.BlackFilters[FilterType.QuestionTitleBlackSpam].Terms.First(t => t.Regex.ToString() == term.ToString()).IsAuto;

				GlobalInfo.BlackFilters[FilterType.QuestionTitleBlackSpam].SetAuto(term, !isAuto, persistence);

				return ":" + message.MessageID + " `Auto toggled (now " + !isAuto + ").`";
			}

			if (editCommand.StartsWith("lq"))
			{
				if (!GlobalInfo.BlackFilters[FilterType.QuestionTitleBlackLQ].Terms.Contains(term)) { return "`Blacklist term does not exist.`"; }

				var isAuto = GlobalInfo.BlackFilters[FilterType.QuestionTitleBlackLQ].Terms.First(t => t.Regex.ToString() == term.ToString()).IsAuto;

				GlobalInfo.BlackFilters[FilterType.QuestionTitleBlackLQ].SetAuto(term, !isAuto, persistence);

				return ":" + message.MessageID + " `Auto toggled (now " + !isAuto + ").`";
			}

			if (editCommand.StartsWith("name"))
			{
				if (!GlobalInfo.BlackFilters[FilterType.QuestionTitleBlackName].Terms.Contains(term)) { return "`Blacklist term does not exist.`"; }

				var isAuto = GlobalInfo.BlackFilters[FilterType.QuestionTitleBlackName].Terms.First(t => t.Regex.ToString() == term.ToString()).IsAuto;

				GlobalInfo.BlackFilters[FilterType.QuestionTitleBlackName].SetAuto(term, !isAuto, persistence);

				return ":" + message.MessageID + " `Auto toggled (now " + !isAuto + ").`";
			}

			return ":" + message.MessageID + " `Command not recognised.`";
		}

		private static string AutoBQBTerm(string command)
		{
			var editCommand = command.Remove(0, 10);

			if (!editCommand.StartsWith("off") && !editCommand.StartsWith("spam") && !editCommand.StartsWith("lq")) { return "`Command not recognised.`"; }

			var startIndex = command.IndexOf(' ') + 1;
			var persistence = command[startIndex - 2] == 'p' || command[startIndex - 2] == 'P';
			var term = new Regex(command.Substring(startIndex, command.Length - startIndex));

			if (editCommand.StartsWith("off"))
			{
				if (!GlobalInfo.BlackFilters[FilterType.QuestionBodyBlackOff].Terms.Contains(term)) { return "`Blacklist term does not exist.`"; }

				var isAuto = GlobalInfo.BlackFilters[FilterType.QuestionBodyBlackOff].Terms.First(t => t.Regex.ToString() == term.ToString()).IsAuto;

				GlobalInfo.BlackFilters[FilterType.QuestionBodyBlackOff].SetAuto(term, !isAuto, persistence);

				return ":" + message.MessageID + " `Auto toggled (now " + !isAuto + ").`";
			}

			if (editCommand.StartsWith("spam"))
			{
				if (!GlobalInfo.BlackFilters[FilterType.QuestionBodyBlackSpam].Terms.Contains(term)) { return "`Blacklist term does not exist.`"; }

				var isAuto = GlobalInfo.BlackFilters[FilterType.QuestionBodyBlackSpam].Terms.First(t => t.Regex.ToString() == term.ToString()).IsAuto;

				GlobalInfo.BlackFilters[FilterType.QuestionBodyBlackSpam].SetAuto(term, !isAuto, persistence);

				return ":" + message.MessageID + " `Auto toggled (now " + !isAuto + ").`";
			}

			if (editCommand.StartsWith("lq"))
			{
				if (!GlobalInfo.BlackFilters[FilterType.QuestionBodyBlackLQ].Terms.Contains(term)) { return "`Blacklist term does not exist.`"; }

				var isAuto = GlobalInfo.BlackFilters[FilterType.QuestionBodyBlackLQ].Terms.First(t => t.Regex.ToString() == term.ToString()).IsAuto;

				GlobalInfo.BlackFilters[FilterType.QuestionBodyBlackLQ].SetAuto(term, !isAuto, persistence);

				return ":" + message.MessageID + " `Auto toggled (now " + !isAuto + ").`";
			}

			return ":" + message.MessageID + " `Term updated.`";
		}

		private static string AutoWQTTerm(string command)
		{
			var editCommand = command.Remove(0, 10);

			if (!editCommand.StartsWith("off") && !editCommand.StartsWith("spam") && !editCommand.StartsWith("lq") && !editCommand.StartsWith("name")) { return "`Command not recognised.`"; }

			var firstSpace = command.IndexOf(' ');
			var secondSpace = command.IndexOf(' ', firstSpace + 1);

			var persistence = command[firstSpace - 1] == 'p' || command[firstSpace - 1] == 'P';
			var site = command.Substring(firstSpace + 1, secondSpace - firstSpace - 1);
			var term = new Regex(command.Substring(secondSpace + 1, command.Length - secondSpace - 1));

			if (editCommand.StartsWith("off"))
			{
				if (!GlobalInfo.WhiteFilters[FilterType.QuestionTitleWhiteOff].Terms.Contains(term, site)) { return "`Whitelist term does not exist.`"; }

				var isAuto = GlobalInfo.WhiteFilters[FilterType.QuestionTitleWhiteOff].Terms.First(t => t.Regex.ToString() == term.ToString()).IsAuto;

				GlobalInfo.WhiteFilters[FilterType.QuestionTitleWhiteOff].SetAuto(term, !isAuto, site, persistence);

				return ":" + message.MessageID + " `Auto toggled (now " + !isAuto + ").`";
			}

			if (editCommand.StartsWith("spam"))
			{
				if (!GlobalInfo.WhiteFilters[FilterType.QuestionTitleWhiteSpam].Terms.Contains(term, site)) { return "`Whitelist term does not exist.`"; }

				var isAuto = GlobalInfo.WhiteFilters[FilterType.QuestionTitleWhiteSpam].Terms.First(t => t.Regex.ToString() == term.ToString()).IsAuto;

				GlobalInfo.WhiteFilters[FilterType.QuestionTitleWhiteSpam].SetAuto(term, !isAuto, site, persistence);

				return ":" + message.MessageID + " `Auto toggled (now " + !isAuto + ").`";
			}

			if (editCommand.StartsWith("lq"))
			{
				if (!GlobalInfo.WhiteFilters[FilterType.QuestionTitleWhiteLQ].Terms.Contains(term, site)) { return "`Whitelist term does not exist.`"; }

				var isAuto = GlobalInfo.WhiteFilters[FilterType.QuestionTitleWhiteLQ].Terms.First(t => t.Regex.ToString() == term.ToString()).IsAuto;

				GlobalInfo.WhiteFilters[FilterType.QuestionTitleWhiteLQ].SetAuto(term, !isAuto, site, persistence);

				return ":" + message.MessageID + " `Auto toggled (now " + !isAuto + ").`";
			}

			if (editCommand.StartsWith("name"))
			{
				if (!GlobalInfo.WhiteFilters[FilterType.QuestionTitleWhiteName].Terms.Contains(term, site)) { return "`Whitelist term does not exist.`"; }

				var isAuto = GlobalInfo.WhiteFilters[FilterType.QuestionTitleWhiteName].Terms.First(t => t.Regex.ToString() == term.ToString()).IsAuto;

				GlobalInfo.WhiteFilters[FilterType.QuestionTitleWhiteName].SetAuto(term, !isAuto, site, persistence);

				return ":" + message.MessageID + " `Auto toggled (now " + !isAuto + ").`";
			}

			return ":" + message.MessageID + " `Term updated.`";
		}

		private static string AutoWQBTerm(string command)
		{
			var editCommand = command.Remove(0, 10);

			if (!editCommand.StartsWith("off") && !editCommand.StartsWith("spam") && !editCommand.StartsWith("lq")) { return "`Command not recognised.`"; }

			var firstSpace = command.IndexOf(' ');
			var secondSpace = command.IndexOf(' ', firstSpace + 1);

			var persistence = command[firstSpace - 1] == 'p' || command[firstSpace - 1] == 'P';
			var site = command.Substring(firstSpace + 1, secondSpace - firstSpace - 1);
			var term = new Regex(command.Substring(secondSpace + 1, command.Length - secondSpace - 1));

			if (editCommand.StartsWith("off"))
			{
				if (!GlobalInfo.WhiteFilters[FilterType.QuestionBodyWhiteOff].Terms.Contains(term, site)) { return "`Whitelist term does not exist.`"; }

				var isAuto = GlobalInfo.WhiteFilters[FilterType.QuestionBodyWhiteOff].Terms.First(t => t.Regex.ToString() == term.ToString()).IsAuto;

				GlobalInfo.WhiteFilters[FilterType.QuestionBodyWhiteOff].SetAuto(term, !isAuto, site, persistence);

				return ":" + message.MessageID + " `Auto toggled (now " + !isAuto + ").`";
			}

			if (editCommand.StartsWith("spam"))
			{
				if (!GlobalInfo.WhiteFilters[FilterType.QuestionBodyWhiteSpam].Terms.Contains(term, site)) { return "`Whitelist term does not exist.`"; }

				var isAuto = GlobalInfo.WhiteFilters[FilterType.QuestionBodyWhiteSpam].Terms.First(t => t.Regex.ToString() == term.ToString()).IsAuto;

				GlobalInfo.WhiteFilters[FilterType.QuestionBodyWhiteSpam].SetAuto(term, !isAuto, site, persistence);

				return ":" + message.MessageID + " `Auto toggled (now " + !isAuto + ").`";
			}

			if (editCommand.StartsWith("lq"))
			{
				if (!GlobalInfo.WhiteFilters[FilterType.QuestionBodyWhiteLQ].Terms.Contains(term, site)) { return "`Whitelist term does not exist.`"; }

				var isAuto = GlobalInfo.WhiteFilters[FilterType.QuestionBodyWhiteLQ].Terms.First(t => t.Regex.ToString() == term.ToString()).IsAuto;

				GlobalInfo.WhiteFilters[FilterType.QuestionBodyWhiteLQ].SetAuto(term, !isAuto, site, persistence);

				return ":" + message.MessageID + " `Auto toggled (now " + !isAuto + ").`";
			}

			return ":" + message.MessageID + " `Term updated.`";
		}

		private static string AutoBATerm(string command)
		{
			var editCommand = command.Remove(0, 9);

			if (!editCommand.StartsWith("off") && !editCommand.StartsWith("spam") && !editCommand.StartsWith("lq") && !editCommand.StartsWith("name")) { return "`Command not recognised.`"; }

			var startIndex = command.IndexOf(' ') + 1;
			var persistence = command[startIndex - 2] == 'p' || command[startIndex - 2] == 'P';
			var term = new Regex(command.Substring(startIndex, command.Length - startIndex));

			if (editCommand.StartsWith("off"))
			{
				if (!GlobalInfo.BlackFilters[FilterType.AnswerBlackOff].Terms.Contains(term)) { return "`Blacklist term does not exist.`"; }

				var isAuto = GlobalInfo.BlackFilters[FilterType.AnswerBlackOff].Terms.First(t => t.Regex.ToString() == term.ToString()).IsAuto;

				GlobalInfo.BlackFilters[FilterType.AnswerBlackOff].SetAuto(term, !isAuto, persistence);

				return ":" + message.MessageID + " `Auto toggled (now " + !isAuto + ").`";
			}

			if (editCommand.StartsWith("spam"))
			{
				if (!GlobalInfo.BlackFilters[FilterType.AnswerBlackSpam].Terms.Contains(term)) { return "`Blacklist term does not exist.`"; }

				var isAuto = GlobalInfo.BlackFilters[FilterType.AnswerBlackSpam].Terms.First(t => t.Regex.ToString() == term.ToString()).IsAuto;

				GlobalInfo.BlackFilters[FilterType.AnswerBlackSpam].SetAuto(term, !isAuto, persistence);

				return ":" + message.MessageID + " `Auto toggled (now " + !isAuto + ").`";
			}

			if (editCommand.StartsWith("lq"))
			{
				if (!GlobalInfo.BlackFilters[FilterType.AnswerBlackLQ].Terms.Contains(term)) { return "`Blacklist term does not exist.`"; }

				var isAuto = GlobalInfo.BlackFilters[FilterType.AnswerBlackLQ].Terms.First(t => t.Regex.ToString() == term.ToString()).IsAuto;

				GlobalInfo.BlackFilters[FilterType.AnswerBlackLQ].SetAuto(term, !isAuto, persistence);

				return ":" + message.MessageID + " `Auto toggled (now " + !isAuto + ").`";
			}

			if (editCommand.StartsWith("name"))
			{
				if (!GlobalInfo.BlackFilters[FilterType.AnswerBlackName].Terms.Contains(term)) { return "`Blacklist term does not exist.`"; }

				var isAuto = GlobalInfo.BlackFilters[FilterType.AnswerBlackName].Terms.First(t => t.Regex.ToString() == term.ToString()).IsAuto;

				GlobalInfo.BlackFilters[FilterType.AnswerBlackName].SetAuto(term, !isAuto, persistence);

				return ":" + message.MessageID + " `Auto toggled (now " + !isAuto + ").`";
			}

			return ":" + message.MessageID + " `Term updated.`";
		}

		private static string AutoWATerm(string command)
		{
			var editCommand = command.Remove(0, 9);

			if (!editCommand.StartsWith("off") && !editCommand.StartsWith("spam") && !editCommand.StartsWith("lq") && !editCommand.StartsWith("name")) { return "`Command not recognised.`"; }

			var firstSpace = command.IndexOf(' ');
			var secondSpace = command.IndexOf(' ', firstSpace + 1);

			var persistence = command[firstSpace - 1] == 'p' || command[firstSpace - 1] == 'P';
			var site = command.Substring(firstSpace + 1, secondSpace - firstSpace - 1);
			var term = new Regex(command.Substring(secondSpace + 1, command.Length - secondSpace - 1));

			if (editCommand.StartsWith("off"))
			{
				if (!GlobalInfo.WhiteFilters[FilterType.AnswerWhiteOff].Terms.Contains(term, site)) { return "`Whitelist term does not exist.`"; }

				var isAuto = GlobalInfo.WhiteFilters[FilterType.AnswerWhiteOff].Terms.First(t => t.Regex.ToString() == term.ToString()).IsAuto;

				GlobalInfo.WhiteFilters[FilterType.AnswerWhiteOff].SetAuto(term, !isAuto, site, persistence);

				return ":" + message.MessageID + " `Auto toggled (now " + !isAuto + ").`";
			}

			if (editCommand.StartsWith("spam"))
			{
				if (!GlobalInfo.WhiteFilters[FilterType.AnswerWhiteSpam].Terms.Contains(term, site)) { return "`Whitelist term does not exist.`"; }

				var isAuto = GlobalInfo.WhiteFilters[FilterType.AnswerWhiteSpam].Terms.First(t => t.Regex.ToString() == term.ToString()).IsAuto;

				GlobalInfo.WhiteFilters[FilterType.AnswerWhiteSpam].SetAuto(term, !isAuto, site, persistence);

				return ":" + message.MessageID + " `Auto toggled (now " + !isAuto + ").`";
			}

			if (editCommand.StartsWith("lq"))
			{
				if (!GlobalInfo.WhiteFilters[FilterType.AnswerWhiteLQ].Terms.Contains(term, site)) { return "`Whitelist term does not exist.`"; }

				var isAuto = GlobalInfo.WhiteFilters[FilterType.AnswerWhiteLQ].Terms.First(t => t.Regex.ToString() == term.ToString()).IsAuto;

				GlobalInfo.WhiteFilters[FilterType.AnswerWhiteLQ].SetAuto(term, !isAuto, site, persistence);

				return ":" + message.MessageID + " `Auto toggled (now " + !isAuto + ").`";
			}

			if (editCommand.StartsWith("name"))
			{
				if (!GlobalInfo.WhiteFilters[FilterType.AnswerWhiteName].Terms.Contains(term, site)) { return "`Whitelist term does not exist.`"; }

				var isAuto = GlobalInfo.WhiteFilters[FilterType.AnswerWhiteName].Terms.First(t => t.Regex.ToString() == term.ToString()).IsAuto;

				GlobalInfo.WhiteFilters[FilterType.AnswerWhiteName].SetAuto(term, !isAuto, site, persistence);

				return ":" + message.MessageID + " `Auto toggled (now " + !isAuto + ").`";
			}

			return ":" + message.MessageID + " `Term updated.`";
		}

		# endregion

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
				var newMessage = MessageCleaner.GetCleanMessage(reportID);

				MessageHandler.EditMessage(newMessage, reportID);
			}

			return "";
		}

		private static string DeletePost()
		{
			var reportID = message.RepliesToMessageID;

			if (GlobalInfo.PostedReports.ContainsKey(reportID))
			{
				var url = GlobalInfo.PostedReports[reportID].Post.URL;

				MessageHandler.DeleteMessage(url, reportID, false);
			}

			return "";
		}

		# endregion

		# region Owner commands.

		private static string SetStatus(string command)
		{
			var newStatus = command.Remove(0, 10).Trim();

			GlobalInfo.Status = newStatus;

			return "`Status updated.`";
		}


		private static string AddUser(string command)
		{
			var id = int.Parse(command.Replace("add user", "").Trim());

			if (UserAccess.CommandAccessUsers.Contains(id)) { return "`User already has command access.`"; }

			UserAccess.AddUser(id);

			return "`User added.`";
		}

		private static string BanUser(string command)
		{
			var id = command.Replace("ban user", "").Trim();

			if (BannedUsers.IsUserBanned(id)) { return "`User is already banned.`"; }

			return BannedUsers.AddUser(id) ? "`User banned.`" : "`Warning: the banned users file is missing (unable to add user). All commands have been disabled until the issue has been resolved.`";
		}


		private static string SetAccuracyThreshold(string command)
		{
			if (command.IndexOf(" ", StringComparison.Ordinal) == -1 || command.All(c => !Char.IsDigit(c))) { return "`Command not recognised.`"; }

			var newLimit = command.Remove(0, 10);

			GlobalInfo.AccuracyThreshold = float.Parse(newLimit, CultureInfo.InvariantCulture);

			return "`Accuracy threshold updated.`";
		}


		private static string FullScan()
		{
			if (GlobalInfo.FullScanEnabled)
			{
				GlobalInfo.FullScanEnabled = false;

				return "`Full scan disabled.`";
			}

			GlobalInfo.FullScanEnabled = true;

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

		# endregion
	}
}
