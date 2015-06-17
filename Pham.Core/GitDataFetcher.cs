///*
// * Phamhilator. A .Net based bot network catching spam/low quality posts for Stack Exchange.
// * Copyright © 2015, ArcticEcho.
// *
// * This program is free software: you can redistribute it and/or modify
// * it under the terms of the GNU General Public License as published by
// * the Free Software Foundation, either version 3 of the License, or
// * (at your option) any later version.
// *
// * This program is distributed in the hope that it will be useful,
// * but WITHOUT ANY WARRANTY; without even the implied warranty of
// * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// * GNU General Public License for more details.
// *
// * You should have received a copy of the GNU General Public License
// * along with this program.  If not, see <http://www.gnu.org/licenses/>.
// */





//using System;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.IO;
//using System.Linq;
//using System.Text;
//using System.Text.RegularExpressions;
//using System.Threading.Tasks;
//using Phamhilator.Yam.Core;

//namespace Phamhilator.Pham.Core
//{
//    public static class GitDataFetcher
//    {
//        public static void GetData(out string commitHash, out string commitMessage, out string commitAuthor, bool escapeData = true)
//        {
//            string output;

//            using (var p = new Process())
//            {
//                p.StartInfo = new ProcessStartInfo
//                {
//                    FileName = "git",
//                    Arguments = "log --pretty=format:\"%h|||%s|||%cn\" -n 1",
//                    WorkingDirectory = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).FullName,
//                    UseShellExecute = false,
//                    CreateNoWindow = true,
//                    RedirectStandardOutput = true
//                };
//                p.Start();

//                output = p.StandardOutput.ReadToEnd();

//                p.WaitForExit();
//            }

//            var data = Regex.Split(output, @"\|\|\|");

//            if (escapeData)
//            {
//                for (var i = 0; i < data.Length; i++)
//                {
//                    data[i] = PostFetcher.ChatEscapeString(data[i]);
//                }
//            }

//            commitHash = data[0];
//            commitMessage = data[1];
//            commitAuthor = data[2];
//        }
//    }
//}
