using System.Text.RegularExpressions;



namespace Phamhilator
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
