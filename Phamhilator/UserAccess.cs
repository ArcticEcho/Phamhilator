using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;



namespace Phamhilator
{
    public static class UserAccess
    {
        private static List<int> commandAccessUsers;

        public static List<int> CommandAccessUsers
        {
            get
            {
                if (commandAccessUsers == null)
                {
                    PopulateCommandAccessUsers();
                }

                return commandAccessUsers;
            }
        }



        public static void AddUser(int id)
        {
            CommandAccessUsers.Add(id);

            File.AppendAllLines(DirectoryTools.GetCommandAccessUsersFile(), new[] { id.ToString(CultureInfo.InvariantCulture) });
        }



        private static void PopulateCommandAccessUsers()
        {
            commandAccessUsers = new List<int>();

            var users = File.ReadAllLines(DirectoryTools.GetCommandAccessUsersFile());

            foreach (var user in users)
            {
                if (!String.IsNullOrWhiteSpace(user))
                {
                    commandAccessUsers.Add(int.Parse(user.Trim()));
                }
            }
        }
    }
}
