using System.Collections.Generic;
using System.Linq;



namespace Phamhilator.Analysers
{
	public static class QuestionTitle
	{
		public static bool IsSpam(Question post, ref QuestionAnalysis info)
		{
			var filtersUsed = 0;

			// Loop over blacklist.

			foreach (var blackTerm in GlobalInfo.BlackFilters[FilterType.QuestionTitleBlackSpam].Terms)
			{
				if (blackTerm.Key.IsMatch(post.Title))
				{
					info.Accuracy += blackTerm.Value;
					info.BlackTermsFound.Add(blackTerm.Key, blackTerm.Value);

					filtersUsed++;
				}
			}

			// Otherwise, if no blacklist terms were found, assume the post is clean.

			if (filtersUsed == 0) { return false; }

			// Loop over whitelist.

			if (GlobalInfo.WhiteFilters[FilterType.QuestionTitleWhiteSpam].Terms.ContainsKey(post.Site))
			{
				foreach (var whiteTerm in GlobalInfo.WhiteFilters[FilterType.QuestionTitleWhiteSpam].Terms[post.Site])
				{
					if (whiteTerm.Key.IsMatch(post.Title))
					{
						info.Accuracy -= whiteTerm.Value;
						info.WhiteTermsFound.Add(whiteTerm.Key, whiteTerm.Value);
						filtersUsed++;
					}
				}
			}
			
			info.Accuracy /= filtersUsed;
			info.Accuracy /= GlobalInfo.BlackFilters[FilterType.QuestionTitleBlackSpam].HighestScore;
			info.Accuracy *= 100;
			info.Type = PostType.Spam;

			return true;
		}

		public static bool IsLowQuality(Question post, ref QuestionAnalysis info)
		{
			var filtersUsed = 0;

			// Loop over blacklist.

			foreach (var blackTerm in GlobalInfo.BlackFilters[FilterType.QuestionTitleBlackLQ].Terms)
			{
				if (blackTerm.Key.IsMatch(post.Title))
				{
					info.Accuracy += blackTerm.Value;
					info.BlackTermsFound.Add(blackTerm.Key, blackTerm.Value);

					filtersUsed++;
				}
			}

			// Otherwise, if no blacklist terms were found, assume the post is clean.

			if (filtersUsed == 0) { return false; }

			// Loop over whitelist.

			if (GlobalInfo.WhiteFilters[FilterType.QuestionTitleWhiteLQ].Terms.ContainsKey(post.Site))
			{
				foreach (var whiteTerm in GlobalInfo.WhiteFilters[FilterType.QuestionTitleWhiteLQ].Terms[post.Site])
				{
					if (whiteTerm.Key.IsMatch(post.Title))
					{
						info.Accuracy -= whiteTerm.Value;
						info.WhiteTermsFound.Add(whiteTerm.Key, whiteTerm.Value);
						filtersUsed++;
					}
				}
			}

			info.Accuracy /= filtersUsed;
			info.Accuracy /= GlobalInfo.BlackFilters[FilterType.QuestionTitleBlackLQ].HighestScore;
			info.Accuracy *= 100;
			info.Type = PostType.LowQuality;

			return true;
		}

		public static bool IsOffensive(Question post, ref QuestionAnalysis info)
		{
			var filtersUsed = 0;

			// Loop over blacklist.

			foreach (var blackTerm in GlobalInfo.BlackFilters[FilterType.QuestionTitleBlackOff].Terms)
			{
				if (blackTerm.Key.IsMatch(post.Title))
				{
					info.Accuracy += blackTerm.Value;
					info.BlackTermsFound.Add(blackTerm.Key, blackTerm.Value);

					filtersUsed++;
				}
			}

			// Otherwise, if no blacklist terms were found, assume the post is clean.

			if (filtersUsed == 0) { return false; }

			// Loop over whitelist.

			if (GlobalInfo.WhiteFilters[FilterType.QuestionTitleWhiteOff].Terms.ContainsKey(post.Site))
			{
				foreach (var whiteTerm in GlobalInfo.WhiteFilters[FilterType.QuestionTitleWhiteOff].Terms[post.Site])
				{
					if (whiteTerm.Key.IsMatch(post.Title))
					{
						info.Accuracy -= whiteTerm.Value;
						info.WhiteTermsFound.Add(whiteTerm.Key, whiteTerm.Value);
						filtersUsed++;
					}
				}
			}

			info.Accuracy /= filtersUsed;
			info.Accuracy /= GlobalInfo.BlackFilters[FilterType.QuestionTitleBlackOff].HighestScore;
			info.Accuracy *= 100;
			info.Type = PostType.Offensive;

			return true;
		}

		public static bool IsBadUsername(Question post, ref QuestionAnalysis info)
		{
			var filtersUsed = 0;

			// Loop over blacklist.

			foreach (var blackTerm in GlobalInfo.BlackFilters[FilterType.QuestionTitleBlackName].Terms)
			{
				if (blackTerm.Key.IsMatch(post.AuthorName))
				{
					info.Accuracy += blackTerm.Value;
					info.BlackTermsFound.Add(blackTerm.Key, blackTerm.Value);

					filtersUsed++;
				}
			}

			// Otherwise, if no blacklist terms were found, assume the post is clean.

			if (filtersUsed == 0) { return false; }

			// Loop over whitelist.

			if (GlobalInfo.WhiteFilters[FilterType.QuestionTitleWhiteName].Terms.ContainsKey(post.Site))
			{
				foreach (var whiteTerm in GlobalInfo.WhiteFilters[FilterType.QuestionTitleWhiteName].Terms[post.Site])
				{
					if (whiteTerm.Key.IsMatch(post.AuthorName))
					{
						info.Accuracy -= whiteTerm.Value;
						info.WhiteTermsFound.Add(whiteTerm.Key, whiteTerm.Value);
						filtersUsed++;
					}
				}
			}

			info.Accuracy /= filtersUsed;
			info.Accuracy /= GlobalInfo.BlackFilters[FilterType.QuestionTitleBlackName].HighestScore;
			info.Accuracy *= 100;
			info.Type = PostType.BadUsername;

			return true;
		}

		public static Dictionary<string, string> IsBadTagUsed(Question post, ref QuestionAnalysis info)
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
