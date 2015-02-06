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
            //string output;

            //using (var p = new Process())
            //{
            //    p.StartInfo = new ProcessStartInfo
            //    {
            //        FileName = "git",
            //        Arguments = "log --pretty=format:\"[`%h` *`(%s by %cn)`*]\" -n 1",
            //        WorkingDirectory = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).FullName,
            //        UseShellExecute = false,
            //        RedirectStandardOutput = true,
            //        CreateNoWindow = true
            //    };

            //    p.Start();

            //    output = p.StandardOutput.ReadToEnd();

            //    p.WaitForExit();
            //}



            //var psi = new ProcessStartInfo
            //{
            //    FileName = "cmd.exe",
            //    Arguments = "git log --pretty=format:\"[`%h` (%s by %cn)]\" -n 1",
            //    WorkingDirectory = @"C:\Users\Samuel\Documents\GitHub\Phamhilator"/*Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).FullName*///,
            //    //UseShellExecute = false,
            //    //RedirectStandardOutput = true,
            //    //RedirectStandardError = true//,
            //    //CreateNoWindow = true
            //};

            //var p = Process.Start(psi);

            //var t = p.StandardError.ReadToEnd();
            //var g = p.StandardOutput.ReadToEnd();

            //var output = Process.Start(psi).StandardOutput.ReadToEnd();

            //dataFormatted = output;
            //commitHash = output.Split('`')[1];




            //Process p = new Process();
            //ProcessStartInfo psi = new ProcessStartInfo();
            //psi.FileName = "cmd";
            //psi.Arguments = "git log --pretty=format:\"[`%h` (%s by %cn)]\" -n 1";
            //psi.WorkingDirectory = @"C:\Users\Samuel\Documents\GitHub\Phamhilator";//Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).FullName;
            //psi.UseShellExecute = false;
            //psi.RedirectStandardOutput = true;
            ////psi.CreateNoWindow = true;
            //p.StartInfo = psi;
            //p.Start();
            //string output = p.StandardOutput.ReadToEnd();
            //p.WaitForExit();
            //p.Dispose();

            //dataFormatted = output;
            //commitHash = output.Split('`')[1];


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
