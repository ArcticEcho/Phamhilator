using System.Linq;
using System.Collections.Generic;



namespace Phamhilator
{
	public static class PostChecker
	{
		public static PostTypeInfo CheckPost(Post post)
		{
			var info = new PostTypeInfo();

			if ((info.BadTags = IsBadTagUsed(post)).Count != 0)
			{
				info.Type = PostType.BadTagUsed;

				return info;
			}

			//if (post.Score >= 2)
			//{
			//	return info;
			//}

			if (IsLowQuality(post, ref info))
			{
				info.Type = PostType.LowQuality;

				info.Accuracy = (info.Accuracy / GlobalInfo.LQ.HighestScore) * 100;
			}
			else if ((info.BadTags = IsBadTagUsed(post)).Count != 0)
			{
				info.Type = PostType.BadTagUsed;
			}
			else if (IsSpam(post, ref info))
			{
				info.Type = PostType.Spam;

				info.Accuracy = (info.Accuracy / GlobalInfo.Spam.HighestScore) * 100;
			}
			else if (IsOffensive(post, ref info))
			{
				info.Type = PostType.Offensive;

				info.Accuracy = (info.Accuracy / GlobalInfo.Off.HighestScore) * 100;
			}
			else if (IsBadUsername(post, ref info))
			{
				info.Type = PostType.BadUsername;

				info.Accuracy = (info.Accuracy / GlobalInfo.Name.HighestScore) * 100;
			}

			return info;
		}

		private static bool IsSpam(Post post, ref PostTypeInfo info)
		{
			foreach (var ignoreFilter in GlobalInfo.IgnoreSpam.Terms)
			{
				if (ignoreFilter.Key.IsMatch(post.Title) && post.Site.StartsWith(ignoreFilter.Value))
				{
					return false;
				}
			}

			var filtersUsed = 0;

			for (var i = 0; i < GlobalInfo.Spam.Terms.Count; i++)
			{
				var filter = GlobalInfo.Spam.Terms.Keys.ElementAt(i);

				if (filter.IsMatch(post.Title))
				{
					info.Accuracy += GlobalInfo.Spam.Terms[filter];
					info.TermsFound.Add(filter);

					filtersUsed++;
				}

				if (i == GlobalInfo.Spam.Terms.Count - 1 && filtersUsed > 0)
				{
					info.Accuracy /= filtersUsed;

					return true;
				}
			}

			return false;
		}

		private static bool IsLowQuality(Post post, ref PostTypeInfo info)
		{
			foreach (var ignoreFilter in GlobalInfo.IgnoreLQ.Terms)
			{
				if (ignoreFilter.Key.IsMatch(post.Title) && post.Site.StartsWith(ignoreFilter.Value))
				{
					return false;
				}
			}

			var filtersUsed = 0;

			for (var i = 0; i < GlobalInfo.LQ.Terms.Count; i++)
			{
				var filter = GlobalInfo.LQ.Terms.Keys.ElementAt(i);

				if (filter.IsMatch(post.Title))
				{
					info.Accuracy += GlobalInfo.LQ.Terms[filter];
					info.TermsFound.Add(filter);

					filtersUsed++;
				}

				if (i == GlobalInfo.LQ.Terms.Count - 1 && filtersUsed > 0)
				{
					info.Accuracy /= filtersUsed;

					return true;
				}
			}

			return false;
		}

		private static bool IsOffensive(Post post, ref PostTypeInfo info)
		{
			foreach (var ignoreFilter in GlobalInfo.IgnoreOff.Terms)
			{
				if (ignoreFilter.Key.IsMatch(post.Title) && post.Site.StartsWith(ignoreFilter.Value))
				{
					return false;
				}
			}

			var filtersUsed = 0;

			for (var i = 0; i < GlobalInfo.Off.Terms.Count; i++)
			{
				var filter = GlobalInfo.Off.Terms.Keys.ElementAt(i);

				if (filter.IsMatch(post.Title))
				{
					info.Accuracy += GlobalInfo.Off.Terms[filter];
					info.TermsFound.Add(filter);

					filtersUsed++;
				}

				if (i == GlobalInfo.Off.Terms.Count - 1 && filtersUsed > 0)
				{
					info.Accuracy /= filtersUsed;

					return true;
				}
			}

			return false;
		}

		private static bool IsBadUsername(Post post, ref PostTypeInfo info)
		{
			foreach (var ignoreFilter in GlobalInfo.IgnoreName.Terms)
			{
				if (ignoreFilter.Key.IsMatch(post.Title) && post.Site.StartsWith(ignoreFilter.Value))
				{
					return false;
				}
			}

			var filtersUsed = 0;

			for (var i = 0; i < GlobalInfo.Name.Terms.Count; i++)
			{
				var filter = GlobalInfo.Name.Terms.Keys.ElementAt(i);

				if (filter.IsMatch(post.Title))
				{
					info.Accuracy += GlobalInfo.Name.Terms[filter];
					info.TermsFound.Add(filter);

					filtersUsed++;
				}

				if (i == GlobalInfo.Name.Terms.Count - 1 && filtersUsed > 0)
				{
					info.Accuracy /= filtersUsed;

					return true;
				}
			}

			return false;
		}

		private static List<string> IsBadTagUsed(Post post)
		{
			var tags = new List<string>();

			if (!BadTagDefinitions.BadTags.Keys.Contains(post.Site)) { return tags; }

			foreach (var tag in post.Tags)
			{
				if (BadTagDefinitions.BadTags[post.Site].Contains(tag.ToLowerInvariant()))
				{
					tags.Add(tag);
				}
			}

			return tags;
		}
	}
}
