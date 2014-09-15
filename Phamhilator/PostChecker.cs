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
		private static readonly Regex phoneNumber = new Regex(@"\d(?!\d-?\d{2}-\d{4})(?>\W*\d){7}");
		private static readonly Regex badUser = new Regex("sumer|kolcak");
		private static readonly Regex offensive = new Regex("\b(nigg|asshole|piss|penis|bumhole|retard|bastard|bitch|crap|fag|fuck|idiot|shit|whore)\b");
		private static readonly Regex lowQuality = new Regex(@"homework|builtwith.com|very difficult|newbie|\bq\b|question|tried|\bif (yes|no)\b|my best|our feelings|says wrong|confusing|\bh(i|ello)\b|\bgreeting(s)?\b|error(s)?|problem(s)?|(\{|\[|\(|\-)((re)?solved|edit(ed)?|update(d)?|fixed)(\}|\]|\)|\-)|correct this|school project|beginner|promblem|asap|urgent|pl(z|s|ease)|(need|some)( halp| help| answer)|(no(t)?|doesn('t|t)?) work(ing|ed|d)?|(help|halp)");
		private static readonly Regex spam = new Regex(@"\b(yoga|relax(ing)?|beautiful(est)?|we lost|mover(s)?|delhi|colon cleanse|is helpful|my huge|bulky figure|bangalore|supplement(s)?|you know|get your|got my|six(-pack|pack| pack)|selling|customer review(s)?|wanted|help(s)? you(r)?|work out|body( building|builder(s)?| builder(s)?|building)|muscle(s)?|weight loss|\bbrand\b|http://|\.com|super herbal|treatment|cheap(er)?|naturally|heal(ing|th|thly)?|care(ing)?|nurish(es|ing)?|ripped|full( movie| film)|free (trial|film|moive|movie|help|assistance))\b");



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
			else if (IsBadUsername(post))
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
				info.InaccuracyPossible = !lowQuality.IsMatch(post.Title.ToLowerInvariant()) || post.Title.All(c => Char.IsUpper(c) || !Char.IsLetter(c));
			}

			return info;
		}

		private static bool IsSpam(Post post)
		{
			var lower = post.Title.ToLowerInvariant();

			if (post.Site.StartsWith("fitness") ||
				((post.Site.StartsWith("stackoverflow") || post.Site.StartsWith("codereview") || post.Site.StartsWith("english")) && lower.Contains("best")) ||
				(post.Site.StartsWith("homebrew") && lower.Contains("naturally")) ||
				(post.Site.StartsWith("math") && lower.Contains("work out")) ||
				(post.Site.StartsWith("rpg") && lower.Contains("health")) ||
				(phoneNumber.IsMatch(lower) && lower.Contains("error")) ||
				(lower.Contains("bettery") && lower.Contains("health")))
			{
				return false;
			}

			return spam.IsMatch(lower) ||
				(phoneNumber.IsMatch(lower) && !post.Site.StartsWith("math") && !post.Site.StartsWith("patents") && !post.Site.StartsWith("history")) ||
				(lower.Contains("http://") && lower.Contains(".com"));
		}

		private static bool IsLowQuality(Post post)
		{
			var wordCount = SpaceCount(post.Title);
			var lower = post.Title.ToLowerInvariant();

			if ((lower.Contains("q") && post.Site.StartsWith("math")) ||
				((lower.Contains("question") || lower.Contains("help")) && post.Site.StartsWith("meta")) ||
				(lower.Contains("error") && lower.Length > 40 && lower.All(c => !Char.IsDigit(c))) ||
				(lower.Contains("problem") && (post.Site.StartsWith("math") || post.Site.StartsWith("gardening"))) ||
				(lower.Contains("error") && (lower.Contains("certificate") || lower.Contains("results in") || post.Site.StartsWith("programmers"))))
			{
				return false;
			}

			return post.Title.All(c => Char.IsUpper(c) || !Char.IsLetter(c)) ||
				   wordCount <= 1 ||
				   lowQuality.IsMatch(lower) ||
				   (lower.Contains("true") && lower.Contains("false")) ||
				   (lower.Length < 35 && (lower.Contains("problem") || lower.Contains("issue")) && Char.IsLower(post.Title[0])) ||
				   (lower.Length < 20 && !post.Site.Contains("codereview") && !post.Site.Contains("ell") && !post.Site.Contains("english") && !post.Site.Contains("math") && !post.Site.Contains("codegolf")) ||
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
