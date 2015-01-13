using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using ChatExchangeDotNet;



namespace Phamhilator
{
    public static class CommandProcessor
    {
        private static Room room;
        private static Message message;
        private static PostAnalysis report;
        private static Post post;
        private static bool fileMissingWarningMessagePosted;
        private const RegexOptions cmdRegexOptions = RegexOptions.Compiled | RegexOptions.CultureInvariant;
        private static readonly Random random = new Random();

        private static readonly HashSet<ChatCommand> commands = new HashSet<ChatCommand>
        {
            #region Normal user commands.

            new ChatCommand(new Regex("(?i)^status$", cmdRegexOptions), command => new[]
            {
                new ReplyMessage(String.Concat("`", GlobalInfo.Status, "`." /*" @ ", GlobalInfo.CommitFormatted, "(https://github.com/ArcticEcho/Phamhilator/commit/", GlobalInfo.CommitHash, ").")*/))
            }, CommandAccessLevel.NormalUser),

            new ChatCommand(new Regex("(?i)^(info(rmation)?|about)$", cmdRegexOptions), command => new[]
            {
                new ReplyMessage("[`Phamhilator`](https://github.com/ArcticEcho/Phamhilator/wiki) `is a` [`.NET`](http://en.wikipedia.org/wiki/.NET_Framework)-`based` [`internet bot`](http://en.wikipedia.org/wiki/Internet_bot) `written in` [`C#`](http://stackoverflow.com/questions/tagged/c%23) `which watches over` [`the /realtime tab`](http://stackexchange.com/questions?tab=realtime) `of` [`Stack Exchange`](http://stackexchange.com/)`. Owners: " + GlobalInfo.OwnerNames + ".`")
            }, CommandAccessLevel.NormalUser),

            new ChatCommand(new Regex("(?i)^help$", cmdRegexOptions), command => new[]
            {
                new ReplyMessage("`See` [`here`](https://github.com/ArcticEcho/Phamhilator/wiki/Chat-Commands) `for a full list of commands.`")
            }, CommandAccessLevel.NormalUser),

            new ChatCommand(new Regex("(?i)^(help (add|del)|(add|del) help)$", cmdRegexOptions), command => new[]
            {
                new ReplyMessage("`To add or delete a term, use \">>(add/del)-(b/w)-(a/qt/qb)-(lq/spam/off/name) (if w, term's site name) {regex-term}\". To add or delete a tag, use \">>(add/remove) {site-name} {tag-name} {link}\".`")
            }, CommandAccessLevel.NormalUser),

            new ChatCommand(new Regex("(?i)^(help edit|edit help)$", cmdRegexOptions), command => new[]
            {
                new ReplyMessage("`To edit a term, use \">>edit-(b/w)-(a/qt/qb)-(lq/spam/off/name) (if w, term's site name) {old-term}¬¬¬{new-term}\".`")
            }, CommandAccessLevel.NormalUser),

            new ChatCommand(new Regex("(?i)^(help auto|auto help)$", cmdRegexOptions), command => new[]
            {
                new ReplyMessage("`To add an automatic term, use \">>auto-b-(a/qt/qb)-(lq/spam/off/name)(-p) {regex-term}\". Use \"-p\" if the change should persist past the bot's restart.`")
            }, CommandAccessLevel.NormalUser),

            new ChatCommand(new Regex("(?i)^(help list|list help|commands)$", cmdRegexOptions), command => new[]
            {
                new ReplyMessage("    @" + message.AuthorName.Replace(" ", "") + "\n    Supported commands: info, stats, status & env.\n    Supported replies: (fp/tp/tpa), why, ask, clean & del.\n    Owner-only commands: resume, pause, (add/ban)-user {user-id}, threshold {percentage}, kill-it-with-no-regrets-for-sure, full-scan & set-status {message}.", false)
            }, CommandAccessLevel.NormalUser),

            new ChatCommand(new Regex("(?i)^stats$", cmdRegexOptions), command =>
            {
                var ignorePercent = Math.Round(((GlobalInfo.Stats.TotalCheckedPosts - (GlobalInfo.Stats.TotalFPCount + GlobalInfo.Stats.TotalTPCount)) / GlobalInfo.Stats.TotalCheckedPosts) * 100, 1);

                return new[]
                {
                    new ReplyMessage("`Total terms: " + GlobalInfo.TermCount + ". Posts caught (last 7 days): " + GlobalInfo.PostsCaught + ". Total posts checked: " + GlobalInfo.Stats.TotalCheckedPosts + ". " + "Reports ignored: " + ignorePercent + "%. Uptime: " + (DateTime.UtcNow - GlobalInfo.UpTime) + ".`")
                };
            }, CommandAccessLevel.NormalUser),

            new ChatCommand(new Regex(@"(?i)^(terms|why)\b", cmdRegexOptions), command => new[]
            {
                GetTerms()
            }, CommandAccessLevel.NormalUser),

            new ChatCommand(new Regex("(?i)^env$", cmdRegexOptions), command =>
            {
                var totalMem = new Microsoft.VisualBasic.Devices.ComputerInfo().TotalPhysicalMemory / 1024f / 1024 / 1024;

                return new[]
                {
                    new ReplyMessage("    @" + message.AuthorName.Replace(" ", "") + "\n    Cores (logical): " + Environment.ProcessorCount + "\n    Total RAM: " +  totalMem + "GB\n    OS: " + Environment.OSVersion.VersionString + "\n    64-bit: " + Environment.Is64BitOperatingSystem + "\n    CLR version: " + Environment.Version, false)
                };
            }, CommandAccessLevel.NormalUser),

            #region Toys.

            new ChatCommand(new Regex("(?i)^red button$", cmdRegexOptions), command => new[]
            {
                new ReplyMessage("`Warning: now launching " + random.Next(1, 101) + " anti-spammer homing missiles...`", false)
            }, CommandAccessLevel.NormalUser),

            new ChatCommand(new Regex("(?i)^panic$", cmdRegexOptions), command => new[]
            {
                new ReplyMessage("http://rack.0.mshcdn.com/media/ZgkyMDEzLzA2LzE4LzdjL0JlYWtlci4zOWJhOC5naWYKcAl0aHVtYgkxMjAweDk2MDA-/4a93e3c4/4a4/Beaker.gif")
            }, CommandAccessLevel.NormalUser),

            new ChatCommand(new Regex("(?i)^fox$", cmdRegexOptions), command => new[]
            {
                new ReplyMessage("http://i.stack.imgur.com/0qaHz.gif")
            }, CommandAccessLevel.NormalUser),

            #endregion

            #endregion

            #region Privileged user commands.

            #region Add black term commands.

            new ChatCommand(new Regex(@"(?i)^add\-b\-qt\-(spam|off|name|lq) \S", cmdRegexOptions), command => new[]
            {
                AddBQTTerm(command)
            }, CommandAccessLevel.PrivilegedUser),          
            new ChatCommand(new Regex(@"(?i)^add\-b\-qb\-(spam|off|name|lq) \S", cmdRegexOptions), command => new[]
            {
                AddBQBTerm(command)
            }, CommandAccessLevel.PrivilegedUser),
            new ChatCommand(new Regex(@"(?i)^add\-b\-a\-(spam|off|name|lq) \S", cmdRegexOptions), command => new[]
            {
                AddBATerm(command)
            }, CommandAccessLevel.PrivilegedUser),
            #endregion

            #region Add white term commands.

            new ChatCommand(new Regex(@"(?i)^add\-w\-qt\-(spam|off|name|lq) \S+ \S", cmdRegexOptions), command => new[]
            {
                AddWQTTerm(command)
            }, CommandAccessLevel.PrivilegedUser),
            new ChatCommand(new Regex(@"(?i)^add\-w\-qb\-(spam|off|name|lq) \S+ \S", cmdRegexOptions), command => new[]
            {
                AddWQBTerm(command)
            }, CommandAccessLevel.PrivilegedUser),
            new ChatCommand(new Regex(@"(?i)^add\-w\-a\-(spam|off|name|lq) \S+ \S", cmdRegexOptions), command => new[]
            {
                AddWATerm(command)
            }, CommandAccessLevel.PrivilegedUser),

            #endregion

            #region Edit black term commands.
            
            new ChatCommand(new Regex(@"(?i)^edit\-b\-qt\-(spam|off|name|lq) .+¬¬¬.+$", cmdRegexOptions), command => new[]
            {
                EditBQTTerm(command)
            }, CommandAccessLevel.PrivilegedUser),
            new ChatCommand(new Regex(@"(?i)^edit\-b\-qb\-(spam|off|name|lq) .+¬¬¬.+$", cmdRegexOptions), command => new[]
            {
                EditBQBTerm(command)
            }, CommandAccessLevel.PrivilegedUser),
            new ChatCommand(new Regex(@"(?i)^edit\-b\-a\-(spam|off|name|lq) .+¬¬¬.+$", cmdRegexOptions), command => new[]
            {
                EditBATerm(command)
            }, CommandAccessLevel.PrivilegedUser),

            #endregion

            #region Edit white term commands.

            new ChatCommand(new Regex(@"(?i)^edit\-w\-qt\-(spam|off|name|lq) \S+ .+¬¬¬.+$", cmdRegexOptions), command => new[]
            {
                EditWQTTerm(command)
            }, CommandAccessLevel.PrivilegedUser),
            new ChatCommand(new Regex(@"(?i)^edit\-w\-qb\-(spam|off|name|lq) \S+ .+¬¬¬.+$", cmdRegexOptions), command => new[]
            {
                EditWQBTerm(command)
            }, CommandAccessLevel.PrivilegedUser),
            new ChatCommand(new Regex(@"(?i)^edit\-w\-a\-(spam|off|name|lq) \S+ .+¬¬¬.+$", cmdRegexOptions), command => new[]
            {
                EditWATerm(command)
            }, CommandAccessLevel.PrivilegedUser),

            #endregion

            #region Remove black term commands.

            new ChatCommand(new Regex(@"(?i)^del\-b\-qt\-(spam|off|name|lq) \S", cmdRegexOptions), command => new[]
            {
                RemoveBQTTerm(command)
            }, CommandAccessLevel.PrivilegedUser),
            new ChatCommand(new Regex(@"(?i)^del\-b\-qb\-(spam|off|name|lq) \S", cmdRegexOptions), command => new[]
            {
                RemoveBQBTerm(command)
            }, CommandAccessLevel.PrivilegedUser),
            new ChatCommand(new Regex(@"(?i)^del\-b\-a\-(spam|off|name|lq) \S", cmdRegexOptions), command => new[]
            {
                RemoveBATerm(command)
            }, CommandAccessLevel.PrivilegedUser),

            #endregion

            #region Remove white term commands.

            new ChatCommand(new Regex(@"(?i)^del\-w\-qt\-(spam|off|name|lq) \S+ .*", cmdRegexOptions), command => new[]
            {
                RemoveWQTTerm(command)
            }, CommandAccessLevel.PrivilegedUser),
            new ChatCommand(new Regex(@"(?i)^del\-w\-qb\-(spam|off|name|lq) \S+ .*", cmdRegexOptions), command => new[]
            {
                RemoveWQBTerm(command)
            }, CommandAccessLevel.PrivilegedUser),
            new ChatCommand(new Regex(@"(?i)^del\-w\-a\-(spam|off|name|lq) \S+ .*", cmdRegexOptions), command => new[]
            {
                RemoveWATerm(command)
            }, CommandAccessLevel.PrivilegedUser),

            #endregion

            #region FP/TP commands.

            new ChatCommand(new Regex(@"^f(p|alse)((?!\s(why|del|clean)).)*$", cmdRegexOptions), command => new[]
            {
                FalsePositive()
            }, CommandAccessLevel.PrivilegedUser),
            new ChatCommand(new Regex(@"(?i)^fp why\b", cmdRegexOptions), command => new[]
            {
                FalsePositive(),
                GetTerms()
            }, CommandAccessLevel.PrivilegedUser),
            new ChatCommand(new Regex(@"(?i)^fp del\b", cmdRegexOptions), command => new[]
            {
                FalsePositive(),
                DeleteMessage()
            }, CommandAccessLevel.PrivilegedUser),
            new ChatCommand(new Regex(@"(?i)^fp clean\b", cmdRegexOptions), command => new[]
            {
                FalsePositive(),
                CleanMessage()
            }, CommandAccessLevel.PrivilegedUser),
            new ChatCommand(new Regex(@"^tpa?((?!\s(why|clean)).)*$", cmdRegexOptions), command => new[]
            {
                TruePositive(command)
            }, CommandAccessLevel.PrivilegedUser),
            new ChatCommand(new Regex(@"(?i)^tpa? why\b", cmdRegexOptions), command => new[]
            {
                TruePositive(command),
                GetTerms()
            }, CommandAccessLevel.PrivilegedUser),
            new ChatCommand(new Regex(@"(?i)^tpa? clean\b", cmdRegexOptions), command => new[]
            {
                TruePositive(command),
                CleanMessage()
            }, CommandAccessLevel.PrivilegedUser),

            #endregion

            #region Black term auto toggling commands.
            
            new ChatCommand(new Regex(@"(?i)^auto\-b-qt\-(spam|off|name|lq)(\-p) \S?$", cmdRegexOptions), command => new[]
            {
                AutoBQTTerm(command)
            }, CommandAccessLevel.PrivilegedUser),
            new ChatCommand(new Regex(@"(?i)^auto\-b-qb\-(spam|off|name|lq)(\-p) \S?$", cmdRegexOptions), command => new[]
            {
                AutoBQBTerm(command)
            }, CommandAccessLevel.PrivilegedUser),
            new ChatCommand(new Regex(@"(?i)^auto\-b-a\-(spam|off|name|lq)(\-p) \S?$", cmdRegexOptions), command => new[]
            {
                AutoBATerm(command)
            }, CommandAccessLevel.PrivilegedUser),

            #endregion

            new ChatCommand(new Regex(@"(?i)^add-tag \S+ \S+$", cmdRegexOptions), command => new[]
            {
                AddTag(command)
            }, CommandAccessLevel.PrivilegedUser),
            new ChatCommand(new Regex(@"(?i)^del-tag \S+ \S+$", cmdRegexOptions), command => new[]
            {
                RemoveTag(command)
            }, CommandAccessLevel.PrivilegedUser),
            new ChatCommand(new Regex(@"(?i)^clean\b", cmdRegexOptions), command => new[]
            {
                CleanMessage()
            }, CommandAccessLevel.PrivilegedUser),
            new ChatCommand(new Regex(@"(?i)^del\b", cmdRegexOptions), command => new[]
            {
                DeleteMessage()
            }, CommandAccessLevel.PrivilegedUser),
            new ChatCommand(new Regex(@"(?i)^ask\b", cmdRegexOptions), command => new[]
            {
                Ask()
            }, CommandAccessLevel.PrivilegedUser),

            #endregion

            #region Owner commands.

            new ChatCommand(new Regex(@"(?i)^add-user \d+$", RegexOptions.Compiled | RegexOptions.CultureInvariant), command => new[]
            {
                AddUser(command)
            }, CommandAccessLevel.Owner),
            new ChatCommand(new Regex(@"(?i)^ban-user \d+$", RegexOptions.Compiled | RegexOptions.CultureInvariant), command => new[]
            {
                BanUser(command)
            }, CommandAccessLevel.Owner),
            new ChatCommand(new Regex(@"(?i)^resume\b", RegexOptions.Compiled | RegexOptions.CultureInvariant), command => new[]
            {
                ResumeBot()
            }, CommandAccessLevel.Owner),
            new ChatCommand(new Regex(@"(?i)^pause\b", RegexOptions.Compiled | RegexOptions.CultureInvariant), command => new[]
            {
                PauseBot()
            }, CommandAccessLevel.Owner),
            new ChatCommand(new Regex(@"(?i)^full-scan\b", RegexOptions.Compiled | RegexOptions.CultureInvariant), command => new[]
            {
                FullScan()
            }, CommandAccessLevel.Owner),
            new ChatCommand(new Regex(@"(?i)^threshold (\d|\.)+$", RegexOptions.Compiled | RegexOptions.CultureInvariant), command => new[]
            {
                SetAccuracyThreshold(command)
            }, CommandAccessLevel.Owner),
            new ChatCommand(new Regex(@"(?i)^set-status .+$", RegexOptions.Compiled | RegexOptions.CultureInvariant), command => new[]
            {
                SetStatus(command)
            }, CommandAccessLevel.Owner),

            #endregion
        };



