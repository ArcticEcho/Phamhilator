using System;
using System.Collections.Generic;
using System.IO;



namespace Phamhilator
{
	public static class PostPersistence
	{
		private static List<Post> messages;
		private static readonly DateTime twentyTen = new DateTime(2010, 01, 01);

		public static List<Post> Messages
		{
			get
			{
				if (messages == null)
				{
					PopulateMessages();
				}

				return messages;
			}
		}



		public static void AddPost(Post post)
		{
			if (Messages.Count == 0)
			{
				Messages.Add(post);
			}
			else
			{
				Messages.Insert(0, post);
			}

			File.AppendAllText(DirectoryTools.GetPostPersitenceFile(), (DateTime.Now - twentyTen).TotalMinutes + "]" + post.Title + "\n");
			GlobalInfo.PostsCaught++;
		}



		private static void PopulateMessages()
		{
			if (!File.Exists(DirectoryTools.GetPostPersitenceFile())) { return; }

			var titles = new List<string>(File.ReadAllLines(DirectoryTools.GetPostPersitenceFile()));
			double date;

			messages = new List<Post>();

			for (var i = 0; i < titles.Count; i++)
			{
				var dateString = titles[i].Split(']')[0].Trim();

				if (dateString == "")
				{
					continue;
				}

				date = double.Parse(dateString);

				if ((DateTime.Now - twentyTen).TotalMinutes - date > 2880)
				{
					titles.Remove(titles[i]);

					continue;
				}

				messages.Add(new Post { Title = titles[i].Split(']')[1].Trim() });
				GlobalInfo.PostsCaught++;
			}

			File.WriteAllText(DirectoryTools.GetPostPersitenceFile(), "");

			foreach (var post in titles)
			{
				if (!String.IsNullOrEmpty(post))
				{
					File.AppendAllText(DirectoryTools.GetPostPersitenceFile(), post + "\n");
				}
			}
		}
	}
}
