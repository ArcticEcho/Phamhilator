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

//namespace Phamhilator.Pham.Core
//{
//    public class BadTags
//    {
//        public Dictionary<string, Dictionary<string, string>> Tags { get; private set; }



//        public BadTags()
//        {
//            Tags = new Dictionary<string, Dictionary<string, string>>();

//            foreach (var dir in Directory.EnumerateDirectories(DirectoryTools.GetBadTagsFolder()))
//            {
//                var site = Path.GetFileName(dir);

//                if (String.IsNullOrEmpty(site) || Tags.ContainsKey(site)) { continue; }

//                Tags.Add(site, new Dictionary<string, string>());

//                var lines = File.ReadAllLines(Path.Combine(dir, "BadTags.txt")).ToArray();

//                for (var i = 0; i < lines.Length; i++)
//                {
//                    lines[i] = lines[i].Trim();

//                    if (!String.IsNullOrWhiteSpace(lines[i]))
//                    {
//                        string metaPost;
//                        string tag;

//                        if (lines[i].IndexOf(" ", StringComparison.Ordinal) != -1) // Check if tag has meta post
//                        {
//                            tag = lines[i].Substring(0, lines[i].IndexOf(" ", StringComparison.Ordinal));
//                            metaPost = lines[i].Remove(0, lines[i].IndexOf(" ", StringComparison.Ordinal) + 1);
//                        }
//                        else
//                        {
//                            tag = lines[i];
//                            metaPost = "";
//                        }

//                        if (!Tags[site].ContainsKey(tag))
//                        {
//                            Tags[site].Add(tag, metaPost);
//                        }
//                    }
//                }
//            }
//        }

//        public void AddTag(string site, string tag, string metaPost = "")
//        {
//            if (Tags.ContainsKey(site))
//            {
//                Tags[site].Add(tag, metaPost);

//                File.AppendAllText(Path.Combine(DirectoryTools.GetBadTagsFolder(), site, "BadTags.txt"), Environment.NewLine + tag + (metaPost == "" ? "" : " " + metaPost));
//            }
//            else
//            {
//                var path = Path.Combine(DirectoryTools.GetBadTagsFolder(), site);

//                Directory.CreateDirectory(path);

//                Tags.Add(site, new Dictionary<string, string> { { tag, metaPost } });

//                File.AppendAllText(Path.Combine(DirectoryTools.GetBadTagsFolder(), site, "BadTags.txt"), Environment.NewLine + tag + (metaPost == "" ? "" : " " + metaPost));
//            }
//        }

//        public void RemoveTag(string site, string tag)
//        {
//            if (!Tags.ContainsKey(site) || !Tags[site].ContainsKey(tag)) { return; }

//            Tags[site].Remove(tag);

//            var data = File.ReadAllLines(Path.Combine(DirectoryTools.GetBadTagsFolder(), site, "BadTags.txt")).ToList();

//            for (var i = 0; i < data.Count; i++)
//            {
//                var savedTagIndex = data[i].IndexOf(" ", StringComparison.Ordinal);

//                if (savedTagIndex != -1)
//                {
//                    var t = data[i].Remove(savedTagIndex);

//                    if (t == tag)
//                    {
//                        data.RemoveAt(i);

//                        break;
//                    }
//                }
//                else
//                {
//                    if (data[i] == tag)
//                    {
//                        data.RemoveAt(i);

//                        break;
//                    }
//                }
//            }

//            File.WriteAllLines(Path.Combine(DirectoryTools.GetBadTagsFolder(), site, "BadTags.txt"), data);
//        }
//    }
//}