        public static ReplyMessage[] ExacuteCommand(Room messageRoom, Message input)
        {
            if (!BannedUsers.SystemIsClear && !fileMissingWarningMessagePosted)
            {
                fileMissingWarningMessagePosted = true;

                return new[]
                {
                    new ReplyMessage("`Warning: the banned users file is missing. All commands have been disabled until the issue has been resolved.`")
                };
            }
            if (BannedUsers.IsUserBanned(input.AuthorID.ToString(CultureInfo.InvariantCulture))) { return new[] { new ReplyMessage("", false) }; }

            string command;
            room = messageRoom;
            message = input;

            try
            {
                if (input.Content.StartsWith(">>"))
                {
                    command = input.Content.Remove(0, 2).TrimStart();
                }
                else if (input.ParentID != -1 && room[input.ParentID].AuthorID == room.Me.ID)
                {
                    command = input.Content.TrimStart();
                }
                else
                {
                    return new[] { new ReplyMessage("", false) };
                }
            }
            catch (Exception)
            {
                return new[] { new ReplyMessage("", false) };
            }

            var requestedCmd = commands.FirstOrDefault(cmd => cmd.Syntax.IsMatch(command));

            if (requestedCmd == null)
            {
                return new[] { new ReplyMessage("`Command not recognised.`") };
            }

            if (GlobalInfo.PostedReports.ContainsKey(input.ParentID))
            {
                report = GlobalInfo.PostedReports[input.ParentID].Report;
                post = GlobalInfo.PostedReports[input.ParentID].Post;
            }
            else
            {
                report = null;
                post = null;
            }

            try
            {
                switch (requestedCmd.AccessLevel)
                {
                    case CommandAccessLevel.NormalUser:
                    {
                        return requestedCmd.Command(command);
                    }

                    case CommandAccessLevel.PrivilegedUser:
                    {
                        if (!UserAccess.CommandAccessUsers.Contains(input.AuthorID) && GlobalInfo.Owners.All(user => user.ID != input.AuthorID))
                        {
                            return new[]
                            {
                                new ReplyMessage("`Access denied.`")
                            };
                        }

                        return requestedCmd.Command(command);
                    }

                    case CommandAccessLevel.Owner:
                    {
                        if (GlobalInfo.Owners.All(user => user.ID != input.AuthorID))
                        {
                            return new[]
                            {
                                new ReplyMessage("`Access denied.`")
                            };
                        }

                        return requestedCmd.Command(command);
                    }
                }
            }
            catch (Exception)
            {
                return new[] { new ReplyMessage("`Error executing command.`") };
            }

            return null;
        }

        public static bool IsValidCommand(Room messageRoom, Message command)
        {            
            var trimmedCommand = command.Content.Trim();

            try
            {
                if (trimmedCommand.StartsWith(">>"))
                {
                    trimmedCommand = trimmedCommand.Remove(0, 2).TrimStart();
                }
                else if (command.ParentID != -1 && messageRoom[command.ParentID].AuthorID == messageRoom.Me.ID)
                {
                    trimmedCommand = command.Content.TrimStart();
                }
            }
            catch (Exception)
            {
                return false;
            }

            return commands.Any(cmd => cmd.Syntax.IsMatch(trimmedCommand));
        }



        #region Normal user commands.

