namespace Phamhilator.Analysers
{
	public static class Answer
	{
		public static bool IsSpam(Phamhilator.Answer post, ref AnswerAnalysis info)
		{
			var filtersUsed = 0;

			// Loop over blacklist.

			foreach (var blackTerm in GlobalInfo.ABSpam.Terms)
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

			if (GlobalInfo.AWSpam.Terms.ContainsKey(post.Site))
			{
				foreach (var whiteTerm in GlobalInfo.AWSpam.Terms[post.Site])
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
			info.Accuracy /= GlobalInfo.ABSpam.HighestScore;
			info.Accuracy *= 100;
			info.Type = PostType.Spam;

			return true;
		}

		public static bool IsLowQuality(Phamhilator.Answer post, ref AnswerAnalysis info)
		{
			var filtersUsed = 0;

			// Loop over blacklist.

			foreach (var blackTerm in GlobalInfo.ABLQ.Terms)
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

			if (GlobalInfo.AWLQ.Terms.ContainsKey(post.Site))
			{
				foreach (var whiteTerm in GlobalInfo.AWLQ.Terms[post.Site])
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
			info.Accuracy /= GlobalInfo.ABLQ.HighestScore;
			info.Accuracy *= 100;
			info.Type = PostType.LowQuality;

			return true;				
		}

		public static bool IsOffensive(Phamhilator.Answer post, ref AnswerAnalysis info)
		{
			var filtersUsed = 0;

			// Loop over blacklist.

			foreach (var blackTerm in GlobalInfo.ABOff.Terms)
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

			if (GlobalInfo.AWOff.Terms.ContainsKey(post.Site))
			{
				foreach (var whiteTerm in GlobalInfo.AWOff.Terms[post.Site])
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
			info.Accuracy /= GlobalInfo.ABOff.HighestScore;
			info.Accuracy *= 100;
			info.Type = PostType.Offensive;

			return true;
		}

		public static bool IsBadUsername(Phamhilator.Answer post, ref AnswerAnalysis info)
		{
			var filtersUsed = 0;

			// Loop over blacklist.

			foreach (var blackTerm in GlobalInfo.ABName.Terms)
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

			if (GlobalInfo.AWName.Terms.ContainsKey(post.Site))
			{
				foreach (var whiteTerm in GlobalInfo.AWName.Terms[post.Site])
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
			info.Accuracy /= GlobalInfo.ABName.HighestScore;
			info.Accuracy *= 100;
			info.Type = PostType.BadUsername;

			return true;
		}
	}
}
