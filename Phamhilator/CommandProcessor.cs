using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;


  
namespace Phamhilator
{
	public static class CommandProcessor
	{
		public static string ExacuteCommand(KeyValuePair<string, string> input)
		{
			string command;

			if (input.Value.StartsWith("&gt;&gt;"))
			{
				command = input.Value.Remove(0, 8).TrimStart();
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
			var dvCommand = command.Substring(command.IndexOf(" ") + 1);

			if (dvCommand.StartsWith("") || dvCommand.StartsWith("") || dvCommand.StartsWith("") || dvCommand.StartsWith(""))
			{
				var score = 0;
				Regex term;

				if (dvCommand.StartsWith("off"))
				{
					term = new Regex(dvCommand.Remove(0, 4));

					if (!FilterTerms.OffensiveTerms.ContainsTerm(term))
					{
						return "`Term does not exist.`";
					}

					score = FilterTerms.GetTermScore(PostType.Offensive, term) - 1;

					FilterTerms.SetTermScore(PostType.Offensive, term, score);
				}

				if (dvCommand.StartsWith("spam"))
				{
					term = new Regex(dvCommand.Remove(0, 5));

					if (!FilterTerms.SpamTerms.ContainsTerm(term))
					{
						return "`Term does not exist.`";
					}

					score = FilterTerms.GetTermScore(PostType.Spam, term) - 1;

					FilterTerms.SetTermScore(PostType.Spam, term, score);
				}

				if (dvCommand.StartsWith("lq"))
				{
					term = new Regex(dvCommand.Remove(0, 3));

					if (!FilterTerms.LQTerms.ContainsTerm(term))
					{
						return "`Term does not exist.`";
					}

					score = FilterTerms.GetTermScore(PostType.LowQuality, term) - 1;

					FilterTerms.SetTermScore(PostType.LowQuality, term, score);
				}

				if (dvCommand.StartsWith("name"))
				{
					term = new Regex(dvCommand.Remove(0, 5));

					if (!FilterTerms.BadUsernameTerms.ContainsTerm(term))
					{
						return "`Term does not exist.`";
					}

					score = FilterTerms.GetTermScore(PostType.BadUsername, term) - 1;

					FilterTerms.SetTermScore(PostType.BadUsername, term, score);
				}

				return "`Term downvoted. New score: " + score + ".`";
			}

			return "`Command not recognised.`";
		}

		private static string UpvoteTerm(string command)
		{
			var uvCommand = command.Substring(command.IndexOf(" ") + 1);

			if (uvCommand.StartsWith("") || uvCommand.StartsWith("") || uvCommand.StartsWith("") || uvCommand.StartsWith(""))
			{
				var score = -1;
				Regex term;

				if (uvCommand.StartsWith("off"))
				{
					term = new Regex(uvCommand.Remove(0, 4));

					if (!FilterTerms.OffensiveTerms.ContainsTerm(term))
					{
						return "`Term does not exist.`";
					}

					score = FilterTerms.GetTermScore(PostType.Offensive, term) + 1;

					FilterTerms.SetTermScore(PostType.Offensive, term, score);
				}

				if (uvCommand.StartsWith("spam"))
				{
					term = new Regex(uvCommand.Remove(0, 5));

					if (!FilterTerms.SpamTerms.ContainsTerm(term))
					{
						return "`Term does not exist.`";
					}

					score = FilterTerms.GetTermScore(PostType.Spam, term) + 1;

					FilterTerms.SetTermScore(PostType.Spam, term, score);
				}

				if (uvCommand.StartsWith("lq"))
				{
					term = new Regex(uvCommand.Remove(0, 3));

					if (!FilterTerms.LQTerms.ContainsTerm(term))
					{
						return "`Term does not exist.`";
					}

					score = FilterTerms.GetTermScore(PostType.LowQuality, term) + 1;

					FilterTerms.SetTermScore(PostType.LowQuality, term, score);
				}

				if (uvCommand.StartsWith("name"))
				{
					term = new Regex(uvCommand.Remove(0, 5));

					if (!FilterTerms.BadUsernameTerms.ContainsTerm(term))
					{
						return "`Term does not exist.`";
					}

					score = FilterTerms.GetTermScore(PostType.BadUsername, term) + 1;

					FilterTerms.SetTermScore(PostType.BadUsername, term, score);
				}

				return "`Term upvoted. New score: " + score + ".`";
			}

			return "`Command not recognised.`";
		}

		private static string AddTerm(string command)
		{
			var addCommand = command.Substring(command.IndexOf(" ", command.IndexOf(" ") + 1) + 1);

			if (addCommand.StartsWith("off") || addCommand.StartsWith("spam") || addCommand.StartsWith("lq") || addCommand.StartsWith("name"))
			{
				Regex term;

				if (addCommand.StartsWith("off"))
				{
					term = new Regex(addCommand.Remove(0, 4));

					if (FilterTerms.OffensiveTerms.ContainsTerm(term)) { return "`Term already exists.`"; }

					FilterTerms.AddTerm(PostType.Offensive, term);
				}

				if (addCommand.StartsWith("spam"))
				{
					term = new Regex(addCommand.Remove(0, 5));

					if (FilterTerms.SpamTerms.ContainsTerm(term)) { return "`Term already exists.`"; }

					FilterTerms.AddTerm(PostType.Spam, term);
				}

				if (addCommand.StartsWith("lq"))
				{
					term = new Regex(addCommand.Remove(0, 3));

					if (FilterTerms.LQTerms.ContainsTerm(term)) { return "`Term already exists.`"; }

					FilterTerms.AddTerm(PostType.LowQuality, term);
				}

				if (addCommand.StartsWith("name"))
				{
					term = new Regex(addCommand.Remove(0, 5));

					if (FilterTerms.BadUsernameTerms.ContainsTerm(term)) { return "`Term already exists.`"; }

					FilterTerms.AddTerm(PostType.BadUsername, term);
				}

				Stats.TermCount = FilterTerms.TermCount + IgnoreFilterTerms.TermCount;

				return "`Term added.`";
			}

			return "`Command not recognised.`";
		}

		private static string RemoveTerm(string command)
		{
			var removeCommand = command.Substring(command.IndexOf(" ", command.IndexOf(" ") + 1) + 1);

			if (removeCommand.StartsWith("off") || removeCommand.StartsWith("spam") || removeCommand.StartsWith("lq") || removeCommand.StartsWith("name"))
			{
				Regex term;

				if (removeCommand.StartsWith("off"))
				{
					term = new Regex(removeCommand.Remove(0, 4));

					if (!FilterTerms.OffensiveTerms.ContainsTerm(term)) { return "`Term does not exist.`"; }

					FilterTerms.RemoveTerm(PostType.Offensive, term);
				}

				if (removeCommand.StartsWith("spam"))
				{
					term = new Regex(removeCommand.Remove(0, 5));

					if (!FilterTerms.SpamTerms.ContainsTerm(term)) { return "`Term does not exist.`"; }

					FilterTerms.RemoveTerm(PostType.Spam, term);
				}

				if (removeCommand.StartsWith("lq"))
				{
					term = new Regex(removeCommand.Remove(0, 3));

					if (!FilterTerms.LQTerms.ContainsTerm(term)) { return "`Term does not exist.`"; }

					FilterTerms.RemoveTerm(PostType.LowQuality, term);
				}

				if (removeCommand.StartsWith("name"))
				{
					term = new Regex(removeCommand.Remove(0, 5));

					if (!FilterTerms.BadUsernameTerms.ContainsTerm(term)) { return "`Term does not exist.`"; }

					FilterTerms.RemoveTerm(PostType.BadUsername, term);
				}

				Stats.TermCount = FilterTerms.TermCount + IgnoreFilterTerms.TermCount;

				return "`Term removed.`";
			}

			return "`Command not recognised.`";
		}


		private static string AddIgnoreTerm(string command)
		{
			var addCommand = command.Substring(command.IndexOf(" ", command.IndexOf(" ") + 1) + 1);

			if (addCommand.StartsWith("off") || addCommand.StartsWith("spam") || addCommand.StartsWith("lq") || addCommand.StartsWith("name"))
			{
				Regex term;
				string site;

				if (addCommand.StartsWith("off"))
				{
					addCommand = addCommand.Remove(0, 4);

					if (addCommand.IndexOf(" ") == -1) { return "`Command not recognised.`"; }

					term = new Regex(addCommand.Remove(0, addCommand.IndexOf(" ") + 1));
					site = addCommand.Substring(0, addCommand.IndexOf(" "));

					if (IgnoreFilterTerms.OffensiveTerms.ContainsTerm(term)) { return "`Ignore term already exists.`"; }

					IgnoreFilterTerms.AddTerm(PostType.Offensive, term, site);
				}

				if (addCommand.StartsWith("spam"))
				{
					addCommand = addCommand.Remove(0, 5);

					if (addCommand.IndexOf(" ") == -1) { return "`Command not recognised.`"; }

					term = new Regex(addCommand.Remove(0, addCommand.IndexOf(" ") + 1));
					site = addCommand.Substring(0, addCommand.IndexOf(" "));

					if (IgnoreFilterTerms.SpamTerms.ContainsTerm(term)) { return "`Ignore term already exists.`"; }

					IgnoreFilterTerms.AddTerm(PostType.Spam, term, site);
				}

				if (addCommand.StartsWith("lq"))
				{
					addCommand = addCommand.Remove(0, 3);

					if (addCommand.IndexOf(" ") == -1) { return "`Command not recognised.`"; }

					term = new Regex(addCommand.Remove(0, addCommand.IndexOf(" ") + 1));
					site = addCommand.Substring(0, addCommand.IndexOf(" "));

					if (IgnoreFilterTerms.LQTerms.ContainsTerm(term)) { return "`Ignore term already exists.`"; }
					
					IgnoreFilterTerms.AddTerm(PostType.LowQuality, term, site);
				}

				if (addCommand.StartsWith("name"))
				{
					addCommand = addCommand.Remove(0, 5);

					if (addCommand.IndexOf(" ") == -1) { return "`Command not recognised.`"; }

					term = new Regex(addCommand.Remove(0, addCommand.IndexOf(" ") + 1));
					site = addCommand.Substring(0, addCommand.IndexOf(" "));

					if (IgnoreFilterTerms.BadUsernameTerms.ContainsTerm(term)) { return "`Ignore term already exists.`"; }

					IgnoreFilterTerms.AddTerm(PostType.BadUsername, term, site);
				}

				Stats.TermCount = FilterTerms.TermCount + IgnoreFilterTerms.TermCount;

				return "`Ignore term added.`";
			}

			return "`Command not recognised.`";
		}

		private static string RemoveIgnoreTerm(string command)
		{
			var removeCommand = command.Substring(command.IndexOf(" ", command.IndexOf(" ") + 1) + 1);

			if (removeCommand.StartsWith("off") || removeCommand.StartsWith("spam") || removeCommand.StartsWith("lq") || removeCommand.StartsWith("name"))
			{
				Regex term;
				string site;

				if (removeCommand.StartsWith("off"))
				{
					removeCommand = removeCommand.Remove(0, 4);

					if (removeCommand.IndexOf(" ") == -1) { return "`Command not recognised.`"; }

					term = new Regex(removeCommand.Remove(0, removeCommand.IndexOf(" ") + 1));
					site = removeCommand.Substring(0, removeCommand.IndexOf(" "));

					if (!IgnoreFilterTerms.OffensiveTerms.ContainsTerm(term)) { return "`Ignore term does not exist.`"; }

					IgnoreFilterTerms.RemoveTerm(PostType.Offensive, term, site);
				}

				if (removeCommand.StartsWith("spam"))
				{
					removeCommand = removeCommand.Remove(0, 5);

					if (removeCommand.IndexOf(" ") == -1) { return "`Command not recognised.`"; }

					term = new Regex(removeCommand.Remove(0, removeCommand.IndexOf(" ") + 1));
					site = removeCommand.Substring(0, removeCommand.IndexOf(" "));

					if (!IgnoreFilterTerms.SpamTerms.ContainsTerm(term)) { return "`Ignore term does not exist.`"; }

					IgnoreFilterTerms.RemoveTerm(PostType.Spam, term, site);
				}

				if (removeCommand.StartsWith("lq"))
				{
					removeCommand = removeCommand.Remove(0, 3);

					if (removeCommand.IndexOf(" ") == -1) { return "`Command not recognised.`"; }

					term = new Regex(removeCommand.Remove(0, removeCommand.IndexOf(" ") + 1));
					site = removeCommand.Substring(0, removeCommand.IndexOf(" "));

					if (!IgnoreFilterTerms.LQTerms.ContainsTerm(term)) { return "`Ignore term does not exist.`"; }

					IgnoreFilterTerms.RemoveTerm(PostType.LowQuality, term, site);
				}

				if (removeCommand.StartsWith("name"))
				{
					removeCommand = removeCommand.Remove(0, 5);

					if (removeCommand.IndexOf(" ") == -1) { return "`Command not recognised.`"; }

					term = new Regex(removeCommand.Remove(0, removeCommand.IndexOf(" ") + 1));
					site = removeCommand.Substring(0, removeCommand.IndexOf(" "));

					if (!IgnoreFilterTerms.BadUsernameTerms.ContainsTerm(term)) { return "`Ignore term does not exist.`"; }

					IgnoreFilterTerms.RemoveTerm(PostType.BadUsername, term, site);
				}

				Stats.TermCount = FilterTerms.TermCount + IgnoreFilterTerms.TermCount;

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
			var tagCommand = command.Remove(0, command.IndexOf("tag") + 4);

			var site = tagCommand.Substring(0, tagCommand.IndexOf(" "));
			var tag = tagCommand.Remove(0, tagCommand.IndexOf(" ") + 1);

			if (BadTagDefinitions.BadTags.ContainsKey(site) && BadTagDefinitions.BadTags[site].Contains(tag)) { return "`Tag already exists.`"; }

			BadTagDefinitions.AddTag(site, tag);

			return "`Tag added.`";
		}

		private static string RemoveTag(string command)
		{
			var tagCommand = command.Remove(0, command.IndexOf("tag") + 4);

			var site = tagCommand.Substring(0, tagCommand.IndexOf(" "));
			var tag = tagCommand.Remove(0, tagCommand.IndexOf(" ") + 1);

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
