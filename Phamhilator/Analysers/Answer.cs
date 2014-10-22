namespace Phamhilator.Analysers
{
	public static class Answer
	{
		public static bool IsSpam(Phamhilator.Answer post, ref AnswerAnalysis info)
		{
			var filtersUsed = 0;

			// Loop over blacklist.

			foreach (var blackTerm in GlobalInfo.BlackFilters[FilterType.AnswerBlackSpam].Terms)
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

			if (GlobalInfo.WhiteFilters[FilterType.AnswerWhiteSpam].Terms.ContainsKey(post.Site))
			{
				foreach (var whiteTerm in GlobalInfo.WhiteFilters[FilterType.AnswerWhiteSpam].Terms[post.Site])
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
			info.Accuracy /= GlobalInfo.BlackFilters[FilterType.AnswerBlackSpam].HighestScore;
			info.Accuracy *= 100;
			info.Type = PostType.Spam;

			return true;
		}

		public static bool IsLowQuality(Phamhilator.Answer post, ref AnswerAnalysis info)
		{
			var filtersUsed = 0;

			// Loop over blacklist.

			foreach (var blackTerm in GlobalInfo.BlackFilters[FilterType.AnswerBlackLQ].Terms)
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

			if (GlobalInfo.WhiteFilters[FilterType.AnswerWhiteLQ].Terms.ContainsKey(post.Site))
			{
				foreach (var whiteTerm in GlobalInfo.WhiteFilters[FilterType.AnswerWhiteLQ].Terms[post.Site])
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
			info.Accuracy /= GlobalInfo.BlackFilters[FilterType.AnswerBlackLQ].HighestScore;
			info.Accuracy *= 100;
			info.Type = PostType.LowQuality;

			return true;				
		}

		public static bool IsOffensive(Phamhilator.Answer post, ref AnswerAnalysis info)
		{
			var filtersUsed = 0;

			// Loop over blacklist.

			foreach (var blackTerm in GlobalInfo.BlackFilters[FilterType.AnswerBlackOff].Terms)
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

			if (GlobalInfo.WhiteFilters[FilterType.AnswerWhiteOff].Terms.ContainsKey(post.Site))
			{
				foreach (var whiteTerm in GlobalInfo.WhiteFilters[FilterType.AnswerWhiteOff].Terms[post.Site])
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
			info.Accuracy /= GlobalInfo.BlackFilters[FilterType.AnswerBlackOff].HighestScore;
			info.Accuracy *= 100;
			info.Type = PostType.Offensive;

			return true;
		}

		public static bool IsBadUsername(Phamhilator.Answer post, ref AnswerAnalysis info)
		{
			var filtersUsed = 0;

			// Loop over blacklist.

			foreach (var blackTerm in GlobalInfo.BlackFilters[FilterType.AnswerBlackName].Terms)
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

			if (GlobalInfo.WhiteFilters[FilterType.AnswerWhiteName].Terms.ContainsKey(post.Site))
			{
				foreach (var whiteTerm in GlobalInfo.WhiteFilters[FilterType.AnswerWhiteName].Terms[post.Site])
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
			info.Accuracy /= GlobalInfo.BlackFilters[FilterType.AnswerBlackName].HighestScore;
			info.Accuracy *= 100;
			info.Type = PostType.BadUsername;

			return true;
		}
	}
}
