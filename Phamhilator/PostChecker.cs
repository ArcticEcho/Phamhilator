using System.Linq;
using System.Collections.Generic;



namespace Phamhilator
{
	public static class PostChecker
	{
		public static PostTypeInfo CheckPost(Post post)
		{
			var info = new PostTypeInfo();

			if ((info.BadTags = IsBadTagUsed(post, ref info)).Count != 0)
			{
				return info;
			}

			if (IsSpam(post, ref info))
			{
				return info;
			}

			if (IsLowQuality(post, ref info))
			{
				return info;
			}

			if (IsOffensive(post, ref info))
			{
				return info;
			}

			IsBadUsername(post, ref info);

			return info;
		}

		private static bool IsSpam(Post post, ref PostTypeInfo info)
		{
			var filtersUsed = 0;

			// Loop over whitelist.

			if (GlobalInfo.WhiteSpam.Terms.ContainsKey(post.Site))
			{
				foreach (var whiteFilter in GlobalInfo.WhiteSpam.Terms[post.Site])
				{
					if (whiteFilter.Key.IsMatch(post.Title))
					{
						info.Accuracy -= whiteFilter.Value;
						info.TermsFound.Add(whiteFilter.Key);
						filtersUsed++;
					}
				}
			}

			// Loop over blacklist.

			for (var i = 0; i < GlobalInfo.BlackSpam.Terms.Count; i++)
			{
				var filter = GlobalInfo.BlackSpam.Terms.Keys.ElementAt(i);

				if (filter.IsMatch(post.Title))
				{
					info.Accuracy += GlobalInfo.BlackSpam.Terms[filter];
					info.TermsFound.Add(filter);

					filtersUsed++;
				}
			} 
			
			if (filtersUsed != 0)
			{
				info.Accuracy /= filtersUsed;
				info.Accuracy /= GlobalInfo.BlackSpam.HighestScore;
				info.Accuracy *= 100;
				info.Type = PostType.Spam;

				return true;
			}

			// Otherwise, if no terms were found, assume the post is clean.

			return false;
		}

		private static bool IsLowQuality(Post post, ref PostTypeInfo info)
		{
			var filtersUsed = 0;

			// Loop over whitelist.

			if (GlobalInfo.WhiteLQ.Terms.ContainsKey(post.Site))
			{
				foreach (var whiteFilter in GlobalInfo.WhiteLQ.Terms[post.Site])
				{
					if (whiteFilter.Key.IsMatch(post.Title))
					{
						info.Accuracy -= whiteFilter.Value;
						info.TermsFound.Add(whiteFilter.Key);
						filtersUsed++;
					}
				}
			}

			// Loop over blacklist.

			for (var i = 0; i < GlobalInfo.BlackLQ.Terms.Count; i++)
			{
				var filter = GlobalInfo.BlackLQ.Terms.Keys.ElementAt(i);

				if (filter.IsMatch(post.Title))
				{
					info.Accuracy += GlobalInfo.BlackLQ.Terms[filter];
					info.TermsFound.Add(filter);

					filtersUsed++;
				}
			}

			if (filtersUsed != 0)
			{
				info.Accuracy /= filtersUsed;
				info.Accuracy /= GlobalInfo.BlackLQ.HighestScore;
				info.Accuracy *= 100;
				info.Type = PostType.LowQuality;

				return true;
			}

			// Otherwise, if no terms were found, assume the post is clean.

			return false;
		}

		private static bool IsOffensive(Post post, ref PostTypeInfo info)
		{
			var filtersUsed = 0;

			// Loop over whitelist.

			if (GlobalInfo.WhiteOff.Terms.ContainsKey(post.Site))
			{
				foreach (var whiteFilter in GlobalInfo.WhiteOff.Terms[post.Site])
				{
					if (whiteFilter.Key.IsMatch(post.Title))
					{
						info.Accuracy -= whiteFilter.Value;
						info.TermsFound.Add(whiteFilter.Key);
						filtersUsed++;
					}
				}
			}

			filtersUsed = 0;

			// Loop over blacklist.

			for (var i = 0; i < GlobalInfo.BlackOff.Terms.Count; i++)
			{
				var filter = GlobalInfo.BlackOff.Terms.Keys.ElementAt(i);

				if (filter.IsMatch(post.Title))
				{
					info.Accuracy += GlobalInfo.BlackOff.Terms[filter];
					info.TermsFound.Add(filter);

					filtersUsed++;
				}
			}

			if (filtersUsed != 0)
			{
				info.Accuracy /= filtersUsed;
				info.Accuracy /= GlobalInfo.BlackOff.HighestScore;
				info.Accuracy *= 100;
				info.Type = PostType.Offensive;

				return true;
			}

			// Otherwise, if no terms were found, assume the post is clean.

			return false;
		}

		private static bool IsBadUsername(Post post, ref PostTypeInfo info)
		{
			var filtersUsed = 0;

			// Loop over whitelist.

			if (GlobalInfo.WhiteName.Terms.ContainsKey(post.Site))
			{
				foreach (var whiteFilter in GlobalInfo.WhiteName.Terms[post.Site])
				{
					if (whiteFilter.Key.IsMatch(post.Title))
					{
						info.Accuracy -= whiteFilter.Value;
						info.TermsFound.Add(whiteFilter.Key);
						filtersUsed++;
					}
				}
			}

			filtersUsed = 0;

			// Loop over blacklist.

			for (var i = 0; i < GlobalInfo.BlackName.Terms.Count; i++)
			{
				var filter = GlobalInfo.BlackName.Terms.Keys.ElementAt(i);

				if (filter.IsMatch(post.Title))
				{
					info.Accuracy += GlobalInfo.BlackName.Terms[filter];
					info.TermsFound.Add(filter);

					filtersUsed++;
				}
			}

			if (filtersUsed != 0)
			{
				info.Accuracy /= filtersUsed;
				info.Accuracy /= GlobalInfo.BlackName.HighestScore;
				info.Accuracy *= 100;
				info.Type = PostType.BadUsername;

				return true;
			}

			// Otherwise, if no terms were found, assume the post is clean.

			return false;
		}

		private static Dictionary<string, string> IsBadTagUsed(Post post, ref PostTypeInfo info)
		{
			var tags = new Dictionary<string, string>();

			if (!BadTagDefinitions.BadTags.Keys.Contains(post.Site)) { return tags; }

			foreach (var tag in post.Tags)
			{
				if (BadTagDefinitions.BadTags[post.Site].ContainsKey(tag.ToLowerInvariant()))
				{
					tags.Add(tag, BadTagDefinitions.BadTags[post.Site][tag]);
				}
			}

			if (tags.Count != 0)
			{
				info.Accuracy = 100;
				info.Type = PostType.BadTagUsed;
			}

			return tags;
		}
	}
}
