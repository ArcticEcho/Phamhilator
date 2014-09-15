using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



// TODO: Current access list: Me, Uni, Fox, Rene & Jan.

// Example commands.
// >> add term name john
// >> add term lq need help
// >> dv john
// >> uv need help
// >> remove term name john


  
namespace Phamhilator
{
	public static class CommandProcessor
	{
		public static string ExacuteCommand(KeyValuePair<string, string> input, int postedMessageCount)
		{
			var command = input.Value.Replace("&gt;&gt;", "").Trim().ToLowerInvariant();
			var user = input.Key;

			if (command == "stats" || command == "info")
			{
				var totalTermCount = FilterTerms.BadUsernameTerms.Count + FilterTerms.LQTerms.Count + FilterTerms.OffensiveTerms.Count + FilterTerms.SpamTerms.Count;

				return "`Owners: Sam, Unihedron & ProgramFOX. Users with command access: Jan Dvorak. Total terms: " + totalTermCount + ". Caught posts over last 2 days: " + postedMessageCount + ".`";
			}

			if (user != "Sam" && user != "Unihedron" && user != "ProgramFOX" && user != "Jan Dvorak" && user != "rene") { return ""; }

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

			return "`Command not recognised.`";
		}




		private static string DownvoteTerm(string command)
		{
			var dvCommand = command.Substring(command.IndexOf(" ") + 1);
			var term = "";

			if (dvCommand.StartsWith("off"))
			{
				term = dvCommand.Remove(0, 4);

				if (!FilterTerms.OffensiveTerms.ContainsKey(command)) { return "`Term does not exist.`"; }

				FilterTerms.OffensiveTerms[term]--;

				return "`Term downvoted.`";
			}

			if (dvCommand.StartsWith("spam"))
			{
				term = dvCommand.Remove(0, 5);

				if (!FilterTerms.SpamTerms.ContainsKey(command)) { return "`Term does not exist.`"; }

				FilterTerms.SpamTerms[term]--;

				return "`Term downvoted.`";
			}

			if (dvCommand.StartsWith("lq"))
			{
				term = dvCommand.Remove(0, 3);

				if (!FilterTerms.LQTerms.ContainsKey(command)) { return "`Term does not exist.`"; }

				FilterTerms.LQTerms[term]--;

				return "`Term downvoted.`";
			}

			if (dvCommand.StartsWith("name"))
			{
				term = dvCommand.Remove(0, 5);

				if (!FilterTerms.BadUsernameTerms.ContainsKey(command)) { return "`Term does not exist.`"; }

				FilterTerms.BadUsernameTerms[term]--;

				return "`Term downvoted.`";
			}

			return "`Command not recognised.`";
		}

		private static string UpvoteTerm(string command)
		{
			var uvCommand = command.Substring(command.IndexOf(" ") + 1);
			var term = "";

			if (uvCommand.StartsWith("off"))
			{
				term = uvCommand.Remove(0, 4);

				if (!FilterTerms.OffensiveTerms.ContainsKey(command))
				{
					return "`Term does not exist.`"; 
					
				}
				FilterTerms.OffensiveTerms[term]++;

				return "`Term upvoted.`";
			}

			if (uvCommand.StartsWith("spam"))
			{
				term = uvCommand.Remove(0, 5);

				if (!FilterTerms.SpamTerms.ContainsKey(command)) { return "`Term does not exist.`"; }

				FilterTerms.SpamTerms[term]++;

				return "`Term upvoted.`";
			}

			if (uvCommand.StartsWith("lq"))
			{
				term = uvCommand.Remove(0, 3);

				if (!FilterTerms.LQTerms.ContainsKey(command)) { return "`Term does not exist.`"; }

				FilterTerms.LQTerms[term]++;

				return "`Term upvoted.`";
			}

			if (uvCommand.StartsWith("name"))
			{
				term = uvCommand.Remove(0, 5);

				if (!FilterTerms.BadUsernameTerms.ContainsKey(command)) { return "`Term does not exist.`"; }

				FilterTerms.BadUsernameTerms[term]++;

				return "`Term upvoted.`";
			}

			return "`Command not recognised.`";
		}

		private static string AddTerm(string command)
		{
			var addCommand = command.Substring(command.IndexOf(" ") + 1);
			var term = "";

			if (addCommand.StartsWith("off"))
			{
				term = addCommand.Remove(0, 4);

				FilterTerms.AddTerm(PostType.Offensive, term);

				return "`Term added.`";			
			}

			if (addCommand.StartsWith("spam"))
			{
				term = addCommand.Remove(0, 5);

				FilterTerms.AddTerm(PostType.Spam, term);

				return "`Term added.`";			
			}

			if (addCommand.StartsWith("lq"))
			{
				term = addCommand.Remove(0, 3);

				FilterTerms.AddTerm(PostType.LowQuality, term);

				return "`Term added.`";	
			}

			if (addCommand.StartsWith("name"))
			{
				term = addCommand.Remove(0, 5);

				FilterTerms.AddTerm(PostType.BadUsername, term);

				return "`Term added.`";			
			}

			return "`Command not recognised.`";
		}

		private static string RemoveTerm(string command)
		{
			var removeCommand = command.Substring(command.IndexOf(" ") + 1);
			var term = "";

			if (removeCommand.StartsWith("off"))
			{
				term = removeCommand.Remove(3);

				FilterTerms.RemoveTerm(PostType.Offensive, term);

				return "`Term removed.`";			
			}

			if (removeCommand.StartsWith("spam"))
			{
				term = removeCommand.Remove(4);

				FilterTerms.RemoveTerm(PostType.Spam, term);

				return "`Term removed.`";		
			}

			if (removeCommand.StartsWith("lq"))
			{
				term = removeCommand.Remove(2);

				FilterTerms.RemoveTerm(PostType.LowQuality, term);

				return "`Term removed.`";	
			}

			if (removeCommand.StartsWith("name"))
			{
				term = removeCommand.Remove(4);

				FilterTerms.RemoveTerm(PostType.BadUsername, term);

				return "`Term removed.`";	
			}

			return "`Command not recognised.`";
		}
	}
}
