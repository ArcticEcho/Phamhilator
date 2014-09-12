using System;
using System.Linq;
using System.Text.RegularExpressions;



namespace Yekoms
{
	public static class PostChecker
	{
		private static readonly Regex offensive = new Regex("nigg|asshole|penis|bumhole|retard|bastard|bich|crap|fag|fuck|idiot|shit|whore");
		private static readonly Regex lowQuality = new Regex("(homework|newbie|urgent|please|need?( halp| help| answer)|no(t)? work(ing|ed|d)?|(help|halp) need)");
		private static readonly Regex spam = new Regex(@"\b(yoga|relax(ing)?|beautiful(est)?|supplement(s)?|you know|best|get your|for me|body|smooth|sell(ing)?|food|customer review(s)?|wanted|help(s)? you(r)?|work out|buy(ing)?|muscle(s)?|weight loss|sure|brand|www|\.com|super herbal|benefi(t|ts|cial)|treatment|cheap(est)?|(wo)?m(a|e)n|natural(ly)?|product|heal(ing|th|thly)?|care(ing)?|nurish(es|ing)?|exercise|ripped|full( movie| film)|free (trial|film|moive|help|assistance))\b");



		public static PostTypeInfo CheckPost(Post post)
		{
			var info = new PostTypeInfo();

			if (IsOffensive(post))
			{
				info.Type = PostType.Offensive;
				info.InaccuracyPossible = post.AuthorName.Contains("user");
			}
			else if (IsLowQuality(post))
			{
				info.Type = PostType.LowQuality;
				info.InaccuracyPossible = post.AuthorName.Contains("user") || lowQuality.IsMatch(post.Title);
			}
			else if (IsSpam(post))
			{
				info.Type = PostType.Spam;
				info.InaccuracyPossible = post.AuthorName.Contains("user");
			}

			return info;
		}

		private static bool IsSpam(Post post)
		{
			if (!post.Site.Contains("fitness") || 
				(post.Site.Contains("magento") && post.Title.ToLowerInvariant().Contains("product")) ||
				(post.Site.Contains("math") && post.Title.Contains("product")))
			{
				return false;
			}

			return spam.IsMatch(post.Title) || post.Title.Count(Char.IsDigit) >= 7;
		}

		private static bool IsLowQuality(Post post)
		{
			var wordCount = SpaceCount(post.Title);
			var lower = post.Title.ToLowerInvariant();

			return post.Title.All(c => Char.IsUpper(c) || !Char.IsLetter(c)) ||
			       wordCount <= 1 ||
				   lowQuality.IsMatch(lower) ||
				   (lower.Length < 35 && (lower.Contains("problem") || lower.Contains("issue")) && Char.IsLower(post.Title[0])) ||
				   (lower.Length < 20 && !post.Site.Contains("codereview") && !post.Site.Contains("math") && !post.Site.Contains("codegolf")) ||
				   (lower.Contains("how do i") && post.Title.Length < 75 && Char.IsLower(post.Title[0])) ||
				   (lower.Contains("error") && (post.Title.Length < 35 || lower.Any(Char.IsDigit)));
		}

		private static bool IsOffensive(Post post)
		{
			return offensive.IsMatch(post.Title.ToLowerInvariant());
		}



		private static int SpaceCount(string input)
		{
			return input.Count(c => c == ' ');
		}
	}
}
