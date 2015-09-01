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
using System.Threading;
using System.Threading.Tasks;

namespace Phamhilator.Yam.UI
{
    public class AuthorisedUsers : IDisposable
    {
        private const string dataKey = "Authorised Users";
        private readonly ManualResetEvent mre = new ManualResetEvent(false);
        private bool dispose;

        public List<int> IDs { get; private set; }



        public AuthorisedUsers()
        {
            PopulateIDs();
            Task.Run(() => RefreshIDs());
        }

        ~AuthorisedUsers()
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

        public void AddUser(int ID)
        {
            var newIDs = "";

            if (DataManager.DataExists("Yam", dataKey))
            {
                newIDs = DataManager.LoadData("Yam", dataKey) + "\n";
            }

            DataManager.SaveData("Yam", dataKey, newIDs + ID);
        }



        private void PopulateIDs()
        {
            IDs = new List<int>();

            if (!DataManager.DataExists("Yam", dataKey)) { return; }

            var data = DataManager.LoadData("Yam", dataKey);
            var idsStr = data.Split(new[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var id in idsStr)
            {
                IDs.Add(int.Parse(id));
            }
        }

        private void RefreshIDs()
        {
            while (!dispose)
            {
                mre.WaitOne(TimeSpan.FromSeconds(1));

                PopulateIDs();
            }
        }
    }
}
