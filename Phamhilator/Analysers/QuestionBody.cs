namespace Phamhilator.Analysers
{
	public static class QuestionBody
	{
		public static bool IsSpam(Question post, ref QuestionAnalysis info)
		{
			var filtersUsed = 0;

			// Loop over blacklist.

			foreach (var blackTerm in GlobalInfo.QBBSpam.Terms)
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

			if (GlobalInfo.QBWSpam.Terms.ContainsKey(post.Site))
			{
				foreach (var whiteTerm in GlobalInfo.QBWSpam.Terms[post.Site])
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
			info.Accuracy /= GlobalInfo.QBBSpam.HighestScore;
			info.Accuracy *= 100;
			info.Type = PostType.Spam;

			return true;
		}

		public static bool IsLowQuality(Question post, ref QuestionAnalysis info)
		{
			var filtersUsed = 0;

			// Loop over blacklist.

			foreach (var blackTerm in GlobalInfo.QBBLQ.Terms)
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

			if (GlobalInfo.QBWLQ.Terms.ContainsKey(post.Site))
			{
				foreach (var whiteTerm in GlobalInfo.QBWLQ.Terms[post.Site])
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
			info.Accuracy /= GlobalInfo.QBBLQ.HighestScore;
			info.Accuracy *= 100;
			info.Type = PostType.LowQuality;

			return true;
		}

		public static bool IsOffensive(Question post, ref QuestionAnalysis info)
		{
			var filtersUsed = 0;

			// Loop over blacklist.

			foreach (var blackTerm in GlobalInfo.QBBOff.Terms)
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

			if (GlobalInfo.QBWhOff.Terms.ContainsKey(post.Site))
			{
				foreach (var whiteTerm in GlobalInfo.QBWhOff.Terms[post.Site])
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
			info.Accuracy /= GlobalInfo.QBBOff.HighestScore;
			info.Accuracy *= 100;
			info.Type = PostType.Offensive;

			return true;
		}
	}
}
