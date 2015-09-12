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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using CsQuery;
using Phamhilator.Yam.Core;

namespace Phamhilator.Pham.UI
{
    public class PostLogModelGenerator : IDisposable
    {
        private const string cvDataKey = "CV Models";
        private const string dvQDataKey = "DV Q Models";
        private const string dvADataKey = "DV A Models";
        private readonly ManualResetEvent mre = new ManualResetEvent(false);
        private readonly HashSet<int> checkedPosts = new HashSet<int>();
        private readonly ModelGenerator modelGen = new ModelGenerator();
        private readonly LocalRequestClient client;
        private readonly PostClassifier cvC;
        private readonly PostClassifier dvC;
        private bool dispose;



        public PostLogModelGenerator(ref LocalRequestClient yamClient, ref PostClassifier cvClassifier, ref PostClassifier dvClassifier)
        {
            if (yamClient == null) { throw new ArgumentNullException("yamClient"); }
            if (cvClassifier == null) { throw new ArgumentNullException("cvClassifier"); }
            if (dvClassifier == null) { throw new ArgumentNullException("dvClassifier"); }

            client = yamClient;
            cvC = cvClassifier;
            dvC = dvClassifier;

            Task.Run(() => GenLoop());
        }



        public void Dispose()
        {
            if (dispose) { return; }
            dispose = true;
            mre.Set();
            GC.SuppressFinalize(this);
        }



        private void GenLoop()
        {
            while (!dispose)
            {
                var posts = client.SendLoqRequest(new RemoteLogRequest
                {
                    Site = "(?i)^stackoverflow.com$",
                    CreatedAfter = DateTime.UtcNow.AddDays(-2).Date.ToString("s", CultureInfo.InvariantCulture),
                    CreatedBefore = DateTime.UtcNow.AddDays(-1).Date.ToString("s", CultureInfo.InvariantCulture)
                });

                posts = posts.Where(e => checkedPosts.All(hc => hc != e.GetHashCode())).ToArray();
                var max = Math.Min(360, posts.Length);

                for (var i = 0; i < max; i++)
                {
                    var fullWait = true;
                    checkedPosts.Add(posts[i].GetHashCode());

                    if (IsPostDeleted(posts[i].Post.Url))
                    {
                        AddDVPostModel(posts[i]);
                    }
                    else
                    {
                        fullWait = false;
                        mre.WaitOne(TimeSpan.FromSeconds(5));

                        if (posts[i].IsQuestion && IsQuestionClosed(posts[i].Post.Url))
                        {
                            AddCVPostModel(posts[i]);
                        }
                    }

                    mre.WaitOne(TimeSpan.FromSeconds(fullWait ? 10 : 5));
                    if (dispose) { return; }
                }

                mre.WaitOne(TimeSpan.FromSeconds((max - 360) * 10));
            }
        }

        private bool IsPostDeleted(string url)
        {
            try
            {
                var dehDataz = new WebClient().DownloadData(url);
                dehDataz = null; // Prevent this from being optimised away.
            }
            catch (WebException ex)
            {
                if (ex.Response != null && ((HttpWebResponse)ex.Response).StatusCode == HttpStatusCode.NotFound)
                {
                    using (var str = ex.Response.GetResponseStream())
                    using (var sr = new StreamReader(str))
                    {
                        var html = sr.ReadToEnd();
                        var dom = CQ.Create(html);

                        if (dom[".leftcol"].Html().Contains("reasons of moderation"))
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        private bool IsQuestionClosed(string url)
        {
            try
            {
                var dom = CQ.CreateFromUrl(url);
                var qStatus = dom[".question-status"].Html();

                if (qStatus.Contains("too broad") || qStatus.Contains("closed"))
                {
                    return true;
                }
            }
            catch (WebException) { }

            return false;
        }

        private void AddDVPostModel(LogEntry entry)
        {
            var modelAry = modelGen.GenerateModel(entry.Post.Body);
            var model = "";
            foreach (var tag in modelAry)
            {
                model += tag + " ";
            }

            var allModels = "";
            if (client.DataExists("Pham", dvQDataKey))
            {
                allModels = client.RequestData("Pham", dvQDataKey);
            }
            allModels += "\n" + model;

            if (entry.IsQuestion)
            {
                client.UpdateData("Pham", dvQDataKey, allModels.Trim());
            }
            else
            {
                client.UpdateData("Pham", dvADataKey, allModels.Trim());
            }
        }

        private void AddCVPostModel(LogEntry entry)
        {
            var modelAry = modelGen.GenerateModel(entry.Post.Body);
            var model = "";
            foreach (var tag in modelAry)
            {
                model += tag + " ";
            }

            var allModels = "";
            if (client.DataExists("Pham", cvDataKey))
            {
                allModels = client.RequestData("Pham", cvDataKey);
            }
            allModels += "\n" + model;

            client.UpdateData("Pham", cvDataKey, allModels.Trim());
        }
    }
}
