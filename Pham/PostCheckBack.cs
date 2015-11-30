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
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Phamhilator.Yam.Core;

namespace Phamhilator.Pham.UI
{
    public class PostCheckBack : IDisposable
    {
        private readonly ManualResetEvent checkBackMre = new ManualResetEvent(false);
        private Logger<Post> logger;
        private bool dispose;

        public Action<Post> PostMatch { get; set; }



        public PostCheckBack(string postLogPath, TimeSpan postTTL, TimeSpan flushRate)
        {
            logger = new Logger<Post>(postLogPath, postTTL, flushRate);

            Task.Run(() => CheckPosts());
        }

        ~PostCheckBack()
        {
            Dispose();
        }



        public void Dispose()
        {
            if (dispose) return;
            dispose = true;

            //TODO: Do stuff.

            GC.SuppressFinalize(this);
        }



        private void CheckPosts()
        {
            while (!dispose)
            {
                checkBackMre.WaitOne(TimeSpan.FromMinutes(5));

                // No clue how this will work, just some ideas.
                // Severity 0: 1 hour
                // Severity 1: 6 hours
                // Severity 2: 12 hours
                // Severity 3: 24 hours

                //TODO: Add more complex stuff (PostFetcher may contain some helpful methods)...
            }
        }
    }
}
