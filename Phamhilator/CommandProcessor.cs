using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using ChatExchangeDotNet;



namespace Phamhilator
{
	public static class CommandProcessor
	{
		private static Room room;
		private static Message message;
		private static PostAnalysis report;
		private static Post post;
		private static string commandLower = "";
		private static readonly Regex termCommands = new Regex(@"(?i)^(add|del|edit|auto)\-(b|w)\-(a|qb|qt)\-(spam|off|name|lq)(\-p)? ", RegexOptions.Compiled | RegexOptions.CultureInvariant);



		public static ReplyMessage[] ExacuteCommand(Room roomMessage, Message input)
		{
            if (BannedUsers.IsUserBanned(input.AuthorID.ToString(CultureInfo.InvariantCulture))) { return new[] { new ReplyMessage("", false) }; }

			string command;

			if (input.Content.StartsWith(">>"))
			{
				command = input.Content.Remove(0, 2).TrimStart();
			}
			else if (input.ParentID != -1 && GlobalInfo.PostedReports.ContainsKey(input.ParentID))
			{
				command = input.Content.TrimStart();
			}
			else
			{
				return new[] { new ReplyMessage("", false) };
			}

			commandLower = command.ToLowerInvariant();
			room = roomMessage;
			message = input;

			if (GlobalInfo.PostedReports.ContainsKey(input.ParentID))
			{
				report = GlobalInfo.PostedReports[input.ParentID].Report;
				post = GlobalInfo.PostedReports[input.ParentID].Post;
			}
			else
			{
				report = null;
				post = null;
			}

			if (IsNormalUserCommand(commandLower))
			{
				try
				{
					return NormalUserCommands();
				}
				catch (Exception)
				{
					return new[] { new ReplyMessage("`Error executing command.`") };
				}
			}

			if (IsPrivilegedUserCommand(commandLower))
			{
				if (!UserAccess.CommandAccessUsers.Contains(input.AuthorID) && !UserAccess.Owners.Contains(input.AuthorID))
				{
					return new[] { new ReplyMessage("`Access denied.`") };
				}

				try
				{
					return PrivilegedUserCommands(command);
				}
				catch (Exception ex)
				{
					return new[] { new ReplyMessage("`Error executing command.`") };
				}		
			}
			
			if (IsOwnerCommand(commandLower))
			{
				if (!UserAccess.Owners.Contains(input.AuthorID))
				{
					return new[] { new ReplyMessage("`Access denied.`") };
				}
				
				try
				{
					return OwnerCommand(command);
				}
				catch (Exception)
				{
					return new[] { new ReplyMessage("`Error executing command.`") };
				}
			}

			return new[] { new ReplyMessage("`Command not recognised.`") };
		}

		public static bool IsValidCommand(string command)
		{
			var lower = command.ToLowerInvariant();

			if (lower.StartsWith(">>"))
			{
				lower = lower.Remove(0, 2).TrimStart();
			}
			else if (lower.StartsWith("@" + GlobalInfo.PrimaryRoom.Me.Name.ToLowerInvariant()))
			{
				lower = lower.Remove(0, GlobalInfo.PrimaryRoom.Me.Name.Length + 1).TrimStart();
			}
			else
			{
				return false;
			}

			return IsNormalUserCommand(lower) || IsPrivilegedUserCommand(lower) || IsOwnerCommand(lower);
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

		private static ReplyMessage[] OwnerCommand(string command)
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

			return new[] { new ReplyMessage("`Command not recognised.`") };
		}


		private static bool IsPrivilegedUserCommand(string command)
		{
			return command == "fp" || command == "fp why" ||
				   command == "tp" || command == "tpa" || command == "tp why" || command == "tpa why" ||
				   command == "clean" || command == "sanitise" || command == "sanitize" ||
                   command == "del" || command == "delete" || command == "remove" ||
                   command.StartsWith("remove tag") || command.StartsWith("add tag") ||
                   command.StartsWith("flag spam http") || command.StartsWith("flag off http") ||
				   termCommands.IsMatch(command);
		}

		private static ReplyMessage[] PrivilegedUserCommands(string command)
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

            if (command.StartsWith("flag spam http"))
            {
                return new[] { FlagSpam(command) };
            }

            if (command.StartsWith("flag off http"))
            {
                return new[] { FlagOff(command) };
            }

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
				return new[] { CleanMessage() };
			}

			if (commandLower == "del" || commandLower == "delete" || commandLower == "remove")
			{
				return new[] { DeleteMessage() };
			}

