﻿using System;
using System.Linq;
using System.Text.RegularExpressions;



namespace Phamhilator
{
	public static class CommandProcessor
	{
		private static MessageInfo message;
		private static string commandLower = "";
		private static readonly Regex termCommands = new Regex(@"(?i)^(add|del)\-(b|w)\-(a|qb|qt)\-(spam|off|name|lq) ");



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
				   command.StartsWith("set status") ||
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

			if (commandLower.StartsWith("set status"))
			{
				return SetStatus(command);
			}

			return "`Command not recognised.`";
		}


		private static bool IsPrivilegedUserCommand(string command)
		{
			return command == "fp" ||
				   command == "tp" || command == "tpa" ||
				   command == "clean" || command == "sanitise" || command == "sanitize" ||
				   command == "del" || command == "delete" || command == "remove" ||
				   termCommands.IsMatch(command);
		}

		private static string PrivilegedUserCommands(string command)
		{
			if (commandLower == "fp")
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

			// QT term commands.

			if (commandLower.StartsWith("del-b-qt"))
			{
				return RemoveBQTTerm(command);
			}

			if (commandLower.StartsWith("add-b-qt"))
			{
				return AddBQTTerm(command);
			}

			if (commandLower.StartsWith("del-w-qt"))
			{
				return RemoveWQTTerm(command);
			}

			if (commandLower.StartsWith("add-w-qt"))
			{
				return AddWQTTerm(command);
			}

			// QB term commands.

			if (commandLower.StartsWith("del-b-qb"))
			{
				return RemoveBQBTerm(command);
			}

			if (commandLower.StartsWith("add-b-qb"))
			{
				return AddBQBTerm(command);
			}

			if (commandLower.StartsWith("del-w-qb"))
			{
				return RemoveWQBTerm(command);
			}

			if (commandLower.StartsWith("add-w-qb"))
			{
				return AddWQBTerm(command);
			}

			// A term commands.

			if (commandLower.StartsWith("del-b-a"))
			{
				return RemoveBATerm(command);
			}

			if (commandLower.StartsWith("add-b-a"))
			{
				return AddBATerm(command);
			}

			if (commandLower.StartsWith("del-w-a"))
			{
				return RemoveWATerm(command);
			}

			if (commandLower.StartsWith("add-w-a"))
			{
				return AddWATerm(command);
			}

			// Tag commands.

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
			return command == "stats" || command == "info" || command == "help" || command == "commands" || command == "status";
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

			if (commandLower == "status")
			{
				return "`Current status: " + GlobalInfo.Status + "`.";
			}

			return "`Command not recognised.`";
		}



		// Privileged user commands.



		private static string AddBQTTerm(string command)
		{
			var addCommand = command.Remove(0, 9);

			if (addCommand.StartsWith("off") || addCommand.StartsWith("spam") || addCommand.StartsWith("lq") || addCommand.StartsWith("name"))
			{
				Regex term;

				if (addCommand.StartsWith("off"))
				{
					term = new Regex(addCommand.Remove(0, 4));

					if (GlobalInfo.QTBOff.Terms.ContainsTerm(term)) { return "`Blacklist term already exists.`"; }

					GlobalInfo.QTBOff.AddTerm(term);
				}

				if (addCommand.StartsWith("spam"))
				{
					term = new Regex(addCommand.Remove(0, 5));

					if (GlobalInfo.QTBSpam.Terms.ContainsTerm(term)) { return "`Blacklist term already exists.`"; }

					GlobalInfo.QTBSpam.AddTerm(term);
				}

				if (addCommand.StartsWith("lq"))
				{
					term = new Regex(addCommand.Remove(0, 3));

					if (GlobalInfo.QTBLQ.Terms.ContainsTerm(term)) { return "`Blacklist term already exists.`"; }

					GlobalInfo.QTBLQ.AddTerm(term);
				}

				if (addCommand.StartsWith("name"))
				{
					term = new Regex(addCommand.Remove(0, 5));

					if (GlobalInfo.QTBName.Terms.ContainsTerm(term)) { return "`Blacklist term already exists.`"; }

					GlobalInfo.QTBName.AddTerm(term);
				}

				return "`Blacklist term added.`";
			}

			return "`Command not recognised.`";
		}

		private static string RemoveBQTTerm(string command)
		{
			var removeCommand = command.Remove(0, 9);

			if (removeCommand.StartsWith("off") || removeCommand.StartsWith("spam") || removeCommand.StartsWith("lq") || removeCommand.StartsWith("name"))
			{
				Regex term;

				if (removeCommand.StartsWith("off"))
				{
					term = new Regex(removeCommand.Remove(0, 4));

					if (!GlobalInfo.QTBOff.Terms.ContainsTerm(term)) { return "`Blacklist term does not exist.`"; }

					GlobalInfo.QTBOff.RemoveTerm(term);
				}

				if (removeCommand.StartsWith("spam"))
				{
					term = new Regex(removeCommand.Remove(0, 5));

					if (!GlobalInfo.QTBSpam.Terms.ContainsTerm(term)) { return "`Blacklist term does not exist.`"; }

					GlobalInfo.QTBSpam.RemoveTerm(term);
				}

				if (removeCommand.StartsWith("lq"))
				{
					term = new Regex(removeCommand.Remove(0, 3));

					if (!GlobalInfo.QTBLQ.Terms.ContainsTerm(term)) { return "`Blacklist term does not exist.`"; }

					GlobalInfo.QTBLQ.RemoveTerm(term);
				}

				if (removeCommand.StartsWith("name"))
				{
					term = new Regex(removeCommand.Remove(0, 5));

					if (!GlobalInfo.QTBName.Terms.ContainsTerm(term)) { return "`Blacklist term does not exist.`"; }

					GlobalInfo.QTBName.RemoveTerm(term);
				}

				return "`Blacklist term removed.`";
			}

			return "`Command not recognised.`";
		}


