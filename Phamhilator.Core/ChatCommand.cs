/*
 * Phamhilator. A .Net based bot network catching spam/low quality posts for Stack Exchange.
 * Copyright © 2015, ArcticEcho.
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */





using System.Text.RegularExpressions;



namespace Phamhilator.Core
{
    class ChatCommand
    {
        public delegate ReplyMessage[] CommandDel(string command);

        public Regex Syntax { get; private set; }
        public CommandDel Command { get; private set;}
        public CommandAccessLevel AccessLevel { get; private set; }



        public ChatCommand(Regex syntax, CommandDel command, CommandAccessLevel accessLevel)
        {
            Syntax = syntax;
            Command = command;
            AccessLevel = accessLevel;
        }
    }
}
