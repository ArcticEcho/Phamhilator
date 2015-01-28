using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;



namespace Phamhilator
{
    public class BadTags
    {
        public Dictionary<string, Dictionary<string, string>> Tags { get; private set; }



        public BadTags()
        {
            Tags = new Dictionary<string, Dictionary<string, string>>();

            foreach (var dir in Directory.EnumerateDirectories(DirectoryTools.GetBadTagsFolder()))
            {
                var site = Path.GetFileName(dir);

                if (String.IsNullOrEmpty(site) || Tags.ContainsKey(site)) { continue; }

                Tags.Add(site, new Dictionary<string, string>());

                var lines = File.ReadAllLines(Path.Combine(dir, "BadTags.txt")).ToArray();

                for (var i = 0; i < lines.Length; i++)
                {
                    lines[i] = lines[i].Trim();

                    if (!String.IsNullOrWhiteSpace(lines[i]))
                    {
                        string metaPost;
                        string tag;

                        if (lines[i].IndexOf(" ", StringComparison.Ordinal) != -1) // Check if tag has meta post
                        {
                            tag = lines[i].Substring(0, lines[i].IndexOf(" ", StringComparison.Ordinal));
                            metaPost = lines[i].Remove(0, lines[i].IndexOf(" ", StringComparison.Ordinal) + 1);
                        }
                        else
                        {
                            tag = lines[i];
                            metaPost = "";
                        }

                        if (!Tags[site].ContainsKey(tag))
                        {
                            Tags[site].Add(tag, metaPost);
                        }
                    }
                }
            }
        }

        public void AddTag(string site, string tag, string metaPost = "")
        {
            if (Tags.ContainsKey(site))
            {
                Tags[site].Add(tag, metaPost);

                File.AppendAllText(Path.Combine(DirectoryTools.GetBadTagsFolder(), site, "BadTags.txt"), Environment.NewLine + tag + (metaPost == "" ? "" : " " + metaPost));
            }
            else
            {
                var path = Path.Combine(DirectoryTools.GetBadTagsFolder(), site);

                Directory.CreateDirectory(path);

                Tags.Add(site, new Dictionary<string, string> { { tag, metaPost } });

                File.AppendAllText(Path.Combine(DirectoryTools.GetBadTagsFolder(), site, "BadTags.txt"), Environment.NewLine + tag + (metaPost == "" ? "" : " " + metaPost));
            }
        }

        public void RemoveTag(string site, string tag)
        {
            if (!Tags.ContainsKey(site) || !Tags[site].ContainsKey(tag)) { return; }

            Tags[site].Remove(tag);

            var data = File.ReadAllLines(Path.Combine(DirectoryTools.GetBadTagsFolder(), site, "BadTags.txt")).ToList();

            for (var i = 0; i < data.Count; i++)
            {
                var savedTagIndex = data[i].IndexOf(" ", StringComparison.Ordinal);

                if (savedTagIndex != -1)
                {
                    var t = data[i].Remove(savedTagIndex);

                    if (t == tag)
                    {
                        data.RemoveAt(i);

                        break;
                    }
                }
                else
                {
                    if (data[i] == tag)
                    {
                        data.RemoveAt(i);

                        break;
                    }
                }
            }

            File.WriteAllLines(Path.Combine(DirectoryTools.GetBadTagsFolder(), site, "BadTags.txt"), data);
        }
    }
}
