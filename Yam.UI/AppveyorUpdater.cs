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





using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using ServiceStack;
using ServiceStack.Text;

namespace Phamhilator.Yam.UI
{
    public class AppveyorUpdater
    {
        private static readonly Version currentVer = new Version(FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileVersion);
        private const string apiUrl = "https://ci.appveyor.com/api/";
        private readonly string owner;
        private readonly string proj;
        private readonly string tkn;

        public Version CurrentVersion
        {
            get
            {
                return currentVer;
            }
        }

        public Version LatestVersion
        {
            get
            {
                var projJson = Encoding.UTF8.GetString(Get($"projects/{owner}/{proj}"));
                var verStr = DynamicJson.Deserialize(projJson).build.version;
                return new Version(verStr);
            }
        }

        public string LatestVerMessage
        {
            get
            {
                var projJson = Encoding.UTF8.GetString(Get($"projects/{owner}/{proj}"));
                return DynamicJson.Deserialize(projJson).build.message;
            }
        }



        public AppveyorUpdater(string apiToken, string projectOwner, string projectSlug)
        {
            tkn = apiToken;
            owner = projectOwner;
            proj = projectSlug;
        }



        /// <summary>
        /// Fetches the latest pre-built assemblies from Appveyor.
        /// </summary>
        /// <returns>
        /// The file path(s) of the new executable(s).
        /// Returns null if the operation was unsuccessful.
        /// </returns>
        public List<string> UpdateAssemblies()
        {
            var remVer = LatestVersion;

            if (CurrentVersion == remVer) return null;

            //try
            //{
                var newExes = new List<string>();
                var projJson = Encoding.UTF8.GetString(Get($"projects/{owner}/{proj}"));
                var jobId = DynamicJson.Deserialize(projJson).build.jobs[0].jobId;
                var artifRes = Encoding.UTF8.GetString(Get("buildjobs/" + jobId + "/artifacts"));
                var artifJson = JsonSerializer.DeserializeFromString<Dictionary<string, object>[]>(artifRes);

                foreach (var file in artifJson)
                {
                    var newFile = $"{remVer} - {file["name"]}";
                    RemoveOldFile(file["name"]);
                    GetFile($"buildjobs/{jobId}/artifacts/{file["fileName"]}", newFile);

                    if (file["name"].EndsWith(".exe"))
                    {
                        newExes.Add(Path.Combine(Directory.GetCurrentDirectory(), newFile));
                    }
                }

                return newExes;
            //}
            //catch // For now, we'll just return null. Might change this later.
            //{
            //    return null;
            //}
        }



        private byte[] Get(string url, bool file = false)
        {
            var req = (HttpWebRequest)WebRequest.Create(apiUrl + url);

            req.Method = "get";
            req.Headers.Add("Authorization", "Bearer " + tkn);
            if (!file)
            {
                req.ContentType = "application/json";
            }

            var res = (HttpWebResponse)req.GetResponse();
            using (var strm = res.GetResponseStream())
            {
                return strm.ReadFully();
            }
        }

        private void GetFile(string url, string filepath)
        {
            var bytes = Get(url, true);
            using (var file = new FileStream(filepath, FileMode.Create))
            {
                file.Write(bytes, 0, bytes.Length);
            }
        }

        private void RemoveOldFile(string filename)
        {
            var files = Directory.EnumerateFiles(".");
            var fn = Path.GetFileNameWithoutExtension(filename).Replace(".", @"\.");

            foreach (var file in files)
            {
                var oldFn = Path.GetFileNameWithoutExtension(file);
                var fileInfo = FileVersionInfo.GetVersionInfo(file);
                var fileVer = default(Version);

                if (Version.MatchesFormat(fileInfo.FileVersion))
                {
                    fileVer = new Version(fileInfo.FileVersion);
                }

                if (Regex.IsMatch(oldFn, @"\d+\.\d+\.\d+\.\d+ - " + fn) && fileVer < currentVer)
                {
                    File.Delete(file);
                }
            }
        }
    }
}