        private static ReplyMessage GetTerms()
        {
            if (report.BlackTermsFound.Count == 1)
            {
                var term = report.BlackTermsFound.First();
                var m = "`Term found: " + term.Regex.ToString().Replace("\\n", "(?# new line)");

                if (term.TPCount + term.FPCount >= 5)
                {
                    m += " (Sensitivity: " + Math.Round(term.Sensitivity * 100, 1);
                    m += "%. Specificity: " + Math.Round(term.Specificity * 100, 1);
                    m += "%. Ignored: " + Math.Round((term.IgnoredCount / term.CaughtCount) * 100, 1);
                    m += "%. Score: " + Math.Round(term.Score, 1);
                    m += ". Auto: " + term.IsAuto + ")`";
                }
                else
                {
                    m += " (Ignored: " + Math.Round((term.IgnoredCount / term.CaughtCount) * 100, 1);
                    m += "%. Score: " + Math.Round(term.Score, 1);
                    m += ". Auto: " + term.IsAuto + ")`";
                }

                return new ReplyMessage(m);
            }

            var builder = new StringBuilder("    @" + message.AuthorName.Replace(" ", "") + "\n    Terms found:\n");

            foreach (var term in report.BlackTermsFound)
            {
                var termString = term.Regex.ToString().Replace("\n", "(?# new line)");

                if (term.TPCount + term.FPCount >= 5)
                {
                    builder.Append("    " + termString + " ");
                    builder.Append(" (Sensitivity: " + Math.Round(term.Sensitivity * 100, 1));
                    builder.Append("%. Specificity: " + Math.Round(term.Specificity * 100, 1));
                    builder.Append("%. Ignored: " + Math.Round((term.IgnoredCount / term.CaughtCount) * 100, 1));
                    builder.Append("%. Score: " + Math.Round(term.Score, 1));
                    builder.Append(". Auto: " + term.IsAuto + ")\n    \n");
                }
                else
                {
                    builder.Append("    " + termString + " ");
                    builder.Append(" (Ignored: " + Math.Round((term.IgnoredCount / term.CaughtCount) * 100, 1));
                    builder.Append("%. Score: " + Math.Round(term.Score, 1));
                    builder.Append(". Auto: " + term.IsAuto + ")\n    \n");
                }
            }

            return new ReplyMessage(builder.ToString().TrimEnd(), false);
        }

        #endregion

        # region Privileged user commands.

        # region Add term commands.

        private static ReplyMessage AddBQTTerm(string command)
        {
            var addCommand = command.Remove(0, 9);
            var term = new Regex(addCommand.Substring(addCommand.IndexOf(' ') + 1), RegexOptions.Compiled);

            if (term.IsFlakRegex()) { return new ReplyMessage("`ReDoS detected (term not added).`"); }

            switch (addCommand.ToLowerInvariant()[0])
            {
                case 'o':
                {
                    if (GlobalInfo.BlackFilters[FilterType.QuestionTitleBlackOff].Terms.Contains(term)) { return new ReplyMessage("`Blacklist term already exists.`"); }

                    GlobalInfo.BlackFilters[FilterType.QuestionTitleBlackOff].AddTerm(new Term(FilterType.QuestionTitleBlackOff, term, GlobalInfo.BlackFilters[FilterType.QuestionTitleBlackOff].AverageScore));

                    break;
                }

                case 's':
                {
                    if (GlobalInfo.BlackFilters[FilterType.QuestionTitleBlackSpam].Terms.Contains(term)) { return new ReplyMessage("`Blacklist term already exists.`"); }

                    GlobalInfo.BlackFilters[FilterType.QuestionTitleBlackSpam].AddTerm(new Term(FilterType.QuestionTitleBlackSpam, term, GlobalInfo.BlackFilters[FilterType.QuestionTitleBlackSpam].AverageScore));

                    break;
                }

                case 'l':
                {
                    if (GlobalInfo.BlackFilters[FilterType.QuestionTitleBlackLQ].Terms.Contains(term)) { return new ReplyMessage("`Blacklist term already exists.`"); }

                    GlobalInfo.BlackFilters[FilterType.QuestionTitleBlackLQ].AddTerm(new Term(FilterType.QuestionTitleBlackLQ, term, GlobalInfo.BlackFilters[FilterType.QuestionTitleBlackLQ].AverageScore));

                    break;
                }

                case 'n':
                {
                    if (GlobalInfo.BlackFilters[FilterType.QuestionTitleBlackName].Terms.Contains(term)) { return new ReplyMessage("`Blacklist term already exists.`"); }

                    GlobalInfo.BlackFilters[FilterType.QuestionTitleBlackName].AddTerm(new Term(FilterType.QuestionTitleBlackName, term, GlobalInfo.BlackFilters[FilterType.QuestionTitleBlackName].AverageScore));

                    break;
                }
            }

            return new ReplyMessage("`Blacklist term added.`");
        }

        private static ReplyMessage AddWQTTerm(string command)
        {
            var addCommand = command.Remove(0, 9);
            var firstSpace = command.IndexOf(' ');
            var secondSpace = command.IndexOf(' ', firstSpace + 1);
            var term = new Regex(command.Substring(secondSpace + 1));
            var site = command.Substring(firstSpace + 1, secondSpace - firstSpace - 1);

            if (term.IsFlakRegex()) { return new ReplyMessage("`ReDoS detected (term not added).`"); }

            switch (addCommand.ToLowerInvariant()[0])
            {
                case 'o':
                {
                    if (GlobalInfo.WhiteFilters[FilterType.QuestionTitleWhiteOff].Terms.Contains(term, site)) { return new ReplyMessage("`Whitelist term already exists.`"); }

                    var score = GlobalInfo.WhiteFilters[FilterType.QuestionTitleWhiteOff].Terms.Where(t => t.Site == site).Select(t => t.Score).Average();

                    GlobalInfo.WhiteFilters[FilterType.QuestionTitleWhiteOff].AddTerm(new Term(FilterType.QuestionTitleWhiteOff, term, score, site));

                    break;
                }

                case 's':
                {
                    if (GlobalInfo.WhiteFilters[FilterType.QuestionTitleWhiteSpam].Terms.Contains(term, site)) { return new ReplyMessage("`Whitelist term already exists.`"); }

                    var score = GlobalInfo.WhiteFilters[FilterType.QuestionTitleWhiteSpam].Terms.Where(t => t.Site == site).Select(t => t.Score).Average();

                    GlobalInfo.WhiteFilters[FilterType.QuestionTitleWhiteSpam].AddTerm(new Term(FilterType.QuestionTitleWhiteSpam, term, score, site));

                    break;
                }

                case 'l':
                {
                    if (GlobalInfo.WhiteFilters[FilterType.QuestionTitleWhiteLQ].Terms.Contains(term, site)) { return new ReplyMessage("`Whitelist term already exists.`"); }

                    var score = GlobalInfo.WhiteFilters[FilterType.QuestionTitleWhiteLQ].Terms.Where(t => t.Site == site).Select(t => t.Score).Average();

                    GlobalInfo.WhiteFilters[FilterType.QuestionTitleWhiteLQ].AddTerm(new Term(FilterType.QuestionTitleWhiteLQ, term, score, site));

                    break;
                }

                case 'n':
                {
                    if (GlobalInfo.WhiteFilters[FilterType.QuestionTitleWhiteName].Terms.Contains(term, site)) { return new ReplyMessage("`Whitelist term already exists.`"); }

                    var score = GlobalInfo.WhiteFilters[FilterType.QuestionTitleWhiteName].Terms.Where(t => t.Site == site).Select(t => t.Score).Average();

                    GlobalInfo.WhiteFilters[FilterType.QuestionTitleWhiteName].AddTerm(new Term(FilterType.QuestionTitleWhiteName, term, score, site));

                    break;
                }
            }

            return new ReplyMessage("`Whitelist term added.`");
        }

        private static ReplyMessage AddBQBTerm(string command)
        {
            var addCommand = command.Remove(0, 9);
            var term = new Regex(addCommand.Substring(addCommand.IndexOf(' ') + 1), RegexOptions.Compiled);

            if (term.IsFlakRegex()) { return new ReplyMessage("`ReDoS detected (term not added).`"); }

            switch (addCommand.ToLowerInvariant()[0])
            {
                case 'o':
                {
                    if (GlobalInfo.BlackFilters[FilterType.QuestionBodyBlackOff].Terms.Contains(term)) { return new ReplyMessage("`Blacklist term already exists.`"); }

                    GlobalInfo.BlackFilters[FilterType.QuestionBodyBlackOff].AddTerm(new Term(FilterType.QuestionBodyBlackOff, term, GlobalInfo.BlackFilters[FilterType.QuestionBodyBlackOff].AverageScore));

                    break;
                }

                case 's':
                {
                    if (GlobalInfo.BlackFilters[FilterType.QuestionBodyBlackSpam].Terms.Contains(term)) { return new ReplyMessage("`Blacklist term already exists.`"); }

                    GlobalInfo.BlackFilters[FilterType.QuestionBodyBlackSpam].AddTerm(new Term(FilterType.QuestionBodyBlackSpam, term, GlobalInfo.BlackFilters[FilterType.QuestionBodyBlackSpam].AverageScore));

                    break;
                }

                case 'l':
                {
                    if (GlobalInfo.BlackFilters[FilterType.QuestionBodyBlackLQ].Terms.Contains(term)) { return new ReplyMessage("`Blacklist term already exists.`"); }

                    GlobalInfo.BlackFilters[FilterType.QuestionBodyBlackLQ].AddTerm(new Term(FilterType.QuestionBodyBlackLQ, term, GlobalInfo.BlackFilters[FilterType.QuestionBodyBlackLQ].AverageScore));

                    break;
                }
            }

            return new ReplyMessage("`Blacklist term added.`");
        }

        private static ReplyMessage AddWQBTerm(string command)
        {
            var addCommand = command.Remove(0, 9);
            var firstSpace = command.IndexOf(' ');
            var secondSpace = command.IndexOf(' ', firstSpace + 1);
            var term = new Regex(command.Substring(secondSpace + 1));
            var site = command.Substring(firstSpace + 1, secondSpace - firstSpace - 1);

            if (term.IsFlakRegex()) { return new ReplyMessage("`ReDoS detected (term not added).`"); }

            switch (addCommand.ToLowerInvariant()[0])
            {
                case 'o':
                {
                    if (GlobalInfo.WhiteFilters[FilterType.QuestionBodyWhiteOff].Terms.Contains(term, site)) { return new ReplyMessage("`Whitelist term already exists.`"); }

                    var score = GlobalInfo.WhiteFilters[FilterType.QuestionBodyWhiteOff].Terms.Where(t => t.Site == site).Select(t => t.Score).Average();

                    GlobalInfo.WhiteFilters[FilterType.QuestionBodyWhiteOff].AddTerm(new Term(FilterType.QuestionBodyWhiteOff, term, score, site));

                    break;
                }

                case 's':
                {
                    if (GlobalInfo.WhiteFilters[FilterType.QuestionBodyWhiteSpam].Terms.Contains(term, site)) { return new ReplyMessage("`Whitelist term already exists.`"); }

                    var score = GlobalInfo.WhiteFilters[FilterType.QuestionBodyWhiteSpam].Terms.Where(t => t.Site == site).Select(t => t.Score).Average();

                    GlobalInfo.WhiteFilters[FilterType.QuestionBodyWhiteSpam].AddTerm(new Term(FilterType.QuestionBodyWhiteSpam, term, score, site));

                    break;
                }

                case 'l':
                {
                    if (GlobalInfo.WhiteFilters[FilterType.QuestionBodyWhiteLQ].Terms.Contains(term, site)) { return new ReplyMessage("`Whitelist term already exists.`"); }

                    var score = GlobalInfo.WhiteFilters[FilterType.QuestionBodyWhiteLQ].Terms.Where(t => t.Site == site).Select(t => t.Score).Average();

                    GlobalInfo.WhiteFilters[FilterType.QuestionBodyWhiteLQ].AddTerm(new Term(FilterType.QuestionBodyWhiteLQ, term, score, site));

                    break;
                }
            }

            return new ReplyMessage("`Whitelist term added.`");
        }

