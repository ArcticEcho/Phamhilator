﻿using System;
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
			else if (input.Body.ToLowerInvariant().StartsWith("@sam") && GlobalInfo.PostedReports.ContainsKey(input.RepliesToMessageID)) // TODO: Check if message is reply to a bot message. May need to create a new type to hold chat message data.
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

				// Sam, Unihedron, ProgramFox, Jan Dvorak, rene Infinite Recursion (in that order).

				if (user != 227577 && user != 266094 && user != 229438 && user != 194047 && user != 158100 && user != 245167)
				{
					return "`Access denied.`"; 			
				}

				return PrivilegedUserCommands(command);			
			}

			return "`Command not recognised.`";
		}



		private static bool IsPrivilegedUserCommand(string command)
		{
			return command.StartsWith("fp") || command.StartsWith("false") || command.StartsWith("false pos") || command.StartsWith("false positive") ||
				   command.StartsWith("tp") || command.StartsWith("true") || command.StartsWith("true pos") || command.StartsWith("true positive") ||
				   command.StartsWith("-1") || command.StartsWith("dv") || command.StartsWith("downvote") ||
				   command.StartsWith("+1") || command.StartsWith("uv") || command.StartsWith("upvote") ||
				   command.StartsWith("remove term") ||
				   command.StartsWith("add term") ||
				   command.StartsWith("removeis term") ||
				   command.StartsWith("addis term") ||
				   command.StartsWith("add tag") ||
				   command.StartsWith("remove tag") ||
				   command.StartsWith("start") ||
				   command.StartsWith("pause");
		}

		private static string PrivilegedUserCommands(string command)
		{
			if (command.StartsWith("fp") || command.StartsWith("false") || command.StartsWith("false pos") || command.StartsWith("false positive"))
			{
				return FalsePositive(command);
			}
			if (command.StartsWith("tp") || command.StartsWith("true") || command.StartsWith("true pos") || command.StartsWith("true positive"))
			{
				return TruePositive(command);
			}

			if (command.StartsWith("-1") || command.StartsWith("dv") || command.StartsWith("downvote"))
			{
				return DownvoteTerm(command);
			}

			if (command.StartsWith("+1") || command.StartsWith("uv") || command.StartsWith("upvote"))
			{
				return UpvoteTerm(command);
			}

			if (command.StartsWith("remove term"))
			{
				return RemoveTerm(command);
			}

			if (command.StartsWith("add term"))
			{
				return AddTerm(command);
			}

			if (command.StartsWith("removeis term"))
			{
				return RemoveIgnoreTerm(command);
			}

			if (command.StartsWith("addis term"))
			{
				return AddIgnoreTerm(command);
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

			// TODO: Add commands to: get term score,
		}


		private static bool IsNormalUserCommand(string command)
		{
			return command == "stats" || command == "info" || command == "help" || command == "commands";
		}

		private static string NormalUserCommands(string command)
		{
			if (command == "stats" || command == "info")
			{
				return "`Owners: " + GlobalInfo.Owners + ". Users with command access: Jan Dvorak, rene & Infinite Recursion. Total terms: " + GlobalInfo.TermCount + ". Caught posts over last 2 days: " + GlobalInfo.PostsCaught + ". Uptime: " + (DateTime.UtcNow - GlobalInfo.UpTime) + ".`";
			}

			if (command == "help" || command == "commands")
			{
				return "`See` [`here`](https://github.com/ArcticWinter/Phamhilator/blob/master/Phamhilator/Readme%20-%20Chat%20Commands.md) `for a list of commands.`";
			}

			return "`Command not recognised.`";
		}


		private static string DownvoteTerm(string command)
		{
			var dvCommand = command.Substring(command.IndexOf(" ", StringComparison.Ordinal) + 1);

			if (dvCommand.StartsWith("") || dvCommand.StartsWith("") || dvCommand.StartsWith("") || dvCommand.StartsWith(""))
			{
				var score = 0;
				Regex term;

				if (dvCommand.StartsWith("off"))
				{
					term = new Regex(dvCommand.Remove(0, 4));

					if (!GlobalInfo.Off.Terms.ContainsTerm(term))
					{
						return "`Term does not exist.`";
					}

					score = GlobalInfo.Off.GetScore(term) - 1;

					GlobalInfo.Off.SetScore(term, score);
				}

				if (dvCommand.StartsWith("spam"))
				{
					term = new Regex(dvCommand.Remove(0, 5));

					if (!GlobalInfo.Spam.Terms.ContainsTerm(term))
					{
						return "`Term does not exist.`";
					}

					score = GlobalInfo.Spam.GetScore(term) - 1;

					GlobalInfo.Spam.SetScore(term, score);
				}

				if (dvCommand.StartsWith("lq"))
				{
					term = new Regex(dvCommand.Remove(0, 3));

					if (!GlobalInfo.LQ.Terms.ContainsTerm(term))
					{
						return "`Term does not exist.`";
					}

					score = GlobalInfo.LQ.GetScore(term) - 1;

					GlobalInfo.LQ.SetScore(term, score);
				}

				if (dvCommand.StartsWith("name"))
				{
					term = new Regex(dvCommand.Remove(0, 5));

					if (!GlobalInfo.Name.Terms.ContainsTerm(term))
					{
						return "`Term does not exist.`";
					}

					score = GlobalInfo.Name.GetScore(term) - 1;

					GlobalInfo.Name.SetScore(term, score);
				}

				return "`Term downvoted. New score: " + score + ".`";
			}

			return "`Command not recognised.`";
		}

		private static string UpvoteTerm(string command)
		{
			var uvCommand = command.Substring(command.IndexOf(" ", StringComparison.Ordinal) + 1);

			if (uvCommand.StartsWith("") || uvCommand.StartsWith("") || uvCommand.StartsWith("") || uvCommand.StartsWith(""))
			{
				var score = -1;
				Regex term;

				if (uvCommand.StartsWith("off"))
				{
					term = new Regex(uvCommand.Remove(0, 4));

					if (!GlobalInfo.Off.Terms.ContainsTerm(term))
					{
						return "`Term does not exist.`";
					}

					score = GlobalInfo.Off.GetScore(term) + 1;

					GlobalInfo.Off.SetScore(term, score);
				}

				if (uvCommand.StartsWith("spam"))
				{
					term = new Regex(uvCommand.Remove(0, 5));

					if (!GlobalInfo.Spam.Terms.ContainsTerm(term))
					{
						return "`Term does not exist.`";
					}

					score = GlobalInfo.Spam.GetScore(term) + 1;

					GlobalInfo.Spam.SetScore(term, score);
				}

				if (uvCommand.StartsWith("lq"))
				{
					term = new Regex(uvCommand.Remove(0, 3));

					if (!GlobalInfo.LQ.Terms.ContainsTerm(term))
					{
						return "`Term does not exist.`";
					}

					score = GlobalInfo.LQ.GetScore(term) + 1;

					GlobalInfo.LQ.SetScore(term, score);
				}

				if (uvCommand.StartsWith("name"))
				{
					term = new Regex(uvCommand.Remove(0, 5));

					if (!GlobalInfo.Name.Terms.ContainsTerm(term))
					{
						return "`Term does not exist.`";
					}

					score = GlobalInfo.Name.GetScore(term) + 1;

					GlobalInfo.Name.SetScore(term, score);
				}

				return "`Term upvoted. New score: " + score + ".`";
			}

			return "`Command not recognised.`";
		}

		private static string AddTerm(string command)
		{
			var addCommand = command.Substring(command.IndexOf(" ", command.IndexOf(" ", StringComparison.Ordinal) + 1, StringComparison.Ordinal) + 1);

			if (addCommand.StartsWith("off") || addCommand.StartsWith("spam") || addCommand.StartsWith("lq") || addCommand.StartsWith("name"))
			{
				Regex term;

				if (addCommand.StartsWith("off"))
				{
					term = new Regex(addCommand.Remove(0, 4));

					if (GlobalInfo.Off.Terms.ContainsTerm(term)) { return "`Term already exists.`"; }

					GlobalInfo.Off.AddTerm(term);
				}

				if (addCommand.StartsWith("spam"))
				{
					term = new Regex(addCommand.Remove(0, 5));

					if (GlobalInfo.Spam.Terms.ContainsTerm(term)) { return "`Term already exists.`"; }

					GlobalInfo.Spam.AddTerm(term);
				}

				if (addCommand.StartsWith("lq"))
				{
					term = new Regex(addCommand.Remove(0, 3));

					if (GlobalInfo.LQ.Terms.ContainsTerm(term)) { return "`Term already exists.`"; }

					GlobalInfo.LQ.AddTerm(term);
				}

				if (addCommand.StartsWith("name"))
				{
					term = new Regex(addCommand.Remove(0, 5));

					if (GlobalInfo.Name.Terms.ContainsTerm(term)) { return "`Term already exists.`"; }

					GlobalInfo.Name.AddTerm(term);
				}

				return "`Term added.`";
			}

			return "`Command not recognised.`";
		}

		private static string RemoveTerm(string command)
		{
			var removeCommand = command.Substring(command.IndexOf(" ", command.IndexOf(" ", StringComparison.Ordinal) + 1, StringComparison.Ordinal) + 1);

			if (removeCommand.StartsWith("off") || removeCommand.StartsWith("spam") || removeCommand.StartsWith("lq") || removeCommand.StartsWith("name"))
			{
				Regex term;

				if (removeCommand.StartsWith("off"))
				{
					term = new Regex(removeCommand.Remove(0, 4));

					if (!GlobalInfo.Off.Terms.ContainsTerm(term)) { return "`Term does not exist.`"; }

					GlobalInfo.Off.RemoveTerm(term);
				}

				if (removeCommand.StartsWith("spam"))
				{
					term = new Regex(removeCommand.Remove(0, 5));

					if (!GlobalInfo.Spam.Terms.ContainsTerm(term)) { return "`Term does not exist.`"; }

					GlobalInfo.Spam.RemoveTerm(term);
				}

				if (removeCommand.StartsWith("lq"))
				{
					term = new Regex(removeCommand.Remove(0, 3));

					if (!GlobalInfo.LQ.Terms.ContainsTerm(term)) { return "`Term does not exist.`"; }

					GlobalInfo.LQ.RemoveTerm( term);
				}

				if (removeCommand.StartsWith("name"))
				{
					term = new Regex(removeCommand.Remove(0, 5));

					if (!GlobalInfo.Name.Terms.ContainsTerm(term)) { return "`Term does not exist.`"; }

					GlobalInfo.Name.RemoveTerm(term);
				}

				return "`Term removed.`";
			}

			return "`Command not recognised.`";
		}


		private static string AddIgnoreTerm(string command)
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

					if (GlobalInfo.IgnoreOff.Terms.ContainsTerm(term)) { return "`Ignore term already exists.`"; }

					GlobalInfo.IgnoreOff.AddTerm(term, site);
				}

				if (addCommand.StartsWith("spam"))
				{
					addCommand = addCommand.Remove(0, 5);

					if (addCommand.IndexOf(" ", StringComparison.Ordinal) == -1) { return "`Command not recognised.`"; }

					term = new Regex(addCommand.Remove(0, addCommand.IndexOf(" ", StringComparison.Ordinal) + 1));
					site = addCommand.Substring(0, addCommand.IndexOf(" ", StringComparison.Ordinal));

					if (GlobalInfo.IgnoreSpam.Terms.ContainsTerm(term)) { return "`Ignore term already exists.`"; }

					GlobalInfo.IgnoreSpam.AddTerm(term, site);
				}

				if (addCommand.StartsWith("lq"))
				{
					addCommand = addCommand.Remove(0, 3);

					if (addCommand.IndexOf(" ", StringComparison.Ordinal) == -1) { return "`Command not recognised.`"; }

					term = new Regex(addCommand.Remove(0, addCommand.IndexOf(" ", StringComparison.Ordinal) + 1));
					site = addCommand.Substring(0, addCommand.IndexOf(" ", StringComparison.Ordinal));

					if (GlobalInfo.IgnoreLQ.Terms.ContainsTerm(term)) { return "`Ignore term already exists.`"; }

					GlobalInfo.IgnoreSpam.AddTerm(term, site);
				}

				if (addCommand.StartsWith("name"))
				{
					addCommand = addCommand.Remove(0, 5);

					if (addCommand.IndexOf(" ", StringComparison.Ordinal) == -1) { return "`Command not recognised.`"; }

					term = new Regex(addCommand.Remove(0, addCommand.IndexOf(" ", StringComparison.Ordinal) + 1));
					site = addCommand.Substring(0, addCommand.IndexOf(" ", StringComparison.Ordinal));

					if (GlobalInfo.IgnoreName.Terms.ContainsTerm(term)) { return "`Ignore term already exists.`"; }

					GlobalInfo.IgnoreName.AddTerm(term, site);
				}

				return "`Ignore term added.`";
			}

			return "`Command not recognised.`";
		}

		private static string RemoveIgnoreTerm(string command)
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

					if (!GlobalInfo.IgnoreOff.Terms.ContainsTerm(term)) { return "`Ignore term does not exist.`"; }

					GlobalInfo.IgnoreOff.RemoveTerm(term, site);
				}

				if (removeCommand.StartsWith("spam"))
				{
					removeCommand = removeCommand.Remove(0, 5);

					if (removeCommand.IndexOf(" ", StringComparison.Ordinal) == -1) { return "`Command not recognised.`"; }

					term = new Regex(removeCommand.Remove(0, removeCommand.IndexOf(" ", StringComparison.Ordinal) + 1));
					site = removeCommand.Substring(0, removeCommand.IndexOf(" ", StringComparison.Ordinal));

					if (!GlobalInfo.IgnoreSpam.Terms.ContainsTerm(term)) { return "`Ignore term does not exist.`"; }

					GlobalInfo.IgnoreSpam.RemoveTerm(term, site);
				}

				if (removeCommand.StartsWith("lq"))
				{
					removeCommand = removeCommand.Remove(0, 3);

					if (removeCommand.IndexOf(" ", StringComparison.Ordinal) == -1) { return "`Command not recognised.`"; }

					term = new Regex(removeCommand.Remove(0, removeCommand.IndexOf(" ", StringComparison.Ordinal) + 1));
					site = removeCommand.Substring(0, removeCommand.IndexOf(" ", StringComparison.Ordinal));

					if (!GlobalInfo.IgnoreLQ.Terms.ContainsTerm(term)) { return "`Ignore term does not exist.`"; }

					GlobalInfo.IgnoreLQ.RemoveTerm(term, site);
				}

				if (removeCommand.StartsWith("name"))
				{
					removeCommand = removeCommand.Remove(0, 5);

					if (removeCommand.IndexOf(" ", StringComparison.Ordinal) == -1) { return "`Command not recognised.`"; }

					term = new Regex(removeCommand.Remove(0, removeCommand.IndexOf(" ", StringComparison.Ordinal) + 1));
					site = removeCommand.Substring(0, removeCommand.IndexOf(" ", StringComparison.Ordinal));

					if (!GlobalInfo.IgnoreName.Terms.ContainsTerm(term)) { return "`Ignore term does not exist.`"; }

					GlobalInfo.IgnoreName.RemoveTerm(term, site);
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


		private static string FalsePositive(string command)
		{
			switch (message.Report.Type)
			{
				case PostType.LowQuality:
				{
					foreach (var term in message.Report.TermsFound)
					{
						GlobalInfo.LQ.SetScore(term, GlobalInfo.LQ.GetScore(term) - 1);
					}

					return "FP registered.";
				}

				case PostType.Offensive:
				{

					foreach (var term in message.Report.TermsFound)
					{
						GlobalInfo.Off.SetScore(term, GlobalInfo.Off.GetScore(term) - 1);
					}

					return "FP registered.";
				}

				case PostType.Spam:
				{
					foreach (var term in message.Report.TermsFound)
					{
						GlobalInfo.Spam.SetScore(term, GlobalInfo.Spam.GetScore(term) - 1);
					}

					return "FP registered.";
				}

				case PostType.BadUsername:
				{
					foreach (var term in message.Report.TermsFound)
					{
						GlobalInfo.Name.SetScore(term, GlobalInfo.Name.GetScore(term) - 1);
					}

					return "FP registered.";
				}
			} 
			
			return "`Command not recognised.`";
		}

		private static string TruePositive(string command)
		{
			switch (message.Report.Type) // TODO: Object reference not set to an instance of an object.
			{
				case PostType.LowQuality:
				{
					foreach (var term in message.Report.TermsFound)
					{
						GlobalInfo.LQ.SetScore(term, GlobalInfo.LQ.GetScore(term) + 1);
					}

					return "TP registered.";
				}

				case PostType.Offensive:
				{
					foreach (var term in message.Report.TermsFound)
					{
						GlobalInfo.Off.SetScore(term, GlobalInfo.Off.GetScore(term) + 1);
					}

					return "TP registered.";
				}

				case PostType.Spam:
				{
					foreach (var term in message.Report.TermsFound)
					{
						GlobalInfo.Spam.SetScore(term, GlobalInfo.Spam.GetScore(term) + 1);
					}

					return "TP registered.";
				}

				case PostType.BadUsername:
				{
					foreach (var term in message.Report.TermsFound)
					{
						GlobalInfo.Name.SetScore(term, GlobalInfo.Name.GetScore(term) + 1);
					}

					return "TP registered.";
				}
			}

			return "`Command not recognised.`";
		}
	}
}