		private static string AddWQTTerm(string command)
		{
			var addCommand = command.Remove(0, 9);

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

					if (GlobalInfo.QTWhOff.Terms.ContainsKey(site) && GlobalInfo.QTWhOff.Terms[site].ContainsTerm(term)) { return "`Whitelist term already exists.`"; }

					GlobalInfo.QTWhOff.AddTerm(term, site);
				}

				if (addCommand.StartsWith("spam"))
				{
					addCommand = addCommand.Remove(0, 5);

					if (addCommand.IndexOf(" ", StringComparison.Ordinal) == -1) { return "`Command not recognised.`"; }

					term = new Regex(addCommand.Remove(0, addCommand.IndexOf(" ", StringComparison.Ordinal) + 1));
					site = addCommand.Substring(0, addCommand.IndexOf(" ", StringComparison.Ordinal));

					if (GlobalInfo.QTWSpam.Terms.ContainsKey(site) && GlobalInfo.QTWSpam.Terms[site].ContainsTerm(term)) { return "`Whitelist term already exists.`"; }

					GlobalInfo.QTWSpam.AddTerm(term, site);
				}

				if (addCommand.StartsWith("lq"))
				{
					addCommand = addCommand.Remove(0, 3);

					if (addCommand.IndexOf(" ", StringComparison.Ordinal) == -1) { return "`Command not recognised.`"; }

					term = new Regex(addCommand.Remove(0, addCommand.IndexOf(" ", StringComparison.Ordinal) + 1));
					site = addCommand.Substring(0, addCommand.IndexOf(" ", StringComparison.Ordinal));

					if (GlobalInfo.QTWLQ.Terms.ContainsKey(site) && GlobalInfo.QTWLQ.Terms[site].ContainsTerm(term)) { return "`Whitelist term already exists.`"; }

					GlobalInfo.QTWSpam.AddTerm(term, site);
				}

				if (addCommand.StartsWith("name"))
				{
					addCommand = addCommand.Remove(0, 5);

					if (addCommand.IndexOf(" ", StringComparison.Ordinal) == -1) { return "`Command not recognised.`"; }

					term = new Regex(addCommand.Remove(0, addCommand.IndexOf(" ", StringComparison.Ordinal) + 1));
					site = addCommand.Substring(0, addCommand.IndexOf(" ", StringComparison.Ordinal));

					if (GlobalInfo.QTWName.Terms.ContainsKey(site) && GlobalInfo.QTWName.Terms[site].ContainsTerm(term)) { return "`Whitelist term already exists.`"; }

					GlobalInfo.QTWName.AddTerm(term, site);
				}

