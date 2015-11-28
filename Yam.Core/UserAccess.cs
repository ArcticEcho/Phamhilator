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





using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Phamhilator.Yam.Core
{
    public class UserAccess : IDisposable
    {
        private const string dataKey = "Authorised Users";
        private readonly ManualResetEvent mre = new ManualResetEvent(false);
        private readonly LocalRequestClient client;
        private bool dispose;

        public List<int> AuthorisedUsers { get; private set; }

        static public int[] Owners
        {
            get
            {
                return new[]
                {
                    227577,  // Sam (MSE)
                    2246344, // Sam (SO)
                    266094,  // Uni (MSE)
                    3622940, // Uni (SO)
                    229438,  // Fox (MSE)
                    2619912, // Fox (SO)
                    194047,  // Jan (MSE)
                    245360,  // Pat (MSE)
                    202832,  // Moo (MSE)
                };
            }
        }



        public UserAccess(ref LocalRequestClient client)
        {
            if (client == null) { throw new ArgumentNullException("client"); }

            this.client = client;
            PopulateAuthorisedUsers();
            Task.Run(() => RefreshAuthorisedUsers());
        }

        ~UserAccess()
        {
            Dispose();
        }



        public void Dispose()
        {
            if (dispose) { return; }
            dispose = true;

            mre.Set();
            mre.Dispose();
            GC.SuppressFinalize(this);
        }

        public void AddAuthorisedUser(int id)
        {
            AuthorisedUsers.Add(id);

            var sb = new StringBuilder();

            foreach (var i in AuthorisedUsers)
            {
                sb.Append(i);
                sb.Append("\r\n");
            }

            client.UpdateData("Yam", dataKey, sb.ToString().Trim());
        }



        private void PopulateAuthorisedUsers()
        {
            AuthorisedUsers = new List<int>();

            if (!client.DataExists("Yam", dataKey)) { return; }

            var data = client.RequestData("Yam", dataKey);
            var idsStr = data.Split(new[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var id in idsStr)
            {
                AuthorisedUsers.Add(int.Parse(id));
            }
        }

        private void RefreshAuthorisedUsers()
        {
            while (!dispose)
            {
                mre.WaitOne(TimeSpan.FromSeconds(1));

                PopulateAuthorisedUsers();
            }
        }
    }
}