			return new[] { new ReplyMessage("`Command not recognised.`") };
		}


		private static bool IsNormalUserCommand(string command)
		{
			return command == "stats" || command == "info" ||
				   command == "status" || command == "commands" ||
                   command == "terms" || command == "why" ||  
                   command == "help" || commandLower == "help edit" || commandLower == "help add" ||
                   commandLower == "help del" || commandLower == "help edit" ||
                   commandLower == "help auto" || commandLower == "help list";
		}

		private static ReplyMessage[] NormalUserCommands()
		{
			if (commandLower == "info")
			{
				return new[] { GetInfo() };
			}

			if (commandLower == "stats")
			{
				return new[] { GetStats() };
            }

            if (commandLower == "help")
            {
                return new[] { GetHelp() };
            }

            if (commandLower == "help add" || commandLower == "help del")
            {
                return new[] { GetAddDelHelp() };
            }

            if (commandLower == "help edit")
            {
                return new[] { GetEditHelp() };
            }

            if (commandLower == "help auto")
            {
                return new[] { GetAutoHelp() };
            }

            if (commandLower == "help list" || commandLower == "commands")
            {
                return new[] { GetHelpList() };
            }

			if (commandLower == "status")
			{
				return new[] { GetStatus() };
			}

			if (commandLower == "terms" || commandLower == "why")
			{
				return new[] { GetTerms() };
			}

			return new[] { new ReplyMessage("`Command not recognised.`") };
		}



		# region Normal user commands.

		private static ReplyMessage GetStatus()
		{
			return new ReplyMessage("`" + GlobalInfo.Status + "`.");
		}

        private static ReplyMessage GetHelp()
        {
            return new ReplyMessage("`See` [`here`](https://github.com/ArcticEcho/Phamhilator/wiki/Chat-Commands) `for a full list of commands.`");
        }

        private static ReplyMessage GetAddDelHelp()
        {
            return new ReplyMessage("`To add or delete a term, use \">>(add/del)-(b/w)-(a/qt/qb)-(lq/spam/off/name) (if w, term's site name) {regex-term}\". To add or delete a tag, use \">>(add/remove) {site-name} {tag-name} {link}\".`");
        }

        private static ReplyMessage GetEditHelp()
        {
            return new ReplyMessage("`To edit a term, use \">>edit-(b/w)-(a/qt/qb)-(lq/spam/off/name) (if w, term's site name) {old-term}¬¬¬{new-term}\".`");
        }

        private static ReplyMessage GetAutoHelp()
        {
            return new ReplyMessage("`To add an automatic term, use \">>auto-b-(a/qt/qb)-(lq/spam/off/name)(-p) {regex-term}\". Use \"-p\" if the change should persist past the bot's restart.`");
        }

        private static ReplyMessage GetHelpList()
        {
            return new ReplyMessage("    @" + message.AuthorName.Replace(" ", "") + "\n    Supported commands: info, stats, status, flag (spam/off) {post url}.\n    Supported replies: (fp/tp/tpa), why, sanitise/sanitize/clean, del/delete/remove.\n    Owner-only commands: start, pause, (add/ban) user {user-id}, threshold {percentage}, kill-it-with-no-regrets-for-sure, full scan, set status {message}.", false);
        }

		private static ReplyMessage GetInfo()
		{
			return new ReplyMessage("[`Phamhilator`](https://github.com/ArcticEcho/Phamhilator/wiki) `is a` [`.NET`](http://en.wikipedia.org/wiki/.NET_Framework)-`based` [`internet bot`](http://en.wikipedia.org/wiki/Internet_bot) `written in` [`C#`](http://stackoverflow.com/questions/tagged/c%23) `which watches over` [`the /realtime tab`](http://stackexchange.com/questions?tab=realtime) `of` [`Stack Exchange`](http://stackexchange.com/)`. Owners: " + GlobalInfo.Owners + ".`");
		}

		private static ReplyMessage GetStats()
		{
			var ignorePercent = Math.Round(((GlobalInfo.Stats.TotalCheckedPosts - (GlobalInfo.Stats.TotalFPCount + GlobalInfo.Stats.TotalTPCount)) / GlobalInfo.Stats.TotalCheckedPosts) * 100, 1);

			return new ReplyMessage("`Total terms: " + GlobalInfo.TermCount + ". Posts caught: " + GlobalInfo.PostsCaught + " (last 7 days), " + GlobalInfo.Stats.TotalCheckedPosts + " (total). " + "Reports ignored: " + ignorePercent + "%. Uptime: " + (DateTime.UtcNow - GlobalInfo.UpTime) + ".`");
		}

		private static ReplyMessage GetTerms()
		{
			if (report.BlackTermsFound.Count == 1)
			{
				var term = report.BlackTermsFound.First();
				var m = "`Term found: "+ term.Regex;

				if (term.TPCount + term.FPCount >= 5)
				{
					m += " (Sensitivity: " + Math.Round(term.Sensitivity * 100, 1);
					m += "%. Specificity: " + Math.Round(term.Specificity * 100, 1);
					m += "%. Ignored: " + Math.Round((term.IgnoredCount / term.CaughtCount) * 100, 1);
					m += "%. Score: " + Math.Round(term.Score, 1);
					m += ". Auto: " + term.IsAuto + ")`";
				}
				else
				{
					m += " (Ignored: " + Math.Round((term.IgnoredCount / term.CaughtCount) * 100, 1);
					m += "%. Score: " + Math.Round(term.Score, 1);
					m += ". Auto: " + term.IsAuto + ")`";
				}

				return new ReplyMessage(m);
			}

            var builder = new StringBuilder("    @" + message.AuthorName.Replace(" ", "") + "\n    Terms found:\n");

			foreach (var term in report.BlackTermsFound)
			{
				if (term.TPCount + term.FPCount >= 5)
                {
                    builder.Append("    " + term.Regex + " ");
                    builder.Append(" (Sensitivity: " + Math.Round(term.Sensitivity * 100, 1));
					builder.Append("%. Specificity: " + Math.Round(term.Specificity * 100, 1));
					builder.Append("%. Ignored: " + Math.Round((term.IgnoredCount / term.CaughtCount) * 100, 1));
					builder.Append("%. Score: " + Math.Round(term.Score, 1));
					builder.Append(". Is Auto: " + term.IsAuto + ")\n\n");
				}
				else
                {
                    builder.Append("    " + term.Regex + " ");
                    builder.Append(" (Ignored: " + Math.Round((term.IgnoredCount / term.CaughtCount) * 100, 1));
					builder.Append("%. Score: " + Math.Round(term.Score, 1));
					builder.Append(". Is Auto: " + term.IsAuto + ")\n\n");
				}		
			}

			return new ReplyMessage(builder.ToString().TrimEnd(), false);
		}

		#endregion

		# region Privileged user commands.

		# region Add term commands.

		private static ReplyMessage AddBQTTerm(string command)
		{
			var addCommand = command.Remove(0, 9);
			var term = new Regex(addCommand.Substring(addCommand.IndexOf(' ') + 1), RegexOptions.Compiled);

			if (term.IsFlakRegex()) { return new ReplyMessage("`ReDoS detected (term not added).`"); }

			switch (addCommand.ToLowerInvariant()[0])
			{
				case 'o':
				{
					if (GlobalInfo.BlackFilters[FilterType.QuestionTitleBlackOff].Terms.Contains(term)) { return new ReplyMessage("`Blacklist term already exists.`"); }

					GlobalInfo.BlackFilters[FilterType.QuestionTitleBlackOff].AddTerm(new Term(FilterType.QuestionTitleBlackOff, term, GlobalInfo.BlackFilters[FilterType.QuestionTitleBlackOff].AverageScore));

					break;
				}

				case 's':
				{
					if (GlobalInfo.BlackFilters[FilterType.QuestionTitleBlackSpam].Terms.Contains(term)) { return new ReplyMessage("`Blacklist term already exists.`"); }

					GlobalInfo.BlackFilters[FilterType.QuestionTitleBlackSpam].AddTerm(new Term(FilterType.QuestionTitleBlackSpam, term, GlobalInfo.BlackFilters[FilterType.QuestionTitleBlackSpam].AverageScore));

					break;
				}

				case 'l':
				{
					if (GlobalInfo.BlackFilters[FilterType.QuestionTitleBlackLQ].Terms.Contains(term)) { return new ReplyMessage("`Blacklist term already exists.`"); }

					GlobalInfo.BlackFilters[FilterType.QuestionTitleBlackLQ].AddTerm(new Term(FilterType.QuestionTitleBlackLQ, term, GlobalInfo.BlackFilters[FilterType.QuestionTitleBlackLQ].AverageScore));

					break;
				}

				case 'n':
				{
					if (GlobalInfo.BlackFilters[FilterType.QuestionTitleBlackName].Terms.Contains(term)) { return new ReplyMessage("`Blacklist term already exists.`"); }

					GlobalInfo.BlackFilters[FilterType.QuestionTitleBlackName].AddTerm(new Term(FilterType.QuestionTitleBlackName, term, GlobalInfo.BlackFilters[FilterType.QuestionTitleBlackName].AverageScore));

					break;
				}
			}

			return new ReplyMessage("`Blacklist term added.`");
		}

		private static ReplyMessage AddWQTTerm(string command)
		{
			var addCommand = command.Remove(0, 9);
			var firstSpace = command.IndexOf(' ');
			var secondSpace = command.IndexOf(' ', firstSpace + 1);
			var term = new Regex(command.Substring(secondSpace + 1));
			var site = command.Substring(firstSpace + 1, secondSpace - firstSpace - 1);

			if (term.IsFlakRegex()) { return new ReplyMessage("`ReDoS detected (term not added).`"); }

			switch (addCommand.ToLowerInvariant()[0])
			{
				case 'o':
				{
					if (GlobalInfo.WhiteFilters[FilterType.QuestionTitleWhiteOff].Terms.Contains(term, site)) { return new ReplyMessage("`Whitelist term already exists.`"); }

					var score = GlobalInfo.WhiteFilters[FilterType.QuestionTitleWhiteOff].Terms.Where(t => t.Site == site).Select(t => t.Score).Average();

					GlobalInfo.WhiteFilters[FilterType.QuestionTitleWhiteOff].AddTerm(new Term(FilterType.QuestionTitleWhiteOff, term, score, site));

					break;
				}

				case 's':
				{
					if (GlobalInfo.WhiteFilters[FilterType.QuestionTitleWhiteSpam].Terms.Contains(term, site)) { return new ReplyMessage("`Whitelist term already exists.`"); }

					var score = GlobalInfo.WhiteFilters[FilterType.QuestionTitleWhiteSpam].Terms.Where(t => t.Site == site).Select(t => t.Score).Average();

					GlobalInfo.WhiteFilters[FilterType.QuestionTitleWhiteSpam].AddTerm(new Term(FilterType.QuestionTitleWhiteSpam, term, score, site));

					break;
				}

				case 'l':
				{
					if (GlobalInfo.WhiteFilters[FilterType.QuestionTitleWhiteLQ].Terms.Contains(term, site)) { return new ReplyMessage("`Whitelist term already exists.`"); }

					var score = GlobalInfo.WhiteFilters[FilterType.QuestionTitleWhiteLQ].Terms.Where(t => t.Site == site).Select(t => t.Score).Average();

					GlobalInfo.WhiteFilters[FilterType.QuestionTitleWhiteLQ].AddTerm(new Term(FilterType.QuestionTitleWhiteLQ, term, score, site));

					break;
				}

				case 'n':
				{
					if (GlobalInfo.WhiteFilters[FilterType.QuestionTitleWhiteName].Terms.Contains(term, site)) { return new ReplyMessage("`Whitelist term already exists.`"); }

					var score = GlobalInfo.WhiteFilters[FilterType.QuestionTitleWhiteName].Terms.Where(t => t.Site == site).Select(t => t.Score).Average();

					GlobalInfo.WhiteFilters[FilterType.QuestionTitleWhiteName].AddTerm(new Term(FilterType.QuestionTitleWhiteName, term, score, site));

					break;
				}
			}

			return new ReplyMessage("`Whitelist term added.`");
		}

		private static ReplyMessage AddBQBTerm(string command)
		{
			var addCommand = command.Remove(0, 9);
			var term = new Regex(addCommand.Substring(addCommand.IndexOf(' ') + 1), RegexOptions.Compiled);

			if (term.IsFlakRegex()) { return new ReplyMessage("`ReDoS detected (term not added).`"); }

			switch (addCommand.ToLowerInvariant()[0])
			{
				case 'o':
				{
					if (GlobalInfo.BlackFilters[FilterType.QuestionBodyBlackOff].Terms.Contains(term)) { return new ReplyMessage("`Blacklist term already exists.`"); }

					GlobalInfo.BlackFilters[FilterType.QuestionBodyBlackOff].AddTerm(new Term(FilterType.QuestionBodyBlackOff, term, GlobalInfo.BlackFilters[FilterType.QuestionBodyBlackOff].AverageScore));

					break;
				}

				case 's':
				{
					if (GlobalInfo.BlackFilters[FilterType.QuestionBodyBlackSpam].Terms.Contains(term)) { return new ReplyMessage("`Blacklist term already exists.`"); }

					GlobalInfo.BlackFilters[FilterType.QuestionBodyBlackSpam].AddTerm(new Term(FilterType.QuestionBodyBlackSpam, term, GlobalInfo.BlackFilters[FilterType.QuestionBodyBlackSpam].AverageScore));

					break;
				}

				case 'l':
				{
					if (GlobalInfo.BlackFilters[FilterType.QuestionBodyBlackLQ].Terms.Contains(term)) { return new ReplyMessage("`Blacklist term already exists.`"); }

					GlobalInfo.BlackFilters[FilterType.QuestionBodyBlackLQ].AddTerm(new Term(FilterType.QuestionBodyBlackLQ, term, GlobalInfo.BlackFilters[FilterType.QuestionBodyBlackLQ].AverageScore));

					break;
				}
			}

			return new ReplyMessage("`Blacklist term added.`");
		}

		private static ReplyMessage AddWQBTerm(string command)
		{
			var addCommand = command.Remove(0, 9);
			var firstSpace = command.IndexOf(' ');
			var secondSpace = command.IndexOf(' ', firstSpace + 1);
			var term = new Regex(command.Substring(secondSpace + 1));
			var site = command.Substring(firstSpace + 1, secondSpace - firstSpace - 1);

			if (term.IsFlakRegex()) { return new ReplyMessage("`ReDoS detected (term not added).`"); }

			switch (addCommand.ToLowerInvariant()[0])
			{
				case 'o':
				{
					if (GlobalInfo.WhiteFilters[FilterType.QuestionBodyWhiteOff].Terms.Contains(term, site)) { return new ReplyMessage("`Whitelist term already exists.`"); }

					var score = GlobalInfo.WhiteFilters[FilterType.QuestionBodyWhiteOff].Terms.Where(t => t.Site == site).Select(t => t.Score).Average();

					GlobalInfo.WhiteFilters[FilterType.QuestionBodyWhiteOff].AddTerm(new Term(FilterType.QuestionBodyWhiteOff, term, score, site));

					break;
				}

				case 's':
				{
					if (GlobalInfo.WhiteFilters[FilterType.QuestionBodyWhiteSpam].Terms.Contains(term, site)) { return new ReplyMessage("`Whitelist term already exists.`"); }

					var score = GlobalInfo.WhiteFilters[FilterType.QuestionBodyWhiteSpam].Terms.Where(t => t.Site == site).Select(t => t.Score).Average();

					GlobalInfo.WhiteFilters[FilterType.QuestionBodyWhiteSpam].AddTerm(new Term(FilterType.QuestionBodyWhiteSpam, term, score, site));

					break;
				}

				case 'l':
				{
					if (GlobalInfo.WhiteFilters[FilterType.QuestionBodyWhiteLQ].Terms.Contains(term, site)) { return new ReplyMessage("`Whitelist term already exists.`"); }

					var score = GlobalInfo.WhiteFilters[FilterType.QuestionBodyWhiteLQ].Terms.Where(t => t.Site == site).Select(t => t.Score).Average();

					GlobalInfo.WhiteFilters[FilterType.QuestionBodyWhiteLQ].AddTerm(new Term(FilterType.QuestionBodyWhiteLQ, term, score, site));

					break;
				}
			}

			return new ReplyMessage("`Whitelist term added.`");
		}

		private static ReplyMessage AddBATerm(string command)
		{
			var addCommand = command.Remove(0, 8);
			var term = new Regex(addCommand.Substring(addCommand.IndexOf(' ') + 1), RegexOptions.Compiled);

			if (term.IsFlakRegex()) { return new ReplyMessage("`ReDoS detected (term not added).`"); }

			switch (addCommand.ToLowerInvariant()[0])
			{
				case 'o':
				{
					if (GlobalInfo.BlackFilters[FilterType.AnswerBlackOff].Terms.Contains(term)) { return new ReplyMessage("`Blacklist term already exists.`"); }

					GlobalInfo.BlackFilters[FilterType.AnswerBlackOff].AddTerm(new Term(FilterType.AnswerBlackOff, term, GlobalInfo.BlackFilters[FilterType.AnswerBlackOff].AverageScore));

					break;
				}

				case 's':
				{
					if (GlobalInfo.BlackFilters[FilterType.AnswerBlackSpam].Terms.Contains(term)) { return new ReplyMessage("`Blacklist term already exists.`"); }

					GlobalInfo.BlackFilters[FilterType.AnswerBlackSpam].AddTerm(new Term(FilterType.AnswerBlackSpam, term, GlobalInfo.BlackFilters[FilterType.AnswerBlackSpam].AverageScore));

					break;
				}

				case 'l':
				{
					if (GlobalInfo.BlackFilters[FilterType.AnswerBlackLQ].Terms.Contains(term)) { return new ReplyMessage("`Blacklist term already exists.`"); }

					GlobalInfo.BlackFilters[FilterType.AnswerBlackLQ].AddTerm(new Term(FilterType.AnswerBlackLQ, term, GlobalInfo.BlackFilters[FilterType.AnswerBlackLQ].AverageScore));

					break;
				}

				case 'n':
				{
					if (GlobalInfo.BlackFilters[FilterType.AnswerBlackName].Terms.Contains(term)) { return new ReplyMessage("`Blacklist term already exists.`"); }

					GlobalInfo.BlackFilters[FilterType.AnswerBlackName].AddTerm(new Term(FilterType.AnswerBlackName, term, GlobalInfo.BlackFilters[FilterType.AnswerBlackName].AverageScore));

					break;
				}
			}

			return new ReplyMessage("`Blacklist term added.`");
		}

		private static ReplyMessage AddWATerm(string command)
		{
			var addCommand = command.Substring(0, 8);
			var firstSpace = command.IndexOf(' ');
			var secondSpace = command.IndexOf(' ', firstSpace + 1);
			var term = new Regex(command.Substring(secondSpace + 1));
			var site = command.Substring(firstSpace + 1, secondSpace - firstSpace - 1);

			if (term.IsFlakRegex()) { return new ReplyMessage("`ReDoS detected (term not added).`"); }

			switch (addCommand.ToLowerInvariant()[0])
			{
				case 'o':
				{
					if (GlobalInfo.WhiteFilters[FilterType.AnswerWhiteOff].Terms.Contains(term, site)) { return new ReplyMessage("`Whitelist term already exists.`"); }

					var score = GlobalInfo.WhiteFilters[FilterType.AnswerWhiteOff].Terms.Where(t => t.Site == site).Select(t => t.Score).Average();

					GlobalInfo.WhiteFilters[FilterType.AnswerWhiteOff].AddTerm(new Term(FilterType.AnswerWhiteOff, term, score, site));

					break;
				}

				case 's':
				{
					if (GlobalInfo.WhiteFilters[FilterType.AnswerWhiteSpam].Terms.Contains(term, site)) { return new ReplyMessage("`Whitelist term already exists.`"); }

					var score = GlobalInfo.WhiteFilters[FilterType.AnswerWhiteSpam].Terms.Where(t => t.Site == site).Select(t => t.Score).Average();

					GlobalInfo.WhiteFilters[FilterType.AnswerWhiteSpam].AddTerm(new Term(FilterType.AnswerWhiteSpam, term, score, site));

					break;
				}

				case 'l':
				{
					if (GlobalInfo.WhiteFilters[FilterType.AnswerWhiteLQ].Terms.Contains(term, site)) { return new ReplyMessage("`Whitelist term already exists.`"); }

					var score = GlobalInfo.WhiteFilters[FilterType.AnswerWhiteLQ].Terms.Where(t => t.Site == site).Select(t => t.Score).Average();

					GlobalInfo.WhiteFilters[FilterType.AnswerWhiteLQ].AddTerm(new Term(FilterType.AnswerWhiteLQ, term, score, site));

					break;
				}

				case 'n':
				{
					if (GlobalInfo.WhiteFilters[FilterType.AnswerWhiteName].Terms.Contains(term, site)) { return new ReplyMessage("`Whitelist term already exists.`"); }

					var score = GlobalInfo.WhiteFilters[FilterType.AnswerWhiteName].Terms.Where(t => t.Site == site).Select(t => t.Score).Average();

					GlobalInfo.WhiteFilters[FilterType.AnswerWhiteName].AddTerm(new Term(FilterType.AnswerWhiteName, term, score, site));

					break;
				}
			}

			return new ReplyMessage("`Whitelist term added.`");
		}

		# endregion

		# region Remove term commands.

		private static ReplyMessage RemoveBQTTerm(string command)
		{
			var removeCommand = command.Remove(0, 9);
			var term = new Regex(removeCommand.Substring(removeCommand.IndexOf(' ') + 1), RegexOptions.Compiled);

			switch (removeCommand.ToLowerInvariant()[0])
			{
				case 'o':
				{
					if (!GlobalInfo.BlackFilters[FilterType.QuestionTitleBlackOff].Terms.Contains(term)) { return new ReplyMessage("`Blacklist term does not exist.`"); }

                    GlobalInfo.Stats.TotalCheckedPosts -= GlobalInfo.BlackFilters[FilterType.QuestionTitleBlackOff].Terms.GetRealTerm(term).CaughtCount;
					GlobalInfo.BlackFilters[FilterType.QuestionTitleBlackOff].RemoveTerm(term);

					break;
				}

				case 's':
				{
					if (!GlobalInfo.BlackFilters[FilterType.QuestionTitleBlackSpam].Terms.Contains(term)) { return new ReplyMessage("`Blacklist term does not exist.`"); }
                    
                    GlobalInfo.Stats.TotalCheckedPosts -= GlobalInfo.BlackFilters[FilterType.QuestionTitleBlackSpam].Terms.GetRealTerm(term).CaughtCount;
					GlobalInfo.BlackFilters[FilterType.QuestionTitleBlackSpam].RemoveTerm(term);

					break;
				}

				case 'l':
				{
					if (!GlobalInfo.BlackFilters[FilterType.QuestionTitleBlackLQ].Terms.Contains(term)) { return new ReplyMessage("`Blacklist term does not exist.`"); }

                    GlobalInfo.Stats.TotalCheckedPosts -= GlobalInfo.BlackFilters[FilterType.QuestionTitleBlackLQ].Terms.GetRealTerm(term).CaughtCount;
					GlobalInfo.BlackFilters[FilterType.QuestionTitleBlackLQ].RemoveTerm(term);

					break;
				}

				case 'n':
				{
					if (!GlobalInfo.BlackFilters[FilterType.QuestionTitleBlackName].Terms.Contains(term)) { return new ReplyMessage("`Blacklist term does not exist.`"); }

				    GlobalInfo.Stats.TotalCheckedPosts -= GlobalInfo.BlackFilters[FilterType.QuestionTitleBlackName].Terms.GetRealTerm(term).CaughtCount;
					GlobalInfo.BlackFilters[FilterType.QuestionTitleBlackName].RemoveTerm(term);

					break;
				}
			}

            

			return new ReplyMessage("`Blacklist term removed.`");
		}

		private static ReplyMessage RemoveWQTTerm(string command)
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
					if (!GlobalInfo.WhiteFilters[FilterType.QuestionTitleWhiteOff].Terms.Contains(term, site)) { return new ReplyMessage("`Whitelist term does not exist.`"); }

					GlobalInfo.WhiteFilters[FilterType.QuestionTitleWhiteOff].RemoveTerm(new Term(FilterType.QuestionTitleWhiteOff, term, 0, site));

					break;
				}

				case 's':
				{
					if (!GlobalInfo.WhiteFilters[FilterType.QuestionTitleWhiteSpam].Terms.Contains(term, site)) { return new ReplyMessage("`Whitelist term does not exist.`"); }

					GlobalInfo.WhiteFilters[FilterType.QuestionTitleWhiteSpam].RemoveTerm(new Term(FilterType.QuestionTitleWhiteSpam, term, 0, site));

					break;
				}

				case 'l':
				{
					if (!GlobalInfo.WhiteFilters[FilterType.QuestionTitleWhiteLQ].Terms.Contains(term, site)) { return new ReplyMessage("`Whitelist term does not exist.`"); }

					GlobalInfo.WhiteFilters[FilterType.QuestionTitleWhiteLQ].RemoveTerm(new Term(FilterType.QuestionTitleWhiteLQ, term, 0, site));

					break;
				}
				
				case 'n':
				{
					if (!GlobalInfo.WhiteFilters[FilterType.QuestionTitleWhiteName].Terms.Contains(term, site)) { return new ReplyMessage("`Whitelist term does not exist.`"); }

					GlobalInfo.WhiteFilters[FilterType.QuestionTitleWhiteName].RemoveTerm(new Term(FilterType.QuestionTitleWhiteName, term, 0, site));

					break;
				}
			}

			return new ReplyMessage("`Whitelist term removed.`");
		}

		private static ReplyMessage RemoveBQBTerm(string command)
		{
			var removeCommand = command.Remove(0, 9);
			var term = new Regex(removeCommand.Substring(removeCommand.IndexOf(' ') + 1), RegexOptions.Compiled);

			switch (removeCommand.ToLowerInvariant()[0])
			{
				case 'o':
				{
					if (!GlobalInfo.BlackFilters[FilterType.QuestionBodyBlackOff].Terms.Contains(term)) { return new ReplyMessage("`Blacklist term does not exist.`"); }

                    GlobalInfo.Stats.TotalCheckedPosts -= GlobalInfo.BlackFilters[FilterType.QuestionBodyBlackOff].Terms.GetRealTerm(term).CaughtCount;
					GlobalInfo.BlackFilters[FilterType.QuestionBodyBlackOff].RemoveTerm(term);

					break;
				}

				case 's':
				{
					if (!GlobalInfo.BlackFilters[FilterType.QuestionBodyBlackSpam].Terms.Contains(term)) { return new ReplyMessage("`Blacklist term does not exist.`"); }

                    GlobalInfo.Stats.TotalCheckedPosts -= GlobalInfo.BlackFilters[FilterType.QuestionBodyBlackSpam].Terms.GetRealTerm(term).CaughtCount;
					GlobalInfo.BlackFilters[FilterType.QuestionBodyBlackSpam].RemoveTerm(term);

					break;
				}

				case 'l':
				{
					if (!GlobalInfo.BlackFilters[FilterType.QuestionBodyBlackLQ].Terms.Contains(term)) { return new ReplyMessage("`Blacklist term does not exist.`"); }

                    GlobalInfo.Stats.TotalCheckedPosts -= GlobalInfo.BlackFilters[FilterType.QuestionBodyBlackLQ].Terms.GetRealTerm(term).CaughtCount;
					GlobalInfo.BlackFilters[FilterType.QuestionBodyBlackLQ].RemoveTerm(term);

					break;
				}
			}

			return new ReplyMessage("`Blacklist term removed.`");
		}

		private static ReplyMessage RemoveWQBTerm(string command)
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
					if (!GlobalInfo.WhiteFilters[FilterType.QuestionBodyWhiteOff].Terms.Contains(term, site)) { return new ReplyMessage("`Whitelist term does not exist.`"); }

					GlobalInfo.WhiteFilters[FilterType.QuestionBodyWhiteOff].RemoveTerm(new Term(FilterType.QuestionBodyWhiteOff, term, 0, site));

					break;
				}

				case 's':
				{
					if (!GlobalInfo.WhiteFilters[FilterType.QuestionBodyWhiteSpam].Terms.Contains(term, site)) { return new ReplyMessage("`Whitelist term does not exist.`"); }

					GlobalInfo.WhiteFilters[FilterType.QuestionBodyWhiteSpam].RemoveTerm(new Term(FilterType.QuestionBodyWhiteSpam, term, 0, site));

					break;
				}

				case 'l':
				{
					if (!GlobalInfo.WhiteFilters[FilterType.QuestionBodyWhiteLQ].Terms.Contains(term, site)) { return new ReplyMessage("`Whitelist term does not exist.`"); }

					GlobalInfo.WhiteFilters[FilterType.QuestionBodyWhiteLQ].RemoveTerm(new Term(FilterType.QuestionBodyWhiteLQ, term, 0, site));

					break;
				}
			}

			return new ReplyMessage("`Whitelist term removed.`");
		}

		private static ReplyMessage RemoveBATerm(string command)
		{
			var removeCommand = command.Remove(0, 8);
			var term = new Regex(removeCommand.Substring(removeCommand.IndexOf(' ') + 1), RegexOptions.Compiled);

			switch (removeCommand.ToLowerInvariant()[0])
			{
				case 'o':
				{
					if (!GlobalInfo.BlackFilters[FilterType.AnswerBlackOff].Terms.Contains(term)) { return new ReplyMessage("`Blacklist term does not exist.`"); }

                    GlobalInfo.Stats.TotalCheckedPosts -= GlobalInfo.BlackFilters[FilterType.AnswerBlackOff].Terms.GetRealTerm(term).CaughtCount;
					GlobalInfo.BlackFilters[FilterType.AnswerBlackOff].RemoveTerm(term);

					break;
				}

				case 's':
				{
					if (!GlobalInfo.BlackFilters[FilterType.AnswerBlackSpam].Terms.Contains(term)) { return new ReplyMessage("`Blacklist term does not exist.`"); }

                    GlobalInfo.Stats.TotalCheckedPosts -= GlobalInfo.BlackFilters[FilterType.AnswerBlackSpam].Terms.GetRealTerm(term).CaughtCount;
					GlobalInfo.BlackFilters[FilterType.AnswerBlackSpam].RemoveTerm(term);

					break;
				}

				case 'l':
				{
					if (!GlobalInfo.BlackFilters[FilterType.AnswerBlackLQ].Terms.Contains(term)) { return new ReplyMessage("`Blacklist term does not exist.`"); }

                    GlobalInfo.Stats.TotalCheckedPosts -= GlobalInfo.BlackFilters[FilterType.AnswerBlackLQ].Terms.GetRealTerm(term).CaughtCount;
					GlobalInfo.BlackFilters[FilterType.AnswerBlackLQ].RemoveTerm(term);

					break;
				}

				case 'n':
				{
					if (!GlobalInfo.BlackFilters[FilterType.AnswerBlackName].Terms.Contains(term)) { return new ReplyMessage("`Blacklist term does not exist.`"); }

                    GlobalInfo.Stats.TotalCheckedPosts -= GlobalInfo.BlackFilters[FilterType.AnswerBlackName].Terms.GetRealTerm(term).CaughtCount;
					GlobalInfo.BlackFilters[FilterType.AnswerBlackName].RemoveTerm(term);

					break;
				}
			}

			return new ReplyMessage("`Blacklist term removed.`");
		}

		private static ReplyMessage RemoveWATerm(string command)
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
					if (!GlobalInfo.WhiteFilters[FilterType.AnswerWhiteOff].Terms.Contains(term)) { return new ReplyMessage("`Whitelist term does not exist.`"); }

					GlobalInfo.WhiteFilters[FilterType.AnswerWhiteOff].RemoveTerm(new Term(FilterType.AnswerWhiteOff, term, 0, site));

					break;
				}

				case 's':
				{
					if (!GlobalInfo.WhiteFilters[FilterType.AnswerWhiteSpam].Terms.Contains(term)) { return new ReplyMessage("`Whitelist term does not exist.`"); }

					GlobalInfo.WhiteFilters[FilterType.AnswerWhiteSpam].RemoveTerm(new Term(FilterType.AnswerWhiteSpam, term, 0, site));

					break;
				}

				case 'l':
				{
					if (!GlobalInfo.WhiteFilters[FilterType.AnswerWhiteLQ].Terms.Contains(term)) { return new ReplyMessage("`Whitelist term does not exist.`"); }

					GlobalInfo.WhiteFilters[FilterType.AnswerWhiteLQ].RemoveTerm(new Term(FilterType.AnswerWhiteLQ, term, 0, site));

					break;
				}

				case 'n':
				{
					if (!GlobalInfo.WhiteFilters[FilterType.AnswerWhiteName].Terms.Contains(term)) { return new ReplyMessage("`Whitelist term does not exist.`"); }

					GlobalInfo.WhiteFilters[FilterType.AnswerWhiteName].RemoveTerm(new Term(FilterType.AnswerWhiteName, term, 0, site));

					break;
				}
			}

			return new ReplyMessage("`Whitelist term removed.`");
		}

		# endregion

		# region Edit term commands.

		private static ReplyMessage EditBQTTerm(string command)
		{
			var editCommand = command.Remove(0, 10);

			if (!editCommand.StartsWith("off") && !editCommand.StartsWith("spam") && !editCommand.StartsWith("lq") && !editCommand.StartsWith("name")) { return new ReplyMessage("`Command not recognised.`"); }

			var startIndex = command.IndexOf(' ') + 1;
			var delimiterIndex = command.IndexOf("¬¬¬", StringComparison.Ordinal);
			var oldTerm = new Regex(command.Substring(startIndex, delimiterIndex - startIndex));
			var newTerm = new Regex(command.Remove(0, delimiterIndex + 3), RegexOptions.Compiled);

			if (newTerm.IsFlakRegex()) { return new ReplyMessage("`ReDoS detected (term not updated).`"); }

			switch (editCommand.ToLowerInvariant()[0])
			{
				case 'o':
				{
					if (!GlobalInfo.BlackFilters[FilterType.QuestionTitleBlackOff].Terms.Contains(oldTerm)) { return new ReplyMessage("`Blacklist term does not exist.`"); }

					GlobalInfo.BlackFilters[FilterType.QuestionTitleBlackOff].EditTerm(oldTerm, newTerm);

					break;
				}

				case 's':
				{
					if (!GlobalInfo.BlackFilters[FilterType.QuestionTitleBlackSpam].Terms.Contains(oldTerm)) { return new ReplyMessage("`Blacklist term does not exist.`"); }

					GlobalInfo.BlackFilters[FilterType.QuestionTitleBlackSpam].EditTerm(oldTerm, newTerm);

					break;
				}

				case 'l':
				{
					if (!GlobalInfo.BlackFilters[FilterType.QuestionTitleBlackLQ].Terms.Contains(oldTerm)) { return new ReplyMessage("`Blacklist term does not exist.`"); }

					GlobalInfo.BlackFilters[FilterType.QuestionTitleBlackLQ].EditTerm(oldTerm, newTerm);

					break;
				}

				case 'n':
				{
					if (!GlobalInfo.BlackFilters[FilterType.QuestionTitleBlackName].Terms.Contains(oldTerm)) { return new ReplyMessage("`Blacklist term does not exist.`"); }

					GlobalInfo.BlackFilters[FilterType.QuestionTitleBlackName].EditTerm(oldTerm, newTerm);

					break;
				}
			}

			return new ReplyMessage("`Term updated.`");
		}

		private static ReplyMessage EditBQBTerm(string command)
		{
			var editCommand = command.Remove(0, 10);

			if (!editCommand.StartsWith("off") && !editCommand.StartsWith("spam") && !editCommand.StartsWith("lq")) { return new ReplyMessage("`Command not recognised.`"); }

			var startIndex = command.IndexOf(' ') + 1;
			var delimiterIndex = command.IndexOf("¬¬¬", StringComparison.Ordinal);
			var oldTerm = new Regex(command.Substring(startIndex, delimiterIndex - startIndex));
			var newTerm = new Regex(command.Remove(0, delimiterIndex + 3), RegexOptions.Compiled);

			if (newTerm.IsFlakRegex()) { return new ReplyMessage("`ReDoS detected (term not updated).`"); }

			switch (editCommand.ToLowerInvariant()[0])
			{
				case 'o':
				{
					if (!GlobalInfo.BlackFilters[FilterType.QuestionBodyBlackOff].Terms.Contains(oldTerm)) { return new ReplyMessage("`Blacklist term does not exist.`"); }

					GlobalInfo.BlackFilters[FilterType.QuestionBodyBlackOff].EditTerm(oldTerm, newTerm);

					break;
				}

				case 's':
				{
					if (!GlobalInfo.BlackFilters[FilterType.QuestionBodyBlackSpam].Terms.Contains(oldTerm)) { return new ReplyMessage("`Blacklist term does not exist.`"); }

					GlobalInfo.BlackFilters[FilterType.QuestionBodyBlackSpam].EditTerm(oldTerm, newTerm);

					break;
				}

				case 'l':
				{
					if (!GlobalInfo.BlackFilters[FilterType.QuestionBodyBlackLQ].Terms.Contains(oldTerm)) { return new ReplyMessage("`Blacklist term does not exist.`"); }

					GlobalInfo.BlackFilters[FilterType.QuestionBodyBlackLQ].EditTerm(oldTerm, newTerm);

					break;
				}
			}

			return new ReplyMessage("`Term updated.`");
		}

		private static ReplyMessage EditWQTTerm(string command)
		{
			var editCommand = command.Remove(0, 10);

			if (!editCommand.StartsWith("off") && !editCommand.StartsWith("spam") && !editCommand.StartsWith("lq") && !editCommand.StartsWith("name")) { return new ReplyMessage("`Command not recognised.`"); }

			var firstSpace = command.IndexOf(' ');
			var secondSpace = command.IndexOf(' ', firstSpace + 1);
			var delimiter = command.IndexOf("¬¬¬", StringComparison.Ordinal);

			var site = command.Substring(firstSpace + 1, secondSpace - firstSpace - 1);
			var oldTerm = new Regex(command.Substring(secondSpace + 1, delimiter - secondSpace - 1));
			var newTerm = new Regex(command.Remove(0, delimiter + 3), RegexOptions.Compiled);

			if (newTerm.IsFlakRegex()) { return new ReplyMessage("`ReDoS detected (term not updated).`"); }

			switch (editCommand.ToLowerInvariant()[0])
			{
				case 'o':
				{
					if (!GlobalInfo.WhiteFilters[FilterType.QuestionTitleWhiteOff].Terms.Contains(oldTerm, site)) { return new ReplyMessage("`Whitelist term does not exist.`"); }

					GlobalInfo.WhiteFilters[FilterType.QuestionTitleWhiteOff].EditTerm(oldTerm, newTerm, site);

					break;
				}

				case 's':
				{
					if (!GlobalInfo.WhiteFilters[FilterType.QuestionTitleWhiteSpam].Terms.Contains(oldTerm, site)) { return new ReplyMessage("`Whitelist term does not exist.`"); }

					GlobalInfo.WhiteFilters[FilterType.QuestionTitleWhiteSpam].EditTerm(oldTerm, newTerm, site);

					break;
				}

				case 'l':
				{
					if (!GlobalInfo.WhiteFilters[FilterType.QuestionTitleWhiteLQ].Terms.Contains(oldTerm, site)) { return new ReplyMessage("`Whitelist term does not exist.`"); }

					GlobalInfo.WhiteFilters[FilterType.QuestionTitleWhiteLQ].EditTerm(oldTerm, newTerm, site);

					break;
				}

				case 'n':
				{
					if (!GlobalInfo.WhiteFilters[FilterType.QuestionTitleWhiteName].Terms.Contains(oldTerm, site)) { return new ReplyMessage("`Whitelist term does not exist.`"); }

					GlobalInfo.WhiteFilters[FilterType.QuestionTitleWhiteName].EditTerm(oldTerm, newTerm, site);

					break;
				}
			}

			return new ReplyMessage("`Term updated.`");
		}

		private static ReplyMessage EditWQBTerm(string command)
		{
			var editCommand = command.Remove(0, 10);

			if (!editCommand.StartsWith("off") && !editCommand.StartsWith("spam") && !editCommand.StartsWith("lq")) { return new ReplyMessage("`Command not recognised.`"); }

			var firstSpace = command.IndexOf(' ');
			var secondSpace = command.IndexOf(' ', firstSpace + 1);
			var delimiter = command.IndexOf("¬¬¬", StringComparison.Ordinal);

			var site = command.Substring(firstSpace + 1, secondSpace - firstSpace - 1);
			var oldTerm = new Regex(command.Substring(secondSpace + 1, delimiter - secondSpace - 1));
			var newTerm = new Regex(command.Remove(0, delimiter + 3), RegexOptions.Compiled);

			if (newTerm.IsFlakRegex()) { return new ReplyMessage("`ReDoS detected (term not updated).`"); }

			switch (editCommand.ToLowerInvariant()[0])
			{
				case 'o':
				{
					if (!GlobalInfo.WhiteFilters[FilterType.QuestionBodyWhiteOff].Terms.Contains(oldTerm, site)) { return new ReplyMessage("`Whitelist term does not exist.`"); }

					GlobalInfo.WhiteFilters[FilterType.QuestionBodyWhiteOff].EditTerm(oldTerm, newTerm, site);

					break;
				}

				case 's':
				{
					if (!GlobalInfo.WhiteFilters[FilterType.QuestionBodyWhiteSpam].Terms.Contains(oldTerm, site)) { return new ReplyMessage("`Whitelist term does not exist.`"); }

					GlobalInfo.WhiteFilters[FilterType.QuestionBodyWhiteSpam].EditTerm(oldTerm, newTerm, site);

					break;
				}

				case 'l':
				{
					if (!GlobalInfo.WhiteFilters[FilterType.QuestionBodyWhiteLQ].Terms.Contains(oldTerm, site)) { return new ReplyMessage("`Whitelist term does not exist.`"); }

					GlobalInfo.WhiteFilters[FilterType.QuestionBodyWhiteLQ].EditTerm(oldTerm, newTerm, site);

					break;
				}
			}

			return new ReplyMessage("`Term updated.`");
		}

		private static ReplyMessage EditBATerm(string command)
		{
			var editCommand = command.Remove(0, 9);

			if (!editCommand.StartsWith("off") && !editCommand.StartsWith("spam") && !editCommand.StartsWith("lq") && !editCommand.StartsWith("name")) { return new ReplyMessage("`Command not recognised.`"); }

			var startIndex = command.IndexOf(' ') + 1;
			var delimiterIndex = command.IndexOf("¬¬¬", StringComparison.Ordinal);
			var oldTerm = new Regex(command.Substring(startIndex, delimiterIndex - startIndex));
			var newTerm = new Regex(command.Remove(0, delimiterIndex + 3), RegexOptions.Compiled);

			if (newTerm.IsFlakRegex()) { return new ReplyMessage("`ReDoS detected (term not updated).`"); }

			switch (editCommand.ToLowerInvariant()[0])
			{
				case 'o':
				{
					if (!GlobalInfo.BlackFilters[FilterType.AnswerBlackOff].Terms.Contains(oldTerm)) { return new ReplyMessage("`Blacklist term does not exist.`"); }

					GlobalInfo.BlackFilters[FilterType.AnswerBlackOff].EditTerm(oldTerm, newTerm);

					break;
				}

				case 's':
				{
					if (!GlobalInfo.BlackFilters[FilterType.AnswerBlackSpam].Terms.Contains(oldTerm)) { return new ReplyMessage("`Blacklist term does not exist.`"); }

					GlobalInfo.BlackFilters[FilterType.AnswerBlackSpam].EditTerm(oldTerm, newTerm);

					break;
				}

				case 'l':
				{
					if (!GlobalInfo.BlackFilters[FilterType.AnswerBlackLQ].Terms.Contains(oldTerm)) { return new ReplyMessage("`Blacklist term does not exist.`"); }

					GlobalInfo.BlackFilters[FilterType.AnswerBlackLQ].EditTerm(oldTerm, newTerm);

					break;
				}

				case 'n':
				{
					if (!GlobalInfo.BlackFilters[FilterType.AnswerBlackName].Terms.Contains(oldTerm)) { return new ReplyMessage("`Blacklist term does not exist.`"); }

					GlobalInfo.BlackFilters[FilterType.AnswerBlackName].EditTerm(oldTerm, newTerm);

					break;
				}
			}

			return new ReplyMessage("`Term updated.`");
		}

		private static ReplyMessage EditWATerm(string command)
		{
			var editCommand = command.Remove(0, 9);

			if (!editCommand.StartsWith("off") && !editCommand.StartsWith("spam") && !editCommand.StartsWith("lq") && !editCommand.StartsWith("name")) { return new ReplyMessage("`Command not recognised.`"); }

			var firstSpace = command.IndexOf(' ');
			var secondSpace = command.IndexOf(' ', firstSpace + 1);
			var delimiter = command.IndexOf("¬¬¬", StringComparison.Ordinal);

			var site = command.Substring(firstSpace + 1, secondSpace - firstSpace - 1);
			var oldTerm = new Regex(command.Substring(secondSpace + 1, delimiter - secondSpace - 1));
			var newTerm = new Regex(command.Remove(0, delimiter + 3), RegexOptions.Compiled);

			if (newTerm.IsFlakRegex()) { return new ReplyMessage("`ReDoS detected (term not updated).`"); }

			switch (editCommand.ToLowerInvariant()[0])
			{
				case 'o':
				{
					if (!GlobalInfo.WhiteFilters[FilterType.AnswerWhiteOff].Terms.Contains(oldTerm, site)) { return new ReplyMessage("`Whitelist term does not exist.`"); }

					GlobalInfo.WhiteFilters[FilterType.AnswerWhiteOff].EditTerm(oldTerm, newTerm, site);

					break;
				}

				case 's':
				{
					if (!GlobalInfo.WhiteFilters[FilterType.AnswerWhiteSpam].Terms.Contains(oldTerm, site)) { return new ReplyMessage("`Whitelist term does not exist.`"); }

					GlobalInfo.WhiteFilters[FilterType.AnswerWhiteSpam].EditTerm(oldTerm, newTerm, site);

					break;
				}

				case 'l':
				{
					if (!GlobalInfo.WhiteFilters[FilterType.AnswerWhiteLQ].Terms.Contains(oldTerm, site)) { return new ReplyMessage("`Whitelist term does not exist.`"); }

					GlobalInfo.WhiteFilters[FilterType.AnswerWhiteLQ].EditTerm(oldTerm, newTerm, site);

					break;
				}

				case 'n':
				{
					if (!GlobalInfo.WhiteFilters[FilterType.AnswerWhiteName].Terms.Contains(oldTerm, site)) { return new ReplyMessage("`Whitelist term does not exist.`"); }

					GlobalInfo.WhiteFilters[FilterType.AnswerWhiteName].EditTerm(oldTerm, newTerm, site);

					break;
				}
			}

			return new ReplyMessage("`Term updated.`");
		}

		# endregion

		# region FP/TP(A) commands

		private static ReplyMessage FalsePositive()
		{
			if (report.Type == PostType.BadTagUsed) { return new ReplyMessage(""); }

			var m = RegisterFalsePositive();

			return room.DeleteMessage(message.ParentID) ? new ReplyMessage("") : m;
		}

		private static ReplyMessage RegisterFalsePositive()
		{
			GlobalInfo.Stats.TotalFPCount++;

			var newWhiteTermScore = report.BlackTermsFound.Select(t => t.Score).Max() / 2;

			foreach (var filter in report.FiltersUsed)
			{
				if ((int)filter > 99) // White filter
				{
					for (var i = 0; i < report.WhiteTermsFound.Count; i++) // Do NOT change to foreach. We're (indirectly) modifying the collection.
					{
						var term = report.WhiteTermsFound.ElementAt(i);

						if (term.Site == post.Site)
						{
							GlobalInfo.WhiteFilters[filter].SetScore(term, term.Score + 1);
						}
					}
				}
				else // Black filter
				{
					foreach (var term in report.BlackTermsFound)
					{
						term.FPCount++;

						var corFilter = filter.GetCorrespondingWhiteFilter();

						if (GlobalInfo.WhiteFilters[corFilter].Terms.All(tt => tt.Site != term.Site && tt.Regex.ToString() != term.Regex.ToString()))
						{
							GlobalInfo.WhiteFilters[corFilter].AddTerm(new Term(corFilter, term.Regex, newWhiteTermScore, post.Site));
						}
					}
				}
			}

			return new ReplyMessage("`FP acknowledged.`");
		}


		private static ReplyMessage TruePositive()
		{
			if (commandLower == "tpa")
			{
				var m = room[message.ParentID].Content;

				if (report.Type == PostType.Offensive)
				{
                    m = ReportCleaner.GetSemiCleanReport(message.ParentID, report.BlackTermsFound);
				}	

				foreach (var secondaryRoom in GlobalInfo.ChatClient.Rooms.Where(r => r.ID != GlobalInfo.PrimaryRoom.ID))
				{
					var postedMessage = secondaryRoom.PostMessage(m);

				    GlobalInfo.PostedReports.Add(postedMessage.ID, new MessageInfo
				    {
				        MessageID = postedMessage.ID, Body = postedMessage.Content, RoomID = room.ID, Report = report
				    });
				}		
			}

            if (report.Type == PostType.BadTagUsed) { return new ReplyMessage(""); }

			var returnMessage = RegisterTruePositive();

			return commandLower == "tpa" ? new ReplyMessage("") : returnMessage;
		}

		private static ReplyMessage RegisterTruePositive()
		{
			GlobalInfo.Stats.TotalTPCount++;

			foreach (var filter in report.FiltersUsed.Where(filter => (int)filter < 100)) // Make sure we only get black filters.
			foreach (var blackTerm in report.BlackTermsFound.Where(blackTerm => GlobalInfo.BlackFilters[filter].Terms.Contains(blackTerm)))
			{
				GlobalInfo.BlackFilters[filter].SetScore(blackTerm, blackTerm.Score + 1);

				blackTerm.TPCount++;

				for (var i = 0; i < GlobalInfo.WhiteFilters[filter.GetCorrespondingWhiteFilter()].Terms.Count; i++) // Do NOT change to foreach. We're (indirectly) modifying the collection.
				{
					var whiteTerm = GlobalInfo.WhiteFilters[filter.GetCorrespondingWhiteFilter()].Terms.ElementAt(i);

					if (whiteTerm.Regex.ToString() != blackTerm.Regex.ToString() || whiteTerm.Site == post.Site) { continue; }

					var x = whiteTerm.Score / blackTerm.Score;

					GlobalInfo.WhiteFilters[filter.GetCorrespondingWhiteFilter()].SetScore(whiteTerm, x * (blackTerm.Score + 1));
				}
			}

			return new ReplyMessage("`TP acknowledged.`");
		}

		# endregion

		# region Auto commands

		private static ReplyMessage AutoBQTTerm(string command)
		{
			var editCommand = command.Remove(0, 10);

			if (!editCommand.StartsWith("off") && !editCommand.StartsWith("spam") && !editCommand.StartsWith("lq") && !editCommand.StartsWith("name")) { return new ReplyMessage("`Command not recognised.`"); }

			var startIndex = command.IndexOf(' ') + 1;
			var persistence = command[startIndex - 2] == 'p' || command[startIndex - 2] == 'P';
			var term = new Regex(command.Substring(startIndex, command.Length - startIndex));

			if (editCommand.StartsWith("off"))
			{
				if (!GlobalInfo.BlackFilters[FilterType.QuestionTitleBlackOff].Terms.Contains(term)) { return new ReplyMessage("`Blacklist term does not exist.`"); }

				var isAuto = GlobalInfo.BlackFilters[FilterType.QuestionTitleBlackOff].Terms.First(t => t.Regex.ToString() == term.ToString()).IsAuto;

				GlobalInfo.BlackFilters[FilterType.QuestionTitleBlackOff].SetAuto(term, !isAuto, persistence);

				return new ReplyMessage("`Auto toggled (now " + !isAuto + ").`");
			}

			if (editCommand.StartsWith("spam"))
			{
				if (!GlobalInfo.BlackFilters[FilterType.QuestionTitleBlackSpam].Terms.Contains(term)) { return new ReplyMessage("`Blacklist term does not exist.`"); }

				var isAuto = GlobalInfo.BlackFilters[FilterType.QuestionTitleBlackSpam].Terms.First(t => t.Regex.ToString() == term.ToString()).IsAuto;

				GlobalInfo.BlackFilters[FilterType.QuestionTitleBlackSpam].SetAuto(term, !isAuto, persistence);

				return new ReplyMessage("`Auto toggled (now " + !isAuto + ").`");
			}

			if (editCommand.StartsWith("lq"))
			{
				if (!GlobalInfo.BlackFilters[FilterType.QuestionTitleBlackLQ].Terms.Contains(term)) { return new ReplyMessage("`Blacklist term does not exist.`"); }

				var isAuto = GlobalInfo.BlackFilters[FilterType.QuestionTitleBlackLQ].Terms.First(t => t.Regex.ToString() == term.ToString()).IsAuto;

				GlobalInfo.BlackFilters[FilterType.QuestionTitleBlackLQ].SetAuto(term, !isAuto, persistence);

				return new ReplyMessage("`Auto toggled (now " + !isAuto + ").`");
			}

			if (editCommand.StartsWith("name"))
			{
				if (!GlobalInfo.BlackFilters[FilterType.QuestionTitleBlackName].Terms.Contains(term)) { return new ReplyMessage("`Blacklist term does not exist.`"); }

				var isAuto = GlobalInfo.BlackFilters[FilterType.QuestionTitleBlackName].Terms.First(t => t.Regex.ToString() == term.ToString()).IsAuto;

				GlobalInfo.BlackFilters[FilterType.QuestionTitleBlackName].SetAuto(term, !isAuto, persistence);

				return new ReplyMessage("`Auto toggled (now " + !isAuto + ").`");
			}

			return new ReplyMessage("`Command not recognised.`");
		}

		private static ReplyMessage AutoBQBTerm(string command)
		{
			var editCommand = command.Remove(0, 10);

			if (!editCommand.StartsWith("off") && !editCommand.StartsWith("spam") && !editCommand.StartsWith("lq")) { return new ReplyMessage("`Command not recognised.`"); }

			var startIndex = command.IndexOf(' ') + 1;
			var persistence = command[startIndex - 2] == 'p' || command[startIndex - 2] == 'P';
			var term = new Regex(command.Substring(startIndex, command.Length - startIndex));

			if (editCommand.StartsWith("off"))
			{
				if (!GlobalInfo.BlackFilters[FilterType.QuestionBodyBlackOff].Terms.Contains(term)) { return new ReplyMessage("`Blacklist term does not exist.`"); }

				var isAuto = GlobalInfo.BlackFilters[FilterType.QuestionBodyBlackOff].Terms.First(t => t.Regex.ToString() == term.ToString()).IsAuto;

				GlobalInfo.BlackFilters[FilterType.QuestionBodyBlackOff].SetAuto(term, !isAuto, persistence);

				return new ReplyMessage("`Auto toggled (now " + !isAuto + ").`");
			}

			if (editCommand.StartsWith("spam"))
			{
				if (!GlobalInfo.BlackFilters[FilterType.QuestionBodyBlackSpam].Terms.Contains(term)) { return new ReplyMessage("`Blacklist term does not exist.`"); }

				var isAuto = GlobalInfo.BlackFilters[FilterType.QuestionBodyBlackSpam].Terms.First(t => t.Regex.ToString() == term.ToString()).IsAuto;

				GlobalInfo.BlackFilters[FilterType.QuestionBodyBlackSpam].SetAuto(term, !isAuto, persistence);

				return new ReplyMessage("`Auto toggled (now " + !isAuto + ").`");
			}

			if (editCommand.StartsWith("lq"))
			{
				if (!GlobalInfo.BlackFilters[FilterType.QuestionBodyBlackLQ].Terms.Contains(term)) { return new ReplyMessage("`Blacklist term does not exist.`"); }

				var isAuto = GlobalInfo.BlackFilters[FilterType.QuestionBodyBlackLQ].Terms.First(t => t.Regex.ToString() == term.ToString()).IsAuto;

				GlobalInfo.BlackFilters[FilterType.QuestionBodyBlackLQ].SetAuto(term, !isAuto, persistence);

				return new ReplyMessage("`Auto toggled (now " + !isAuto + ").`");
			}

			return new ReplyMessage("`Term updated.`");
		}

		private static ReplyMessage AutoWQTTerm(string command)
		{
			var editCommand = command.Remove(0, 10);

			if (!editCommand.StartsWith("off") && !editCommand.StartsWith("spam") && !editCommand.StartsWith("lq") && !editCommand.StartsWith("name")) { return new ReplyMessage("`Command not recognised.`"); }

			var firstSpace = command.IndexOf(' ');
			var secondSpace = command.IndexOf(' ', firstSpace + 1);

			var persistence = command[firstSpace - 1] == 'p' || command[firstSpace - 1] == 'P';
			var site = command.Substring(firstSpace + 1, secondSpace - firstSpace - 1);
			var term = new Regex(command.Substring(secondSpace + 1, command.Length - secondSpace - 1));

			if (editCommand.StartsWith("off"))
			{
				if (!GlobalInfo.WhiteFilters[FilterType.QuestionTitleWhiteOff].Terms.Contains(term, site)) { return new ReplyMessage("`Whitelist term does not exist.`"); }

				var isAuto = GlobalInfo.WhiteFilters[FilterType.QuestionTitleWhiteOff].Terms.First(t => t.Regex.ToString() == term.ToString()).IsAuto;

				GlobalInfo.WhiteFilters[FilterType.QuestionTitleWhiteOff].SetAuto(term, !isAuto, site, persistence);

				return new ReplyMessage("`Auto toggled (now " + !isAuto + ").`");
			}

			if (editCommand.StartsWith("spam"))
			{
				if (!GlobalInfo.WhiteFilters[FilterType.QuestionTitleWhiteSpam].Terms.Contains(term, site)) { return new ReplyMessage("`Whitelist term does not exist.`"); }

				var isAuto = GlobalInfo.WhiteFilters[FilterType.QuestionTitleWhiteSpam].Terms.First(t => t.Regex.ToString() == term.ToString()).IsAuto;

				GlobalInfo.WhiteFilters[FilterType.QuestionTitleWhiteSpam].SetAuto(term, !isAuto, site, persistence);

				return new ReplyMessage("`Auto toggled (now " + !isAuto + ").`");
			}

			if (editCommand.StartsWith("lq"))
			{
				if (!GlobalInfo.WhiteFilters[FilterType.QuestionTitleWhiteLQ].Terms.Contains(term, site)) { return new ReplyMessage("`Whitelist term does not exist.`"); }

				var isAuto = GlobalInfo.WhiteFilters[FilterType.QuestionTitleWhiteLQ].Terms.First(t => t.Regex.ToString() == term.ToString()).IsAuto;

				GlobalInfo.WhiteFilters[FilterType.QuestionTitleWhiteLQ].SetAuto(term, !isAuto, site, persistence);

				return new ReplyMessage("`Auto toggled (now " + !isAuto + ").`");
			}

			if (editCommand.StartsWith("name"))
			{
				if (!GlobalInfo.WhiteFilters[FilterType.QuestionTitleWhiteName].Terms.Contains(term, site)) { return new ReplyMessage("`Whitelist term does not exist.`"); }

				var isAuto = GlobalInfo.WhiteFilters[FilterType.QuestionTitleWhiteName].Terms.First(t => t.Regex.ToString() == term.ToString()).IsAuto;

				GlobalInfo.WhiteFilters[FilterType.QuestionTitleWhiteName].SetAuto(term, !isAuto, site, persistence);

				return new ReplyMessage("`Auto toggled (now " + !isAuto + ").`");
			}

			return new ReplyMessage("`Term updated.`");
		}

		private static ReplyMessage AutoWQBTerm(string command)
		{
			var editCommand = command.Remove(0, 10);

			if (!editCommand.StartsWith("off") && !editCommand.StartsWith("spam") && !editCommand.StartsWith("lq")) { return new ReplyMessage("`Command not recognised.`"); }

			var firstSpace = command.IndexOf(' ');
			var secondSpace = command.IndexOf(' ', firstSpace + 1);

			var persistence = command[firstSpace - 1] == 'p' || command[firstSpace - 1] == 'P';
			var site = command.Substring(firstSpace + 1, secondSpace - firstSpace - 1);
			var term = new Regex(command.Substring(secondSpace + 1, command.Length - secondSpace - 1));

			if (editCommand.StartsWith("off"))
			{
				if (!GlobalInfo.WhiteFilters[FilterType.QuestionBodyWhiteOff].Terms.Contains(term, site)) { return new ReplyMessage("`Whitelist term does not exist.`"); }

				var isAuto = GlobalInfo.WhiteFilters[FilterType.QuestionBodyWhiteOff].Terms.First(t => t.Regex.ToString() == term.ToString()).IsAuto;

				GlobalInfo.WhiteFilters[FilterType.QuestionBodyWhiteOff].SetAuto(term, !isAuto, site, persistence);

				return new ReplyMessage("`Auto toggled (now " + !isAuto + ").`");
			}

			if (editCommand.StartsWith("spam"))
			{
				if (!GlobalInfo.WhiteFilters[FilterType.QuestionBodyWhiteSpam].Terms.Contains(term, site)) { return new ReplyMessage("`Whitelist term does not exist.`"); }

				var isAuto = GlobalInfo.WhiteFilters[FilterType.QuestionBodyWhiteSpam].Terms.First(t => t.Regex.ToString() == term.ToString()).IsAuto;

				GlobalInfo.WhiteFilters[FilterType.QuestionBodyWhiteSpam].SetAuto(term, !isAuto, site, persistence);

				return new ReplyMessage("`Auto toggled (now " + !isAuto + ").`");
			}

			if (editCommand.StartsWith("lq"))
			{
				if (!GlobalInfo.WhiteFilters[FilterType.QuestionBodyWhiteLQ].Terms.Contains(term, site)) { return new ReplyMessage("`Whitelist term does not exist.`"); }

				var isAuto = GlobalInfo.WhiteFilters[FilterType.QuestionBodyWhiteLQ].Terms.First(t => t.Regex.ToString() == term.ToString()).IsAuto;

				GlobalInfo.WhiteFilters[FilterType.QuestionBodyWhiteLQ].SetAuto(term, !isAuto, site, persistence);

				return new ReplyMessage("`Auto toggled (now " + !isAuto + ").`");
			}

			return new ReplyMessage("`Term updated.`");
		}

		private static ReplyMessage AutoBATerm(string command)
		{
			var editCommand = command.Remove(0, 9);

			if (!editCommand.StartsWith("off") && !editCommand.StartsWith("spam") && !editCommand.StartsWith("lq") && !editCommand.StartsWith("name")) { return new ReplyMessage("`Command not recognised.`"); }

			var startIndex = command.IndexOf(' ') + 1;
			var persistence = command[startIndex - 2] == 'p' || command[startIndex - 2] == 'P';
			var term = new Regex(command.Substring(startIndex, command.Length - startIndex));

			if (editCommand.StartsWith("off"))
			{
				if (!GlobalInfo.BlackFilters[FilterType.AnswerBlackOff].Terms.Contains(term)) { return new ReplyMessage("`Blacklist term does not exist.`"); }

				var isAuto = GlobalInfo.BlackFilters[FilterType.AnswerBlackOff].Terms.First(t => t.Regex.ToString() == term.ToString()).IsAuto;

				GlobalInfo.BlackFilters[FilterType.AnswerBlackOff].SetAuto(term, !isAuto, persistence);

				return new ReplyMessage("`Auto toggled (now " + !isAuto + ").`");
			}

			if (editCommand.StartsWith("spam"))
			{
				if (!GlobalInfo.BlackFilters[FilterType.AnswerBlackSpam].Terms.Contains(term)) { return new ReplyMessage("`Blacklist term does not exist.`"); }

				var isAuto = GlobalInfo.BlackFilters[FilterType.AnswerBlackSpam].Terms.First(t => t.Regex.ToString() == term.ToString()).IsAuto;

				GlobalInfo.BlackFilters[FilterType.AnswerBlackSpam].SetAuto(term, !isAuto, persistence);

				return new ReplyMessage("`Auto toggled (now " + !isAuto + ").`");
			}

			if (editCommand.StartsWith("lq"))
			{
				if (!GlobalInfo.BlackFilters[FilterType.AnswerBlackLQ].Terms.Contains(term)) { return new ReplyMessage("`Blacklist term does not exist.`"); }

				var isAuto = GlobalInfo.BlackFilters[FilterType.AnswerBlackLQ].Terms.First(t => t.Regex.ToString() == term.ToString()).IsAuto;

				GlobalInfo.BlackFilters[FilterType.AnswerBlackLQ].SetAuto(term, !isAuto, persistence);

				return new ReplyMessage("`Auto toggled (now " + !isAuto + ").`");
			}

			if (editCommand.StartsWith("name"))
			{
				if (!GlobalInfo.BlackFilters[FilterType.AnswerBlackName].Terms.Contains(term)) { return new ReplyMessage("`Blacklist term does not exist.`"); }

				var isAuto = GlobalInfo.BlackFilters[FilterType.AnswerBlackName].Terms.First(t => t.Regex.ToString() == term.ToString()).IsAuto;

				GlobalInfo.BlackFilters[FilterType.AnswerBlackName].SetAuto(term, !isAuto, persistence);

				return new ReplyMessage("`Auto toggled (now " + !isAuto + ").`");
			}

			return new ReplyMessage("`Term updated.`");
		}

		private static ReplyMessage AutoWATerm(string command)
		{
			var editCommand = command.Remove(0, 9);

			if (!editCommand.StartsWith("off") && !editCommand.StartsWith("spam") && !editCommand.StartsWith("lq") && !editCommand.StartsWith("name")) { return new ReplyMessage("`Command not recognised.`"); }

			var firstSpace = command.IndexOf(' ');
			var secondSpace = command.IndexOf(' ', firstSpace + 1);

			var persistence = command[firstSpace - 1] == 'p' || command[firstSpace - 1] == 'P';
			var site = command.Substring(firstSpace + 1, secondSpace - firstSpace - 1);
			var term = new Regex(command.Substring(secondSpace + 1, command.Length - secondSpace - 1));

			if (editCommand.StartsWith("off"))
			{
				if (!GlobalInfo.WhiteFilters[FilterType.AnswerWhiteOff].Terms.Contains(term, site)) { return new ReplyMessage("`Whitelist term does not exist.`"); }

				var isAuto = GlobalInfo.WhiteFilters[FilterType.AnswerWhiteOff].Terms.First(t => t.Regex.ToString() == term.ToString()).IsAuto;

				GlobalInfo.WhiteFilters[FilterType.AnswerWhiteOff].SetAuto(term, !isAuto, site, persistence);

				return new ReplyMessage("`Auto toggled (now " + !isAuto + ").`");
			}

			if (editCommand.StartsWith("spam"))
			{
				if (!GlobalInfo.WhiteFilters[FilterType.AnswerWhiteSpam].Terms.Contains(term, site)) { return new ReplyMessage("`Whitelist term does not exist.`"); }

				var isAuto = GlobalInfo.WhiteFilters[FilterType.AnswerWhiteSpam].Terms.First(t => t.Regex.ToString() == term.ToString()).IsAuto;

				GlobalInfo.WhiteFilters[FilterType.AnswerWhiteSpam].SetAuto(term, !isAuto, site, persistence);

				return new ReplyMessage("`Auto toggled (now " + !isAuto + ").`");
			}

			if (editCommand.StartsWith("lq"))
			{
				if (!GlobalInfo.WhiteFilters[FilterType.AnswerWhiteLQ].Terms.Contains(term, site)) { return new ReplyMessage("`Whitelist term does not exist.`"); }

				var isAuto = GlobalInfo.WhiteFilters[FilterType.AnswerWhiteLQ].Terms.First(t => t.Regex.ToString() == term.ToString()).IsAuto;

				GlobalInfo.WhiteFilters[FilterType.AnswerWhiteLQ].SetAuto(term, !isAuto, site, persistence);

				return new ReplyMessage("`Auto toggled (now " + !isAuto + ").`");
			}

			if (editCommand.StartsWith("name"))
			{
				if (!GlobalInfo.WhiteFilters[FilterType.AnswerWhiteName].Terms.Contains(term, site)) { return new ReplyMessage("`Whitelist term does not exist.`"); }

				var isAuto = GlobalInfo.WhiteFilters[FilterType.AnswerWhiteName].Terms.First(t => t.Regex.ToString() == term.ToString()).IsAuto;

				GlobalInfo.WhiteFilters[FilterType.AnswerWhiteName].SetAuto(term, !isAuto, site, persistence);

				return new ReplyMessage("`Auto toggled (now " + !isAuto + ").`");
			}

			return new ReplyMessage("`Term updated.`");
		}

		# endregion

		private static ReplyMessage AddTag(string command)
		{
			var tagCommand = command.Remove(0, command.IndexOf("tag", StringComparison.Ordinal) + 4);

			if (tagCommand.Count(c => c == ' ') != 1 && tagCommand.Count(c => c == ' ') != 3) { return new ReplyMessage("`Command not recognised.`"); }

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

			if (BadTagDefinitions.BadTags.ContainsKey(site) && BadTagDefinitions.BadTags[site].ContainsKey(tag)) { return new ReplyMessage("`Tag already exists.`"); }

			BadTagDefinitions.AddTag(site, tag, metaPost);

			return new ReplyMessage("`Tag added.`");
		}

		private static ReplyMessage RemoveTag(string command)
		{
			var tagCommand = command.Remove(0, command.IndexOf("tag", StringComparison.Ordinal) + 4);

			if (tagCommand.Count(c => c == ' ') != 1) { return new ReplyMessage("`Command not recognised.`"); }

			var site = tagCommand.Substring(0, tagCommand.IndexOf(" ", StringComparison.Ordinal));
			var tag = tagCommand.Remove(0, tagCommand.IndexOf(" ", StringComparison.Ordinal) + 1);

			if (BadTagDefinitions.BadTags.ContainsKey(site))
			{
				if (BadTagDefinitions.BadTags[site].ContainsKey(tag))
				{
					BadTagDefinitions.RemoveTag(site, tag);

					return new ReplyMessage("`Tag removed.`");
				}

				return new ReplyMessage("`Tag does not exist.`");
			}

			return new ReplyMessage("`Site does not exist.`");
		}

		private static ReplyMessage CleanMessage()
		{
            var newMessage = ReportCleaner.GetCleanReport(message.ParentID);

			room.EditMessage(message.ParentID, newMessage);
			
			return new ReplyMessage("");
		}

		private static ReplyMessage DeleteMessage()
		{
			room.DeleteMessage(message.ParentID);

			return new ReplyMessage("", false);
		}

        private static ReplyMessage FlagSpam(string command)
        {
            if (!command.Contains("http")) { return new ReplyMessage("`Command not recognised.`"); }

            var postUrl = command.Substring(command.IndexOf("http", StringComparison.Ordinal)).Trim();

            var success = GlobalInfo.Flagger.FlagSpam(postUrl);

            return new ReplyMessage(success ? "`Post successfuly flagged.`" : "`Unable to flag post.`");
        }

        private static ReplyMessage FlagOff(string command)
        {
            if (!command.Contains("http")) { return new ReplyMessage("`Command not recognised.`"); }

            var postUrl = command.Substring(command.IndexOf("http", StringComparison.Ordinal)).Trim();

            var success = GlobalInfo.Flagger.FlagOffensive(postUrl);

            return new ReplyMessage(success ? "`Post successfuly flagged.`" : "`Unable to flag post.`");
        }

		# endregion

		# region Owner commands.

		private static ReplyMessage SetStatus(string command)
		{
			var newStatus = command.Remove(0, 10).Trim();

			GlobalInfo.Status = newStatus;

			return new ReplyMessage("`Status updated.`");
		}


		private static ReplyMessage AddUser(string command)
		{
			var id = int.Parse(command.Replace("add user", "").Trim());

			if (UserAccess.CommandAccessUsers.Contains(id)) { return new ReplyMessage("`User already has command access.`"); }

			UserAccess.AddUser(id);

			return new ReplyMessage("`User added.`");
		}

		private static ReplyMessage BanUser(string command)
		{
			var id = command.Replace("ban user", "").Trim();

			if (BannedUsers.IsUserBanned(id)) { return new ReplyMessage("`User is already banned.`"); }

			return new ReplyMessage(BannedUsers.AddUser(id) ? "`User banned.`" : "`Warning: the banned users file is missing (unable to add user). All commands have been disabled until the issue has been resolved.`");
		}


		private static ReplyMessage SetAccuracyThreshold(string command)
		{
			if (command.IndexOf(" ", StringComparison.Ordinal) == -1 || command.All(c => !Char.IsDigit(c))) { return new ReplyMessage("`Command not recognised.`"); }

			var newLimit = command.Remove(0, 10);

			GlobalInfo.AccuracyThreshold = float.Parse(newLimit, CultureInfo.InvariantCulture);

			return new ReplyMessage("`Accuracy threshold updated.`");
		}


		private static ReplyMessage FullScan()
		{
			if (GlobalInfo.FullScanEnabled)
			{
				GlobalInfo.FullScanEnabled = false;

				return new ReplyMessage("`Full scan disabled.`");
			}

			GlobalInfo.FullScanEnabled = true;

		    return new ReplyMessage("`Full scan enabled.`");
		}


		private static ReplyMessage StartBot()
		{
			GlobalInfo.BotRunning = true;

			return new ReplyMessage("`Phamhilator™ started.`");
		}

		private static ReplyMessage PauseBot()
		{
			GlobalInfo.BotRunning = false;

			return new ReplyMessage("`Phamhilator™ paused.`");
		}

		# endregion
	}
}
