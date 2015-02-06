using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Phamhilator.Core
{
    public static class GitDataFetcher
    {
        public static void GetData(out string commitHash, out string commitMessage, out string commitAuthor)
        {
            string output;

            using (var p = new Process())
            {
                p.StartInfo = new ProcessStartInfo
                {
                    FileName = "git",
                    Arguments = "log --pretty=format:\"%h|||%s|||%cn\" -n 1",
                    WorkingDirectory = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).FullName,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true
                };
                p.Start();

                output = p.StandardOutput.ReadToEnd();

                p.WaitForExit();
            }

            var data = Regex.Split(output, @"\|\|\|");

            commitHash = data[0];
            commitMessage = data[1];
            commitAuthor = data[2];
        }
    }
}