        private static ReplyMessage AddBATerm(string command)
        {
            var addCommand = command.Remove(0, 8);
            var term = new Regex(addCommand.Substring(addCommand.IndexOf(' ') + 1), RegexOptions.Compiled);

            if (term.IsFlakRegex()) { return new ReplyMessage("`ReDoS detected (term not added).`"); }

            switch (addCommand.ToLowerInvariant()[0])
            {
                case 'o':
                {
                    if (GlobalInfo.BlackFilters[FilterType.AnswerBlackOff].Terms.Contains(term)) { return new ReplyMessage("`Blacklist term already exists.`"); }

                    GlobalInfo.BlackFilters[FilterType.AnswerBlackOff].AddTerm(new Term(FilterType.AnswerBlackOff, term, GlobalInfo.BlackFilters[FilterType.AnswerBlackOff].AverageScore));

                    break;
                }

                case 's':
                {
                    if (GlobalInfo.BlackFilters[FilterType.AnswerBlackSpam].Terms.Contains(term)) { return new ReplyMessage("`Blacklist term already exists.`"); }

                    GlobalInfo.BlackFilters[FilterType.AnswerBlackSpam].AddTerm(new Term(FilterType.AnswerBlackSpam, term, GlobalInfo.BlackFilters[FilterType.AnswerBlackSpam].AverageScore));

                    break;
                }

                case 'l':
                {
                    if (GlobalInfo.BlackFilters[FilterType.AnswerBlackLQ].Terms.Contains(term)) { return new ReplyMessage("`Blacklist term already exists.`"); }

                    GlobalInfo.BlackFilters[FilterType.AnswerBlackLQ].AddTerm(new Term(FilterType.AnswerBlackLQ, term, GlobalInfo.BlackFilters[FilterType.AnswerBlackLQ].AverageScore));

                    break;
                }

                case 'n':
                {
                    if (GlobalInfo.BlackFilters[FilterType.AnswerBlackName].Terms.Contains(term)) { return new ReplyMessage("`Blacklist term already exists.`"); }

                    GlobalInfo.BlackFilters[FilterType.AnswerBlackName].AddTerm(new Term(FilterType.AnswerBlackName, term, GlobalInfo.BlackFilters[FilterType.AnswerBlackName].AverageScore));

                    break;
                }
            }

            return new ReplyMessage("`Blacklist term added.`");
        }

        private static ReplyMessage AddWATerm(string command)
        {
            var addCommand = command.Substring(0, 8);
            var firstSpace = command.IndexOf(' ');
            var secondSpace = command.IndexOf(' ', firstSpace + 1);
            var term = new Regex(command.Substring(secondSpace + 1));
            var site = command.Substring(firstSpace + 1, secondSpace - firstSpace - 1);

            if (term.IsFlakRegex()) { return new ReplyMessage("`ReDoS detected (term not added).`"); }

            switch (addCommand.ToLowerInvariant()[0])
            {
                case 'o':
                {
                    if (GlobalInfo.WhiteFilters[FilterType.AnswerWhiteOff].Terms.Contains(term, site)) { return new ReplyMessage("`Whitelist term already exists.`"); }

                    var score = GlobalInfo.WhiteFilters[FilterType.AnswerWhiteOff].Terms.Where(t => t.Site == site).Select(t => t.Score).Average();

                    GlobalInfo.WhiteFilters[FilterType.AnswerWhiteOff].AddTerm(new Term(FilterType.AnswerWhiteOff, term, score, site));

                    break;
                }

                case 's':
                {
                    if (GlobalInfo.WhiteFilters[FilterType.AnswerWhiteSpam].Terms.Contains(term, site)) { return new ReplyMessage("`Whitelist term already exists.`"); }

                    var score = GlobalInfo.WhiteFilters[FilterType.AnswerWhiteSpam].Terms.Where(t => t.Site == site).Select(t => t.Score).Average();

                    GlobalInfo.WhiteFilters[FilterType.AnswerWhiteSpam].AddTerm(new Term(FilterType.AnswerWhiteSpam, term, score, site));

                    break;
                }

                case 'l':
                {
                    if (GlobalInfo.WhiteFilters[FilterType.AnswerWhiteLQ].Terms.Contains(term, site)) { return new ReplyMessage("`Whitelist term already exists.`"); }

                    var score = GlobalInfo.WhiteFilters[FilterType.AnswerWhiteLQ].Terms.Where(t => t.Site == site).Select(t => t.Score).Average();

                    GlobalInfo.WhiteFilters[FilterType.AnswerWhiteLQ].AddTerm(new Term(FilterType.AnswerWhiteLQ, term, score, site));

                    break;
                }

                case 'n':
                {
                    if (GlobalInfo.WhiteFilters[FilterType.AnswerWhiteName].Terms.Contains(term, site)) { return new ReplyMessage("`Whitelist term already exists.`"); }

                    var score = GlobalInfo.WhiteFilters[FilterType.AnswerWhiteName].Terms.Where(t => t.Site == site).Select(t => t.Score).Average();

                    GlobalInfo.WhiteFilters[FilterType.AnswerWhiteName].AddTerm(new Term(FilterType.AnswerWhiteName, term, score, site));

                    break;
                }
            }

            return new ReplyMessage("`Whitelist term added.`");
        }

        # endregion

        # region Remove term commands.

        private static ReplyMessage RemoveBQTTerm(string command)
        {
            var removeCommand = command.Remove(0, 9);
            var term = new Regex(removeCommand.Substring(removeCommand.IndexOf(' ') + 1), RegexOptions.Compiled);

            switch (removeCommand.ToLowerInvariant()[0])
            {
                case 'o':
                {
                    if (!GlobalInfo.BlackFilters[FilterType.QuestionTitleBlackOff].Terms.Contains(term)) { return new ReplyMessage("`Blacklist term does not exist.`"); }

                    GlobalInfo.Stats.TotalCheckedPosts -= GlobalInfo.BlackFilters[FilterType.QuestionTitleBlackOff].Terms.GetRealTerm(term).CaughtCount;
                    GlobalInfo.BlackFilters[FilterType.QuestionTitleBlackOff].RemoveTerm(term);

                    break;
                }

                case 's':
                {
                    if (!GlobalInfo.BlackFilters[FilterType.QuestionTitleBlackSpam].Terms.Contains(term)) { return new ReplyMessage("`Blacklist term does not exist.`"); }
                    
                    GlobalInfo.Stats.TotalCheckedPosts -= GlobalInfo.BlackFilters[FilterType.QuestionTitleBlackSpam].Terms.GetRealTerm(term).CaughtCount;
                    GlobalInfo.BlackFilters[FilterType.QuestionTitleBlackSpam].RemoveTerm(term);

                    break;
                }

                case 'l':
                {
                    if (!GlobalInfo.BlackFilters[FilterType.QuestionTitleBlackLQ].Terms.Contains(term)) { return new ReplyMessage("`Blacklist term does not exist.`"); }

                    GlobalInfo.Stats.TotalCheckedPosts -= GlobalInfo.BlackFilters[FilterType.QuestionTitleBlackLQ].Terms.GetRealTerm(term).CaughtCount;
                    GlobalInfo.BlackFilters[FilterType.QuestionTitleBlackLQ].RemoveTerm(term);

                    break;
                }

                case 'n':
                {
                    if (!GlobalInfo.BlackFilters[FilterType.QuestionTitleBlackName].Terms.Contains(term)) { return new ReplyMessage("`Blacklist term does not exist.`"); }

                    GlobalInfo.Stats.TotalCheckedPosts -= GlobalInfo.BlackFilters[FilterType.QuestionTitleBlackName].Terms.GetRealTerm(term).CaughtCount;
                    GlobalInfo.BlackFilters[FilterType.QuestionTitleBlackName].RemoveTerm(term);

                    break;
                }
            }

            

            return new ReplyMessage("`Blacklist term removed.`");
        }

        private static ReplyMessage RemoveWQTTerm(string command)
        {
            var removeCommand = command.Remove(0, 9);
            var firstSpace = command.IndexOf(' ');
            var secondSpace = command.IndexOf(' ', firstSpace + 1);
            var term = new Regex(command.Substring(secondSpace + 1));
            var site = command.Substring(firstSpace + 1, secondSpace - firstSpace - 1);

            switch (removeCommand.ToLowerInvariant()[0])
            {
                case 'o':
                {
                    if (!GlobalInfo.WhiteFilters[FilterType.QuestionTitleWhiteOff].Terms.Contains(term, site)) { return new ReplyMessage("`Whitelist term does not exist.`"); }

                    GlobalInfo.WhiteFilters[FilterType.QuestionTitleWhiteOff].RemoveTerm(new Term(FilterType.QuestionTitleWhiteOff, term, 0, site));

                    break;
                }

                case 's':
                {
                    if (!GlobalInfo.WhiteFilters[FilterType.QuestionTitleWhiteSpam].Terms.Contains(term, site)) { return new ReplyMessage("`Whitelist term does not exist.`"); }

                    GlobalInfo.WhiteFilters[FilterType.QuestionTitleWhiteSpam].RemoveTerm(new Term(FilterType.QuestionTitleWhiteSpam, term, 0, site));

                    break;
                }

                case 'l':
                {
                    if (!GlobalInfo.WhiteFilters[FilterType.QuestionTitleWhiteLQ].Terms.Contains(term, site)) { return new ReplyMessage("`Whitelist term does not exist.`"); }

                    GlobalInfo.WhiteFilters[FilterType.QuestionTitleWhiteLQ].RemoveTerm(new Term(FilterType.QuestionTitleWhiteLQ, term, 0, site));

                    break;
                }
                
                case 'n':
                {
                    if (!GlobalInfo.WhiteFilters[FilterType.QuestionTitleWhiteName].Terms.Contains(term, site)) { return new ReplyMessage("`Whitelist term does not exist.`"); }

                    GlobalInfo.WhiteFilters[FilterType.QuestionTitleWhiteName].RemoveTerm(new Term(FilterType.QuestionTitleWhiteName, term, 0, site));

                    break;
                }
            }

            return new ReplyMessage("`Whitelist term removed.`");
        }

