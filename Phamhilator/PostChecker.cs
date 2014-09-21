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

			if (IsLowQuality(post, ref info))
			{
				info.Type = PostType.LowQuality;
			}
			else if (IsSpam(post, ref info))
			{
				info.Type = PostType.Spam;
			}
			else if (IsOffensive(post, ref info))
			{
				info.Type = PostType.Offensive;
			}
			else if (IsBadUsername(post, ref info))
			{
				info.Type = PostType.BadUsername;
			}

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
						info.Score -= whiteFilter.Value;
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
					info.Score += GlobalInfo.BlackSpam.Terms[filter];
					info.TermsFound.Add(filter);

					filtersUsed++;
				}
			} 
			
			if (filtersUsed != 0)
			{
				info.Score /= filtersUsed;
				info.Score /= GlobalInfo.BlackSpam.HighestScore;
				info.Score *= 100;

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
						info.Score -= whiteFilter.Value;
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
					info.Score += GlobalInfo.BlackLQ.Terms[filter];
					info.TermsFound.Add(filter);

					filtersUsed++;
				}
			}

			if (filtersUsed != 0)
			{
				info.Score /= filtersUsed;
				info.Score /= GlobalInfo.BlackLQ.HighestScore;
				info.Score *= 100;

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
						info.Score -= whiteFilter.Value;
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
					info.Score += GlobalInfo.BlackOff.Terms[filter];
					info.TermsFound.Add(filter);

					filtersUsed++;
				}
			}

			if (filtersUsed != 0)
			{
				info.Score /= filtersUsed;
				info.Score /= GlobalInfo.BlackOff.HighestScore;
				info.Score *= 100;

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
						info.Score -= whiteFilter.Value;
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
					info.Score += GlobalInfo.BlackName.Terms[filter];
					info.TermsFound.Add(filter);

					filtersUsed++;
				}
			}

			if (filtersUsed != 0)
			{
				info.Score /= filtersUsed;
				info.Score /= GlobalInfo.BlackName.HighestScore;
				info.Score *= 100;

				return true;
			}

			// Otherwise, if no terms were found, assume the post is clean.

			return false;
		}

		private static Dictionary<string, string> IsBadTagUsed(Post post)
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

			return tags;
		}
	}
}
