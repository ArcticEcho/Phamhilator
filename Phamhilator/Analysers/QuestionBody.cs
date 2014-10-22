namespace Phamhilator.Analysers
{
	public static class QuestionBody
	{
		public static bool IsSpam(Question post, ref QuestionAnalysis info)
		{
			if (post.PopulateExtraDataFailed) { return false; }

			var filtersUsed = 0;

			// Loop over blacklist.

			foreach (var blackTerm in GlobalInfo.BlackFilters[FilterType.QuestionBodyBlackSpam].Terms)
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

			if (GlobalInfo.WhiteFilters[FilterType.QuestionBodyWhiteSpam].Terms.ContainsKey(post.Site))
			{
				foreach (var whiteTerm in GlobalInfo.WhiteFilters[FilterType.QuestionBodyWhiteSpam].Terms[post.Site])
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
			info.Accuracy /= GlobalInfo.BlackFilters[FilterType.QuestionBodyBlackSpam].HighestScore;
			info.Accuracy *= 100;
			info.Type = PostType.Spam;

			return true;
		}

		public static bool IsLowQuality(Question post, ref QuestionAnalysis info)
		{
			if (post.PopulateExtraDataFailed) { return false; }

			var filtersUsed = 0;

			// Loop over blacklist.

			foreach (var blackTerm in GlobalInfo.BlackFilters[FilterType.QuestionBodyBlackLQ].Terms)
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

			if (GlobalInfo.WhiteFilters[FilterType.QuestionBodyWhiteLQ].Terms.ContainsKey(post.Site))
			{
				foreach (var whiteTerm in GlobalInfo.WhiteFilters[FilterType.QuestionBodyWhiteLQ].Terms[post.Site])
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
			info.Accuracy /= GlobalInfo.BlackFilters[FilterType.QuestionBodyBlackLQ].HighestScore;
			info.Accuracy *= 100;
			info.Type = PostType.LowQuality;

			return true;
		}

		public static bool IsOffensive(Question post, ref QuestionAnalysis info)
		{
			if (post.PopulateExtraDataFailed) { return false; }

			var filtersUsed = 0;

			// Loop over blacklist.

			foreach (var blackTerm in GlobalInfo.BlackFilters[FilterType.QuestionBodyBlackOff].Terms)
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

			if (GlobalInfo.WhiteFilters[FilterType.QuestionBodyWhiteOff].Terms.ContainsKey(post.Site))
			{
				foreach (var whiteTerm in GlobalInfo.WhiteFilters[FilterType.QuestionBodyWhiteOff].Terms[post.Site])
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
			info.Accuracy /= GlobalInfo.BlackFilters[FilterType.QuestionBodyBlackOff].HighestScore;
			info.Accuracy *= 100;
			info.Type = PostType.Offensive;

			return true;
		}
	}
}
