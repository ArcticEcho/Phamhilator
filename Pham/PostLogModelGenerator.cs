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
        private readonly ManualResetEvent mre = new ManualResetEvent(false);
        private readonly HashSet<int> checkedPosts = new HashSet<int>();
        private readonly ModelGenerator modelGen = new ModelGenerator();
        private readonly LocalRequestClient client;
        private readonly ModelClassifier cvC;
        private readonly ModelClassifier dvQC;
        private readonly ModelClassifier dvAC;
        private bool dispose;

        public const string CVDataKey = "CV Models";
        public const string DVQDataKey = "DV Q Models";
        public const string DVADataKey = "DV A Models";



        public PostLogModelGenerator(ref LocalRequestClient yamClient, ref ModelClassifier cvClassifier, ref ModelClassifier dvQClassifier, ref ModelClassifier dvAClassifier)
        {
            //TODO: Disable this for now.

            //if (yamClient == null) { throw new ArgumentNullException("yamClient"); }
            //if (cvClassifier == null) { throw new ArgumentNullException("cvClassifier"); }
            //if (dvQClassifier == null) { throw new ArgumentNullException("dvQClassifier"); }
            //if (dvAClassifier == null) { throw new ArgumentNullException("dvAClassifier"); }

            //client = yamClient;
            //cvC = cvClassifier;
            //dvQC = dvQClassifier;
            //dvAC = dvAClassifier;

            //Task.Run(() => GenLoop());
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

                mre.WaitOne(TimeSpan.FromSeconds(Math.Max((max - 360) * 10, 0)));
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

                if (qStatus.Contains("on hold") || qStatus.Contains("closed"))
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
            if (client.DataExists("Pham", DVQDataKey))
            {
                allModels = client.RequestData("Pham", DVQDataKey);
            }
            allModels += "\n" + model;

            if (entry.IsQuestion)
            {
                dvQC.Models.Add(modelAry);
                client.UpdateData("Pham", DVQDataKey, allModels.Trim());
            }
            else
            {
                dvAC.Models.Add(modelAry);
                client.UpdateData("Pham", DVADataKey, allModels.Trim());
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
            if (client.DataExists("Pham", CVDataKey))
            {
                allModels = client.RequestData("Pham", CVDataKey);
            }
            allModels += "\n" + model;

            cvC.Models.Add(modelAry);
            client.UpdateData("Pham", CVDataKey, allModels.Trim());
        }
    }
}
