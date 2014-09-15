using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



// TODO: Current access list: Me, Uni, Fox, Rene & Jan.



namespace Phamhilator
{
	public static class CommandProcessor
	{
		public static void ExacuteCommand(KeyValuePair<string, string> input)
		{
			var command = input.Value.Replace(">>", "").Replace("@Sam", "").Replace("@sam", "").Trim();
			var user = input.Key;

			if (user != "Sam" && user != "Unihedron" && user != "ProgramFOX" && user != "Jan Dvorak" && user != "rene") { return; }

			if (input.Value.Contains("false pos"))
			{
				
			}
		}
	}
}
