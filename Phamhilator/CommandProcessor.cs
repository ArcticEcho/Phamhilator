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
		private static readonly Regex termCommands = new Regex(@"(?i)^(add|del|edit)\-(b|w)\-(a|qb|qt)\-(spam|off|name|lq) ");



		public static string[] ExacuteCommand(MessageInfo input)
		{
			if (BannedUsers.IsUserBanned(input.AuthorID.ToString(CultureInfo.InvariantCulture))) { return new[] { "" }; }

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
				catch (Exception)
				{
					return new[] { "`Error executing command.`" };
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

			// FP/TP(A) commands.

			if (commandLower == "fp")
			{
				return new[] { FalsePositive() };
			}

			if (commandLower == "fp why")
			{
				return new[] { GetTerms(), FalsePositive() };
			}

			if (commandLower == "tp" || commandLower == "tpa")
			{
				return new[] { TruePositive() };
			}

			if (commandLower == "tp why" || commandLower == "tpa why")
			{
				return new[] { GetTerms(), TruePositive() };
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
			if (commandLower == "stats" || commandLower == "info")
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

		private static string GetStats()
		{
			return "`Owners: " + GlobalInfo.Owners + ". Total terms: " + GlobalInfo.TermCount + ". Accuracy threshold: " + GlobalInfo.AccuracyThreshold + "%. Full scan enabled: " + GlobalInfo.FullScanEnabled + ". Posts caught over last 7 days: " + GlobalInfo.PostsCaught + ". Uptime: " + (DateTime.UtcNow - GlobalInfo.UpTime) + ".`";
		}

		private static string GetTerms()
		{
			var builder = new StringBuilder("`Blacklisted term(s) found: ");

			if (!GlobalInfo.PostedReports.ContainsKey(message.RepliesToMessageID))
			{
				return "`Could not find report's message ID.`";
			}

			var report = GlobalInfo.PostedReports[message.RepliesToMessageID];

			foreach (var term in report.Report.BlackTermsFound)
			{
				builder.Append(Math.Round(term.Score, 1) + "]" + term.Regex + "   ");
			}

			if (report.Report.WhiteTermsFound.Count != 0)
			{
				builder.Append("Whitelisted term(s) found: ");

				foreach (var term in report.Report.WhiteTermsFound)
				{
					builder.Append(Math.Round(term.Score, 1) + "]" + term.Regex + "   ");
				}
			}

			builder.Append("`");

			return ":" + message.MessageID + " " + builder;
		}

		#endregion

		# region Privileged user commands.

		# region Add term commands.

		private static string AddBQTTerm(string command)
		{
			var addCommand = command.Remove(0, 9);

			if (!addCommand.StartsWith("off") && !addCommand.StartsWith("spam") && !addCommand.StartsWith("lq") && !addCommand.StartsWith("name")) { return "`Command not recognised.`"; }

			Regex term;

			if (addCommand.StartsWith("off"))
			{
				term = new Regex(addCommand.Remove(0, 4));

				if (GlobalInfo.BlackFilters[FilterType.QuestionTitleBlackOff].Terms.Contains(term)) { return "`Blacklist term already exists.`"; }

				GlobalInfo.BlackFilters[FilterType.QuestionTitleBlackOff].AddTerm(new Term(term, GlobalInfo.BlackFilters[FilterType.QuestionTitleBlackOff].AverageScore));
			}

			if (addCommand.StartsWith("spam"))
			{
				term = new Regex(addCommand.Remove(0, 5));

				if (GlobalInfo.BlackFilters[FilterType.QuestionTitleBlackSpam].Terms.Contains(term)) { return "`Blacklist term already exists.`"; }

				GlobalInfo.BlackFilters[FilterType.QuestionTitleBlackSpam].AddTerm(new Term(term, GlobalInfo.BlackFilters[FilterType.QuestionTitleBlackSpam].AverageScore));
			}

			if (addCommand.StartsWith("lq"))
			{
				term = new Regex(addCommand.Remove(0, 3));

				if (GlobalInfo.BlackFilters[FilterType.QuestionTitleBlackLQ].Terms.Contains(term)) { return "`Blacklist term already exists.`"; }

				GlobalInfo.BlackFilters[FilterType.QuestionTitleBlackLQ].AddTerm(new Term(term, GlobalInfo.BlackFilters[FilterType.QuestionTitleBlackLQ].AverageScore));
			}

			if (addCommand.StartsWith("name"))
			{
				term = new Regex(addCommand.Remove(0, 5));

				if (GlobalInfo.BlackFilters[FilterType.QuestionTitleBlackName].Terms.Contains(term)) { return "`Blacklist term already exists.`"; }

				GlobalInfo.BlackFilters[FilterType.QuestionTitleBlackName].AddTerm(new Term(term, GlobalInfo.BlackFilters[FilterType.QuestionTitleBlackName].AverageScore));
			}

			return ":" + message.MessageID + " `Blacklist term added.`";
		}

		private static string AddWQTTerm(string command)
		{
			var addCommand = command.Remove(0, 9);

			if (!addCommand.StartsWith("off") && !addCommand.StartsWith("spam") && !addCommand.StartsWith("lq") && !addCommand.StartsWith("name")) { return "`Command not recognised.`"; }

			Regex term;
			string site;

			if (addCommand.StartsWith("off"))
			{
				addCommand = addCommand.Remove(0, 4);

				if (addCommand.IndexOf(" ", StringComparison.Ordinal) == -1) { return "`Command not recognised.`"; }

				term = new Regex(addCommand.Remove(0, addCommand.IndexOf(" ", StringComparison.Ordinal) + 1));
				site = addCommand.Substring(0, addCommand.IndexOf(" ", StringComparison.Ordinal));

				if (GlobalInfo.WhiteFilters[FilterType.QuestionTitleWhiteOff].Terms.Contains(term)) { return "`Whitelist term already exists.`"; }

				var score = GlobalInfo.WhiteFilters[FilterType.QuestionTitleWhiteOff].Terms.Where(t => t.Site == site).Select(t => t.Score).Average();

				GlobalInfo.WhiteFilters[FilterType.QuestionTitleWhiteOff].AddTerm(new Term(term, score, site));
			}

			if (addCommand.StartsWith("spam"))
			{
				addCommand = addCommand.Remove(0, 5);

				if (addCommand.IndexOf(" ", StringComparison.Ordinal) == -1) { return "`Command not recognised.`"; }

				term = new Regex(addCommand.Remove(0, addCommand.IndexOf(" ", StringComparison.Ordinal) + 1));
				site = addCommand.Substring(0, addCommand.IndexOf(" ", StringComparison.Ordinal));

				if (GlobalInfo.WhiteFilters[FilterType.QuestionTitleWhiteSpam].Terms.Contains(term)) { return "`Whitelist term already exists.`"; }

				var score = GlobalInfo.WhiteFilters[FilterType.QuestionTitleWhiteSpam].Terms.Where(t => t.Site == site).Select(t => t.Score).Average();

				GlobalInfo.WhiteFilters[FilterType.QuestionTitleWhiteSpam].AddTerm(new Term(term, score, site));
			}

			if (addCommand.StartsWith("lq"))
			{
				addCommand = addCommand.Remove(0, 3);

				if (addCommand.IndexOf(" ", StringComparison.Ordinal) == -1) { return "`Command not recognised.`"; }

				term = new Regex(addCommand.Remove(0, addCommand.IndexOf(" ", StringComparison.Ordinal) + 1));
				site = addCommand.Substring(0, addCommand.IndexOf(" ", StringComparison.Ordinal));

				if (GlobalInfo.WhiteFilters[FilterType.QuestionTitleWhiteLQ].Terms.Contains(term)) { return "`Whitelist term already exists.`"; }

				var score = GlobalInfo.WhiteFilters[FilterType.QuestionTitleWhiteLQ].Terms.Where(t => t.Site == site).Select(t => t.Score).Average();

				GlobalInfo.WhiteFilters[FilterType.QuestionTitleWhiteSpam].AddTerm(new Term(term, score, site));
			}

			if (addCommand.StartsWith("name"))
			{
				addCommand = addCommand.Remove(0, 5);

				if (addCommand.IndexOf(" ", StringComparison.Ordinal) == -1) { return "`Command not recognised.`"; }

				term = new Regex(addCommand.Remove(0, addCommand.IndexOf(" ", StringComparison.Ordinal) + 1));
				site = addCommand.Substring(0, addCommand.IndexOf(" ", StringComparison.Ordinal));

				if (GlobalInfo.WhiteFilters[FilterType.QuestionTitleWhiteName].Terms.Contains(term)) { return "`Whitelist term already exists.`"; }

				var score = GlobalInfo.WhiteFilters[FilterType.QuestionTitleWhiteLQ].Terms.Where(t => t.Site == site).Select(t => t.Score).Average();

				GlobalInfo.WhiteFilters[FilterType.QuestionTitleWhiteName].AddTerm(new Term(term, score, site));
			}

			return ":" + message.MessageID + " `Whitelist term added.`";
		}

		private static string AddBQBTerm(string command)
		{
			var addCommand = command.Remove(0, 9);

			if (!addCommand.StartsWith("off") && !addCommand.StartsWith("spam") && !addCommand.StartsWith("lq")) { return "`Command not recognised.`"; }

			Regex term;

			if (addCommand.StartsWith("off"))
			{
				term = new Regex(addCommand.Remove(0, 4));

				if (GlobalInfo.BlackFilters[FilterType.QuestionBodyBlackOff].Terms.Contains(term)) { return "`Blacklist term already exists.`"; }

				GlobalInfo.BlackFilters[FilterType.QuestionBodyBlackOff].AddTerm(new Term(term, GlobalInfo.BlackFilters[FilterType.QuestionBodyBlackOff].AverageScore));
			}

			if (addCommand.StartsWith("spam"))
			{
				term = new Regex(addCommand.Remove(0, 5));

				if (GlobalInfo.BlackFilters[FilterType.QuestionBodyBlackSpam].Terms.Contains(term)) { return "`Blacklist term already exists.`"; }

				GlobalInfo.BlackFilters[FilterType.QuestionBodyBlackSpam].AddTerm(new Term(term, GlobalInfo.BlackFilters[FilterType.QuestionBodyBlackSpam].AverageScore));
			}

			if (addCommand.StartsWith("lq"))
			{
				term = new Regex(addCommand.Remove(0, 3));

				if (GlobalInfo.BlackFilters[FilterType.QuestionBodyBlackLQ].Terms.Contains(term)) { return "`Blacklist term already exists.`"; }

				GlobalInfo.BlackFilters[FilterType.QuestionBodyBlackLQ].AddTerm(new Term(term, GlobalInfo.BlackFilters[FilterType.QuestionBodyBlackLQ].AverageScore));
			}

			return ":" + message.MessageID + " `Blacklist term added.`";
		}

		private static string AddWQBTerm(string command)
		{
			var addCommand = command.Remove(0, 9);

			if (!addCommand.StartsWith("off") && !addCommand.StartsWith("spam") && !addCommand.StartsWith("lq")) { return "`Command not recognised.`"; }

			Regex term;
			string site;

			if (addCommand.StartsWith("off"))
			{
				addCommand = addCommand.Remove(0, 4);

				if (addCommand.IndexOf(" ", StringComparison.Ordinal) == -1) { return "`Command not recognised.`"; }

				term = new Regex(addCommand.Remove(0, addCommand.IndexOf(" ", StringComparison.Ordinal) + 1));
				site = addCommand.Substring(0, addCommand.IndexOf(" ", StringComparison.Ordinal));

				if (GlobalInfo.WhiteFilters[FilterType.QuestionBodyWhiteOff].Terms.Contains(term)) { return "`Whitelist term already exists.`"; }

				var score = GlobalInfo.WhiteFilters[FilterType.QuestionBodyWhiteOff].Terms.Where(t => t.Site == site).Select(t => t.Score).Average();

				GlobalInfo.WhiteFilters[FilterType.QuestionBodyWhiteOff].AddTerm(new Term(term, score, site));
			}

			if (addCommand.StartsWith("spam"))
			{
				addCommand = addCommand.Remove(0, 5);

				if (addCommand.IndexOf(" ", StringComparison.Ordinal) == -1) { return "`Command not recognised.`"; }

				term = new Regex(addCommand.Remove(0, addCommand.IndexOf(" ", StringComparison.Ordinal) + 1));
				site = addCommand.Substring(0, addCommand.IndexOf(" ", StringComparison.Ordinal));

				if (GlobalInfo.WhiteFilters[FilterType.QuestionBodyWhiteSpam].Terms.Contains(term)) { return "`Whitelist term already exists.`"; }

				var score = GlobalInfo.WhiteFilters[FilterType.QuestionBodyWhiteSpam].Terms.Where(t => t.Site == site).Select(t => t.Score).Average();

				GlobalInfo.WhiteFilters[FilterType.QuestionBodyWhiteSpam].AddTerm(new Term(term, score, site));
			}

			if (addCommand.StartsWith("lq"))
			{
				addCommand = addCommand.Remove(0, 3);

				if (addCommand.IndexOf(" ", StringComparison.Ordinal) == -1) { return "`Command not recognised.`"; }

				term = new Regex(addCommand.Remove(0, addCommand.IndexOf(" ", StringComparison.Ordinal) + 1));
				site = addCommand.Substring(0, addCommand.IndexOf(" ", StringComparison.Ordinal));

				if (GlobalInfo.WhiteFilters[FilterType.QuestionBodyWhiteLQ].Terms.Contains(term)) { return "`Whitelist term already exists.`"; }

				var score = GlobalInfo.WhiteFilters[FilterType.QuestionBodyWhiteLQ].Terms.Where(t => t.Site == site).Select(t => t.Score).Average();

				GlobalInfo.WhiteFilters[FilterType.QuestionBodyWhiteSpam].AddTerm(new Term(term, score, site));
			}

			return ":" + message.MessageID + " `Whitelist term added.`";
		}

		private static string AddBATerm(string command)
		{
			var addCommand = command.Remove(0, 8);

			if (!addCommand.StartsWith("off") && !addCommand.StartsWith("spam") && !addCommand.StartsWith("lq") && !addCommand.StartsWith("name")) { return "`Command not recognised.`"; }

			Regex term;

			if (addCommand.StartsWith("off"))
			{
				term = new Regex(addCommand.Remove(0, 4));

				if (GlobalInfo.BlackFilters[FilterType.AnswerBlackOff].Terms.Contains(term)) { return "`Blacklist term already exists.`"; }

				GlobalInfo.BlackFilters[FilterType.AnswerBlackOff].AddTerm(new Term(term, GlobalInfo.BlackFilters[FilterType.AnswerBlackOff].AverageScore));
			}

			if (addCommand.StartsWith("spam"))
			{
				term = new Regex(addCommand.Remove(0, 5));

				if (GlobalInfo.BlackFilters[FilterType.AnswerBlackSpam].Terms.Contains(term)) { return "`Blacklist term already exists.`"; }

				GlobalInfo.BlackFilters[FilterType.AnswerBlackSpam].AddTerm(new Term(term, GlobalInfo.BlackFilters[FilterType.AnswerBlackSpam].AverageScore));
			}

			if (addCommand.StartsWith("lq"))
			{
				term = new Regex(addCommand.Remove(0, 3));

				if (GlobalInfo.BlackFilters[FilterType.AnswerBlackLQ].Terms.Contains(term)) { return "`Blacklist term already exists.`"; }

				GlobalInfo.BlackFilters[FilterType.AnswerBlackLQ].AddTerm(new Term(term, GlobalInfo.BlackFilters[FilterType.AnswerBlackLQ].AverageScore));
			}

			if (addCommand.StartsWith("name"))
			{
				term = new Regex(addCommand.Remove(0, 5));

				if (GlobalInfo.BlackFilters[FilterType.AnswerBlackName].Terms.Contains(term)) { return "`Blacklist term already exists.`"; }

				GlobalInfo.BlackFilters[FilterType.AnswerBlackName].AddTerm(new Term(term, GlobalInfo.BlackFilters[FilterType.AnswerBlackName].AverageScore));
			}

			return ":" + message.MessageID + " `Blacklist term added.`";
		}

		private static string AddWATerm(string command)
		{
			var addCommand = command.Substring(0, 8);

			if (!addCommand.StartsWith("off") && !addCommand.StartsWith("spam") && !addCommand.StartsWith("lq") && !addCommand.StartsWith("name")) { return "`Command not recognised.`"; }

			Regex term;
			string site;

			if (addCommand.StartsWith("off"))
			{
				addCommand = addCommand.Remove(0, 4);

				if (addCommand.IndexOf(" ", StringComparison.Ordinal) == -1) { return "`Command not recognised.`"; }

				term = new Regex(addCommand.Remove(0, addCommand.IndexOf(" ", StringComparison.Ordinal) + 1));
				site = addCommand.Substring(0, addCommand.IndexOf(" ", StringComparison.Ordinal));

				if (GlobalInfo.WhiteFilters[FilterType.AnswerWhiteOff].Terms.Contains(term)) { return "`Whitelist term already exists.`"; }

				var score = GlobalInfo.WhiteFilters[FilterType.AnswerWhiteOff].Terms.Where(t => t.Site == site).Select(t => t.Score).Average();

				GlobalInfo.WhiteFilters[FilterType.AnswerWhiteOff].AddTerm(new Term(term, score, site));
			}

			if (addCommand.StartsWith("spam"))
			{
				addCommand = addCommand.Remove(0, 5);

				if (addCommand.IndexOf(" ", StringComparison.Ordinal) == -1) { return "`Command not recognised.`"; }

				term = new Regex(addCommand.Remove(0, addCommand.IndexOf(" ", StringComparison.Ordinal) + 1));
				site = addCommand.Substring(0, addCommand.IndexOf(" ", StringComparison.Ordinal));

				if (GlobalInfo.WhiteFilters[FilterType.AnswerWhiteSpam].Terms.Contains(term)) { return "`Whitelist term already exists.`"; }

				var score = GlobalInfo.WhiteFilters[FilterType.AnswerWhiteSpam].Terms.Where(t => t.Site == site).Select(t => t.Score).Average();

				GlobalInfo.WhiteFilters[FilterType.AnswerWhiteSpam].AddTerm(new Term(term, score, site));
			}

			if (addCommand.StartsWith("lq"))
			{
				addCommand = addCommand.Remove(0, 3);

				if (addCommand.IndexOf(" ", StringComparison.Ordinal) == -1) { return "`Command not recognised.`"; }

				term = new Regex(addCommand.Remove(0, addCommand.IndexOf(" ", StringComparison.Ordinal) + 1));
				site = addCommand.Substring(0, addCommand.IndexOf(" ", StringComparison.Ordinal));

				if (GlobalInfo.WhiteFilters[FilterType.QuestionTitleWhiteLQ].Terms.Contains(term)) { return "`Whitelist term already exists.`"; }

				var score = GlobalInfo.WhiteFilters[FilterType.AnswerWhiteLQ].Terms.Where(t => t.Site == site).Select(t => t.Score).Average();

				GlobalInfo.WhiteFilters[FilterType.QuestionTitleWhiteLQ].AddTerm(new Term(term, score, site));
			}

			if (addCommand.StartsWith("name"))
			{
				addCommand = addCommand.Remove(0, 5);

				if (addCommand.IndexOf(" ", StringComparison.Ordinal) == -1) { return "`Command not recognised.`"; }

				term = new Regex(addCommand.Remove(0, addCommand.IndexOf(" ", StringComparison.Ordinal) + 1));
				site = addCommand.Substring(0, addCommand.IndexOf(" ", StringComparison.Ordinal));

				if (GlobalInfo.WhiteFilters[FilterType.QuestionTitleWhiteName].Terms.Contains(term)) { return "`Whitelist term already exists.`"; }

				var score = GlobalInfo.WhiteFilters[FilterType.AnswerWhiteName].Terms.Where(t => t.Site == site).Select(t => t.Score).Average();

				GlobalInfo.WhiteFilters[FilterType.QuestionTitleWhiteName].AddTerm(new Term(term, score, site));
			}

			return ":" + message.MessageID + " `Whitelist term added.`";
		}

		# endregion

		# region Remove term commands.

		private static string RemoveBQTTerm(string command)
		{
			var removeCommand = command.Remove(0, 9);

			if (!removeCommand.StartsWith("off") && !removeCommand.StartsWith("spam") && !removeCommand.StartsWith("lq") && !removeCommand.StartsWith("name")) { return "`Command not recognised.`"; }

			Regex term;

			if (removeCommand.StartsWith("off"))
			{
				term = new Regex(removeCommand.Remove(0, 4));

				if (!GlobalInfo.BlackFilters[FilterType.QuestionTitleBlackOff].Terms.Contains(term)) { return "`Blacklist term does not exist.`"; }

				GlobalInfo.BlackFilters[FilterType.QuestionTitleBlackOff].RemoveTerm(term);
			}

			if (removeCommand.StartsWith("spam"))
			{
				term = new Regex(removeCommand.Remove(0, 5));

				if (!GlobalInfo.BlackFilters[FilterType.QuestionTitleBlackSpam].Terms.Contains(term)) { return "`Blacklist term does not exist.`"; }

				GlobalInfo.BlackFilters[FilterType.QuestionTitleBlackSpam].RemoveTerm(term);
			}

			if (removeCommand.StartsWith("lq"))
			{
				term = new Regex(removeCommand.Remove(0, 3));

				if (!GlobalInfo.BlackFilters[FilterType.QuestionTitleBlackLQ].Terms.Contains(term)) { return "`Blacklist term does not exist.`"; }

				GlobalInfo.BlackFilters[FilterType.QuestionTitleBlackLQ].RemoveTerm(term);
			}

			if (removeCommand.StartsWith("name"))
			{
				term = new Regex(removeCommand.Remove(0, 5));

				if (!GlobalInfo.BlackFilters[FilterType.QuestionTitleBlackName].Terms.Contains(term)) { return "`Blacklist term does not exist.`"; }

				GlobalInfo.BlackFilters[FilterType.QuestionTitleBlackName].RemoveTerm(term);
			}

			return ":" + message.MessageID + " `Blacklist term removed.`";
		}

		private static string RemoveWQTTerm(string command)
		{
			var removeCommand = command.Remove(0, 9);

			if (!removeCommand.StartsWith("off") && !removeCommand.StartsWith("spam") && !removeCommand.StartsWith("lq") && !removeCommand.StartsWith("name")) { return "`Command not recognised.`"; }

			Regex term;
			string site;

			if (removeCommand.StartsWith("off"))
			{
				removeCommand = removeCommand.Remove(0, 4);

				if (removeCommand.IndexOf(" ", StringComparison.Ordinal) == -1) { return "`Command not recognised.`"; }

				term = new Regex(removeCommand.Remove(0, removeCommand.IndexOf(" ", StringComparison.Ordinal) + 1));
				site = removeCommand.Substring(0, removeCommand.IndexOf(" ", StringComparison.Ordinal));

				if (!GlobalInfo.WhiteFilters[FilterType.QuestionTitleWhiteOff].Terms.Contains(term)) { return "`Whitelist term does not exist.`"; }

				GlobalInfo.WhiteFilters[FilterType.QuestionTitleWhiteOff].RemoveTerm(new Term(term, 0, site));
			}

			if (removeCommand.StartsWith("spam"))
			{
				removeCommand = removeCommand.Remove(0, 5);

				if (removeCommand.IndexOf(" ", StringComparison.Ordinal) == -1) { return "`Command not recognised.`"; }

				term = new Regex(removeCommand.Remove(0, removeCommand.IndexOf(" ", StringComparison.Ordinal) + 1));
				site = removeCommand.Substring(0, removeCommand.IndexOf(" ", StringComparison.Ordinal));

				if (!GlobalInfo.WhiteFilters[FilterType.QuestionTitleWhiteSpam].Terms.Contains(term)) { return "`Whitelist term does not exist.`"; }

				GlobalInfo.WhiteFilters[FilterType.QuestionTitleWhiteSpam].RemoveTerm(new Term(term, 0, site));
			}

			if (removeCommand.StartsWith("lq"))
			{
				removeCommand = removeCommand.Remove(0, 3);

				if (removeCommand.IndexOf(" ", StringComparison.Ordinal) == -1) { return "`Command not recognised.`"; }

				term = new Regex(removeCommand.Remove(0, removeCommand.IndexOf(" ", StringComparison.Ordinal) + 1));
				site = removeCommand.Substring(0, removeCommand.IndexOf(" ", StringComparison.Ordinal));

				if (!GlobalInfo.WhiteFilters[FilterType.QuestionTitleWhiteLQ].Terms.Contains(term)) { return "`Whitelist term does not exist.`"; }

				GlobalInfo.WhiteFilters[FilterType.QuestionTitleWhiteLQ].RemoveTerm(new Term(term, 0, site));
			}

			if (removeCommand.StartsWith("name"))
			{
				removeCommand = removeCommand.Remove(0, 5);

				if (removeCommand.IndexOf(" ", StringComparison.Ordinal) == -1) { return "`Command not recognised.`"; }

				term = new Regex(removeCommand.Remove(0, removeCommand.IndexOf(" ", StringComparison.Ordinal) + 1));
				site = removeCommand.Substring(0, removeCommand.IndexOf(" ", StringComparison.Ordinal));

				if (!GlobalInfo.WhiteFilters[FilterType.QuestionTitleWhiteName].Terms.Contains(term)) { return "`Whitelist term does not exist.`"; }

				GlobalInfo.WhiteFilters[FilterType.QuestionTitleWhiteName].RemoveTerm(new Term(term, 0, site));
			}

			return ":" + message.MessageID + " `Whitelist term removed.`";
		}

		private static string RemoveBQBTerm(string command)
		{
			var removeCommand = command.Remove(0, 9);

			if (!removeCommand.StartsWith("off") && !removeCommand.StartsWith("spam") && !removeCommand.StartsWith("lq")) { return "`Command not recognised.`"; }

			Regex term;

			if (removeCommand.StartsWith("off"))
			{
				term = new Regex(removeCommand.Remove(0, 4));

				if (!GlobalInfo.BlackFilters[FilterType.QuestionBodyBlackOff].Terms.Contains(term)) { return "`Blacklist term does not exist.`"; }

				GlobalInfo.BlackFilters[FilterType.QuestionBodyBlackOff].RemoveTerm(term);
			}

			if (removeCommand.StartsWith("spam"))
			{
				term = new Regex(removeCommand.Remove(0, 5));

				if (!GlobalInfo.BlackFilters[FilterType.QuestionBodyBlackSpam].Terms.Contains(term)) { return "`Blacklist term does not exist.`"; }

				GlobalInfo.BlackFilters[FilterType.QuestionBodyBlackSpam].RemoveTerm(term);
			}

			if (removeCommand.StartsWith("lq"))
			{
				term = new Regex(removeCommand.Remove(0, 3));

				if (!GlobalInfo.BlackFilters[FilterType.QuestionBodyBlackLQ].Terms.Contains(term)) { return "`Blacklist term does not exist.`"; }

				GlobalInfo.BlackFilters[FilterType.QuestionBodyBlackLQ].RemoveTerm(term);
			}

			return ":" + message.MessageID + " `Blacklist term removed.`";
		}

		private static string RemoveWQBTerm(string command)
		{
			var removeCommand = command.Remove(0, 9);

			if (!removeCommand.StartsWith("off") && !removeCommand.StartsWith("spam") && !removeCommand.StartsWith("lq")) { return "`Command not recognised.`"; }

			Regex term;
			string site;

			if (removeCommand.StartsWith("off"))
			{
				removeCommand = removeCommand.Remove(0, 4);

				if (removeCommand.IndexOf(" ", StringComparison.Ordinal) == -1) { return "`Command not recognised.`"; }

				term = new Regex(removeCommand.Remove(0, removeCommand.IndexOf(" ", StringComparison.Ordinal) + 1));
				site = removeCommand.Substring(0, removeCommand.IndexOf(" ", StringComparison.Ordinal));

				if (!GlobalInfo.WhiteFilters[FilterType.QuestionBodyWhiteOff].Terms.Contains(term)) { return "`Whitelist term does not exist.`"; }

				GlobalInfo.WhiteFilters[FilterType.QuestionBodyWhiteOff].RemoveTerm(new Term(term, 0, site));
			}

			if (removeCommand.StartsWith("spam"))
			{
				removeCommand = removeCommand.Remove(0, 5);

				if (removeCommand.IndexOf(" ", StringComparison.Ordinal) == -1) { return "`Command not recognised.`"; }

				term = new Regex(removeCommand.Remove(0, removeCommand.IndexOf(" ", StringComparison.Ordinal) + 1));
				site = removeCommand.Substring(0, removeCommand.IndexOf(" ", StringComparison.Ordinal));

				if (!GlobalInfo.WhiteFilters[FilterType.QuestionBodyWhiteSpam].Terms.Contains(term)) { return "`Whitelist term does not exist.`"; }

				GlobalInfo.WhiteFilters[FilterType.QuestionBodyWhiteSpam].RemoveTerm(new Term(term, 0, site));
			}

			if (removeCommand.StartsWith("lq"))
			{
				removeCommand = removeCommand.Remove(0, 3);

				if (removeCommand.IndexOf(" ", StringComparison.Ordinal) == -1) { return "`Command not recognised.`"; }

				term = new Regex(removeCommand.Remove(0, removeCommand.IndexOf(" ", StringComparison.Ordinal) + 1));
				site = removeCommand.Substring(0, removeCommand.IndexOf(" ", StringComparison.Ordinal));

				if (!GlobalInfo.WhiteFilters[FilterType.QuestionBodyWhiteLQ].Terms.Contains(term)) { return "`Whitelist term does not exist.`"; }

				GlobalInfo.WhiteFilters[FilterType.QuestionBodyWhiteLQ].RemoveTerm(new Term(term, 0, site));
			}

			return ":" + message.MessageID + " `Whitelist term removed.`";
		}

		private static string RemoveBATerm(string command)
		{
			var removeCommand = command.Remove(0, 8);

			if (!removeCommand.StartsWith("off") && !removeCommand.StartsWith("spam") && !removeCommand.StartsWith("lq") && !removeCommand.StartsWith("name")) { return "`Command not recognised.`"; }

			Regex term;

			if (removeCommand.StartsWith("off"))
			{
				term = new Regex(removeCommand.Remove(0, 4));

				if (!GlobalInfo.BlackFilters[FilterType.AnswerBlackOff].Terms.Contains(term)) { return "`Blacklist term does not exist.`"; }

				GlobalInfo.BlackFilters[FilterType.AnswerBlackOff].RemoveTerm(term);
			}

			if (removeCommand.StartsWith("spam"))
			{
				term = new Regex(removeCommand.Remove(0, 5));

				if (!GlobalInfo.BlackFilters[FilterType.AnswerBlackSpam].Terms.Contains(term)) { return "`Blacklist term does not exist.`"; }

				GlobalInfo.BlackFilters[FilterType.AnswerBlackSpam].RemoveTerm(term);
			}

			if (removeCommand.StartsWith("lq"))
			{
				term = new Regex(removeCommand.Remove(0, 3));

				if (!GlobalInfo.BlackFilters[FilterType.AnswerBlackLQ].Terms.Contains(term)) { return "`Blacklist term does not exist.`"; }

				GlobalInfo.BlackFilters[FilterType.AnswerBlackLQ].RemoveTerm(term);
			}

			if (removeCommand.StartsWith("name"))
			{
				term = new Regex(removeCommand.Remove(0, 5));

				if (!GlobalInfo.BlackFilters[FilterType.AnswerBlackName].Terms.Contains(term)) { return "`Blacklist term does not exist.`"; }

				GlobalInfo.BlackFilters[FilterType.AnswerBlackName].RemoveTerm(term);
			}

			return ":" + message.MessageID + " `Blacklist term removed.`";
		}

		private static string RemoveWATerm(string command)
		{
			var removeCommand = command.Remove(0, 8);

			if (!removeCommand.StartsWith("off") && !removeCommand.StartsWith("spam") && !removeCommand.StartsWith("lq") && !removeCommand.StartsWith("name")) { return "`Command not recognised.`"; }

			Regex term;
			string site;

			if (removeCommand.StartsWith("off"))
			{
				removeCommand = removeCommand.Remove(0, 4);

				if (removeCommand.IndexOf(" ", StringComparison.Ordinal) == -1) { return "`Command not recognised.`"; }

				term = new Regex(removeCommand.Remove(0, removeCommand.IndexOf(" ", StringComparison.Ordinal) + 1));
				site = removeCommand.Substring(0, removeCommand.IndexOf(" ", StringComparison.Ordinal));

				if (!GlobalInfo.WhiteFilters[FilterType.AnswerWhiteOff].Terms.Contains(term)) { return "`Whitelist term does not exist.`"; }

				GlobalInfo.WhiteFilters[FilterType.AnswerWhiteOff].RemoveTerm(new Term(term, 0, site));
			}

			if (removeCommand.StartsWith("spam"))
			{
				removeCommand = removeCommand.Remove(0, 5);

				if (removeCommand.IndexOf(" ", StringComparison.Ordinal) == -1) { return "`Command not recognised.`"; }

				term = new Regex(removeCommand.Remove(0, removeCommand.IndexOf(" ", StringComparison.Ordinal) + 1));
				site = removeCommand.Substring(0, removeCommand.IndexOf(" ", StringComparison.Ordinal));

				if (!GlobalInfo.WhiteFilters[FilterType.AnswerWhiteSpam].Terms.Contains(term)) { return "`Whitelist term does not exist.`"; }

				GlobalInfo.WhiteFilters[FilterType.AnswerWhiteSpam].RemoveTerm(new Term(term, 0, site));
			}

			if (removeCommand.StartsWith("lq"))
			{
				removeCommand = removeCommand.Remove(0, 3);

				if (removeCommand.IndexOf(" ", StringComparison.Ordinal) == -1) { return "`Command not recognised.`"; }

				term = new Regex(removeCommand.Remove(0, removeCommand.IndexOf(" ", StringComparison.Ordinal) + 1));
				site = removeCommand.Substring(0, removeCommand.IndexOf(" ", StringComparison.Ordinal));

				if (!GlobalInfo.WhiteFilters[FilterType.AnswerWhiteLQ].Terms.Contains(term)) { return "`Whitelist term does not exist.`"; }

				GlobalInfo.WhiteFilters[FilterType.AnswerWhiteLQ].RemoveTerm(new Term(term, 0, site));
			}

			if (removeCommand.StartsWith("name"))
			{
				removeCommand = removeCommand.Remove(0, 5);

				if (removeCommand.IndexOf(" ", StringComparison.Ordinal) == -1) { return "`Command not recognised.`"; }

				term = new Regex(removeCommand.Remove(0, removeCommand.IndexOf(" ", StringComparison.Ordinal) + 1));
				site = removeCommand.Substring(0, removeCommand.IndexOf(" ", StringComparison.Ordinal));

				if (!GlobalInfo.WhiteFilters[FilterType.AnswerWhiteName].Terms.Contains(term)) { return "`Whitelist term does not exist.`"; }

				GlobalInfo.WhiteFilters[FilterType.AnswerWhiteName].RemoveTerm(new Term(term, 0, site));
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
			var newTerm = new Regex(command.Remove(0, delimiterIndex + 3));

			if (editCommand.StartsWith("off"))
			{
				if (!GlobalInfo.BlackFilters[FilterType.QuestionTitleBlackOff].Terms.Contains(oldTerm)) { return "`Blacklist term does not exist.`"; }

				GlobalInfo.BlackFilters[FilterType.QuestionTitleBlackOff].EditTerm(oldTerm, newTerm);
			}

			if (editCommand.StartsWith("spam"))
			{
				if (!GlobalInfo.BlackFilters[FilterType.QuestionTitleBlackSpam].Terms.Contains(oldTerm)) { return "`Blacklist term does not exist.`"; }

				GlobalInfo.BlackFilters[FilterType.QuestionTitleBlackSpam].EditTerm(oldTerm, newTerm);
			}

			if (editCommand.StartsWith("lq"))
			{
				if (!GlobalInfo.BlackFilters[FilterType.QuestionTitleBlackLQ].Terms.Contains(oldTerm)) { return "`Blacklist term does not exist.`"; }

				GlobalInfo.BlackFilters[FilterType.QuestionTitleBlackLQ].EditTerm(oldTerm, newTerm);
			}

			if (editCommand.StartsWith("name"))
			{
				if (!GlobalInfo.BlackFilters[FilterType.QuestionTitleBlackName].Terms.Contains(oldTerm)) { return "`Blacklist term does not exist.`"; }

				GlobalInfo.BlackFilters[FilterType.QuestionTitleBlackName].EditTerm(oldTerm, newTerm);
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
			var newTerm = new Regex(command.Remove(0, delimiterIndex + 3));

			if (editCommand.StartsWith("off"))
			{
				if (!GlobalInfo.BlackFilters[FilterType.QuestionBodyBlackOff].Terms.Contains(oldTerm)) { return "`Blacklist term does not exist.`"; }

				GlobalInfo.BlackFilters[FilterType.QuestionBodyBlackOff].EditTerm(oldTerm, newTerm);
			}

			if (editCommand.StartsWith("spam"))
			{
				if (!GlobalInfo.BlackFilters[FilterType.QuestionBodyBlackSpam].Terms.Contains(oldTerm)) { return "`Blacklist term does not exist.`"; }

				GlobalInfo.BlackFilters[FilterType.QuestionBodyBlackSpam].EditTerm(oldTerm, newTerm);
			}

			if (editCommand.StartsWith("lq"))
			{
				if (!GlobalInfo.BlackFilters[FilterType.QuestionBodyBlackLQ].Terms.Contains(oldTerm)) { return "`Blacklist term does not exist.`"; }

				GlobalInfo.BlackFilters[FilterType.QuestionBodyBlackLQ].EditTerm(oldTerm, newTerm);
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
			var newTerm = new Regex(command.Remove(0, delimiter + 3));

			if (editCommand.StartsWith("off"))
			{
				if (!GlobalInfo.WhiteFilters[FilterType.QuestionTitleWhiteOff].Terms.Contains(oldTerm)) { return "`Whitelist term does not exist.`"; }

				GlobalInfo.WhiteFilters[FilterType.QuestionTitleWhiteOff].EditTerm(oldTerm, newTerm, site);
			}

			if (editCommand.StartsWith("spam"))
			{
				if (!GlobalInfo.WhiteFilters[FilterType.QuestionTitleWhiteSpam].Terms.Contains(oldTerm)) { return "`Whitelist term does not exist.`"; }

				GlobalInfo.WhiteFilters[FilterType.QuestionTitleWhiteSpam].EditTerm(oldTerm, newTerm, site);
			}

			if (editCommand.StartsWith("lq"))
			{
				if (!GlobalInfo.WhiteFilters[FilterType.QuestionTitleWhiteLQ].Terms.Contains(oldTerm)) { return "`Whitelist term does not exist.`"; }

				GlobalInfo.WhiteFilters[FilterType.QuestionTitleWhiteLQ].EditTerm(oldTerm, newTerm, site);
			}

			if (editCommand.StartsWith("name"))
			{
				if (!GlobalInfo.WhiteFilters[FilterType.QuestionTitleWhiteName].Terms.Contains(oldTerm)) { return "`Whitelist term does not exist.`"; }

				GlobalInfo.WhiteFilters[FilterType.QuestionTitleWhiteName].EditTerm(oldTerm, newTerm, site);
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
			var newTerm = new Regex(command.Remove(0, delimiter + 3));

			if (editCommand.StartsWith("off"))
			{
				if (!GlobalInfo.WhiteFilters[FilterType.QuestionBodyWhiteOff].Terms.Contains(oldTerm)) { return "`Whitelist term does not exist.`"; }

				GlobalInfo.WhiteFilters[FilterType.QuestionBodyWhiteOff].EditTerm(oldTerm, newTerm, site);
			}

			if (editCommand.StartsWith("spam"))
			{
				if (!GlobalInfo.WhiteFilters[FilterType.QuestionBodyWhiteSpam].Terms.Contains(oldTerm)) { return "`Whitelist term does not exist.`"; }

				GlobalInfo.WhiteFilters[FilterType.QuestionBodyWhiteSpam].EditTerm(oldTerm, newTerm, site);
			}

			if (editCommand.StartsWith("lq"))
			{
				if (!GlobalInfo.WhiteFilters[FilterType.QuestionBodyWhiteLQ].Terms.Contains(oldTerm)) { return "`Whitelist term does not exist.`"; }

				GlobalInfo.WhiteFilters[FilterType.QuestionBodyWhiteLQ].EditTerm(oldTerm, newTerm, site);
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
			var newTerm = new Regex(command.Remove(0, delimiterIndex + 3));

			if (editCommand.StartsWith("off"))
			{
				if (!GlobalInfo.BlackFilters[FilterType.AnswerBlackOff].Terms.Contains(oldTerm)) { return "`Blacklist term does not exist.`"; }

				GlobalInfo.BlackFilters[FilterType.AnswerBlackOff].EditTerm(oldTerm, newTerm);
			}

			if (editCommand.StartsWith("spam"))
			{
				if (!GlobalInfo.BlackFilters[FilterType.AnswerBlackSpam].Terms.Contains(oldTerm)) { return "`Blacklist term does not exist.`"; }

				GlobalInfo.BlackFilters[FilterType.AnswerBlackSpam].EditTerm(oldTerm, newTerm);
			}

			if (editCommand.StartsWith("lq"))
			{
				if (!GlobalInfo.BlackFilters[FilterType.AnswerBlackLQ].Terms.Contains(oldTerm)) { return "`Blacklist term does not exist.`"; }

				GlobalInfo.BlackFilters[FilterType.AnswerBlackLQ].EditTerm(oldTerm, newTerm);
			}

			if (editCommand.StartsWith("name"))
			{
				if (!GlobalInfo.BlackFilters[FilterType.AnswerBlackName].Terms.Contains(oldTerm)) { return "`Blacklist term does not exist.`"; }

				GlobalInfo.BlackFilters[FilterType.AnswerBlackName].EditTerm(oldTerm, newTerm);
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
			var newTerm = new Regex(command.Remove(0, delimiter + 3));

			if (editCommand.StartsWith("off"))
			{
				if (!GlobalInfo.WhiteFilters[FilterType.AnswerWhiteOff].Terms.Contains(oldTerm)) { return "`Whitelist term does not exist.`"; }

				GlobalInfo.WhiteFilters[FilterType.AnswerWhiteOff].EditTerm(oldTerm, newTerm, site);
			}

			if (editCommand.StartsWith("spam"))
			{
				if (!GlobalInfo.WhiteFilters[FilterType.AnswerWhiteSpam].Terms.Contains(oldTerm)) { return "`Whitelist term does not exist.`"; }

				GlobalInfo.WhiteFilters[FilterType.AnswerWhiteSpam].EditTerm(oldTerm, newTerm, site);
			}

			if (editCommand.StartsWith("lq"))
			{
				if (!GlobalInfo.WhiteFilters[FilterType.AnswerWhiteLQ].Terms.Contains(oldTerm)) { return "`Whitelist term does not exist.`"; }

				GlobalInfo.WhiteFilters[FilterType.AnswerWhiteLQ].EditTerm(oldTerm, newTerm, site);
			}

			if (editCommand.StartsWith("name"))
			{
				if (!GlobalInfo.WhiteFilters[FilterType.AnswerWhiteName].Terms.Contains(oldTerm)) { return "`Whitelist term does not exist.`"; }

				GlobalInfo.WhiteFilters[FilterType.AnswerWhiteName].EditTerm(oldTerm, newTerm, site);
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

		# region FP/TP(A) commands

		private static string FalsePositive()
		{
			return message.Report.Type == PostType.BadTagUsed ? "" : RegisterFalsePositive();
		}

		private static string RegisterFalsePositive()
		{
			var newWhiteTermScore = message.Report.BlackTermsFound.Select(t => t.Score).Max() / 2;

			foreach (var filter in message.Report.FiltersUsed)
			{
				if ((int)filter > 99) // White filter
				{
					foreach (var term in message.Report.WhiteTermsFound.Where(t => t.Site == message.Post.Site))
					{
						GlobalInfo.WhiteFilters[filter].SetScore(term, term.Score + 1);
					}
				}
				else // Black filter
				{
					foreach (var term in message.Report.BlackTermsFound.Where(t => !GlobalInfo.WhiteFilters[filter.GetCorrespondingWhiteFilter()].Terms.Any(tt => tt.Site == t.Site) && !GlobalInfo.WhiteFilters[filter.GetCorrespondingWhiteFilter()].Terms.Any(tt => tt.Regex.ToString() == t.Regex.ToString())))
					{
						GlobalInfo.WhiteFilters[filter.GetCorrespondingWhiteFilter()].AddTerm(new Term(term.Regex, newWhiteTermScore, message.Post.Site));
					}
				}
			}

			return MessageHandler.DeleteMessage(message.Post.URL, message.RepliesToMessageID) ? "" : ":" + message.MessageID + " `FP acknowledged.`";
		}


		private static string TruePositive()
		{
			if (commandLower == "tpa")
			{
				var reportMessage = GlobalInfo.PostedReports[message.RepliesToMessageID];

				if (reportMessage.Report.Type == PostType.Offensive)
				{
					reportMessage.Body = MessageCleaner.GetCleanMessage(message.RepliesToMessageID);
				}

				GlobalInfo.MessagePoster.MessageQueue.Add(reportMessage, GlobalInfo.AnnouncerRoomID);			
			}

			if (message.Report.Type == PostType.BadTagUsed) { return ""; }

			var returnMessage = RegisterTruePositive();

			return commandLower == "tpa" ? "" : returnMessage;
		}

		private static string RegisterTruePositive()
		{
			foreach (var filter in message.Report.FiltersUsed.Where(filter => (int)filter < 100)) // Make sure we only get black filters.
			foreach (var blackTerm in message.Report.BlackTermsFound)
			{
				if (!GlobalInfo.BlackFilters[filter].Terms.Contains(blackTerm)) { continue; }

				GlobalInfo.BlackFilters[filter].SetScore(blackTerm, blackTerm.Score + 1);

				foreach (var whiteTerm in GlobalInfo.WhiteFilters[filter.GetCorrespondingWhiteFilter()].Terms)
				{
					if (whiteTerm.Regex.ToString() != blackTerm.Regex.ToString() || whiteTerm.Site == message.Post.Site) { continue; }

					var x = whiteTerm.Score / blackTerm.Score;

					GlobalInfo.WhiteFilters[filter.GetCorrespondingWhiteFilter()].SetScore(whiteTerm, x * (blackTerm.Score + 1));
				}
			}

			return ":" + message.MessageID + " `TP acknowledged.`";
		}

		# endregion

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
			var id = command.Replace("add user", "").Trim();

			UserAccess.AddUser(int.Parse(id));

			return "`User added.`";
		}

		private static string BanUser(string command)
		{
			var id = command.Replace("ban user", "").Trim();

			return BannedUsers.AddUser(id) ? "`User banned.`" : "`Warning: banned users dir is missing. Unable to add user.`";
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
