using System.Collections.Generic;
using System.Linq;



namespace Phamhilator
{
	namespace Analyser
	{
		public static class QuestionAnalyser
		{
			public static bool IsSpam(Question post, ref QuestionAnalysis info)
			{
				var filtersUsed = 0;

				// Loop over whitelist.

				if (GlobalInfo.QWhiteSpam.Terms.ContainsKey(post.Site))
				{
					foreach (var whiteTerm in GlobalInfo.QWhiteSpam.Terms[post.Site])
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

				foreach (var blackTerm in GlobalInfo.QBlackSpam.Terms)
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
					info.Accuracy /= GlobalInfo.QBlackSpam.HighestScore;
					info.Accuracy *= 100;
					info.Type = PostType.Spam;

					return true;
				}

				// Otherwise, if no terms were found, assume the post is clean.

				return false;
			}

			public static bool IsLowQuality(Question post, ref QuestionAnalysis info)
			{
				var filtersUsed = 0;

				// Loop over whitelist.

				if (GlobalInfo.QWhiteLQ.Terms.ContainsKey(post.Site))
				{
					foreach (var whiteTerm in GlobalInfo.QWhiteLQ.Terms[post.Site])
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

				foreach (var blackTerm in GlobalInfo.QBlackLQ.Terms)
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
					info.Accuracy /= GlobalInfo.QBlackLQ.HighestScore;
					info.Accuracy *= 100;
					info.Type = PostType.LowQuality;

					return true;
				}

				// Otherwise, if no terms were found, assume the post is clean.

				return false;
			}

			public static bool IsOffensive(Question post, ref QuestionAnalysis info)
			{
				var filtersUsed = 0;

				// Loop over whitelist.

				if (GlobalInfo.QWhiteOff.Terms.ContainsKey(post.Site))
				{
					foreach (var whiteTerm in GlobalInfo.QWhiteOff.Terms[post.Site])
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

				foreach (var blackTerm in GlobalInfo.QBlackOff.Terms)
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
					info.Accuracy /= GlobalInfo.QBlackOff.HighestScore;
					info.Accuracy *= 100;
					info.Type = PostType.Offensive;

					return true;
				}

				// Otherwise, if no terms were found, assume the post is clean.

				return false;
			}

			public static bool IsBadUsername(Question post, ref QuestionAnalysis info)
			{
				var filtersUsed = 0;

				// Loop over whitelist.

				if (GlobalInfo.QWhiteName.Terms.ContainsKey(post.Site))
				{
					foreach (var whiteTerm in GlobalInfo.QWhiteName.Terms[post.Site])
					{
						if (whiteTerm.Key.IsMatch(post.AuthorName))
						{
							info.Accuracy -= whiteTerm.Value;
							info.WhiteTermsFound.Add(whiteTerm.Key, whiteTerm.Value);
							filtersUsed++;
						}
					}
				}

				// Loop over blacklist.

				foreach (var blackTerm in GlobalInfo.QBlackName.Terms)
				{
					if (blackTerm.Key.IsMatch(post.AuthorName))
					{
						info.Accuracy += blackTerm.Value;
						info.BlackTermsFound.Add(blackTerm.Key, blackTerm.Value);

						filtersUsed++;
					}
				}

				if (filtersUsed != 0)
				{
					info.Accuracy /= filtersUsed;
					info.Accuracy /= GlobalInfo.QBlackName.HighestScore;
					info.Accuracy *= 100;
					info.Type = PostType.BadUsername;

					return true;
				}

				// Otherwise, if no terms were found, assume the post is clean.

				return false;
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
}
