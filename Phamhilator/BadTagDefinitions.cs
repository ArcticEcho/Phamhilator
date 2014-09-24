using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;



namespace Phamhilator
{
	public static class BadTagDefinitions
	{
		private static Dictionary<string, Dictionary<string, string>> badTags;

		public static Dictionary<string, Dictionary<string, string>> BadTags
		{
			get
			{
				if (badTags == null)
				{
					PopulateBadTags();
				}

				return badTags;
			}
		}



		public static void AddTag(string site, string tag, string metaPost = "")
		{
			if (badTags.ContainsKey(site))
			{
				badTags[site].Add(tag, metaPost);

				File.AppendAllText(Path.Combine(DirectoryTools.GetBTDFolder(), site, "BadTags.txt"), "\n" + tag + (metaPost == "" ? "" : " " + metaPost));
			}
			else
			{
				var path = Path.Combine(DirectoryTools.GetBTDFolder(), site);

				Directory.CreateDirectory(path);

				badTags.Add(site, new Dictionary<string, string> { { tag, metaPost } });

				File.AppendAllText(Path.Combine(DirectoryTools.GetBTDFolder(), site, "BadTags.txt"), "\n" + tag + (metaPost == "" ? "" : " " + metaPost));
			}
		}

		public static void RemoveTag(string site, string tag)
		{
			if (!badTags.ContainsKey(site) || !badTags[site].ContainsKey(tag)) { return; }

			badTags[site].Remove(tag);

			var data = File.ReadAllLines(Path.Combine(DirectoryTools.GetBTDFolder(), site, "BadTags.txt")).ToList();

			for (var i = 0; i < data.Count; i++)
			{
				var g = data[i].IndexOf(" ", StringComparison.Ordinal);

				if (data[i].IndexOf(" ", StringComparison.Ordinal) != -1)
				{
					var t = data[i].Remove(data[i].IndexOf(" ", StringComparison.Ordinal));

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

			File.WriteAllLines(Path.Combine(DirectoryTools.GetBTDFolder(), site, "BadTags.txt"), data);
		}



		private static void PopulateBadTags()
		{
			badTags = new Dictionary<string, Dictionary<string, string>>();

			foreach (var dir in Directory.EnumerateDirectories(DirectoryTools.GetBTDFolder()))
			{
				var site = Path.GetFileName(dir);

				badTags.Add(site, new Dictionary<string, string>());

				//if (badTags.ContainsKey(key)) { continue; }

				var lines = File.ReadAllLines(Path.Combine(dir, "BadTags.txt")).ToArray();
				var metaPost = "";
				var tag = "";

				for (var i = 0; i < lines.Length; i++)
				{
					lines[i] = lines[i].Trim();

					if (!String.IsNullOrWhiteSpace(lines[i]))
					{
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

						if (!badTags[site].ContainsKey(tag))
						{
							badTags[site].Add(tag, metaPost);	
						}
					}
				}
			}
		}
	}
}
