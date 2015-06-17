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
//using System.IO;
//using System.Linq;
//using System.Text;
//using System.Text.RegularExpressions;
//using System.Threading.Tasks;
//using Newtonsoft.Json;

//namespace Phamhilator.Pham.Core
//{
//    public static class TermCreater
//    {
//        public static void CreateTerm(FilterConfig filter, Regex term, string site = "", float newScore = 0)
//        {
//            if (term == null || String.IsNullOrEmpty(term.ToString())) { throw new ArgumentException("term can not be null or empty.", "term"); }

//            var file = String.IsNullOrEmpty(site) ? DirectoryTools.GetFilterFile(filter) : Path.Combine(DirectoryTools.GetFilterFile(filter), site, "Terms.txt");

//            if (!File.Exists(file))
//            {
//                if (!Directory.Exists(Directory.GetParent(file).FullName))
//                {
//                    Directory.CreateDirectory(Directory.GetParent(file).FullName);
//                }

//                File.Create(file).Dispose();
//            }

//            var t = new Term(filter, term, newScore, site);
            
//            File.WriteAllText(file, JsonConvert.SerializeObject(t.ToJsonTerm(), Formatting.Indented));
//        }
//    }
//}