        private static ReplyMessage RemoveBQBTerm(string command)
        {
            var removeCommand = command.Remove(0, 9);
            var term = new Regex(removeCommand.Substring(removeCommand.IndexOf(' ') + 1), RegexOptions.Compiled);

            switch (removeCommand.ToLowerInvariant()[0])
            {
                case 'o':
                {
                    if (!GlobalInfo.BlackFilters[FilterType.QuestionBodyBlackOff].Terms.Contains(term)) { return new ReplyMessage("`Blacklist term does not exist.`"); }

                    GlobalInfo.Stats.TotalCheckedPosts -= GlobalInfo.BlackFilters[FilterType.QuestionBodyBlackOff].Terms.GetRealTerm(term).CaughtCount;
                    GlobalInfo.BlackFilters[FilterType.QuestionBodyBlackOff].RemoveTerm(term);

                    break;
                }

                case 's':
                {
                    if (!GlobalInfo.BlackFilters[FilterType.QuestionBodyBlackSpam].Terms.Contains(term)) { return new ReplyMessage("`Blacklist term does not exist.`"); }

                    GlobalInfo.Stats.TotalCheckedPosts -= GlobalInfo.BlackFilters[FilterType.QuestionBodyBlackSpam].Terms.GetRealTerm(term).CaughtCount;
                    GlobalInfo.BlackFilters[FilterType.QuestionBodyBlackSpam].RemoveTerm(term);

                    break;
                }

                case 'l':
                {
                    if (!GlobalInfo.BlackFilters[FilterType.QuestionBodyBlackLQ].Terms.Contains(term)) { return new ReplyMessage("`Blacklist term does not exist.`"); }

                    GlobalInfo.Stats.TotalCheckedPosts -= GlobalInfo.BlackFilters[FilterType.QuestionBodyBlackLQ].Terms.GetRealTerm(term).CaughtCount;
                    GlobalInfo.BlackFilters[FilterType.QuestionBodyBlackLQ].RemoveTerm(term);

                    break;
                }
            }

            return new ReplyMessage("`Blacklist term removed.`");
        }

        private static ReplyMessage RemoveWQBTerm(string command)
        {
            var removeCommand = command.Remove(0, 9);
            var firstSpace = command.IndexOf(' ');
            var secondSpace = command.IndexOf(' ', firstSpace + 1);
            var term = new Regex(command.Substring(secondSpace + 1));
            var site = command.Substring(firstSpace + 1, secondSpace - firstSpace - 1);

            switch (removeCommand.ToLowerInvariant()[0])
            {
                case 'o':
                {
                    if (!GlobalInfo.WhiteFilters[FilterType.QuestionBodyWhiteOff].Terms.Contains(term, site)) { return new ReplyMessage("`Whitelist term does not exist.`"); }

                    GlobalInfo.WhiteFilters[FilterType.QuestionBodyWhiteOff].RemoveTerm(new Term(FilterType.QuestionBodyWhiteOff, term, 0, site));

                    break;
                }

                case 's':
                {
                    if (!GlobalInfo.WhiteFilters[FilterType.QuestionBodyWhiteSpam].Terms.Contains(term, site)) { return new ReplyMessage("`Whitelist term does not exist.`"); }

                    GlobalInfo.WhiteFilters[FilterType.QuestionBodyWhiteSpam].RemoveTerm(new Term(FilterType.QuestionBodyWhiteSpam, term, 0, site));

                    break;
                }

                case 'l':
                {
                    if (!GlobalInfo.WhiteFilters[FilterType.QuestionBodyWhiteLQ].Terms.Contains(term, site)) { return new ReplyMessage("`Whitelist term does not exist.`"); }

                    GlobalInfo.WhiteFilters[FilterType.QuestionBodyWhiteLQ].RemoveTerm(new Term(FilterType.QuestionBodyWhiteLQ, term, 0, site));

                    break;
                }
            }

            return new ReplyMessage("`Whitelist term removed.`");
        }

        private static ReplyMessage RemoveBATerm(string command)
        {
            var removeCommand = command.Remove(0, 8);
            var term = new Regex(removeCommand.Substring(removeCommand.IndexOf(' ') + 1), RegexOptions.Compiled);

            switch (removeCommand.ToLowerInvariant()[0])
            {
                case 'o':
                {
                    if (!GlobalInfo.BlackFilters[FilterType.AnswerBlackOff].Terms.Contains(term)) { return new ReplyMessage("`Blacklist term does not exist.`"); }

                    GlobalInfo.Stats.TotalCheckedPosts -= GlobalInfo.BlackFilters[FilterType.AnswerBlackOff].Terms.GetRealTerm(term).CaughtCount;
                    GlobalInfo.BlackFilters[FilterType.AnswerBlackOff].RemoveTerm(term);

                    break;
                }

                case 's':
                {
                    if (!GlobalInfo.BlackFilters[FilterType.AnswerBlackSpam].Terms.Contains(term)) { return new ReplyMessage("`Blacklist term does not exist.`"); }

                    GlobalInfo.Stats.TotalCheckedPosts -= GlobalInfo.BlackFilters[FilterType.AnswerBlackSpam].Terms.GetRealTerm(term).CaughtCount;
                    GlobalInfo.BlackFilters[FilterType.AnswerBlackSpam].RemoveTerm(term);

                    break;
                }

                case 'l':
                {
                    if (!GlobalInfo.BlackFilters[FilterType.AnswerBlackLQ].Terms.Contains(term)) { return new ReplyMessage("`Blacklist term does not exist.`"); }

                    GlobalInfo.Stats.TotalCheckedPosts -= GlobalInfo.BlackFilters[FilterType.AnswerBlackLQ].Terms.GetRealTerm(term).CaughtCount;
                    GlobalInfo.BlackFilters[FilterType.AnswerBlackLQ].RemoveTerm(term);

                    break;
                }

                case 'n':
                {
                    if (!GlobalInfo.BlackFilters[FilterType.AnswerBlackName].Terms.Contains(term)) { return new ReplyMessage("`Blacklist term does not exist.`"); }

                    GlobalInfo.Stats.TotalCheckedPosts -= GlobalInfo.BlackFilters[FilterType.AnswerBlackName].Terms.GetRealTerm(term).CaughtCount;
                    GlobalInfo.BlackFilters[FilterType.AnswerBlackName].RemoveTerm(term);

                    break;
                }
            }

            return new ReplyMessage("`Blacklist term removed.`");
        }

        private static ReplyMessage RemoveWATerm(string command)
        {
            var removeCommand = command.Remove(0, 8);
            var firstSpace = command.IndexOf(' ');
            var secondSpace = command.IndexOf(' ', firstSpace + 1);
            var term = new Regex(command.Substring(secondSpace + 1));
            var site = command.Substring(firstSpace + 1, secondSpace - firstSpace - 1);

            switch (removeCommand.ToLowerInvariant()[0])
            {
                case 'o':
                {
                    if (!GlobalInfo.WhiteFilters[FilterType.AnswerWhiteOff].Terms.Contains(term)) { return new ReplyMessage("`Whitelist term does not exist.`"); }

                    GlobalInfo.WhiteFilters[FilterType.AnswerWhiteOff].RemoveTerm(new Term(FilterType.AnswerWhiteOff, term, 0, site));

                    break;
                }

                case 's':
                {
                    if (!GlobalInfo.WhiteFilters[FilterType.AnswerWhiteSpam].Terms.Contains(term)) { return new ReplyMessage("`Whitelist term does not exist.`"); }

                    GlobalInfo.WhiteFilters[FilterType.AnswerWhiteSpam].RemoveTerm(new Term(FilterType.AnswerWhiteSpam, term, 0, site));

                    break;
                }

                case 'l':
                {
                    if (!GlobalInfo.WhiteFilters[FilterType.AnswerWhiteLQ].Terms.Contains(term)) { return new ReplyMessage("`Whitelist term does not exist.`"); }

                    GlobalInfo.WhiteFilters[FilterType.AnswerWhiteLQ].RemoveTerm(new Term(FilterType.AnswerWhiteLQ, term, 0, site));

                    break;
                }

                case 'n':
                {
                    if (!GlobalInfo.WhiteFilters[FilterType.AnswerWhiteName].Terms.Contains(term)) { return new ReplyMessage("`Whitelist term does not exist.`"); }

                    GlobalInfo.WhiteFilters[FilterType.AnswerWhiteName].RemoveTerm(new Term(FilterType.AnswerWhiteName, term, 0, site));

                    break;
                }
            }

            return new ReplyMessage("`Whitelist term removed.`");
        }

        # endregion

        # region Edit term commands.

