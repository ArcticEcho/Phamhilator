using System.Linq;
using System.Collections.Generic;



namespace Phamhilator
{
	public static class PostChecker
	{
		public static PostAnalysis CheckPost(Post post)
		{
			var info = new PostAnalysis();

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

			// TODO: Returning false positives, even when name dose not contain any blacklisted name terms.

			//IsBadUsername(post, ref info);

			return info;
		}

		private static bool IsSpam(Post post, ref PostAnalysis info)
		{
			var filtersUsed = 0;

			// Loop over whitelist.

			if (GlobalInfo.WhiteSpam.Terms.ContainsKey(post.Site))
			{
				foreach (var whiteTerm in GlobalInfo.WhiteSpam.Terms[post.Site])
				{
					if (whiteTerm.Key.IsMatch(post.Title))
					{
						info.Accuracy -= whiteTerm.Value;
						info.WhiteTermsFound.Add(whiteTerm.Key, whiteTerm.Value);
						filtersUsed++;
					}
				}
			}

			// Loop over blacklist.

			foreach (var blackTerm in GlobalInfo.BlackSpam.Terms)
			{
				if (blackTerm.Key.IsMatch(post.Title))
				{
					info.Accuracy += blackTerm.Value;
					info.BlackTermsFound.Add(blackTerm.Key, blackTerm.Value);

					filtersUsed++;
				}
			}

			//for (var i = 0; i < GlobalInfo.BlackSpam.Terms.Count; i++)
			//{				
				//var filter = GlobalInfo.BlackSpam.Terms.Keys.ElementAt(i);

				//if (filter.IsMatch(post.Title))
				//{
				//	info.Accuracy += GlobalInfo.BlackSpam.Terms[filter];
				//	info.BlackTermsFound.Add(filter);

				//	filtersUsed++;
				//}
			//} 
			
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

		private static bool IsLowQuality(Post post, ref PostAnalysis info)
		{
			var filtersUsed = 0;

			// Loop over whitelist.

			if (GlobalInfo.WhiteLQ.Terms.ContainsKey(post.Site))
			{
				foreach (var whiteTerm in GlobalInfo.WhiteLQ.Terms[post.Site])
				{
					if (whiteTerm.Key.IsMatch(post.Title))
					{
						info.Accuracy -= whiteTerm.Value;
						info.WhiteTermsFound.Add(whiteTerm.Key, whiteTerm.Value);
						filtersUsed++;
					}
				}
			}

			// Loop over blacklist.

			foreach (var blackTerm in GlobalInfo.BlackLQ.Terms)
			{
				if (blackTerm.Key.IsMatch(post.Title))
				{
					info.Accuracy += blackTerm.Value;
					info.BlackTermsFound.Add(blackTerm.Key, blackTerm.Value);

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

		private static bool IsOffensive(Post post, ref PostAnalysis info)
		{
			var filtersUsed = 0;

			// Loop over whitelist.

			if (GlobalInfo.WhiteOff.Terms.ContainsKey(post.Site))
			{
				foreach (var whiteTerm in GlobalInfo.WhiteOff.Terms[post.Site])
				{
					if (whiteTerm.Key.IsMatch(post.Title))
					{
						info.Accuracy -= whiteTerm.Value;
						info.WhiteTermsFound.Add(whiteTerm.Key, whiteTerm.Value);
						filtersUsed++;
					}
				}
			}

			// Loop over blacklist.

			foreach (var blackTerm in GlobalInfo.BlackOff.Terms)
			{
				if (blackTerm.Key.IsMatch(post.Title))
				{
					info.Accuracy += blackTerm.Value;
					info.BlackTermsFound.Add(blackTerm.Key, blackTerm.Value);

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

		private static bool IsBadUsername(Post post, ref PostAnalysis info)
		{
			var filtersUsed = 0;

			// Loop over whitelist.

			if (GlobalInfo.WhiteName.Terms.ContainsKey(post.Site))
			{
				foreach (var whiteTerm in GlobalInfo.WhiteName.Terms[post.Site])
				{
					if (whiteTerm.Key.IsMatch(post.Title))
					{
						info.Accuracy -= whiteTerm.Value;
						info.WhiteTermsFound.Add(whiteTerm.Key, whiteTerm.Value);
						filtersUsed++;
					}
				}
			}

			// Loop over blacklist.

			foreach (var blackTerm in GlobalInfo.BlackName.Terms)
			{
				if (blackTerm.Key.IsMatch(post.Title))
				{
					info.Accuracy += blackTerm.Value;
					info.BlackTermsFound.Add(blackTerm.Key, blackTerm.Value);

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

		private static Dictionary<string, string> IsBadTagUsed(Post post, ref PostAnalysis info)
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