				return "`Whitelist term added.`";
			}

			return "`Command not recognised.`";
		}

		private static string RemoveWQTTerm(string command)
		{
			var removeCommand = command.Remove(0, 9);

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

					if (!GlobalInfo.QTWhOff.Terms.ContainsKey(site) && !GlobalInfo.QTWhOff.Terms[site].ContainsTerm(term)) { return "`Whitelist term does not exist.`"; }

					GlobalInfo.QTWhOff.RemoveTerm(term, site);
				}

				if (removeCommand.StartsWith("spam"))
				{
					removeCommand = removeCommand.Remove(0, 5);

					if (removeCommand.IndexOf(" ", StringComparison.Ordinal) == -1) { return "`Command not recognised.`"; }

					term = new Regex(removeCommand.Remove(0, removeCommand.IndexOf(" ", StringComparison.Ordinal) + 1));
					site = removeCommand.Substring(0, removeCommand.IndexOf(" ", StringComparison.Ordinal));

					if (!GlobalInfo.QTWSpam.Terms.ContainsKey(site) && !GlobalInfo.QTWSpam.Terms[site].ContainsTerm(term)) { return "`Whitelist term does not exist.`"; }

					GlobalInfo.QTWSpam.RemoveTerm(term, site);
				}

				if (removeCommand.StartsWith("lq"))
				{
					removeCommand = removeCommand.Remove(0, 3);

					if (removeCommand.IndexOf(" ", StringComparison.Ordinal) == -1) { return "`Command not recognised.`"; }

					term = new Regex(removeCommand.Remove(0, removeCommand.IndexOf(" ", StringComparison.Ordinal) + 1));
					site = removeCommand.Substring(0, removeCommand.IndexOf(" ", StringComparison.Ordinal));

					if (!GlobalInfo.QTWLQ.Terms.ContainsKey(site) && !GlobalInfo.QTWLQ.Terms[site].ContainsTerm(term)) { return "`Whitelist term does not exist.`"; }

					GlobalInfo.QTWLQ.RemoveTerm(term, site);
				}

				if (removeCommand.StartsWith("name"))
				{
					removeCommand = removeCommand.Remove(0, 5);

					if (removeCommand.IndexOf(" ", StringComparison.Ordinal) == -1) { return "`Command not recognised.`"; }

					term = new Regex(removeCommand.Remove(0, removeCommand.IndexOf(" ", StringComparison.Ordinal) + 1));
					site = removeCommand.Substring(0, removeCommand.IndexOf(" ", StringComparison.Ordinal));

					if (!GlobalInfo.QTWName.Terms.ContainsKey(site) && !GlobalInfo.QTWName.Terms[site].ContainsTerm(term)) { return "`Whitelist term does not exist.`"; }

					GlobalInfo.QTWName.RemoveTerm(term, site);
				}

				return "`Whitelist Term removed.`";
			}

			return "`Command not recognised.`";
		}


		private static string AddBQBTerm(string command)
		{
			var addCommand = command.Remove(0, 9);

			if (addCommand.StartsWith("off") || addCommand.StartsWith("spam") || addCommand.StartsWith("lq"))
			{
				Regex term;

				if (addCommand.StartsWith("off"))
				{
					term = new Regex(addCommand.Remove(0, 4));

					if (GlobalInfo.QBBOff.Terms.ContainsTerm(term)) { return "`Blacklist term already exists.`"; }

					GlobalInfo.QBBOff.AddTerm(term);
				}

				if (addCommand.StartsWith("spam"))
				{
					term = new Regex(addCommand.Remove(0, 5));

					if (GlobalInfo.QBBSpam.Terms.ContainsTerm(term)) { return "`Blacklist term already exists.`"; }

					GlobalInfo.QBBSpam.AddTerm(term);
				}

				if (addCommand.StartsWith("lq"))
				{
					term = new Regex(addCommand.Remove(0, 3));

					if (GlobalInfo.QBBLQ.Terms.ContainsTerm(term)) { return "`Blacklist term already exists.`"; }

					GlobalInfo.QBBLQ.AddTerm(term);
				}

				return "`Blacklist term added.`";
			}

			return "`Command not recognised.`";
		}

		private static string RemoveBQBTerm(string command)
		{
			var removeCommand = command.Remove(0, 9);

			if (removeCommand.StartsWith("off") || removeCommand.StartsWith("spam") || removeCommand.StartsWith("lq"))
			{
				Regex term;

				if (removeCommand.StartsWith("off"))
				{
					term = new Regex(removeCommand.Remove(0, 4));

					if (!GlobalInfo.QBBOff.Terms.ContainsTerm(term)) { return "`Blacklist term does not exist.`"; }

					GlobalInfo.QBBOff.RemoveTerm(term);
				}

				if (removeCommand.StartsWith("spam"))
				{
					term = new Regex(removeCommand.Remove(0, 5));

					if (!GlobalInfo.QBBSpam.Terms.ContainsTerm(term)) { return "`Blacklist term does not exist.`"; }

					GlobalInfo.QBBSpam.RemoveTerm(term);
				}

				if (removeCommand.StartsWith("lq"))
				{
					term = new Regex(removeCommand.Remove(0, 3));

					if (!GlobalInfo.QBBLQ.Terms.ContainsTerm(term)) { return "`Blacklist term does not exist.`"; }

					GlobalInfo.QBBLQ.RemoveTerm(term);
				}

				return "`Blacklist term removed.`";
			}

			return "`Command not recognised.`";
		}


		private static string AddWQBTerm(string command)
		{
			var addCommand = command.Remove(0, 9);

			if (addCommand.StartsWith("off") || addCommand.StartsWith("spam") || addCommand.StartsWith("lq"))
			{
				Regex term;
				string site;

				if (addCommand.StartsWith("off"))
				{
					addCommand = addCommand.Remove(0, 4);

					if (addCommand.IndexOf(" ", StringComparison.Ordinal) == -1) { return "`Command not recognised.`"; }

					term = new Regex(addCommand.Remove(0, addCommand.IndexOf(" ", StringComparison.Ordinal) + 1));
					site = addCommand.Substring(0, addCommand.IndexOf(" ", StringComparison.Ordinal));

					if (GlobalInfo.QBWhOff.Terms.ContainsKey(site) && GlobalInfo.QBWhOff.Terms[site].ContainsTerm(term)) { return "`Whitelist term already exists.`"; }

					GlobalInfo.QBWhOff.AddTerm(term, site);
				}

				if (addCommand.StartsWith("spam"))
				{
					addCommand = addCommand.Remove(0, 5);

					if (addCommand.IndexOf(" ", StringComparison.Ordinal) == -1) { return "`Command not recognised.`"; }

					term = new Regex(addCommand.Remove(0, addCommand.IndexOf(" ", StringComparison.Ordinal) + 1));
					site = addCommand.Substring(0, addCommand.IndexOf(" ", StringComparison.Ordinal));

					if (GlobalInfo.QBWSpam.Terms.ContainsKey(site) && GlobalInfo.QBWSpam.Terms[site].ContainsTerm(term)) { return "`Whitelist term already exists.`"; }

					GlobalInfo.QBWSpam.AddTerm(term, site);
				}

				if (addCommand.StartsWith("lq"))
				{
					addCommand = addCommand.Remove(0, 3);

					if (addCommand.IndexOf(" ", StringComparison.Ordinal) == -1) { return "`Command not recognised.`"; }

					term = new Regex(addCommand.Remove(0, addCommand.IndexOf(" ", StringComparison.Ordinal) + 1));
					site = addCommand.Substring(0, addCommand.IndexOf(" ", StringComparison.Ordinal));

					if (GlobalInfo.QBWLQ.Terms.ContainsKey(site) && GlobalInfo.QBWLQ.Terms[site].ContainsTerm(term)) { return "`Whitelist term already exists.`"; }

					GlobalInfo.QBWSpam.AddTerm(term, site);
				}

				return "`Whitelist term added.`";
			}

			return "`Command not recognised.`";
		}

		private static string RemoveWQBTerm(string command)
		{
			var removeCommand = command.Remove(0, 9);

			if (removeCommand.StartsWith("off") || removeCommand.StartsWith("spam") || removeCommand.StartsWith("lq"))
			{
				Regex term;
				string site;

				if (removeCommand.StartsWith("off"))
				{
					removeCommand = removeCommand.Remove(0, 4);

					if (removeCommand.IndexOf(" ", StringComparison.Ordinal) == -1) { return "`Command not recognised.`"; }

					term = new Regex(removeCommand.Remove(0, removeCommand.IndexOf(" ", StringComparison.Ordinal) + 1));
					site = removeCommand.Substring(0, removeCommand.IndexOf(" ", StringComparison.Ordinal));

					if (!GlobalInfo.QBWhOff.Terms.ContainsKey(site) && !GlobalInfo.QBWhOff.Terms[site].ContainsTerm(term)) { return "`Whitelist term does not exist.`"; }

					GlobalInfo.QBWhOff.RemoveTerm(term, site);
				}

				if (removeCommand.StartsWith("spam"))
				{
					removeCommand = removeCommand.Remove(0, 5);

					if (removeCommand.IndexOf(" ", StringComparison.Ordinal) == -1) { return "`Command not recognised.`"; }

					term = new Regex(removeCommand.Remove(0, removeCommand.IndexOf(" ", StringComparison.Ordinal) + 1));
					site = removeCommand.Substring(0, removeCommand.IndexOf(" ", StringComparison.Ordinal));

					if (!GlobalInfo.QBWSpam.Terms.ContainsKey(site) && !GlobalInfo.QBWSpam.Terms[site].ContainsTerm(term)) { return "`Whitelist term does not exist.`"; }

					GlobalInfo.QBWSpam.RemoveTerm(term, site);
				}

				if (removeCommand.StartsWith("lq"))
				{
					removeCommand = removeCommand.Remove(0, 3);

					if (removeCommand.IndexOf(" ", StringComparison.Ordinal) == -1) { return "`Command not recognised.`"; }

					term = new Regex(removeCommand.Remove(0, removeCommand.IndexOf(" ", StringComparison.Ordinal) + 1));
					site = removeCommand.Substring(0, removeCommand.IndexOf(" ", StringComparison.Ordinal));

					if (!GlobalInfo.QBWLQ.Terms.ContainsKey(site) && !GlobalInfo.QBWLQ.Terms[site].ContainsTerm(term)) { return "`Whitelist term does not exist.`"; }

					GlobalInfo.QBWLQ.RemoveTerm(term, site);
				}

				return "`Whitelist term removed.`";
			}

			return "`Command not recognised.`";
		}


		private static string AddBATerm(string command)
		{
			var addCommand = command.Remove(0, 8);

			if (addCommand.StartsWith("off") || addCommand.StartsWith("spam") || addCommand.StartsWith("lq") || addCommand.StartsWith("name"))
			{
				Regex term;

				if (addCommand.StartsWith("off"))
				{
					term = new Regex(addCommand.Remove(0, 4));

					if (GlobalInfo.ABOff.Terms.ContainsTerm(term)) { return "`Blacklist term already exists.`"; }

					GlobalInfo.ABOff.AddTerm(term);
				}

				if (addCommand.StartsWith("spam"))
				{
					term = new Regex(addCommand.Remove(0, 5));

					if (GlobalInfo.ABSpam.Terms.ContainsTerm(term)) { return "`Blacklist term already exists.`"; }

					GlobalInfo.ABSpam.AddTerm(term);
				}

				if (addCommand.StartsWith("lq"))
				{
					term = new Regex(addCommand.Remove(0, 3));

					if (GlobalInfo.ABLQ.Terms.ContainsTerm(term)) { return "`Blacklist term already exists.`"; }

					GlobalInfo.ABLQ.AddTerm(term);
				}

				if (addCommand.StartsWith("name"))
				{
					term = new Regex(addCommand.Remove(0, 5));

					if (GlobalInfo.ABName.Terms.ContainsTerm(term)) { return "`Blacklist term already exists.`"; }

					GlobalInfo.ABName.AddTerm(term);
				}

				return "`Blacklist term added.`";
			}

			return "`Command not recognised.`";
		}

		private static string RemoveBATerm(string command)
		{
			var removeCommand = command.Remove(0, 8);

			if (removeCommand.StartsWith("off") || removeCommand.StartsWith("spam") || removeCommand.StartsWith("lq") || removeCommand.StartsWith("name"))
			{
				Regex term;

				if (removeCommand.StartsWith("off"))
				{
					term = new Regex(removeCommand.Remove(0, 4));

					if (!GlobalInfo.ABOff.Terms.ContainsTerm(term)) { return "`Blacklist term does not exist.`"; }

					GlobalInfo.ABOff.RemoveTerm(term);
				}

				if (removeCommand.StartsWith("spam"))
				{
					term = new Regex(removeCommand.Remove(0, 5));

					if (!GlobalInfo.ABSpam.Terms.ContainsTerm(term)) { return "`Blacklist term does not exist.`"; }

					GlobalInfo.ABSpam.RemoveTerm(term);
				}

				if (removeCommand.StartsWith("lq"))
				{
					term = new Regex(removeCommand.Remove(0, 3));

					if (!GlobalInfo.ABLQ.Terms.ContainsTerm(term)) { return "`Blacklist term does not exist.`"; }

					GlobalInfo.ABLQ.RemoveTerm(term);
				}

				if (removeCommand.StartsWith("name"))
				{
					term = new Regex(removeCommand.Remove(0, 5));

					if (!GlobalInfo.ABName.Terms.ContainsTerm(term)) { return "`Blacklist term does not exist.`"; }

					GlobalInfo.ABName.RemoveTerm(term);
				}

				return "`Blacklist term removed.`";
			}

			return "`Command not recognised.`";
		}


		private static string AddWATerm(string command)
		{
			var addCommand = command.Substring(0, 8);

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

					if (GlobalInfo.AWOff.Terms.ContainsKey(site) && GlobalInfo.AWOff.Terms[site].ContainsTerm(term)) { return "`Whitelist term already exists.`"; }

					GlobalInfo.AWOff.AddTerm(term, site);
				}

				if (addCommand.StartsWith("spam"))
				{
					addCommand = addCommand.Remove(0, 5);

					if (addCommand.IndexOf(" ", StringComparison.Ordinal) == -1) { return "`Command not recognised.`"; }

					term = new Regex(addCommand.Remove(0, addCommand.IndexOf(" ", StringComparison.Ordinal) + 1));
					site = addCommand.Substring(0, addCommand.IndexOf(" ", StringComparison.Ordinal));

					if (GlobalInfo.AWSpam.Terms.ContainsKey(site) && GlobalInfo.AWSpam.Terms[site].ContainsTerm(term)) { return "`Whitelist term already exists.`"; }

					GlobalInfo.AWSpam.AddTerm(term, site);
				}

				if (addCommand.StartsWith("lq"))
				{
					addCommand = addCommand.Remove(0, 3);

					if (addCommand.IndexOf(" ", StringComparison.Ordinal) == -1) { return "`Command not recognised.`"; }

					term = new Regex(addCommand.Remove(0, addCommand.IndexOf(" ", StringComparison.Ordinal) + 1));
					site = addCommand.Substring(0, addCommand.IndexOf(" ", StringComparison.Ordinal));

					if (GlobalInfo.QTWLQ.Terms.ContainsKey(site) && GlobalInfo.QTWLQ.Terms[site].ContainsTerm(term)) { return "`Whitelist term already exists.`"; }

					GlobalInfo.QTWSpam.AddTerm(term, site);
				}

				if (addCommand.StartsWith("name"))
				{
					addCommand = addCommand.Remove(0, 5);

					if (addCommand.IndexOf(" ", StringComparison.Ordinal) == -1) { return "`Command not recognised.`"; }

					term = new Regex(addCommand.Remove(0, addCommand.IndexOf(" ", StringComparison.Ordinal) + 1));
					site = addCommand.Substring(0, addCommand.IndexOf(" ", StringComparison.Ordinal));

					if (GlobalInfo.QTWName.Terms.ContainsKey(site) && GlobalInfo.QTWName.Terms[site].ContainsTerm(term)) { return "`Whitelist term already exists.`"; }

					GlobalInfo.QTWName.AddTerm(term, site);
				}

				return "`Whitelist term added.`";
			}

			return "`Command not recognised.`";
		}

		private static string RemoveWATerm(string command)
		{
			var removeCommand = command.Remove(0, 8);

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

					if (!GlobalInfo.AWOff.Terms.ContainsKey(site) && !GlobalInfo.AWOff.Terms[site].ContainsTerm(term)) { return "`Whitelist term does not exist.`"; }

					GlobalInfo.AWOff.RemoveTerm(term, site);
				}

				if (removeCommand.StartsWith("spam"))
				{
					removeCommand = removeCommand.Remove(0, 5);

					if (removeCommand.IndexOf(" ", StringComparison.Ordinal) == -1) { return "`Command not recognised.`"; }

					term = new Regex(removeCommand.Remove(0, removeCommand.IndexOf(" ", StringComparison.Ordinal) + 1));
					site = removeCommand.Substring(0, removeCommand.IndexOf(" ", StringComparison.Ordinal));

					if (!GlobalInfo.AWSpam.Terms.ContainsKey(site) && !GlobalInfo.AWSpam.Terms[site].ContainsTerm(term)) { return "`Whitelist term does not exist.`"; }

					GlobalInfo.AWSpam.RemoveTerm(term, site);
				}

				if (removeCommand.StartsWith("lq"))
				{
					removeCommand = removeCommand.Remove(0, 3);

					if (removeCommand.IndexOf(" ", StringComparison.Ordinal) == -1) { return "`Command not recognised.`"; }

					term = new Regex(removeCommand.Remove(0, removeCommand.IndexOf(" ", StringComparison.Ordinal) + 1));
					site = removeCommand.Substring(0, removeCommand.IndexOf(" ", StringComparison.Ordinal));

					if (!GlobalInfo.AWLQ.Terms.ContainsKey(site) && !GlobalInfo.AWLQ.Terms[site].ContainsTerm(term)) { return "`Whitelist term does not exist.`"; }

					GlobalInfo.AWLQ.RemoveTerm(term, site);
				}

				if (removeCommand.StartsWith("name"))
				{
					removeCommand = removeCommand.Remove(0, 5);

					if (removeCommand.IndexOf(" ", StringComparison.Ordinal) == -1) { return "`Command not recognised.`"; }

					term = new Regex(removeCommand.Remove(0, removeCommand.IndexOf(" ", StringComparison.Ordinal) + 1));
					site = removeCommand.Substring(0, removeCommand.IndexOf(" ", StringComparison.Ordinal));

					if (!GlobalInfo.AWName.Terms.ContainsKey(site) && !GlobalInfo.AWName.Terms[site].ContainsTerm(term)) { return "`Whitelist term does not exist.`"; }

					GlobalInfo.AWName.RemoveTerm(term, site);
				}

				return "`Whitelist term removed.`";
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
				var url = GlobalInfo.PostedReports[reportID].Post.URL;

				MessageHandler.DeleteMessage(url, reportID, false);
			}

			return "";
		}


		private static string FalsePositive()
		{
			if (message.Report.Type == PostType.BadTagUsed)
			{
				return "";
			}

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
						if (!GlobalInfo.QTWLQ.Terms.ContainsKey(message.Post.Site) || !GlobalInfo.QTWLQ.Terms[message.Post.Site].ContainsTerm(term.Key))
						{
							GlobalInfo.QTWLQ.AddTerm(term.Key, message.Post.Site, message.Report.BlackTermsFound.Values.Max() / 2);
						}
					}

					foreach (var term in message.Report.WhiteTermsFound)
					{
						GlobalInfo.QTWLQ.SetScore(term.Key, message.Post.Site, term.Value + 1);
					}

					return MessageHandler.DeleteMessage(message.Post.URL, message.RepliesToMessageID) ? "" : ":" + message.MessageID + " `FP acknowledged.`";
				}

				case PostType.Offensive:
				{
					foreach (var term in message.Report.BlackTermsFound)
					{
						if (!GlobalInfo.QTWhOff.Terms.ContainsKey(message.Post.Site) || !GlobalInfo.QTWhOff.Terms[message.Post.Site].ContainsTerm(term.Key))
						{
							GlobalInfo.QTWhOff.AddTerm(term.Key, message.Post.Site, message.Report.BlackTermsFound.Values.Max() / 2);
						}
					}

					foreach (var term in message.Report.WhiteTermsFound)
					{
						GlobalInfo.QTWhOff.SetScore(term.Key, message.Post.Site, term.Value + 1);
					}

					return MessageHandler.DeleteMessage(message.Post.URL, message.RepliesToMessageID) ? "" : ":" + message.MessageID + " `FP acknowledged.`";
				}

				case PostType.Spam:
				{
					foreach (var term in message.Report.BlackTermsFound)
					{
						if (!GlobalInfo.QTWSpam.Terms.ContainsKey(message.Post.Site) || !GlobalInfo.QTWSpam.Terms[message.Post.Site].ContainsTerm(term.Key))
						{
							GlobalInfo.QTWSpam.AddTerm(term.Key, message.Post.Site, message.Report.BlackTermsFound.Values.Max() / 2);
						}
					}

					foreach (var term in message.Report.WhiteTermsFound)
					{
						GlobalInfo.QTWSpam.SetScore(term.Key, message.Post.Site, term.Value + 1);
					}

					return MessageHandler.DeleteMessage(message.Post.URL, message.RepliesToMessageID) ? "" : ":" + message.MessageID + " `FP acknowledged.`";
				}

				case PostType.BadUsername:
				{
					foreach (var term in message.Report.BlackTermsFound)
					{
						if (!GlobalInfo.QTWName.Terms.ContainsKey(message.Post.Site) || !GlobalInfo.QTWName.Terms[message.Post.Site].ContainsTerm(term.Key))
						{
							GlobalInfo.QTWName.AddTerm(term.Key, message.Post.Site, message.Report.BlackTermsFound.Values.Max() / 2);
						}
					}

					foreach (var term in message.Report.WhiteTermsFound)
					{
						GlobalInfo.QTWName.SetScore(term.Key, message.Post.Site, term.Value + 1);
					}

					return MessageHandler.DeleteMessage(message.Post.URL, message.RepliesToMessageID) ? "" : ":" + message.MessageID + " `FP acknowledged.`";
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
						if (!GlobalInfo.AWLQ.Terms.ContainsKey(message.Post.Site) || !GlobalInfo.AWLQ.Terms[message.Post.Site].ContainsTerm(term.Key))
						{
							GlobalInfo.AWLQ.AddTerm(term.Key, message.Post.Site, message.Report.BlackTermsFound.Values.Max() / 2);
						}
					}

					foreach (var term in message.Report.WhiteTermsFound)
					{
						GlobalInfo.AWLQ.SetScore(term.Key, message.Post.Site, term.Value + 1);
					}

					return MessageHandler.DeleteMessage(message.Post.URL, message.RepliesToMessageID) ? "" : ":" + message.MessageID + " `FP acknowledged.`";
				}

				case PostType.Offensive:
				{
					foreach (var term in message.Report.BlackTermsFound)
					{
						if (!GlobalInfo.AWOff.Terms.ContainsKey(message.Post.Site) || !GlobalInfo.AWOff.Terms[message.Post.Site].ContainsTerm(term.Key))
						{
							GlobalInfo.AWOff.AddTerm(term.Key, message.Post.Site, message.Report.BlackTermsFound.Values.Max() / 2);
						}
					}

					foreach (var term in message.Report.WhiteTermsFound)
					{
						GlobalInfo.AWOff.SetScore(term.Key, message.Post.Site, term.Value + 1);
					}

					return MessageHandler.DeleteMessage(message.Post.URL, message.RepliesToMessageID) ? "" : ":" + message.MessageID + " `FP acknowledged.`";
				}

				case PostType.Spam:
				{
					foreach (var term in message.Report.BlackTermsFound)
					{
						if (!GlobalInfo.AWSpam.Terms.ContainsKey(message.Post.Site) || !GlobalInfo.AWSpam.Terms[message.Post.Site].ContainsTerm(term.Key))
						{
							GlobalInfo.AWSpam.AddTerm(term.Key, message.Post.Site, message.Report.BlackTermsFound.Values.Max() / 2);
						}
					}

					foreach (var term in message.Report.WhiteTermsFound)
					{
						GlobalInfo.AWSpam.SetScore(term.Key, message.Post.Site, term.Value + 1);
					}

					return MessageHandler.DeleteMessage(message.Post.URL, message.RepliesToMessageID) ? "" : ":" + message.MessageID + " `FP acknowledged.`";
				}

				case PostType.BadUsername:
				{
					foreach (var term in message.Report.BlackTermsFound)
					{
						if (!GlobalInfo.AWName.Terms.ContainsKey(message.Post.Site) || !GlobalInfo.AWName.Terms[message.Post.Site].ContainsTerm(term.Key))
						{
							GlobalInfo.AWName.AddTerm(term.Key, message.Post.Site, message.Report.BlackTermsFound.Values.Max() / 2);
						}
					}

					foreach (var term in message.Report.WhiteTermsFound)
					{
						GlobalInfo.AWName.SetScore(term.Key, message.Post.Site, term.Value + 1);
					}

					return MessageHandler.DeleteMessage(message.Post.URL, message.RepliesToMessageID) ? "" : ":" + message.MessageID + " `FP acknowledged.`";
				}
			}

			return "`Command not recognised.`";
		}


		private static string TruePositive()
		{
			if (commandLower == "tpa")
			{
				var reportMessage = GlobalInfo.PostedReports[message.RepliesToMessageID];

				GlobalInfo.MessagePoster.MessageQueue.Add(reportMessage, GlobalInfo.AnnouncerRoomID);
			}

			if (message.Report.Type == PostType.BadTagUsed)
			{
				return "";
			}
			
			return message.IsQuestionReport ? TruePositiveQuestion() : TruePositiveAnswer();
		}

		private static string TruePositiveQuestion()
		{
			switch (message.Report.Type)
			{
				case PostType.LowQuality:
				{
					foreach (var blackTerm in message.Report.BlackTermsFound)
					{
						GlobalInfo.QTBLQ.SetScore(blackTerm.Key, blackTerm.Value + 1);

						foreach (var site in GlobalInfo.QTWLQ.Terms)
						{
							for (var i = 0; i < site.Value.Count; i++)
							{
								var whiteTerm = site.Value.ElementAt(i);

								if (whiteTerm.Key.ToString() == blackTerm.Key.ToString() && site.Key != message.Post.Site)
								{
									var oldWhiteScore = GlobalInfo.QTWLQ.GetScore(whiteTerm.Key, site.Key);
									var x = oldWhiteScore / blackTerm.Value;

									GlobalInfo.QTWLQ.SetScore(whiteTerm.Key, site.Key, x * (blackTerm.Value + 1));
								}
							}
						}
					}		

					return ":" + message.MessageID + " `TP acknowledged.`";
				}

				case PostType.Offensive:
				{
					foreach (var blackTerm in message.Report.BlackTermsFound)
					{
						GlobalInfo.QTBOff.SetScore(blackTerm.Key, blackTerm.Value + 1);

						foreach (var site in GlobalInfo.QTWhOff.Terms)
						{
							for (var i = 0; i < site.Value.Count; i++)
							{
								var whiteTerm = site.Value.ElementAt(i);

								if (whiteTerm.Key.ToString() == blackTerm.Key.ToString() && site.Key != message.Post.Site)
								{
									var oldWhiteScore = GlobalInfo.QTWhOff.GetScore(whiteTerm.Key, site.Key);
									var x = oldWhiteScore / blackTerm.Value;

									GlobalInfo.QTWhOff.SetScore(whiteTerm.Key, site.Key, x * (blackTerm.Value + 1));
								}
							}
						}
					}

					return ":" + message.MessageID + " `TP acknowledged.`";
				}

				case PostType.Spam:
				{
					foreach (var blackTerm in message.Report.BlackTermsFound)
					{
						GlobalInfo.QTBSpam.SetScore(blackTerm.Key, blackTerm.Value + 1);

						foreach (var site in GlobalInfo.QTWSpam.Terms)
						{
							for (var i = 0; i < site.Value.Count; i++)
							{
								var whiteTerm = site.Value.ElementAt(i);

								if (whiteTerm.Key.ToString() == blackTerm.Key.ToString() && site.Key != message.Post.Site)
								{
									var oldWhiteScore = GlobalInfo.QTWSpam.GetScore(whiteTerm.Key, site.Key);
									var x = oldWhiteScore / blackTerm.Value;

									GlobalInfo.QTWSpam.SetScore(whiteTerm.Key, site.Key, x * (blackTerm.Value + 1));
								}
							}
						}
					}

					return ":" + message.MessageID + " `TP acknowledged.`";
				}

				case PostType.BadUsername:
				{
					foreach (var blackTerm in message.Report.BlackTermsFound)
					{
						GlobalInfo.QTBName.SetScore(blackTerm.Key, blackTerm.Value + 1);

						foreach (var site in GlobalInfo.QTWName.Terms)
						{
							for (var i = 0; i < site.Value.Count; i++)
							{
								var whiteTerm = site.Value.ElementAt(i);

								if (whiteTerm.Key.ToString() == blackTerm.Key.ToString() && site.Key != message.Post.Site)
								{
									var oldWhiteScore = GlobalInfo.QTWName.GetScore(whiteTerm.Key, site.Key);
									var x = oldWhiteScore / blackTerm.Value;

									GlobalInfo.QTWName.SetScore(whiteTerm.Key, site.Key, x * (blackTerm.Value + 1));
								}
							}
						}
					}

					return ":" + message.MessageID + " `TP acknowledged.`";
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
						GlobalInfo.ABLQ.SetScore(blackTerm.Key, blackTerm.Value + 1);

						foreach (var site in GlobalInfo.AWLQ.Terms)
						{
							for (var i = 0; i < site.Value.Count; i++)
							{
								var whiteTerm = site.Value.ElementAt(i);

								if (whiteTerm.Key.ToString() == blackTerm.Key.ToString() && site.Key != message.Post.Site)
								{
									var oldWhiteScore = GlobalInfo.AWLQ.GetScore(whiteTerm.Key, site.Key);
									var x = oldWhiteScore / blackTerm.Value;

									GlobalInfo.AWLQ.SetScore(whiteTerm.Key, site.Key, x * (blackTerm.Value + 1));
								}
							}
						}
					}

					return ":" + message.MessageID + " `TP acknowledged.`";
				}

				case PostType.Offensive:
				{
					foreach (var blackTerm in message.Report.BlackTermsFound)
					{
						GlobalInfo.ABOff.SetScore(blackTerm.Key, blackTerm.Value + 1);

						foreach (var site in GlobalInfo.AWOff.Terms)
						{
							for (var i = 0; i < site.Value.Count; i++)
							{
								var whiteTerm = site.Value.ElementAt(i);

								if (whiteTerm.Key.ToString() == blackTerm.Key.ToString() && site.Key != message.Post.Site)
								{
									var oldWhiteScore = GlobalInfo.AWOff.GetScore(whiteTerm.Key, site.Key);
									var x = oldWhiteScore / blackTerm.Value;

									GlobalInfo.AWOff.SetScore(whiteTerm.Key, site.Key, x * (blackTerm.Value + 1));
								}
							}
						}
					}

					return ":" + message.MessageID + " `TP acknowledged.`";
				}

				case PostType.Spam:
				{
					foreach (var blackTerm in message.Report.BlackTermsFound)
					{
						GlobalInfo.ABSpam.SetScore(blackTerm.Key, blackTerm.Value + 1);

						foreach (var site in GlobalInfo.AWSpam.Terms)
						{
							for (var i = 0; i < site.Value.Count; i++)
							{
								var whiteTerm = site.Value.ElementAt(i);

								if (whiteTerm.Key.ToString() == blackTerm.Key.ToString() && site.Key != message.Post.Site)
								{
									var oldWhiteScore = GlobalInfo.AWSpam.GetScore(whiteTerm.Key, site.Key);
									var x = oldWhiteScore / blackTerm.Value;

									GlobalInfo.AWSpam.SetScore(whiteTerm.Key, site.Key, x * (blackTerm.Value + 1));
								}
							}
						}
					}

					return ":" + message.MessageID + " `TP acknowledged.`";
				}

				case PostType.BadUsername:
				{
					foreach (var blackTerm in message.Report.BlackTermsFound)
					{
						GlobalInfo.ABName.SetScore(blackTerm.Key, blackTerm.Value + 1);

						foreach (var site in GlobalInfo.AWName.Terms)
						{
							for (var i = 0; i < site.Value.Count; i++)
							{
								var whiteTerm = site.Value.ElementAt(i);

								if (whiteTerm.Key.ToString() == blackTerm.Key.ToString() && site.Key != message.Post.Site)
								{
									var oldWhiteScore = GlobalInfo.AWName.GetScore(whiteTerm.Key, site.Key);
									var x = oldWhiteScore / blackTerm.Value;

									GlobalInfo.AWName.SetScore(whiteTerm.Key, site.Key, x * (blackTerm.Value + 1));
								}
							}
						}
					}

					return ":" + message.MessageID + " `TP acknowledged.`";
				}
			}

			return "`Command not recognised.`";
		}



		// Owner commands.



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


		private static string SetAccuracyThreshold(string command)
		{
			if (command.IndexOf(" ", StringComparison.Ordinal) == -1 || command.All(c => !Char.IsDigit(c))) { return "`Command not recognised.`"; }

			var newLimit = command.Remove(0, 10);

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
