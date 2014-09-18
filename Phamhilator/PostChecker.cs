using System.Linq;
using System.Collections.Generic;



namespace Phamhilator
{
	public static class PostChecker
	{
		public static PostTypeInfo CheckPost(Post post)
		{
			var info = new PostTypeInfo();
			var accuracy = 0.0f;

			if ((info.BadTags = IsBadTagUsed(post)).Count != 0)
			{
				info.Type = PostType.BadTagUsed;

				return info;
			}

			if (post.Score >= 2)
			{
				return info;
			}

			if (IsLowQuality(post, ref info))
			{
				info.Type = PostType.LowQuality;

				info.Accuracy = (accuracy / GlobalInfo.LQ.HighestScore) * 100;
			}
			else if ((info.BadTags = IsBadTagUsed(post)).Count != 0)
			{
				info.Type = PostType.BadTagUsed;
			}
			else if (IsSpam(post, ref info))
			{
				info.Type = PostType.Spam;

				info.Accuracy = (accuracy / GlobalInfo.Spam.HighestScore) * 100;
			}
			else if (IsOffensive(post, ref info))
			{
				info.Type = PostType.Offensive;

				info.Accuracy = (accuracy / GlobalInfo.Off.HighestScore) * 100;
			}
			else if (IsBadUsername(post, ref info))
			{
				info.Type = PostType.BadUsername;

				info.Accuracy = (accuracy / GlobalInfo.Name.HighestScore) * 100;
			}

			return info;
		}

		private static bool IsSpam(Post post, ref PostTypeInfo info)
		{
			foreach (var ignoreFilter in GlobalInfo.IgnoreSpam.Terms)
			{
				if (ignoreFilter.Key.IsMatch(post.Title) && post.Site.StartsWith(ignoreFilter.Value))
				{
					return false;
				}
			}

			var filtersUsed = 0;

			for (var i = 0; i < GlobalInfo.Spam.Terms.Count; i++)
			{
				var filter = GlobalInfo.Spam.Terms.Keys.ElementAt(i);

				if (filter.IsMatch(post.Title))
				{
					info.Accuracy += GlobalInfo.Spam.Terms[filter];
					info.TermsFound.Add(filter);

					filtersUsed++;
				}

				if (i == GlobalInfo.Spam.Terms.Count - 1 && filtersUsed > 0)
				{
					info.Accuracy /= filtersUsed;

					return true;
				}
			}

			return false;
			//if (post.Site.StartsWith("fitness") ||
			//	((post.Site.StartsWith("stackoverflow") || post.Site.StartsWith("codereview") || post.Site.StartsWith("english")) && lower.Contains("best")) ||
			//	(post.Site.StartsWith("homebrew") && lower.Contains("naturally")) ||
			//	(post.Site.StartsWith("math") && lower.Contains("work out")) ||
			//	(post.Site.StartsWith("rpg") && lower.Contains("health")) ||
			//	(phoneNumber.IsMatch(lower) && lower.Contains("error")) ||
			//	(lower.Contains("bettery") && lower.Contains("health")))
			//{
			//	return false;
			//}

			//return spam.IsMatch(lower) ||
			//	(phoneNumber.IsMatch(lower) && !post.Site.StartsWith("math") && !post.Site.StartsWith("patents") && !post.Site.StartsWith("history")) ||
			//	(lower.Contains("http://") && lower.Contains(".com"));
		}