        private static ReplyMessage EditBQTTerm(string command)
        {
            var editCommand = command.Remove(0, 10);

            if (!editCommand.StartsWith("off") && !editCommand.StartsWith("spam") && !editCommand.StartsWith("lq") && !editCommand.StartsWith("name")) { return new ReplyMessage("`Command not recognised.`"); }

            var startIndex = command.IndexOf(' ') + 1;
            var delimiterIndex = command.IndexOf("¬¬¬", StringComparison.Ordinal);
            var oldTerm = new Regex(command.Substring(startIndex, delimiterIndex - startIndex));
            var newTerm = new Regex(command.Remove(0, delimiterIndex + 3), RegexOptions.Compiled);

            if (newTerm.IsFlakRegex()) { return new ReplyMessage("`ReDoS detected (term not updated).`"); }

            switch (editCommand.ToLowerInvariant()[0])
            {
                case 'o':
                {
                    if (!GlobalInfo.BlackFilters[FilterType.QuestionTitleBlackOff].Terms.Contains(oldTerm)) { return new ReplyMessage("`Blacklist term does not exist.`"); }

                    GlobalInfo.BlackFilters[FilterType.QuestionTitleBlackOff].EditTerm(oldTerm, newTerm);

                    break;
                }

                case 's':
                {
                    if (!GlobalInfo.BlackFilters[FilterType.QuestionTitleBlackSpam].Terms.Contains(oldTerm)) { return new ReplyMessage("`Blacklist term does not exist.`"); }

                    GlobalInfo.BlackFilters[FilterType.QuestionTitleBlackSpam].EditTerm(oldTerm, newTerm);

                    break;
                }

                case 'l':
                {
                    if (!GlobalInfo.BlackFilters[FilterType.QuestionTitleBlackLQ].Terms.Contains(oldTerm)) { return new ReplyMessage("`Blacklist term does not exist.`"); }

                    GlobalInfo.BlackFilters[FilterType.QuestionTitleBlackLQ].EditTerm(oldTerm, newTerm);

                    break;
                }

                case 'n':
                {
                    if (!GlobalInfo.BlackFilters[FilterType.QuestionTitleBlackName].Terms.Contains(oldTerm)) { return new ReplyMessage("`Blacklist term does not exist.`"); }

                    GlobalInfo.BlackFilters[FilterType.QuestionTitleBlackName].EditTerm(oldTerm, newTerm);

                    break;
                }
            }

            return new ReplyMessage("`Term updated.`");
        }

        private static ReplyMessage EditBQBTerm(string command)
        {
            var editCommand = command.Remove(0, 10);

            if (!editCommand.StartsWith("off") && !editCommand.StartsWith("spam") && !editCommand.StartsWith("lq")) { return new ReplyMessage("`Command not recognised.`"); }

            var startIndex = command.IndexOf(' ') + 1;
            var delimiterIndex = command.IndexOf("¬¬¬", StringComparison.Ordinal);
            var oldTerm = new Regex(command.Substring(startIndex, delimiterIndex - startIndex));
            var newTerm = new Regex(command.Remove(0, delimiterIndex + 3), RegexOptions.Compiled);

            if (newTerm.IsFlakRegex()) { return new ReplyMessage("`ReDoS detected (term not updated).`"); }

            switch (editCommand.ToLowerInvariant()[0])
            {
                case 'o':
                {
                    if (!GlobalInfo.BlackFilters[FilterType.QuestionBodyBlackOff].Terms.Contains(oldTerm)) { return new ReplyMessage("`Blacklist term does not exist.`"); }

                    GlobalInfo.BlackFilters[FilterType.QuestionBodyBlackOff].EditTerm(oldTerm, newTerm);

                    break;
                }

                case 's':
                {
                    if (!GlobalInfo.BlackFilters[FilterType.QuestionBodyBlackSpam].Terms.Contains(oldTerm)) { return new ReplyMessage("`Blacklist term does not exist.`"); }

                    GlobalInfo.BlackFilters[FilterType.QuestionBodyBlackSpam].EditTerm(oldTerm, newTerm);

                    break;
                }

                case 'l':
                {
                    if (!GlobalInfo.BlackFilters[FilterType.QuestionBodyBlackLQ].Terms.Contains(oldTerm)) { return new ReplyMessage("`Blacklist term does not exist.`"); }

                    GlobalInfo.BlackFilters[FilterType.QuestionBodyBlackLQ].EditTerm(oldTerm, newTerm);

                    break;
                }
            }

            return new ReplyMessage("`Term updated.`");
        }

        private static ReplyMessage EditWQTTerm(string command)
        {
            var editCommand = command.Remove(0, 10);

            if (!editCommand.StartsWith("off") && !editCommand.StartsWith("spam") && !editCommand.StartsWith("lq") && !editCommand.StartsWith("name")) { return new ReplyMessage("`Command not recognised.`"); }

            var firstSpace = command.IndexOf(' ');
            var secondSpace = command.IndexOf(' ', firstSpace + 1);
            var delimiter = command.IndexOf("¬¬¬", StringComparison.Ordinal);

            var site = command.Substring(firstSpace + 1, secondSpace - firstSpace - 1);
            var oldTerm = new Regex(command.Substring(secondSpace + 1, delimiter - secondSpace - 1));
            var newTerm = new Regex(command.Remove(0, delimiter + 3), RegexOptions.Compiled);

            if (newTerm.IsFlakRegex()) { return new ReplyMessage("`ReDoS detected (term not updated).`"); }

            switch (editCommand.ToLowerInvariant()[0])
            {
                case 'o':
                {
                    if (!GlobalInfo.WhiteFilters[FilterType.QuestionTitleWhiteOff].Terms.Contains(oldTerm, site)) { return new ReplyMessage("`Whitelist term does not exist.`"); }

                    GlobalInfo.WhiteFilters[FilterType.QuestionTitleWhiteOff].EditTerm(oldTerm, newTerm, site);

                    break;
                }

                case 's':
                {
                    if (!GlobalInfo.WhiteFilters[FilterType.QuestionTitleWhiteSpam].Terms.Contains(oldTerm, site)) { return new ReplyMessage("`Whitelist term does not exist.`"); }

                    GlobalInfo.WhiteFilters[FilterType.QuestionTitleWhiteSpam].EditTerm(oldTerm, newTerm, site);

                    break;
                }

                case 'l':
                {
                    if (!GlobalInfo.WhiteFilters[FilterType.QuestionTitleWhiteLQ].Terms.Contains(oldTerm, site)) { return new ReplyMessage("`Whitelist term does not exist.`"); }

                    GlobalInfo.WhiteFilters[FilterType.QuestionTitleWhiteLQ].EditTerm(oldTerm, newTerm, site);

                    break;
                }

                case 'n':
                {
                    if (!GlobalInfo.WhiteFilters[FilterType.QuestionTitleWhiteName].Terms.Contains(oldTerm, site)) { return new ReplyMessage("`Whitelist term does not exist.`"); }

                    GlobalInfo.WhiteFilters[FilterType.QuestionTitleWhiteName].EditTerm(oldTerm, newTerm, site);

                    break;
                }
            }

            return new ReplyMessage("`Term updated.`");
        }

        private static ReplyMessage EditWQBTerm(string command)
        {
            var editCommand = command.Remove(0, 10);

            if (!editCommand.StartsWith("off") && !editCommand.StartsWith("spam") && !editCommand.StartsWith("lq")) { return new ReplyMessage("`Command not recognised.`"); }

            var firstSpace = command.IndexOf(' ');
            var secondSpace = command.IndexOf(' ', firstSpace + 1);
            var delimiter = command.IndexOf("¬¬¬", StringComparison.Ordinal);

            var site = command.Substring(firstSpace + 1, secondSpace - firstSpace - 1);
            var oldTerm = new Regex(command.Substring(secondSpace + 1, delimiter - secondSpace - 1));
            var newTerm = new Regex(command.Remove(0, delimiter + 3), RegexOptions.Compiled);

            if (newTerm.IsFlakRegex()) { return new ReplyMessage("`ReDoS detected (term not updated).`"); }

            switch (editCommand.ToLowerInvariant()[0])
            {
                case 'o':
                {
                    if (!GlobalInfo.WhiteFilters[FilterType.QuestionBodyWhiteOff].Terms.Contains(oldTerm, site)) { return new ReplyMessage("`Whitelist term does not exist.`"); }

                    GlobalInfo.WhiteFilters[FilterType.QuestionBodyWhiteOff].EditTerm(oldTerm, newTerm, site);

                    break;
                }

                case 's':
                {
                    if (!GlobalInfo.WhiteFilters[FilterType.QuestionBodyWhiteSpam].Terms.Contains(oldTerm, site)) { return new ReplyMessage("`Whitelist term does not exist.`"); }

                    GlobalInfo.WhiteFilters[FilterType.QuestionBodyWhiteSpam].EditTerm(oldTerm, newTerm, site);

                    break;
                }

                case 'l':
                {
                    if (!GlobalInfo.WhiteFilters[FilterType.QuestionBodyWhiteLQ].Terms.Contains(oldTerm, site)) { return new ReplyMessage("`Whitelist term does not exist.`"); }

                    GlobalInfo.WhiteFilters[FilterType.QuestionBodyWhiteLQ].EditTerm(oldTerm, newTerm, site);

                    break;
                }
            }

            return new ReplyMessage("`Term updated.`");
        }

        private static ReplyMessage EditBATerm(string command)
        {
            var editCommand = command.Remove(0, 9);

            if (!editCommand.StartsWith("off") && !editCommand.StartsWith("spam") && !editCommand.StartsWith("lq") && !editCommand.StartsWith("name")) { return new ReplyMessage("`Command not recognised.`"); }

            var startIndex = command.IndexOf(' ') + 1;
            var delimiterIndex = command.IndexOf("¬¬¬", StringComparison.Ordinal);
            var oldTerm = new Regex(command.Substring(startIndex, delimiterIndex - startIndex));
            var newTerm = new Regex(command.Remove(0, delimiterIndex + 3), RegexOptions.Compiled);

            if (newTerm.IsFlakRegex()) { return new ReplyMessage("`ReDoS detected (term not updated).`"); }

            switch (editCommand.ToLowerInvariant()[0])
            {
                case 'o':
                {
                    if (!GlobalInfo.BlackFilters[FilterType.AnswerBlackOff].Terms.Contains(oldTerm)) { return new ReplyMessage("`Blacklist term does not exist.`"); }

                    GlobalInfo.BlackFilters[FilterType.AnswerBlackOff].EditTerm(oldTerm, newTerm);

                    break;
                }

                case 's':
                {
                    if (!GlobalInfo.BlackFilters[FilterType.AnswerBlackSpam].Terms.Contains(oldTerm)) { return new ReplyMessage("`Blacklist term does not exist.`"); }

                    GlobalInfo.BlackFilters[FilterType.AnswerBlackSpam].EditTerm(oldTerm, newTerm);

                    break;
                }

                case 'l':
                {
                    if (!GlobalInfo.BlackFilters[FilterType.AnswerBlackLQ].Terms.Contains(oldTerm)) { return new ReplyMessage("`Blacklist term does not exist.`"); }

                    GlobalInfo.BlackFilters[FilterType.AnswerBlackLQ].EditTerm(oldTerm, newTerm);

                    break;
                }

                case 'n':
                {
                    if (!GlobalInfo.BlackFilters[FilterType.AnswerBlackName].Terms.Contains(oldTerm)) { return new ReplyMessage("`Blacklist term does not exist.`"); }

                    GlobalInfo.BlackFilters[FilterType.AnswerBlackName].EditTerm(oldTerm, newTerm);

                    break;
                }
            }

            return new ReplyMessage("`Term updated.`");
        }

