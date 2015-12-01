using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ServiceStack;
using ServiceStack.Text;

namespace Phamhilator.Yam.UI
{
    public class AppveyorUpdater
    {
        private static readonly string currentVer = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileVersion;
        private const string apiUrl = "https://ci.appveyor.com/api/";
        private readonly string owner;
        private readonly string proj;
        private readonly string tkn;

        public string CurrentVersion { get; } = currentVer;

        public string LatestVersion
        {
            get
            {
                var projJson = Encoding.UTF8.GetString(Get($"projects/{owner}/{proj}"));
                return DynamicJson.Deserialize(projJson).build.version;
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
            if (CurrentVersion == LatestVersion) return null;

            try
            {
                var newExes = new List<string>();
                var remVer = LatestVersion;
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
            }
            catch // For now, we'll just return null. Might change this later.
            {
                return null;
            }
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
                if (Regex.IsMatch(oldFn, @"\d+\.\d+\.\d+\.\d+ - " + fn) &&
                    !fn.StartsWith(currentVer))
                {
                    File.Delete(file);
                }
            }
        }
    }
}