		private static bool IsLowQuality(Post post, ref PostTypeInfo info)
		{
			foreach (var ignoreFilter in GlobalInfo.IgnoreLQ.Terms)
			{
				if (ignoreFilter.Key.IsMatch(post.Title) && post.Site.StartsWith(ignoreFilter.Value))
				{
					return false;
				}
			}

			var filtersUsed = 0;

			for (var i = 0; i < GlobalInfo.LQ.Terms.Count; i++)
			{
				var filter = GlobalInfo.LQ.Terms.Keys.ElementAt(i);

				if (filter.IsMatch(post.Title))
				{
					info.Accuracy += GlobalInfo.LQ.Terms[filter];
					info.TermsFound.Add(filter);

					filtersUsed++;
				}

				if (i == GlobalInfo.LQ.Terms.Count - 1 && filtersUsed > 0)
				{
					info.Accuracy /= filtersUsed;

					return true;
				}
			}

			return false;
			//var wordCount = SpaceCount(post.Title);

			//if ((lower.Contains("q") && post.Site.StartsWith("math")) ||
			//	(lower.Contains("beginner") && lower.Length > 45) ||
			//	((lower.Contains("question") || lower.Contains("help")) && post.Site.StartsWith("meta")) ||
			//	(lower.Contains("problem") && (post.Site.StartsWith("math") || post.Site.StartsWith("gardening"))) ||
			//	(lower.Contains("error") && (lower.Contains("certificate") || lower.Contains("results in") || post.Site.StartsWith("programmers"))) ||
			//	(lower.Length > 20 && (post.Site.Contains("codereview") || post.Site.StartsWith("skeptics") || post.Site.Contains("ell") || post.Site.Contains("english") || post.Site.Contains("math") || post.Site.Contains("codegolf"))))
			//{
			//	return false;
			//}

			//return wordCount <= 2 ||
			//	   lowQuality.IsMatch(lower) ||
			//	   (lower.Contains("true") && lower.Contains("false")) ||
			//	   post.Title.All(c => Char.IsUpper(c) || !Char.IsLetter(c)) ||	   
			//	   (lower.Length < 25 && lower.All(c => Char.IsLower(c) || !Char.IsLetter(c))) ||
			//	   (lower.Contains("error") && (post.Title.Length < 35 || lower.Any(Char.IsDigit)) ||
			//	   (lower.Contains("how do i") && post.Title.Length < 75 && Char.IsLower(post.Title[0])) ||
			//	   (lower.Length < 35 && (lower.Contains("problem") || lower.Contains("issue"))));
		}

		private static bool IsOffensive(Post post, ref PostTypeInfo info)
		{
			//return offensive.IsMatch(post.Title.ToLowerInvariant());

			foreach (var ignoreFilter in GlobalInfo.IgnoreOff.Terms)
			{
				if (ignoreFilter.Key.IsMatch(post.Title) && post.Site.StartsWith(ignoreFilter.Value))
				{
					return false;
				}
			}

			var filtersUsed = 0;

			for (var i = 0; i < GlobalInfo.Off.Terms.Count; i++)
			{
				var filter = GlobalInfo.Off.Terms.Keys.ElementAt(i);

				if (filter.IsMatch(post.Title))
				{
					info.Accuracy += GlobalInfo.Off.Terms[filter];
					info.TermsFound.Add(filter);

					filtersUsed++;
				}

				if (i == GlobalInfo.Off.Terms.Count - 1 && filtersUsed > 0)
				{
					info.Accuracy /= filtersUsed;

					return true;
				}
			}

			return false;
		}

		private static bool IsBadUsername(Post post, ref PostTypeInfo info)
		{
			foreach (var ignoreFilter in GlobalInfo.IgnoreName.Terms)
			{
				if (ignoreFilter.Key.IsMatch(post.Title) && post.Site.StartsWith(ignoreFilter.Value))
				{
					return false;
				}
			}

			var filtersUsed = 0;

			for (var i = 0; i < GlobalInfo.Name.Terms.Count; i++)
			{
				var filter = GlobalInfo.Name.Terms.Keys.ElementAt(i);

				if (filter.IsMatch(post.Title))
				{
					info.Accuracy += GlobalInfo.Name.Terms[filter];
					info.TermsFound.Add(filter);

					filtersUsed++;
				}

				if (i == GlobalInfo.Name.Terms.Count - 1 && filtersUsed > 0)
				{
					info.Accuracy /= filtersUsed;

					return true;
				}
			}

			return false;
		}

		private static List<string> IsBadTagUsed(Post post)
		{
			var tags = new List<string>();

			if (!BadTagDefinitions.BadTags.Keys.Contains(post.Site)) { return tags; }

			foreach (var tag in post.Tags)
			{
				if (BadTagDefinitions.BadTags[post.Site].Contains(tag.ToLowerInvariant()))
				{
					tags.Add(tag);
				}
			}

			return tags;
		}
	}
}
