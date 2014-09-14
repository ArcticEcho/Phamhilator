using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;



namespace Phamhilator
{
	public static class PostChecker
	{
		private static readonly Dictionary<string, HashSet<string>> badTags = new Dictionary<string, HashSet<string>>();
		private static readonly Regex phoneNumber = new Regex(@"\D*(\d\W*){8}");
		private static readonly Regex badUser = new Regex("sumer|kolcak");
		private static readonly Regex offensive = new Regex("\b(nigg|asshole|piss|penis|bumhole|retard|bastard|bich|crap|fag|fuck|idiot|shit|whore)\b");
		private static readonly Regex lowQuality = new Regex(@"homework|newbie|\bq\b|question|confusing|\bh(i|ello)\b|\bgreeting(s)?\b|error(s)?|problem(s)?|(\{|\[|\(|\-)((re)?solved|edit(ed)?|update(d)?|fixed)(\}|\]|\)|\-)|correct this|school project|beginner|promblem|asap|urgent|please|(need|some)( halp| help| answer)|(no(t)?|doesn('t|t)?) work(ing|ed|d)?|(help|halp)");
		private static readonly Regex spam = new Regex(@"\b(yoga|relax(ing)?|beautiful(est)?|we lost|mover(s)?|bangalore|supplement(s)?|you know|get your|got my|six(-pack|pack| pack)|for me|sell(ing)?|customer review(s)?|wanted|help(s)? you(r)?|work out|buy|muscle(s)?|weight loss|brand|www|\.com|super herbal|treatment|cheap(est)?|(wo)?m(a|e)n|natural(ly)?|heal(ing|th|thly)?|care(ing)?|nurish(es|ing)?|exercise|ripped|full( movie| film)|free (trial|film|moive|help|assistance))\b");



		public static PostTypeInfo CheckPost(Post post, bool refreshTag = false)
		{
			if (refreshTag)
			{
				badTags.Clear();

				PopulateBadTags();
			}

			var info = new PostTypeInfo();

			if (IsOffensive(post))
			{
				info.Type = PostType.Offensive;
			}
			if (IsBadUsername(post))
			{
				info.Type = PostType.BadUsername;
			}
			else if ((info.BadTags = IsBadTagUsed(post)).Count != 0)
			{
				info.Type = PostType.BadTagUsed;
			}
			else if (IsSpam(post))
			{
				info.Type = PostType.Spam;
			}
			else if (IsLowQuality(post))
			{
				info.Type = PostType.LowQuality;
				info.InaccuracyPossible = !lowQuality.IsMatch(post.Title.ToLowerInvariant());
			}

			return info;
		}

		private static bool IsSpam(Post post)
		{
			var lower = post.Title.ToLowerInvariant();

			if (post.Site.StartsWith("fitness") ||
				((post.Site.StartsWith("stackoverflow") || post.Site.StartsWith("codereview") || post.Site.StartsWith("english")) && lower.Contains("best")) ||
				((post.Site.StartsWith("buddhism") || post.Site.StartsWith("stackoverflow")) && lower.Contains("benefit")) ||
				((post.Site.StartsWith("german") || post.Site.StartsWith("scifi") || post.Site.StartsWith("skeptics")) && (lower.Contains("man") || lower.Contains("men") || lower.Contains("woman"))) ||
				(post.Site.StartsWith("bitcoin") && lower.Contains("buy")) ||
				(post.Site.StartsWith("math") && lower.Contains("work out")) ||
				(post.Site.StartsWith("serverfault") && lower.Contains("www")) ||
				(post.Site.StartsWith("rgp") && lower.Contains("health")) ||
				(post.Site.StartsWith("astronomy") && lower.Contains("natural")))
			{
				return false;
			}

			return spam.IsMatch(lower) ||
					(phoneNumber.IsMatch(lower) && !post.Site.StartsWith("math") && !post.Site.StartsWith("patents") && !post.Site.StartsWith("history"));
		}

		private static bool IsLowQuality(Post post)
		{
			var wordCount = SpaceCount(post.Title);
			var lower = post.Title.ToLowerInvariant();

			if (lower.Contains("question") && post.Site.StartsWith("meta"))
			{
				return false;
			}

			return post.Title.All(c => Char.IsUpper(c) || !Char.IsLetter(c)) ||
				   wordCount <= 1 ||
				   lowQuality.IsMatch(lower) ||
				   (lower.Contains("true") && lower.Contains("false")) ||
				   (lower.Length < 35 && (lower.Contains("problem") || lower.Contains("issue")) && Char.IsLower(post.Title[0])) ||
				   (lower.Length < 20 && !post.Site.Contains("codereview") && !post.Site.Contains("english") && !post.Site.Contains("math") && !post.Site.Contains("codegolf")) ||
				   (lower.Contains("how do i") && post.Title.Length < 75 && Char.IsLower(post.Title[0])) ||
				   (lower.Contains("error") && (post.Title.Length < 35 || lower.Any(Char.IsDigit)));
		}

		private static bool IsOffensive(Post post)
		{
			return offensive.IsMatch(post.Title.ToLowerInvariant());
		}

		private static bool IsBadUsername(Post post)
		{
			var lower = post.AuthorName.ToLowerInvariant();

			return offensive.IsMatch(lower) || badUser.IsMatch(lower);
		}

		private static List<string> IsBadTagUsed(Post post)
		{
			var tags = new List<string>();

			if (badTags.Count == 0)
			{
				PopulateBadTags();
			}

			if (!badTags.Keys.Contains(post.Site)) { return tags; }

			foreach (var tag in post.Tags)
			{
				if (badTags[post.Site].Contains(tag.ToLowerInvariant()))
				{
					tags.Add(tag);
				}
			}

			return tags;
		}



		private static int SpaceCount(string input)
		{
			return input.Count(c => c == ' ');
		}

		private static void PopulateBadTags()
		{
			var tagDir = Path.Combine(Directory.GetParent(Directory.GetParent(Environment.CurrentDirectory).FullName).FullName, "Bad Tag Definitions");
			var tags = new List<string>();

			foreach (var dir in Directory.EnumerateDirectories(tagDir))
			{
				tags.AddRange(File.ReadAllText(Path.Combine(dir, "BadTags.txt")).Split('\n'));

				for (var i = 0; i < tags.Count; i++)
				{
					tags[i] = tags[i].Trim();
				}

				badTags.Add(Path.GetFileName(dir), new HashSet<string>(tags));

				tags.Clear();
			}
		}
	}
}