        private static ReplyMessage EditWATerm(string command)
        {
            var editCommand = command.Remove(0, 9);

            if (!editCommand.StartsWith("off") && !editCommand.StartsWith("spam") && !editCommand.StartsWith("lq") && !editCommand.StartsWith("name")) { return new ReplyMessage("`Command not recognised.`"); }

            var firstSpace = command.IndexOf(' ');
            var secondSpace = command.IndexOf(' ', firstSpace + 1);
            var delimiter = command.IndexOf("¬¬¬", StringComparison.Ordinal);

            var site = command.Substring(firstSpace + 1, secondSpace - firstSpace - 1);
            var oldTerm = new Regex(command.Substring(secondSpace + 1, delimiter - secondSpace - 1));
            var newTerm = new Regex(command.Remove(0, delimiter + 3), RegexOptions.Compiled);

            if (newTerm.IsFlakRegex()) { return new ReplyMessage("`ReDoS detected (term not updated).`"); }

            switch (editCommand.ToLowerInvariant()[0])
            {
                case 'o':
                {
                    if (!GlobalInfo.WhiteFilters[FilterType.AnswerWhiteOff].Terms.Contains(oldTerm, site)) { return new ReplyMessage("`Whitelist term does not exist.`"); }

                    GlobalInfo.WhiteFilters[FilterType.AnswerWhiteOff].EditTerm(oldTerm, newTerm, site);

                    break;
                }

                case 's':
                {
                    if (!GlobalInfo.WhiteFilters[FilterType.AnswerWhiteSpam].Terms.Contains(oldTerm, site)) { return new ReplyMessage("`Whitelist term does not exist.`"); }

                    GlobalInfo.WhiteFilters[FilterType.AnswerWhiteSpam].EditTerm(oldTerm, newTerm, site);

                    break;
                }

                case 'l':
                {
                    if (!GlobalInfo.WhiteFilters[FilterType.AnswerWhiteLQ].Terms.Contains(oldTerm, site)) { return new ReplyMessage("`Whitelist term does not exist.`"); }

                    GlobalInfo.WhiteFilters[FilterType.AnswerWhiteLQ].EditTerm(oldTerm, newTerm, site);

                    break;
                }

                case 'n':
                {
                    if (!GlobalInfo.WhiteFilters[FilterType.AnswerWhiteName].Terms.Contains(oldTerm, site)) { return new ReplyMessage("`Whitelist term does not exist.`"); }

                    GlobalInfo.WhiteFilters[FilterType.AnswerWhiteName].EditTerm(oldTerm, newTerm, site);

                    break;
                }
            }

            return new ReplyMessage("`Term updated.`");
        }

        # endregion

        # region FP/TP(A) commands

        private static ReplyMessage FalsePositive()
        {
            if (report.Type == PostType.BadTagUsed) { return new ReplyMessage(""); }

            if (report.Type == PostType.Spam)
            {
                var questionReport = new Regex(@"^\*\*(Low Quality|Spam|Offensive)\*\* \*\*Q\*\*|\*\*Bad Tag Used\*\*", RegexOptions.CultureInvariant);

                if (questionReport.IsMatch(room[message.ParentID].Content))
                {
                    var p = PostRetriever.GetQuestion(post.Url);
                    QuestionAnalysis info;

                    var res = QuestionAnalyser.IsLowQuality(p, out info);

                    if (res)
                    {
                        var newMessage = MessageGenerator.GetQReport(info, p);

                        var postedMessage = GlobalInfo.PrimaryRoom.PostMessage("**Low Quality** " + newMessage);

                        GlobalInfo.PostedReports.Add(postedMessage.ID, new MessageInfo
                        {
                            Message = postedMessage,
                            Post = p,
                            Report = info
                        });
                    }
                }
                else
                {
                    var p = PostRetriever.GetAnswer(post.Url);
                    AnswerAnalysis info;

                    var res = AnswerAnalyser.IsLowQuality(p,out info);

                    if (res)
                    {
                        var newMessage = MessageGenerator.GetAReport(info, p);

                        var postedMessage = GlobalInfo.PrimaryRoom.PostMessage("**Low Quality** " + newMessage);

                        GlobalInfo.PostedReports.Add(postedMessage.ID, new MessageInfo
                        {
                            Message = postedMessage,
                            Post = p,
                            Report = info
                        });
                    }
                }
            }

            Pham.RegisterFP(post, report);

            return room.EditMessage(message.ParentID, "---" + room[message.ParentID].Content + "---") ? new ReplyMessage("") : new ReplyMessage("`FP acknowledged.`");
        }

        private static ReplyMessage TruePositive(string command)
        {
            string postBack = null;

            if (command.ToLowerInvariant().StartsWith("tpa"))
            {
                var m = room[message.ParentID].Content;

                if (report.Type == PostType.Offensive || command.ToLowerInvariant() == "tpa clean")
                {
                    m = ReportCleaner.GetCleanReport(message.ParentID);
                }

                foreach (var secondaryRoom in GlobalInfo.ChatClient.Rooms.Where(r => r.ID != GlobalInfo.PrimaryRoom.ID))
                {
                    var postedMessage = secondaryRoom.PostMessage(m);

                    GlobalInfo.PostedReports.Add(postedMessage.ID, new MessageInfo
                    {
                        Message = postedMessage, Post = post, Report = report
                    });
                }

                postBack = " ***TPA Acknowledged***.";
            }

            if (report.Type == PostType.Spam)
            {
                GlobalInfo.Spammers.Add(new Spammer(post.Site, post.AuthorName));
            }

            if (report.Type != PostType.BadTagUsed)
            {
                Pham.RegisterTP(post, report);
            }

            var reportMessage = room[message.ParentID].Content;
            reportMessage = reportMessage.Remove(reportMessage.Length - 1);

            return room.EditMessage(message.ParentID, reportMessage + (postBack ?? " ***TP Acknowledged***.")) ? new ReplyMessage("") : new ReplyMessage("`TP acknowledged.`");
        }

        # endregion

        # region Auto commands

        private static ReplyMessage AutoBQTTerm(string command)
        {
            var editCommand = command.Remove(0, 10);

            if (!editCommand.StartsWith("off") && !editCommand.StartsWith("spam") && !editCommand.StartsWith("lq") && !editCommand.StartsWith("name")) { return new ReplyMessage("`Command not recognised.`"); }

            var startIndex = command.IndexOf(' ') + 1;
            var persistence = command[startIndex - 2] == 'p' || command[startIndex - 2] == 'P';
            var term = new Regex(command.Substring(startIndex, command.Length - startIndex));

            if (editCommand.StartsWith("off"))
            {
                if (!GlobalInfo.BlackFilters[FilterType.QuestionTitleBlackOff].Terms.Contains(term)) { return new ReplyMessage("`Blacklist term does not exist.`"); }

                var isAuto = GlobalInfo.BlackFilters[FilterType.QuestionTitleBlackOff].Terms.First(t => t.Regex.ToString() == term.ToString()).IsAuto;

                GlobalInfo.BlackFilters[FilterType.QuestionTitleBlackOff].SetAuto(term, !isAuto, persistence);

                return new ReplyMessage("`Auto toggled (now " + !isAuto + ").`");
            }

            if (editCommand.StartsWith("spam"))
            {
                if (!GlobalInfo.BlackFilters[FilterType.QuestionTitleBlackSpam].Terms.Contains(term)) { return new ReplyMessage("`Blacklist term does not exist.`"); }

                var isAuto = GlobalInfo.BlackFilters[FilterType.QuestionTitleBlackSpam].Terms.First(t => t.Regex.ToString() == term.ToString()).IsAuto;

                GlobalInfo.BlackFilters[FilterType.QuestionTitleBlackSpam].SetAuto(term, !isAuto, persistence);

                return new ReplyMessage("`Auto toggled (now " + !isAuto + ").`");
            }

            if (editCommand.StartsWith("lq"))
            {
                if (!GlobalInfo.BlackFilters[FilterType.QuestionTitleBlackLQ].Terms.Contains(term)) { return new ReplyMessage("`Blacklist term does not exist.`"); }

                var isAuto = GlobalInfo.BlackFilters[FilterType.QuestionTitleBlackLQ].Terms.First(t => t.Regex.ToString() == term.ToString()).IsAuto;

                GlobalInfo.BlackFilters[FilterType.QuestionTitleBlackLQ].SetAuto(term, !isAuto, persistence);

                return new ReplyMessage("`Auto toggled (now " + !isAuto + ").`");
            }

            if (editCommand.StartsWith("name"))
            {
                if (!GlobalInfo.BlackFilters[FilterType.QuestionTitleBlackName].Terms.Contains(term)) { return new ReplyMessage("`Blacklist term does not exist.`"); }

                var isAuto = GlobalInfo.BlackFilters[FilterType.QuestionTitleBlackName].Terms.First(t => t.Regex.ToString() == term.ToString()).IsAuto;

                GlobalInfo.BlackFilters[FilterType.QuestionTitleBlackName].SetAuto(term, !isAuto, persistence);

                return new ReplyMessage("`Auto toggled (now " + !isAuto + ").`");
            }

            return new ReplyMessage("`Command not recognised.`");
        }

