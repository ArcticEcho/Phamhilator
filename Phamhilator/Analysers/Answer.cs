using System.Linq;



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
				if (blackTerm.Regex.IsMatch(post.Body))
				{
					info.Accuracy += blackTerm.Score;
					info.BlackTermsFound.Add(blackTerm);

					filtersUsed++;
				}
			}

			// Otherwise, if no black listed terms were found, assume the post is clean.

			if (filtersUsed == 0) { return false; }

			// Loop over whitelist.

			foreach (var whiteTerm in GlobalInfo.WhiteFilters[FilterType.AnswerWhiteSpam].Terms.Where(t => t.Site == post.Site))
			{
				if (whiteTerm.Regex.IsMatch(post.Body))
				{
					info.Accuracy -= whiteTerm.Score;
					info.WhiteTermsFound.Add(whiteTerm);
					info.FiltersUsed.Add(FilterType.AnswerWhiteSpam);
					filtersUsed++;
				}
			}

			info.FiltersUsed.Add(FilterType.AnswerBlackSpam);
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

			foreach (var whiteTerm in GlobalInfo.WhiteFilters[FilterType.AnswerWhiteLQ].Terms.Where(t => t.Site == post.Site))
			{
				if (whiteTerm.Regex.IsMatch(post.Body))
				{
					info.Accuracy -= whiteTerm.Score;
					info.WhiteTermsFound.Add(whiteTerm);
					info.FiltersUsed.Add(FilterType.AnswerWhiteLQ);
					filtersUsed++;
				}
			}

			info.FiltersUsed.Add(FilterType.AnswerBlackLQ);
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

			foreach (var whiteTerm in GlobalInfo.WhiteFilters[FilterType.AnswerWhiteOff].Terms.Where(t => t.Site == post.Site))
			{
				if (whiteTerm.Regex.IsMatch(post.Body))
				{
					info.Accuracy -= whiteTerm.Score;
					info.WhiteTermsFound.Add(whiteTerm);
					info.FiltersUsed.Add(FilterType.AnswerWhiteOff);
					filtersUsed++;
				}
			}

			info.FiltersUsed.Add(FilterType.AnswerBlackOff);
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
				if (blackTerm.Regex.IsMatch(post.AuthorName))
				{
					info.Accuracy += blackTerm.Score;
					info.BlackTermsFound.Add(blackTerm);

					filtersUsed++;
				}
			}

			// Otherwise, if no blacklist terms were found, assume the post is clean.

			if (filtersUsed == 0) { return false; }

			// Loop over whitelist.

			foreach (var whiteTerm in GlobalInfo.WhiteFilters[FilterType.AnswerWhiteName].Terms.Where(t => t.Site == post.Site))
			{
				if (whiteTerm.Regex.IsMatch(post.AuthorName))
				{
					info.Accuracy -= whiteTerm.Score;
					info.WhiteTermsFound.Add(whiteTerm);
					info.FiltersUsed.Add(FilterType.AnswerWhiteName);
					filtersUsed++;
				}
			}

			info.FiltersUsed.Add(FilterType.AnswerBlackName);
			info.Accuracy /= filtersUsed;
			info.Accuracy /= GlobalInfo.BlackFilters[FilterType.AnswerBlackName].HighestScore;
			info.Accuracy *= 100;
			info.Type = PostType.BadUsername;

			return true;
		}
	}
}
