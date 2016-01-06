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
using System.Threading;
using System.Threading.Tasks;
using CsQuery;
using Phamhilator.Yam.Core;

namespace Phamhilator.Pham.UI
{
    public class PostCheckBack : IDisposable
    {
        private const int secsPerDay = 60 * 60 * 24;
        private readonly ManualResetEvent checkBackMre = new ManualResetEvent(false);
        private Logger<Post> logger;
        private TimeSpan chkRate;
        private bool dispose;

        public Action<Post> ClosedPostFound { get; set; }

        public Action<Post> DeletedPostFound { get; set; }



        public PostCheckBack(string postLogPath, TimeSpan checkRate)
        {
            logger = new Logger<Post>(postLogPath);
            chkRate = checkRate;

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

            checkBackMre?.Set();
            checkBackMre?.Dispose();
            logger?.Dispose();

            GC.SuppressFinalize(this);
        }

        public void AddPost(Post post)
        {
            if (dispose) return;

            if (logger.Count < secsPerDay / chkRate.TotalSeconds &&
                (DateTime.UtcNow - post.CreationDate).TotalDays < 1)
            {
                logger.EnqueueItem(post);
            }
        }



        private void CheckPosts()
        {
            while (!dispose)
            {
                checkBackMre.WaitOne(chkRate);

                if (dispose) return;

                var post = new Post
                {
                    CreationDate = DateTime.MaxValue
                };

                foreach (var p in logger)
                {
                    if (p.CreationDate < post.CreationDate)
                    {
                        post = p;
                    }
                }

                var timeAlive = DateTime.UtcNow - post.CreationDate;

                if (timeAlive.TotalDays > 2)
                {
                    logger.RemoveItem(post);
                    continue;
                }

                if (timeAlive.TotalDays > 1)
                {
                    CQ dom;

                    if (PostFetcher.IsPostDeleted(post.Url, out dom) && DeletedPostFound != null)
                    {
                        DeletedPostFound(post);
                    }
                    else if (PostFetcher.IsQuestionClosed(dom, post.Url, false) && ClosedPostFound != null)
                    {
                        ClosedPostFound(post);
                    }

                    logger.RemoveItem(post);
                }
            }
        }
    }
}
