namespace Phamhilator
{
	namespace Analyser
	{
		public static class AnswerAnalyser
		{
			public static bool IsSpam(Answer post, ref AnswerAnalysis info)
			{
				var filtersUsed = 0;

				// Loop over blacklist.

				foreach (var blackTerm in GlobalInfo.ABlackSpam.Terms)
				{
					if (blackTerm.Key.IsMatch(post.Body))
					{
						info.Accuracy += blackTerm.Value;
						info.BlackTermsFound.Add(blackTerm.Key, blackTerm.Value);

						filtersUsed++;
					}
				}

				// Otherwise, if no black listed terms were found, assume the post is clean.

				if (filtersUsed == 0) { return false; }

				// Loop over whitelist.

				if (GlobalInfo.AWhiteSpam.Terms.ContainsKey(post.Site))
				{
					foreach (var whiteTerm in GlobalInfo.AWhiteSpam.Terms[post.Site])
					{
						if (whiteTerm.Key.IsMatch(post.Body))
						{
							info.Accuracy -= whiteTerm.Value;
							info.WhiteTermsFound.Add(whiteTerm.Key, whiteTerm.Value);
							filtersUsed++;
						}
					}
				}

				info.Accuracy /= filtersUsed;
				info.Accuracy /= GlobalInfo.ABlackSpam.HighestScore;
				info.Accuracy *= 100;
				info.Type = PostType.Spam;

				return true;
			}

			public static bool IsLowQuality(Answer post, ref AnswerAnalysis info)
			{
				var filtersUsed = 0;

				// Loop over blacklist.

				foreach (var blackTerm in GlobalInfo.ABlackLQ.Terms)
				{
					if (blackTerm.Key.IsMatch(post.Body))
					{
						info.Accuracy += blackTerm.Value;
						info.BlackTermsFound.Add(blackTerm.Key, blackTerm.Value);

						filtersUsed++;
					}
				}

				// Otherwise, if no blacklist terms were found, assume the post is clean.

				if (filtersUsed == 0) { return false; }

				// Loop over whitelist.

				if (GlobalInfo.AWhiteLQ.Terms.ContainsKey(post.Site))
				{
					foreach (var whiteTerm in GlobalInfo.AWhiteLQ.Terms[post.Site])
					{
						if (whiteTerm.Key.IsMatch(post.Body))
						{
							info.Accuracy -= whiteTerm.Value;
							info.WhiteTermsFound.Add(whiteTerm.Key, whiteTerm.Value);
							filtersUsed++;
						}
					}
				}

				info.Accuracy /= filtersUsed;
				info.Accuracy /= GlobalInfo.ABlackLQ.HighestScore;
				info.Accuracy *= 100;
				info.Type = PostType.LowQuality;

				return true;				
			}

			public static bool IsOffensive(Answer post, ref AnswerAnalysis info)
			{
				var filtersUsed = 0;

				// Loop over blacklist.

				foreach (var blackTerm in GlobalInfo.ABlackOff.Terms)
				{
					if (blackTerm.Key.IsMatch(post.Body))
					{
						info.Accuracy += blackTerm.Value;
						info.BlackTermsFound.Add(blackTerm.Key, blackTerm.Value);

						filtersUsed++;
					}
				}

				// Otherwise, if no blacklist terms were found, assume the post is clean.

				if (filtersUsed == 0) { return false; }

				// Loop over whitelist.

				if (GlobalInfo.AWhiteOff.Terms.ContainsKey(post.Site))
				{
					foreach (var whiteTerm in GlobalInfo.AWhiteOff.Terms[post.Site])
					{
						if (whiteTerm.Key.IsMatch(post.Body))
						{
							info.Accuracy -= whiteTerm.Value;
							info.WhiteTermsFound.Add(whiteTerm.Key, whiteTerm.Value);
							filtersUsed++;
						}
					}
				}
				
				info.Accuracy /= filtersUsed;
				info.Accuracy /= GlobalInfo.ABlackOff.HighestScore;
				info.Accuracy *= 100;
				info.Type = PostType.Offensive;

				return true;
			}

			public static bool IsBadUsername(Answer post, ref AnswerAnalysis info)
			{
				var filtersUsed = 0;

				// Loop over blacklist.

				foreach (var blackTerm in GlobalInfo.ABlackName.Terms)
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

				if (GlobalInfo.AWhiteName.Terms.ContainsKey(post.Site))
				{
					foreach (var whiteTerm in GlobalInfo.AWhiteName.Terms[post.Site])
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
				info.Accuracy /= GlobalInfo.ABlackName.HighestScore;
				info.Accuracy *= 100;
				info.Type = PostType.BadUsername;

				return true;
			}
		}
	}
}
