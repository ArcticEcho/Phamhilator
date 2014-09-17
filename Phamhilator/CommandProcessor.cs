using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Phamhilator.Filters;



namespace Phamhilator
{
	public static class CommandProcessor
	{
		public static string ExacuteCommand(KeyValuePair<string, string> input)
		{
			string command;

			if (input.Value.StartsWith(">>"))
			{
				command = input.Value.Remove(0, 2).TrimStart();
			}
			//else if (input.Value.ToLowerInvariant().StartsWith("@sam")) // TODO: Check if message is reply to a bot message. May need to create a new type to hold chat message data.
			//{
			//	command = input.Value.Remove(0, 4).TrimStart();
			//}
			else
			{
				return "";
			}

			var user = input.Key;

			if (IsNormalUserCommand(command))
			{
				return NormalUserCommands(command);
			}
			
			if (user != "Sam" && user != "Unihedron" && user != "ProgramFOX" && user != "Jan Dvorak" && user != "rene") { return ""; }

			if (IsPrivilegedUserCommand(command))
			{
				return PrivilegedUserCommands(command);			
			}

			return "`Command not recognised.`";
		}



		private static bool IsPrivilegedUserCommand(string command)
		{
			return command.StartsWith("-1") || command.StartsWith("dv") || command.StartsWith("downvote") ||
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
				return "`Owners: " + Stats.Owners + ". Users with command access: Jan Dvorak & rene. Total terms: " + Stats.TermCount + ". Caught posts over last 2 days: " + Stats.PostsCaught + ". Uptime: " + (DateTime.UtcNow - Stats.UpTime) + ".`";
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

					if (!Offensive.Terms.ContainsTerm(term))
					{
						return "`Term does not exist.`";
					}

					score = Offensive.GetScore(term) - 1;

					Offensive.SetScore(term, score);
				}

				if (dvCommand.StartsWith("spam"))
				{
					term = new Regex(dvCommand.Remove(0, 5));

					if (!Spam.Terms.ContainsTerm(term))
					{
						return "`Term does not exist.`";
					}

					score = Spam.GetScore(term) - 1;

					Spam.SetScore(term, score);
				}

				if (dvCommand.StartsWith("lq"))
				{
					term = new Regex(dvCommand.Remove(0, 3));

					if (!LQ.Terms.ContainsTerm(term))
					{
						return "`Term does not exist.`";
					}

					score = LQ.GetScore(term) - 1;

					LQ.SetScore(term, score);
				}

				if (dvCommand.StartsWith("name"))
				{
					term = new Regex(dvCommand.Remove(0, 5));

					if (!BadUsername.Terms.ContainsTerm(term))
					{
						return "`Term does not exist.`";
					}

					score = BadUsername.GetScore(term) - 1;

					BadUsername.SetScore(term, score);
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

					if (!Offensive.Terms.ContainsTerm(term))
					{
						return "`Term does not exist.`";
					}

					score = Offensive.GetScore(term) + 1;

					Offensive.SetScore(term, score);
				}

				if (uvCommand.StartsWith("spam"))
				{
					term = new Regex(uvCommand.Remove(0, 5));

					if (!Spam.Terms.ContainsTerm(term))
					{
						return "`Term does not exist.`";
					}

					score = Spam.GetScore(term) + 1;

					Spam.SetScore(term, score);
				}

				if (uvCommand.StartsWith("lq"))
				{
					term = new Regex(uvCommand.Remove(0, 3));

					if (!LQ.Terms.ContainsTerm(term))
					{
						return "`Term does not exist.`";
					}

					score = LQ.GetScore(term) + 1;

					LQ.SetScore(term, score);
				}

				if (uvCommand.StartsWith("name"))
				{
					term = new Regex(uvCommand.Remove(0, 5));

					if (!BadUsername.Terms.ContainsTerm(term))
					{
						return "`Term does not exist.`";
					}

					score = BadUsername.GetScore(term) + 1;

					BadUsername.SetScore(term, score);
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

					if (Offensive.Terms.ContainsTerm(term)) { return "`Term already exists.`"; }

					Offensive.AddTerm(term);
				}

				if (addCommand.StartsWith("spam"))
				{
					term = new Regex(addCommand.Remove(0, 5));

					if (Spam.Terms.ContainsTerm(term)) { return "`Term already exists.`"; }

					Spam.AddTerm(term);
				}

				if (addCommand.StartsWith("lq"))
				{
					term = new Regex(addCommand.Remove(0, 3));

					if (LQ.Terms.ContainsTerm(term)) { return "`Term already exists.`"; }

					LQ.AddTerm(term);
				}

				if (addCommand.StartsWith("name"))
				{
					term = new Regex(addCommand.Remove(0, 5));

					if (BadUsername.Terms.ContainsTerm(term)) { return "`Term already exists.`"; }

					BadUsername.AddTerm(PostType.BadUsername, term);
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

					if (!Offensive.Terms.ContainsTerm(term)) { return "`Term does not exist.`"; }

					Offensive.RemoveTerm(term);
				}

				if (removeCommand.StartsWith("spam"))
				{
					term = new Regex(removeCommand.Remove(0, 5));

					if (!Spam.Terms.ContainsTerm(term)) { return "`Term does not exist.`"; }

					Spam.RemoveTerm(term);
				}

