using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



// TODO: Current access list: Me, Uni, Fox, Rene & Jan.

// Example commands.



namespace Phamhilator
{
	public static class CommandProcessor
	{
		public static void ExacuteCommand(KeyValuePair<string, string> input)
		{
			var command = input.Value.Replace(">>", "").Replace("@Sam", "").Replace("@sam", "").Trim().ToLowerInvariant();
			var user = input.Key;

			if (user != "Sam" && user != "Unihedron" && user != "ProgramFOX" && user != "Jan Dvorak" && user != "rene") { return; }

			if (command.StartsWith("-1") || command.StartsWith("dv") || command.StartsWith("downvote"))
			{
				DownvoteTerm(command);
			}
			else if (command.StartsWith("+1") || command.StartsWith("uv") || command.StartsWith("upvote"))
			{
				UpvoteTerm(command);
			}
			else if (command.StartsWith("remove term"))
			{
				RemoveTerm(command);
			}
			else if (command.StartsWith("add term"))
			{
				AddTerm(command);
			}
		}




		private static void DownvoteTerm(string command)
		{
			var term = command.Remove(14);

			if (FilterTerms.OffensiveTerms.ContainsKey(command))
			{
				FilterTerms.OffensiveTerms[term]--;
			}
		}

		private static void UpvoteTerm(string command)
		{
			var term = command.Remove(12);

			if (FilterTerms.OffensiveTerms.ContainsKey(command))
			{
				FilterTerms.OffensiveTerms[term]++;
			}
		}

		private static void AddTerm(string command)
		{
			var addCommand = command.Remove(9);
			var term = "";

			if (addCommand.StartsWith("offensive") || addCommand.StartsWith("off"))
			{
				var startIndex = addCommand.IndexOf(' ');

				term = addCommand.Substring(startIndex, addCommand.Length - startIndex).Trim();
			}

			//if (FilterTerms.OffensiveTerms.ContainsKey(command))
			//{
			//	FilterTerms.AddTerm(PostType.term);
			//}
		}

		private static void RemoveTerm(string command)
		{
			var term = command.Remove(9);

			if (FilterTerms.OffensiveTerms.ContainsKey(command))
			{
				FilterTerms.OffensiveTerms[term]--;
			}
		}
	}
}