        private static ReplyMessage AutoBQBTerm(string command)
        {
            var editCommand = command.Remove(0, 10);

            if (!editCommand.StartsWith("off") && !editCommand.StartsWith("spam") && !editCommand.StartsWith("lq")) { return new ReplyMessage("`Command not recognised.`"); }

            var startIndex = command.IndexOf(' ') + 1;
            var persistence = command[startIndex - 2] == 'p' || command[startIndex - 2] == 'P';
            var term = new Regex(command.Substring(startIndex, command.Length - startIndex));

            if (editCommand.StartsWith("off"))
            {
                if (!GlobalInfo.BlackFilters[FilterType.QuestionBodyBlackOff].Terms.Contains(term)) { return new ReplyMessage("`Blacklist term does not exist.`"); }

                var isAuto = GlobalInfo.BlackFilters[FilterType.QuestionBodyBlackOff].Terms.First(t => t.Regex.ToString() == term.ToString()).IsAuto;

                GlobalInfo.BlackFilters[FilterType.QuestionBodyBlackOff].SetAuto(term, !isAuto, persistence);

                return new ReplyMessage("`Auto toggled (now " + !isAuto + ").`");
            }

            if (editCommand.StartsWith("spam"))
            {
                if (!GlobalInfo.BlackFilters[FilterType.QuestionBodyBlackSpam].Terms.Contains(term)) { return new ReplyMessage("`Blacklist term does not exist.`"); }

                var isAuto = GlobalInfo.BlackFilters[FilterType.QuestionBodyBlackSpam].Terms.First(t => t.Regex.ToString() == term.ToString()).IsAuto;

                GlobalInfo.BlackFilters[FilterType.QuestionBodyBlackSpam].SetAuto(term, !isAuto, persistence);

                return new ReplyMessage("`Auto toggled (now " + !isAuto + ").`");
            }

            if (editCommand.StartsWith("lq"))
            {
                if (!GlobalInfo.BlackFilters[FilterType.QuestionBodyBlackLQ].Terms.Contains(term)) { return new ReplyMessage("`Blacklist term does not exist.`"); }

                var isAuto = GlobalInfo.BlackFilters[FilterType.QuestionBodyBlackLQ].Terms.First(t => t.Regex.ToString() == term.ToString()).IsAuto;

                GlobalInfo.BlackFilters[FilterType.QuestionBodyBlackLQ].SetAuto(term, !isAuto, persistence);

                return new ReplyMessage("`Auto toggled (now " + !isAuto + ").`");
            }

            return new ReplyMessage("`Term updated.`");
        }

        private static ReplyMessage AutoBATerm(string command)
        {
            var editCommand = command.Remove(0, 9);

            if (!editCommand.StartsWith("off") && !editCommand.StartsWith("spam") && !editCommand.StartsWith("lq") && !editCommand.StartsWith("name")) { return new ReplyMessage("`Command not recognised.`"); }

            var startIndex = command.IndexOf(' ') + 1;
            var persistence = command[startIndex - 2] == 'p' || command[startIndex - 2] == 'P';
            var term = new Regex(command.Substring(startIndex, command.Length - startIndex));

            if (editCommand.StartsWith("off"))
            {
                if (!GlobalInfo.BlackFilters[FilterType.AnswerBlackOff].Terms.Contains(term)) { return new ReplyMessage("`Blacklist term does not exist.`"); }

                var isAuto = GlobalInfo.BlackFilters[FilterType.AnswerBlackOff].Terms.First(t => t.Regex.ToString() == term.ToString()).IsAuto;

                GlobalInfo.BlackFilters[FilterType.AnswerBlackOff].SetAuto(term, !isAuto, persistence);

                return new ReplyMessage("`Auto toggled (now " + !isAuto + ").`");
            }

            if (editCommand.StartsWith("spam"))
            {
                if (!GlobalInfo.BlackFilters[FilterType.AnswerBlackSpam].Terms.Contains(term)) { return new ReplyMessage("`Blacklist term does not exist.`"); }

                var isAuto = GlobalInfo.BlackFilters[FilterType.AnswerBlackSpam].Terms.First(t => t.Regex.ToString() == term.ToString()).IsAuto;

                GlobalInfo.BlackFilters[FilterType.AnswerBlackSpam].SetAuto(term, !isAuto, persistence);

                return new ReplyMessage("`Auto toggled (now " + !isAuto + ").`");
            }

            if (editCommand.StartsWith("lq"))
            {
                if (!GlobalInfo.BlackFilters[FilterType.AnswerBlackLQ].Terms.Contains(term)) { return new ReplyMessage("`Blacklist term does not exist.`"); }

                var isAuto = GlobalInfo.BlackFilters[FilterType.AnswerBlackLQ].Terms.First(t => t.Regex.ToString() == term.ToString()).IsAuto;

                GlobalInfo.BlackFilters[FilterType.AnswerBlackLQ].SetAuto(term, !isAuto, persistence);

                return new ReplyMessage("`Auto toggled (now " + !isAuto + ").`");
            }

            if (editCommand.StartsWith("name"))
            {
                if (!GlobalInfo.BlackFilters[FilterType.AnswerBlackName].Terms.Contains(term)) { return new ReplyMessage("`Blacklist term does not exist.`"); }

                var isAuto = GlobalInfo.BlackFilters[FilterType.AnswerBlackName].Terms.First(t => t.Regex.ToString() == term.ToString()).IsAuto;

                GlobalInfo.BlackFilters[FilterType.AnswerBlackName].SetAuto(term, !isAuto, persistence);

                return new ReplyMessage("`Auto toggled (now " + !isAuto + ").`");
            }

            return new ReplyMessage("`Term updated.`");
        }

        # endregion

        private static ReplyMessage AddTag(string command)
        {
            var tagCommand = command.Remove(0, command.IndexOf("tag", StringComparison.Ordinal) + 4);

            if (tagCommand.Count(c => c == ' ') != 1 && tagCommand.Count(c => c == ' ') != 3) { return new ReplyMessage("`Command not recognised.`"); }

            var site = tagCommand.Substring(0, tagCommand.IndexOf(" ", StringComparison.Ordinal));
            var metaPost = "";
            string tag;

            if (tagCommand.Count(c => c == ' ') == 3)
            {
                tag = tagCommand.Substring(site.Length + 1, tagCommand.IndexOf(" ", site.Length + 1, StringComparison.Ordinal) - 1 - site.Length);

                metaPost = tagCommand.Substring(tagCommand.LastIndexOf(" ", StringComparison.Ordinal) + 1);
            }
            else
            {
                tag = tagCommand.Remove(0, tagCommand.IndexOf(" ", StringComparison.Ordinal) + 1);
            }

            if (GlobalInfo.BadTagDefinitions.BadTags.ContainsKey(site) && GlobalInfo.BadTagDefinitions.BadTags[site].ContainsKey(tag)) { return new ReplyMessage("`Tag already exists.`"); }

            GlobalInfo.BadTagDefinitions.AddTag(site, tag, metaPost);

            return new ReplyMessage("`Tag added.`");
        }

        private static ReplyMessage RemoveTag(string command)
        {
            var tagCommand = command.Remove(0, command.IndexOf("tag", StringComparison.Ordinal) + 4);

            if (tagCommand.Count(c => c == ' ') != 1) { return new ReplyMessage("`Command not recognised.`"); }

            var site = tagCommand.Substring(0, tagCommand.IndexOf(" ", StringComparison.Ordinal));
            var tag = tagCommand.Remove(0, tagCommand.IndexOf(" ", StringComparison.Ordinal) + 1);

            if (GlobalInfo.BadTagDefinitions.BadTags.ContainsKey(site))
            {
                if (GlobalInfo.BadTagDefinitions.BadTags[site].ContainsKey(tag))
                {
                    GlobalInfo.BadTagDefinitions.RemoveTag(site, tag);

                    return new ReplyMessage("`Tag removed.`");
                }

                return new ReplyMessage("`Tag does not exist.`");
            }

            return new ReplyMessage("`Site does not exist.`");
        }

        private static ReplyMessage CleanMessage()
        {
            try
            {
                var newMessage = ReportCleaner.GetCleanReport(message.ParentID);

                room.EditMessage(message.ParentID, newMessage);
            }
            catch (Exception)
            {
                DeleteMessage();
            }
            
            return new ReplyMessage("");
        }

        private static ReplyMessage DeleteMessage()
        {
            room.DeleteMessage(message.ParentID);

            return new ReplyMessage("", false);
        }

        private static ReplyMessage Ask()
        {
            var newReport = Regex.Replace(GlobalInfo.PostedReports[message.ParentID].Message.Content, @"\*\* \(\d*(\.\d)?\%\)\:", "?**:");

            foreach (var secondaryRoom in GlobalInfo.ChatClient.Rooms.Where(r => r.ID != GlobalInfo.PrimaryRoom.ID))
            {
                var postedMessage = secondaryRoom.PostMessage(newReport);

                GlobalInfo.PostedReports.Add(postedMessage.ID, new MessageInfo
                {
                    Message = postedMessage,
                    Post = post,
                    Report = report
                });
            }

            return new ReplyMessage("");
        }

        # endregion

        # region Owner commands.

        private static ReplyMessage SetStatus(string command)
        {
            var newStatus = command.Remove(0, 10).Trim();

            GlobalInfo.Status = newStatus;

            return new ReplyMessage("`Status updated.`");
        }

        private static ReplyMessage AddUser(string command)
        {
            var id = int.Parse(command.Replace("add user", "").Trim());

            if (UserAccess.CommandAccessUsers.Contains(id)) { return new ReplyMessage("`User already has command access.`"); }

            UserAccess.AddUser(id);

            return new ReplyMessage("`User added.`");
        }

        private static ReplyMessage BanUser(string command)
        {
            var id = command.Replace("ban user", "").Trim();

            if (BannedUsers.IsUserBanned(id)) { return new ReplyMessage("`User is already banned.`"); }

            return new ReplyMessage(BannedUsers.AddUser(id) ? "`User banned.`" : "`Warning: the banned users file is missing (unable to add user). All commands have been disabled until the issue has been resolved.`");
        }

        private static ReplyMessage SetAccuracyThreshold(string command)
        {
            if (command.IndexOf(" ", StringComparison.Ordinal) == -1 || command.All(c => !Char.IsDigit(c))) { return new ReplyMessage("`Command not recognised.`"); }

            var newLimit = command.Remove(0, 10);

            GlobalInfo.AccuracyThreshold = float.Parse(newLimit, CultureInfo.InvariantCulture);

            return new ReplyMessage("`Accuracy threshold updated.`");
        }

        private static ReplyMessage FullScan()
        {
            if (GlobalInfo.FullScanEnabled)
            {
                GlobalInfo.FullScanEnabled = false;

                return new ReplyMessage("`Full scan disabled.`");
            }

            GlobalInfo.FullScanEnabled = true;

            return new ReplyMessage("`Full scan enabled.`");
        }

        private static ReplyMessage ResumeBot()
        {
            GlobalInfo.BotRunning = true;

            return new ReplyMessage("`Phamhilator™ resumed.`");
        }

        private static ReplyMessage PauseBot()
        {
            GlobalInfo.BotRunning = false;

            return new ReplyMessage("`Phamhilator™ paused.`");
        }

        # endregion
    }
}
