using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace Phamhilator
{
	public static class CommandProcessor
	{
		public static void ExacuteCommand(string input)
		{
			var user = GetUser(input);
			var command = GetCommand(input);

			if (command.StartsWith("false pos"))
			{
				
			}
		}



		private static string GetUser(string input)
		{
			return input.Split(new[] { "-=-" } , 1, StringSplitOptions.RemoveEmptyEntries)[0];
		}

		private static string GetCommand(string input)
		{
			return input.Split(new[] { "-=-" }, 2, StringSplitOptions.RemoveEmptyEntries)[1];
		}
	}
}
