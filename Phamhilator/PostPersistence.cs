using System;
using System.Collections.Generic;
using System.IO;



namespace Phamhilator
{
	public static class PostPersistence
	{
		private static bool initialised;
		private static readonly List<string> messages = new List<string>();
		private static readonly DateTime twentyTen = new DateTime(2010, 01, 01);

		public static List<string> Messages
		{
			get
			{
				if (!initialised)
				{
					lock (messages)
					{
						if (messages.Count == 0)
						{
							Initialise();
						}
					}
				}			

				return messages;
			}
		}



		public static void Initialise()
		{
			if (!File.Exists(DirectoryTools.GetPostPersitenceFile()) || initialised) { return; }

			initialised = true;

			var urls = new List<string>(File.ReadAllLines(DirectoryTools.GetPostPersitenceFile()));

			for (var i = 0; i < urls.Count; i++)
			{
				var dateString = urls[i].Split(']')[0].Trim();

				if (dateString == "") { continue; }

				var date = double.Parse(dateString);

				if ((DateTime.Now - twentyTen).TotalMinutes - date > 10080) // Remove posts older than 1 week
				{
					urls.Remove(urls[i]);

					continue;
				}

				messages.Add(urls[i].Split(']')[1].Trim());

				GlobalInfo.PostsCaught++;
			}

			File.WriteAllText(DirectoryTools.GetPostPersitenceFile(), "");

			foreach (var post in urls)
			{
				if (!String.IsNullOrEmpty(post))
				{
					File.AppendAllText(DirectoryTools.GetPostPersitenceFile(), Environment.NewLine + post);
				}
			}
		}

		public static void AddPost(string url)
		{
			if (messages.Contains(url)) { return; }

			if (Messages.Count == 0)
			{
				Messages.Add(url);
			}
			else
			{
				Messages.Insert(0, url);
			}

			File.AppendAllText(DirectoryTools.GetPostPersitenceFile(), Environment.NewLine + (DateTime.Now - twentyTen).TotalMinutes + "]" + url);
			
			GlobalInfo.PostsCaught++;
		}
	}
}