				if (removeCommand.StartsWith("lq"))
				{
					term = new Regex(removeCommand.Remove(0, 3));

					if (!LQ.Terms.ContainsTerm(term)) { return "`Term does not exist.`"; }

					LQ.RemoveTerm( term);
				}

				if (removeCommand.StartsWith("name"))
				{
					term = new Regex(removeCommand.Remove(0, 5));

					if (!BadUsername.Terms.ContainsTerm(term)) { return "`Term does not exist.`"; }

					BadUsername.RemoveTerm(term);
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

					if (IgnoreFilterTerms.OffensiveTerms.ContainsTerm(term)) { return "`Ignore term already exists.`"; }

					IgnoreFilterTerms.AddTerm(PostType.Offensive, term, site);
				}

				if (addCommand.StartsWith("spam"))
				{
					addCommand = addCommand.Remove(0, 5);

					if (addCommand.IndexOf(" ", StringComparison.Ordinal) == -1) { return "`Command not recognised.`"; }

					term = new Regex(addCommand.Remove(0, addCommand.IndexOf(" ", StringComparison.Ordinal) + 1));
					site = addCommand.Substring(0, addCommand.IndexOf(" ", StringComparison.Ordinal));

					if (IgnoreFilterTerms.SpamTerms.ContainsTerm(term)) { return "`Ignore term already exists.`"; }

					IgnoreFilterTerms.AddTerm(PostType.Spam, term, site);
				}

				if (addCommand.StartsWith("lq"))
				{
					addCommand = addCommand.Remove(0, 3);

					if (addCommand.IndexOf(" ", StringComparison.Ordinal) == -1) { return "`Command not recognised.`"; }

					term = new Regex(addCommand.Remove(0, addCommand.IndexOf(" ", StringComparison.Ordinal) + 1));
					site = addCommand.Substring(0, addCommand.IndexOf(" ", StringComparison.Ordinal));

					if (IgnoreFilterTerms.LQTerms.ContainsTerm(term)) { return "`Ignore term already exists.`"; }
					
					IgnoreFilterTerms.AddTerm(PostType.LowQuality, term, site);
				}

				if (addCommand.StartsWith("name"))
				{
					addCommand = addCommand.Remove(0, 5);

					if (addCommand.IndexOf(" ", StringComparison.Ordinal) == -1) { return "`Command not recognised.`"; }

					term = new Regex(addCommand.Remove(0, addCommand.IndexOf(" ", StringComparison.Ordinal) + 1));
					site = addCommand.Substring(0, addCommand.IndexOf(" ", StringComparison.Ordinal));

					if (IgnoreFilterTerms.BadUsernameTerms.ContainsTerm(term)) { return "`Ignore term already exists.`"; }

					IgnoreFilterTerms.AddTerm(PostType.BadUsername, term, site);
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

					if (!IgnoreFilterTerms.OffensiveTerms.ContainsTerm(term)) { return "`Ignore term does not exist.`"; }

					IgnoreFilterTerms.RemoveTerm(PostType.Offensive, term, site);
				}

				if (removeCommand.StartsWith("spam"))
				{
					removeCommand = removeCommand.Remove(0, 5);

					if (removeCommand.IndexOf(" ", StringComparison.Ordinal) == -1) { return "`Command not recognised.`"; }

					term = new Regex(removeCommand.Remove(0, removeCommand.IndexOf(" ", StringComparison.Ordinal) + 1));
					site = removeCommand.Substring(0, removeCommand.IndexOf(" ", StringComparison.Ordinal));

					if (!IgnoreFilterTerms.SpamTerms.ContainsTerm(term)) { return "`Ignore term does not exist.`"; }

					IgnoreFilterTerms.RemoveTerm(PostType.Spam, term, site);
				}

				if (removeCommand.StartsWith("lq"))
				{
					removeCommand = removeCommand.Remove(0, 3);

					if (removeCommand.IndexOf(" ", StringComparison.Ordinal) == -1) { return "`Command not recognised.`"; }

					term = new Regex(removeCommand.Remove(0, removeCommand.IndexOf(" ", StringComparison.Ordinal) + 1));
					site = removeCommand.Substring(0, removeCommand.IndexOf(" ", StringComparison.Ordinal));

					if (!IgnoreFilterTerms.LQTerms.ContainsTerm(term)) { return "`Ignore term does not exist.`"; }

					IgnoreFilterTerms.RemoveTerm(PostType.LowQuality, term, site);
				}

				if (removeCommand.StartsWith("name"))
				{
					removeCommand = removeCommand.Remove(0, 5);

					if (removeCommand.IndexOf(" ", StringComparison.Ordinal) == -1) { return "`Command not recognised.`"; }

					term = new Regex(removeCommand.Remove(0, removeCommand.IndexOf(" ", StringComparison.Ordinal) + 1));
					site = removeCommand.Substring(0, removeCommand.IndexOf(" ", StringComparison.Ordinal));

					if (!IgnoreFilterTerms.BadUsernameTerms.ContainsTerm(term)) { return "`Ignore term does not exist.`"; }

					IgnoreFilterTerms.RemoveTerm(PostType.BadUsername, term, site);
				}

				return "`Ignore Term removed.`";
			}

			return "`Command not recognised.`";
		}


		private static string StartBot()
		{
			Stats.BotRunning = true;

			return "Phamhilator™ started.";
		}

		private static string PauseBot()
		{
			Stats.BotRunning = false;

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
	}
}
