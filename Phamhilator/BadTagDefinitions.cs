using System.Collections.Generic;
using System.IO;
using System.Linq;



namespace Phamhilator
{
	public static class BadTagDefinitions
	{
		private static Dictionary<string, HashSet<string>> badTags;

		public static Dictionary<string, HashSet<string>> BadTags
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



		public static void AddTag(string site, string tag)
		{
			if (badTags.ContainsKey(site))
			{
				badTags[site].Add(tag);

				File.AppendAllText(Path.Combine(DirectoryTools.GetBTDFolder(), site, "BadTags.txt"), tag);
			}
			else
			{
				var path = Path.Combine(DirectoryTools.GetBTDFolder(), site);

				Directory.CreateDirectory(path);

				badTags.Add(site, new HashSet<string> { tag });

				File.AppendAllText(Path.Combine(DirectoryTools.GetBTDFolder(), site, "BadTags.txt"), tag);
			}
		}

		public static void RemoveTag(string site, string tag)
		{
			if (!badTags.ContainsKey(site) || !badTags[site].Contains(tag)) { return; }

			badTags[site].Remove(tag);

			var data = File.ReadAllLines(Path.Combine(DirectoryTools.GetBTDFolder(), site, "BadTags.txt")).ToList();

			for (var i = 0; i < data.Count; i++)
			{
				if (data[i] == tag)
				{
					data.RemoveAt(i);

					break;
				}
			}

			File.WriteAllLines(Path.Combine(DirectoryTools.GetBTDFolder(), site, "BadTags.txt"), data);
		}



		private static void PopulateBadTags()
		{
			var tags = new List<string>();
			badTags = new Dictionary<string, HashSet<string>>();

			foreach (var dir in Directory.EnumerateDirectories(DirectoryTools.GetBTDFolder()))
			{
				var key = Path.GetFileName(dir);

				if (badTags.ContainsKey(key)) { continue; }

				tags.AddRange(File.ReadAllLines(Path.Combine(dir, "BadTags.txt")));

				for (var i = 0; i < tags.Count; i++)
				{
					tags[i] = tags[i].Trim();

					if (tags[i] == "")
					{
						tags.RemoveAt(i);
					}
				}

				badTags.Add(key, new HashSet<string>(tags));

				tags.Clear();
			}
		}
	}
}
