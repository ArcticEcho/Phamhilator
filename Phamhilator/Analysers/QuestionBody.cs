using System.Linq;



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
				if (blackTerm.Regex.IsMatch(post.Body))
				{
					info.Accuracy += blackTerm.Score;
					info.BlackTermsFound.Add(blackTerm);

					filtersUsed++;
				}
			}

			// Otherwise, if no blacklist terms were found, assume the post is clean.

			if (filtersUsed == 0) { return false; }

			// Loop over whitelist.

			foreach (var whiteTerm in GlobalInfo.WhiteFilters[FilterType.QuestionBodyWhiteSpam].Terms.Where(t => t.Site == post.Site))
			{
				if (whiteTerm.Regex.IsMatch(post.Body))
				{
					info.Accuracy -= whiteTerm.Score;
					info.WhiteTermsFound.Add(whiteTerm);
					info.FiltersUsed.Add(FilterType.QuestionBodyWhiteSpam);
					filtersUsed++;
				}
			}

			info.FiltersUsed.Add(FilterType.QuestionBodyBlackSpam);
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
				if (blackTerm.Regex.IsMatch(post.Body))
				{
					info.Accuracy += blackTerm.Score;
					info.BlackTermsFound.Add(blackTerm);

					filtersUsed++;
				}
			}

			// Otherwise, if no blacklist terms were found, assume the post is clean.

			if (filtersUsed == 0) { return false; }

			// Loop over whitelist.

			foreach (var whiteTerm in GlobalInfo.WhiteFilters[FilterType.QuestionBodyWhiteLQ].Terms.Where(t => t.Site == post.Site))
			{
				if (whiteTerm.Regex.IsMatch(post.Body))
				{
					info.Accuracy -= whiteTerm.Score;
					info.WhiteTermsFound.Add(whiteTerm);
					info.FiltersUsed.Add(FilterType.QuestionBodyWhiteLQ);
					filtersUsed++;
				}
			}

			info.FiltersUsed.Add(FilterType.QuestionBodyBlackLQ);
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
				if (blackTerm.Regex.IsMatch(post.Body))
				{
					info.Accuracy += blackTerm.Score;
					info.BlackTermsFound.Add(blackTerm);

					filtersUsed++;
				}
			}

			// Otherwise, if no blacklist terms were found, assume the post is clean.

			if (filtersUsed == 0) { return false; }

			// Loop over whitelist.

			foreach (var whiteTerm in GlobalInfo.WhiteFilters[FilterType.QuestionBodyWhiteOff].Terms.Where(t => t.Site == post.Site))
			{
				if (whiteTerm.Regex.IsMatch(post.Body))
				{
					info.Accuracy -= whiteTerm.Score;
					info.WhiteTermsFound.Add(whiteTerm);
					info.FiltersUsed.Add(FilterType.QuestionBodyWhiteOff);
					filtersUsed++;
				}
			}

			info.FiltersUsed.Add(FilterType.QuestionBodyBlackOff);
			info.Accuracy /= filtersUsed;
			info.Accuracy /= GlobalInfo.BlackFilters[FilterType.QuestionBodyBlackOff].HighestScore;
			info.Accuracy *= 100;
			info.Type = PostType.Offensive;

			return true;
		}
	}
}
