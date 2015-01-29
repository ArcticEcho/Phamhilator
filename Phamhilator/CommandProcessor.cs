using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using ChatExchangeDotNet;
using JsonFx.Json;
using JsonFx.Serialization;
using Microsoft.VisualBasic.Devices;



namespace Phamhilator
{
    public static class CommandProcessor
    {
        private static Room room;
        private static Message message;
        private static PostAnalysis analysis;
        private static Post post;
        private static bool fileMissingWarningMessagePosted;
        private const RegexOptions regexOptions = RegexOptions.Compiled | RegexOptions.CultureInvariant;
        private static readonly Random random = new Random();
        private static readonly HashSet<ChatCommand> commands = new HashSet<ChatCommand>
        {
            #region Normal user commands.

            new ChatCommand(new Regex("(?i)^status$", regexOptions), command => new[]
            {
                new ReplyMessage(String.Concat("`", Config.Status, "`." /*" @ ", GlobalInfo.CommitFormatted, "(https://github.com/ArcticEcho/Phamhilator/commit/", GlobalInfo.CommitHash, ").")*/))
            }, CommandAccessLevel.NormalUser),
            new ChatCommand(new Regex("(?i)^(info(rmation)?|about)$", regexOptions), command => new[]
            {
                new ReplyMessage("[`Phamhilator`](https://github.com/ArcticEcho/Phamhilator/wiki) `is a` [`.NET`](http://en.wikipedia.org/wiki/.NET_Framework)-`based` [`internet bot`](http://en.wikipedia.org/wiki/Internet_bot) `written in` [`C#`](http://stackoverflow.com/questions/tagged/c%23) `which watches over` [`the /realtime tab`](http://stackexchange.com/questions?tab=realtime) `of` [`Stack Exchange`](http://stackexchange.com/)`. Owners: " + Config.UserAccess.OwnerNames + ".`")
            }, CommandAccessLevel.NormalUser),
            new ChatCommand(new Regex("(?i)^help$", regexOptions), command => new[]
            {
                new ReplyMessage("`See` [`here`](https://github.com/ArcticEcho/Phamhilator/wiki/Chat-Commands) `for a full list of commands.`")
            }, CommandAccessLevel.NormalUser),
            new ChatCommand(new Regex("(?i)^(help (add|del)|(add|del) help)$", regexOptions), command => new[]
            {
                new ReplyMessage("`To add or delete a term, use \">>(add/del)-(b/w)-(a/qt/qb)-(lq/spam/off/name) (if w, term's site name) {regex-term}\". To add or delete a tag, use \">>(add/remove) {site-name} {tag-name} {link}\".`")
            }, CommandAccessLevel.NormalUser),
            new ChatCommand(new Regex("(?i)^(help edit|edit help)$", regexOptions), command => new[]
            {
                new ReplyMessage("`To edit a term, use \">>edit-(b/w)-(a/qt/qb)-(lq/spam/off/name) (if w, term's site name) {old-term}¬¬¬{new-term}\".`")
            }, CommandAccessLevel.NormalUser),
            new ChatCommand(new Regex("(?i)^(help auto|auto help)$", regexOptions), command => new[]
            {
                new ReplyMessage("`To add an automatic term, use \">>auto-b-(a/qt/qb)-(lq/spam/off/name)(-p) {regex-term}\". Use \"-p\" if the change should persist past the bot's restart.`")
            }, CommandAccessLevel.NormalUser),
            new ChatCommand(new Regex("(?i)^(help list|list help|commands)$", regexOptions), command => new[]
            {
                new ReplyMessage("    @" + message.AuthorName.Replace(" ", "") + "\n    Supported commands: info, stats, status, env & log.\n    Supported replies: (fp/tp/tpa), why, ask, clean, del & log.\n    Owner-only commands: resume, pause, (add/ban)-user {user-id}, threshold {percentage}, kill-it-with-no-regrets-for-sure, full-scan & set-status {message}.", false)
            }, CommandAccessLevel.NormalUser),
            new ChatCommand(new Regex("(?i)^stats$", regexOptions), command => new []
            {
                new ReplyMessage("    @" + message.AuthorName.Replace(" ", "") + "\n    Posts caught (last 7 days): " + Stats.PostsCaught + ".\n    Terms: " + Stats.TermCount + ".\n    Posts checked: " + Stats.TotalCheckedPosts + ".\n    TPs acknowledged: " + Stats.TotalTPCount + ".\n    FPs acknowledged: " + Stats.TotalFPCount + ".\n    Uptime: " + (DateTime.UtcNow - Stats.UpTime) + ".", false)
            }, CommandAccessLevel.NormalUser),
            new ChatCommand(new Regex(@"(?i)^(terms|why)\b", regexOptions), command => new[]
            {
                GetTerms()
            }, CommandAccessLevel.NormalUser),
            new ChatCommand(new Regex("(?i)^env$", regexOptions), command =>
            {
                var totalMem = Math.Round(new ComputerInfo().TotalPhysicalMemory / 1024f / 1024f / 1024f, 1);

                return new[]
                {
                    new ReplyMessage("    @" + message.AuthorName.Replace(" ", "") + "\n    Cores (logical): " + Environment.ProcessorCount + "\n    Total RAM: " +  totalMem + "GB\n    OS: " + Environment.OSVersion.VersionString + "\n    64-bit: " + Environment.Is64BitOperatingSystem + "\n    CLR version: " + Environment.Version, false)
                };
            }, CommandAccessLevel.NormalUser),
            new ChatCommand(new Regex(@"(?i)^log( \d+)?$", regexOptions), command =>
            {
                var entryID = Regex.Replace(command, @"\D", "");

                if (String.IsNullOrEmpty(entryID))
                {
                    var messageReport = Stats.PostedReports.First(r => r.Message.ID == message.ParentID);

                    entryID = Stats.PostedReports.First(i => i.Post.Url == messageReport.Post.Url && i.Message.RoomID == Config.PrimaryRoom.ID).Message.ID.ToString(CultureInfo.InvariantCulture);
                }

                var logEntry = Config.Log.Entries.FirstOrDefault(i => Regex.Replace(i.ReportLink, @"\D", "") == entryID);

                if (logEntry == null)
                {
                    return new[] { new ReplyMessage("`Unable to find log entry with that ID.`") };
                }

                string link;

                if (Config.Log.EntryLinks.ContainsKey(entryID))
                {
                    link = Config.Log.EntryLinks[entryID];
                }
                else
                {
                    link = Hastebin.PostDocument(new JsonWriter(new DataWriterSettings { PrettyPrint = true, Tab = "    " }).Write(logEntry));
                    Config.Log.EntryLinks.Add(entryID, link);
                }

                return new[] { new ReplyMessage("`Log entry found. See` [`here`](" + link + ") `for details.`") };
            }, CommandAccessLevel.NormalUser),

            #region Toys.

            new ChatCommand(new Regex("(?i)^red button$", regexOptions), command => new[]
            {
                new ReplyMessage("`Warning: now launching " + random.Next(1, 101) + " anti-spammer homing missiles...`", false)
            }, CommandAccessLevel.NormalUser),

            new ChatCommand(new Regex("(?i)^panic$", regexOptions), command => new[]
            {
                new ReplyMessage("http://rack.0.mshcdn.com/media/ZgkyMDEzLzA2LzE4LzdjL0JlYWtlci4zOWJhOC5naWYKcAl0aHVtYgkxMjAweDk2MDA-/4a93e3c4/4a4/Beaker.gif")
            }, CommandAccessLevel.NormalUser),

            new ChatCommand(new Regex("(?i)^fox$", regexOptions), command => new[]
            {
                new ReplyMessage("http://i.stack.imgur.com/0qaHz.gif")
            }, CommandAccessLevel.NormalUser),

            #endregion

            #endregion

            #region Privileged user commands.

            #region Add term commands.

            new ChatCommand(new Regex(@"(?i)^add\-b\-(a|q[bt])\-(spam|off|name|lq) \S", regexOptions), command => new[]
            {
                AddBlackTerm(command)
            }, CommandAccessLevel.PrivilegedUser),
            new ChatCommand(new Regex(@"(?i)^add\-w\-(a|q[bt])\-(spam|off|name|lq) \S+ \S", regexOptions), command => new[]
            {
                AddWhiteTerm(command)
            }, CommandAccessLevel.PrivilegedUser),

            #endregion

            #region Edit term commands.
            
            new ChatCommand(new Regex(@"(?i)^edit\-b\-(a|q[bt])\-(spam|off|name|lq) .+¬¬¬.+$", regexOptions), command => new[]
            {
                EditBlackTerm(command)
            }, CommandAccessLevel.PrivilegedUser),
            new ChatCommand(new Regex(@"(?i)^edit\-w\-(a|q[bt])\-(spam|off|name|lq) \S+ .+¬¬¬.+$", regexOptions), command => new[]
            {
                EditWhiteTerm(command)
            }, CommandAccessLevel.PrivilegedUser),

            #endregion

            #region Remove term commands.

            new ChatCommand(new Regex(@"(?i)^del\-b\-(a|q[bt])\-(spam|off|name|lq) \S", regexOptions), command => new[]
            {
                DelBlackTerm(command)
            }, CommandAccessLevel.PrivilegedUser),
            new ChatCommand(new Regex(@"(?i)^del\-w\-(a|q[bt])\-(spam|off|name|lq) \S+ .*", regexOptions), command => new[]
            {
                DelWhiteTerm(command)
            }, CommandAccessLevel.PrivilegedUser),

            #endregion

            #region FP/TP commands.

            new ChatCommand(new Regex(@"^f(p|alse)((?!\s(why|del|clean)).)*$", regexOptions), command => new[]
            {
                FalsePositive()
            }, CommandAccessLevel.PrivilegedUser),
            new ChatCommand(new Regex(@"(?i)^fp why\b", regexOptions), command => new[]
            {
                FalsePositive(),
                GetTerms()
            }, CommandAccessLevel.PrivilegedUser),
            new ChatCommand(new Regex(@"(?i)^fp del\b", regexOptions), command => new[]
            {
                FalsePositive(),
                DeleteMessage()
            }, CommandAccessLevel.PrivilegedUser),
            new ChatCommand(new Regex(@"(?i)^fp clean\b", regexOptions), command => new[]
            {
                FalsePositive(),
                CleanMessage()
            }, CommandAccessLevel.PrivilegedUser),
            new ChatCommand(new Regex(@"^tpa?((?!\s(why|clean)).)*$", regexOptions), command => new[]
            {
                TruePositive(command)
            }, CommandAccessLevel.PrivilegedUser),
            new ChatCommand(new Regex(@"(?i)^tpa? why\b", regexOptions), command => new[]
            {
                TruePositive(command),
                GetTerms()
            }, CommandAccessLevel.PrivilegedUser),
            new ChatCommand(new Regex(@"(?i)^tpa? clean\b", regexOptions), command => new[]
            {
                TruePositive(command),
                CleanMessage()
            }, CommandAccessLevel.PrivilegedUser),

            #endregion

            # region Tag commands.

            new ChatCommand(new Regex(@"(?i)^add-tag \S+ \S+$", regexOptions), command => new[]
            {
                AddTag(command)
            }, CommandAccessLevel.PrivilegedUser),
            new ChatCommand(new Regex(@"(?i)^del-tag \S+ \S+$", regexOptions), command => new[]
            {
                RemoveTag(command)
            }, CommandAccessLevel.PrivilegedUser),

            # endregion

            new ChatCommand(new Regex(@"(?i)^auto\-b-(a|q[bt])\-(spam|off|name|lq)(\-p) \S?$", regexOptions), command => new[]
            {
                AutoBlackTerm(command)
            }, CommandAccessLevel.PrivilegedUser),
            new ChatCommand(new Regex(@"(?i)^clean\b", regexOptions), command => new[]
            {
                CleanMessage()
            }, CommandAccessLevel.PrivilegedUser),
            new ChatCommand(new Regex(@"(?i)^(del|gone|clear|hide)\b", regexOptions), command => new[]
            {
                DeleteMessage()
            }, CommandAccessLevel.PrivilegedUser),
            new ChatCommand(new Regex(@"(?i)^ask\b", regexOptions), command => new[]
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
            if (!Config.BannedUsers.SystemIsClear && !fileMissingWarningMessagePosted)
            {
                fileMissingWarningMessagePosted = true;

                return new[]
                {
                    new ReplyMessage("`Warning: the banned users file is missing. All commands have been disabled until the issue has been resolved.`")
                };
            }
            if (Config.BannedUsers.IsUserBanned(input.AuthorID.ToString(CultureInfo.InvariantCulture))) { return new[] { new ReplyMessage("", false) }; }

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

            if (Stats.PostedReports.Any(report => report != null && report.Message.ID == input.ParentID))
            {
                var report = Stats.PostedReports.First(r => r.Message.ID == input.ParentID);
                analysis = report.Analysis;
                post = report.Post;
            }
            else
            {
                analysis = null;
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
                        if (!Config.UserAccess.PrivUsers.Contains(input.AuthorID) && Config.UserAccess.Owners.All(user => user.ID != input.AuthorID))
                        {
                            return new[]
                            {
                                new ReplyMessage("`Access denied (this incident will be reported).`")
                            };
                        }

                        return requestedCmd.Command(command);
                    }

                    case CommandAccessLevel.Owner:
                    {
                        if (Config.UserAccess.Owners.All(user => user.ID != input.AuthorID))
                        {
                            return new[]
                            {
                                new ReplyMessage("`Access denied (this incident will be reported).`")
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
            if (analysis.BlackTermsFound.Count == 1)
            {
                var term = analysis.BlackTermsFound.First();
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

            foreach (var term in analysis.BlackTermsFound)
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

        private static ReplyMessage AddBlackTerm(string command)
        {
            var filterInfo = CommandParser.ParseFilterConfig(command);
            Regex term;

            try
            {
                term = new Regex(command.Substring(command.IndexOf(' ') + 1), RegexOptions.Compiled);
            }
            catch (Exception)
            {
                return new ReplyMessage("`Unable to add term. Invalid regex.`");
            }

            if (term.IsFlakRegex())
            { 
                return new ReplyMessage("`ReDoS detected (term not added).`");
            }

            if (Config.BlackFilters[filterInfo].Terms.Contains(term))
            { 
                return new ReplyMessage("`Term already exists.`");
            }

            Config.BlackFilters[filterInfo].AddTerm(new Term(filterInfo, term, Config.BlackFilters[filterInfo].HighestScore / 2));

            return new ReplyMessage("`Term added!`");
        }

        private static ReplyMessage AddWhiteTerm(string command)
        {
            var filterInfo = CommandParser.ParseFilterConfig(command);
            var firstSpace = command.IndexOf(' ');
            var secondSpace = command.IndexOf(' ', firstSpace + 1);
            var site = command.Substring(firstSpace + 1, secondSpace - firstSpace - 1);
            Regex term;

            try
            {
                term = new Regex(command.Substring(secondSpace + 1), regexOptions);
            }
            catch (Exception)
            {
                return new ReplyMessage("`Unable to add term. Invalid regex.`");
            }

            if (term.IsFlakRegex())
            {
                return new ReplyMessage("`ReDoS detected (term not added).`");
            }

            if (Config.WhiteFilters[filterInfo].Terms.Contains(term, site))
            { 
                return new ReplyMessage("`Term not found.`");
            }

            var score = Config.WhiteFilters[filterInfo].Terms.Where(t => t.Site == site).Select(t => t.Score).Max() / 2;

            Config.WhiteFilters[filterInfo].AddTerm(new Term(filterInfo, term, score, site));

            return new ReplyMessage("`Term added!`");
        }

        //private static ReplyMessage AddBQTTerm(string command)
        //{
        //    var addCommand = command.Remove(0, 9);
        //    var term = 

        //    if (term.IsFlakRegex()) { return new ReplyMessage("`ReDoS detected (term not added).`"); }

        //    switch (addCommand.ToLowerInvariant()[0])
        //    {
        //        case 'o':
        //        {

        //            break;
        //        }

        //        case 's':
        //        {
        //            if (GlobalInfo.BlackFilters[FilterClass.QuestionTitleBlackSpam].Terms.Contains(term)) { return new ReplyMessage("`Blacklist term already exists.`"); }

        //            GlobalInfo.BlackFilters[FilterClass.QuestionTitleBlackSpam].AddTerm(new Term(FilterClass.QuestionTitleBlackSpam, term, GlobalInfo.BlackFilters[FilterClass.QuestionTitleBlackSpam].AverageScore));

        //            break;
        //        }

        //        case 'l':
        //        {
        //            if (GlobalInfo.BlackFilters[FilterClass.QuestionTitleBlackLQ].Terms.Contains(term)) { return new ReplyMessage("`Blacklist term already exists.`"); }

        //            GlobalInfo.BlackFilters[FilterClass.QuestionTitleBlackLQ].AddTerm(new Term(FilterClass.QuestionTitleBlackLQ, term, GlobalInfo.BlackFilters[FilterClass.QuestionTitleBlackLQ].AverageScore));

        //            break;
        //        }

        //        case 'n':
        //        {
        //            if (GlobalInfo.BlackFilters[FilterClass.QuestionTitleBlackName].Terms.Contains(term)) { return new ReplyMessage("`Blacklist term already exists.`"); }

        //            GlobalInfo.BlackFilters[FilterClass.QuestionTitleBlackName].AddTerm(new Term(FilterClass.QuestionTitleBlackName, term, GlobalInfo.BlackFilters[FilterClass.QuestionTitleBlackName].AverageScore));

        //            break;
        //        }
        //    }

        //    return new ReplyMessage("`Blacklist term added.`");
        //}

        //private static ReplyMessage AddWQTTerm(string command)
        //{
        //    var addCommand = command.Remove(0, 9);
        //    var firstSpace = command.IndexOf(' ');
        //    var secondSpace = command.IndexOf(' ', firstSpace + 1);
        //    var term = new Regex(command.Substring(secondSpace + 1));
        //    var site = command.Substring(firstSpace + 1, secondSpace - firstSpace - 1);

        //    if (term.IsFlakRegex()) { return new ReplyMessage("`ReDoS detected (term not added).`"); }

        //    switch (addCommand.ToLowerInvariant()[0])
        //    {
        //        case 'o':
        //        {
        //            if (GlobalInfo.WhiteFilters[FilterClass.QuestionTitleWhiteOff].Terms.Contains(term, site)) { return new ReplyMessage("`Whitelist term already exists.`"); }

        //            var score = GlobalInfo.WhiteFilters[FilterClass.QuestionTitleWhiteOff].Terms.Where(t => t.Site == site).Select(t => t.Score).Average();

        //            GlobalInfo.WhiteFilters[FilterClass.QuestionTitleWhiteOff].AddTerm(new Term(FilterClass.QuestionTitleWhiteOff, term, score, site));

        //            break;
        //        }

        //        case 's':
        //        {
        //            if (GlobalInfo.WhiteFilters[FilterClass.QuestionTitleWhiteSpam].Terms.Contains(term, site)) { return new ReplyMessage("`Whitelist term already exists.`"); }

        //            var score = GlobalInfo.WhiteFilters[FilterClass.QuestionTitleWhiteSpam].Terms.Where(t => t.Site == site).Select(t => t.Score).Average();

        //            GlobalInfo.WhiteFilters[FilterClass.QuestionTitleWhiteSpam].AddTerm(new Term(FilterClass.QuestionTitleWhiteSpam, term, score, site));

        //            break;
        //        }

        //        case 'l':
        //        {
        //            if (GlobalInfo.WhiteFilters[FilterClass.QuestionTitleWhiteLQ].Terms.Contains(term, site)) { return new ReplyMessage("`Whitelist term already exists.`"); }

        //            var score = GlobalInfo.WhiteFilters[FilterClass.QuestionTitleWhiteLQ].Terms.Where(t => t.Site == site).Select(t => t.Score).Average();

        //            GlobalInfo.WhiteFilters[FilterClass.QuestionTitleWhiteLQ].AddTerm(new Term(FilterClass.QuestionTitleWhiteLQ, term, score, site));

        //            break;
        //        }

        //        case 'n':
        //        {
        //            if (GlobalInfo.WhiteFilters[FilterClass.QuestionTitleWhiteName].Terms.Contains(term, site)) { return new ReplyMessage("`Whitelist term already exists.`"); }

        //            var score = GlobalInfo.WhiteFilters[FilterClass.QuestionTitleWhiteName].Terms.Where(t => t.Site == site).Select(t => t.Score).Average();

        //            GlobalInfo.WhiteFilters[FilterClass.QuestionTitleWhiteName].AddTerm(new Term(FilterClass.QuestionTitleWhiteName, term, score, site));

        //            break;
        //        }
        //    }

        //    return new ReplyMessage("`Whitelist term added.`");
        //}

        //private static ReplyMessage AddBQBTerm(string command)
        //{
        //    var addCommand = command.Remove(0, 9);
        //    var term = new Regex(addCommand.Substring(addCommand.IndexOf(' ') + 1), RegexOptions.Compiled);

        //    if (term.IsFlakRegex()) { return new ReplyMessage("`ReDoS detected (term not added).`"); }

        //    switch (addCommand.ToLowerInvariant()[0])
        //    {
        //        case 'o':
        //        {
        //            if (GlobalInfo.BlackFilters[FilterClass.QuestionBodyBlackOff].Terms.Contains(term)) { return new ReplyMessage("`Blacklist term already exists.`"); }

        //            GlobalInfo.BlackFilters[FilterClass.QuestionBodyBlackOff].AddTerm(new Term(FilterClass.QuestionBodyBlackOff, term, GlobalInfo.BlackFilters[FilterClass.QuestionBodyBlackOff].AverageScore));

        //            break;
        //        }

        //        case 's':
        //        {
        //            if (GlobalInfo.BlackFilters[FilterClass.QuestionBodyBlackSpam].Terms.Contains(term)) { return new ReplyMessage("`Blacklist term already exists.`"); }

        //            GlobalInfo.BlackFilters[FilterClass.QuestionBodyBlackSpam].AddTerm(new Term(FilterClass.QuestionBodyBlackSpam, term, GlobalInfo.BlackFilters[FilterClass.QuestionBodyBlackSpam].AverageScore));

        //            break;
        //        }

        //        case 'l':
        //        {
        //            if (GlobalInfo.BlackFilters[FilterClass.QuestionBodyBlackLQ].Terms.Contains(term)) { return new ReplyMessage("`Blacklist term already exists.`"); }

        //            GlobalInfo.BlackFilters[FilterClass.QuestionBodyBlackLQ].AddTerm(new Term(FilterClass.QuestionBodyBlackLQ, term, GlobalInfo.BlackFilters[FilterClass.QuestionBodyBlackLQ].AverageScore));

        //            break;
        //        }
        //    }

        //    return new ReplyMessage("`Blacklist term added.`");
        //}

        //private static ReplyMessage AddWQBTerm(string command)
        //{
        //    var addCommand = command.Remove(0, 9);
        //    var firstSpace = command.IndexOf(' ');
        //    var secondSpace = command.IndexOf(' ', firstSpace + 1);
        //    var term = new Regex(command.Substring(secondSpace + 1));
        //    var site = command.Substring(firstSpace + 1, secondSpace - firstSpace - 1);

        //    if (term.IsFlakRegex()) { return new ReplyMessage("`ReDoS detected (term not added).`"); }

        //    switch (addCommand.ToLowerInvariant()[0])
        //    {
        //        case 'o':
        //        {
        //            if (GlobalInfo.WhiteFilters[FilterClass.QuestionBodyWhiteOff].Terms.Contains(term, site)) { return new ReplyMessage("`Whitelist term already exists.`"); }

        //            var score = GlobalInfo.WhiteFilters[FilterClass.QuestionBodyWhiteOff].Terms.Where(t => t.Site == site).Select(t => t.Score).Average();

        //            GlobalInfo.WhiteFilters[FilterClass.QuestionBodyWhiteOff].AddTerm(new Term(FilterClass.QuestionBodyWhiteOff, term, score, site));

        //            break;
        //        }

        //        case 's':
        //        {
        //            if (GlobalInfo.WhiteFilters[FilterClass.QuestionBodyWhiteSpam].Terms.Contains(term, site)) { return new ReplyMessage("`Whitelist term already exists.`"); }

        //            var score = GlobalInfo.WhiteFilters[FilterClass.QuestionBodyWhiteSpam].Terms.Where(t => t.Site == site).Select(t => t.Score).Average();

        //            GlobalInfo.WhiteFilters[FilterClass.QuestionBodyWhiteSpam].AddTerm(new Term(FilterClass.QuestionBodyWhiteSpam, term, score, site));

        //            break;
        //        }

        //        case 'l':
        //        {
        //            if (GlobalInfo.WhiteFilters[FilterClass.QuestionBodyWhiteLQ].Terms.Contains(term, site)) { return new ReplyMessage("`Whitelist term already exists.`"); }

        //            var score = GlobalInfo.WhiteFilters[FilterClass.QuestionBodyWhiteLQ].Terms.Where(t => t.Site == site).Select(t => t.Score).Average();

        //            GlobalInfo.WhiteFilters[FilterClass.QuestionBodyWhiteLQ].AddTerm(new Term(FilterClass.QuestionBodyWhiteLQ, term, score, site));

        //            break;
        //        }
        //    }

        //    return new ReplyMessage("`Whitelist term added.`");
        //}

        //private static ReplyMessage AddBATerm(string command)
        //{
        //    var addCommand = command.Remove(0, 8);
        //    var term = new Regex(addCommand.Substring(addCommand.IndexOf(' ') + 1), RegexOptions.Compiled);

        //    if (term.IsFlakRegex()) { return new ReplyMessage("`ReDoS detected (term not added).`"); }

        //    switch (addCommand.ToLowerInvariant()[0])
        //    {
        //        case 'o':
        //        {
        //            if (GlobalInfo.BlackFilters[FilterClass.AnswerBlackOff].Terms.Contains(term)) { return new ReplyMessage("`Blacklist term already exists.`"); }

        //            GlobalInfo.BlackFilters[FilterClass.AnswerBlackOff].AddTerm(new Term(FilterClass.AnswerBlackOff, term, GlobalInfo.BlackFilters[FilterClass.AnswerBlackOff].AverageScore));

        //            break;
        //        }

        //        case 's':
        //        {
        //            if (GlobalInfo.BlackFilters[FilterClass.AnswerBlackSpam].Terms.Contains(term)) { return new ReplyMessage("`Blacklist term already exists.`"); }

        //            GlobalInfo.BlackFilters[FilterClass.AnswerBlackSpam].AddTerm(new Term(FilterClass.AnswerBlackSpam, term, GlobalInfo.BlackFilters[FilterClass.AnswerBlackSpam].AverageScore));

        //            break;
        //        }

        //        case 'l':
        //        {
        //            if (GlobalInfo.BlackFilters[FilterClass.AnswerBlackLQ].Terms.Contains(term)) { return new ReplyMessage("`Blacklist term already exists.`"); }

        //            GlobalInfo.BlackFilters[FilterClass.AnswerBlackLQ].AddTerm(new Term(FilterClass.AnswerBlackLQ, term, GlobalInfo.BlackFilters[FilterClass.AnswerBlackLQ].AverageScore));

        //            break;
        //        }

        //        case 'n':
        //        {
        //            if (GlobalInfo.BlackFilters[FilterClass.AnswerBlackName].Terms.Contains(term)) { return new ReplyMessage("`Blacklist term already exists.`"); }

        //            GlobalInfo.BlackFilters[FilterClass.AnswerBlackName].AddTerm(new Term(FilterClass.AnswerBlackName, term, GlobalInfo.BlackFilters[FilterClass.AnswerBlackName].AverageScore));

        //            break;
        //        }
        //    }

        //    return new ReplyMessage("`Blacklist term added.`");
        //}

        //private static ReplyMessage AddWATerm(string command)
        //{
        //    var addCommand = command.Substring(0, 8);
        //    var firstSpace = command.IndexOf(' ');
        //    var secondSpace = command.IndexOf(' ', firstSpace + 1);
        //    var term = new Regex(command.Substring(secondSpace + 1));
        //    var site = command.Substring(firstSpace + 1, secondSpace - firstSpace - 1);

        //    if (term.IsFlakRegex()) { return new ReplyMessage("`ReDoS detected (term not added).`"); }

        //    switch (addCommand.ToLowerInvariant()[0])
        //    {
        //        case 'o':
        //        {
        //            if (GlobalInfo.WhiteFilters[FilterClass.AnswerWhiteOff].Terms.Contains(term, site)) { return new ReplyMessage("`Whitelist term already exists.`"); }

        //            var score = GlobalInfo.WhiteFilters[FilterClass.AnswerWhiteOff].Terms.Where(t => t.Site == site).Select(t => t.Score).Average();

        //            GlobalInfo.WhiteFilters[FilterClass.AnswerWhiteOff].AddTerm(new Term(FilterClass.AnswerWhiteOff, term, score, site));

        //            break;
        //        }

        //        case 's':
        //        {
        //            if (GlobalInfo.WhiteFilters[FilterClass.AnswerWhiteSpam].Terms.Contains(term, site)) { return new ReplyMessage("`Whitelist term already exists.`"); }

        //            var score = GlobalInfo.WhiteFilters[FilterClass.AnswerWhiteSpam].Terms.Where(t => t.Site == site).Select(t => t.Score).Average();

        //            GlobalInfo.WhiteFilters[FilterClass.AnswerWhiteSpam].AddTerm(new Term(FilterClass.AnswerWhiteSpam, term, score, site));

        //            break;
        //        }

        //        case 'l':
        //        {
        //            if (GlobalInfo.WhiteFilters[FilterClass.AnswerWhiteLQ].Terms.Contains(term, site)) { return new ReplyMessage("`Whitelist term already exists.`"); }

        //            var score = GlobalInfo.WhiteFilters[FilterClass.AnswerWhiteLQ].Terms.Where(t => t.Site == site).Select(t => t.Score).Average();

        //            GlobalInfo.WhiteFilters[FilterClass.AnswerWhiteLQ].AddTerm(new Term(FilterClass.AnswerWhiteLQ, term, score, site));

        //            break;
        //        }

        //        case 'n':
        //        {
        //            if (GlobalInfo.WhiteFilters[FilterClass.AnswerWhiteName].Terms.Contains(term, site)) { return new ReplyMessage("`Whitelist term already exists.`"); }

        //            var score = GlobalInfo.WhiteFilters[FilterClass.AnswerWhiteName].Terms.Where(t => t.Site == site).Select(t => t.Score).Average();

        //            GlobalInfo.WhiteFilters[FilterClass.AnswerWhiteName].AddTerm(new Term(FilterClass.AnswerWhiteName, term, score, site));

        //            break;
        //        }
        //    }

        //    return new ReplyMessage("`Whitelist term added.`");
        //}

        # endregion

        # region Remove term commands.

        private static ReplyMessage DelBlackTerm(string command)
        {
            var filterInfo = CommandParser.ParseFilterConfig(command);
            Regex term;

            try
            {
                term = new Regex(command.Substring(command.IndexOf(' ') + 1), RegexOptions.CultureInvariant);
            }
            catch (Exception)
            {
                return new ReplyMessage("`Unable to remove term. Invalid regex.`");
            }

            if (term.IsFlakRegex())
            {
                return new ReplyMessage("`ReDoS detected (term not removed).`");
            }

            if (!Config.BlackFilters[filterInfo].Terms.Contains(term))
            {
                return new ReplyMessage("`Term not found.`");
            }

            Config.BlackFilters[filterInfo].RemoveTerm(term);

            return new ReplyMessage("`Term removed!`");
        }

        private static ReplyMessage DelWhiteTerm(string command)
        {
            var filterInfo = CommandParser.ParseFilterConfig(command);
            var firstSpace = command.IndexOf(' ');
            var secondSpace = command.IndexOf(' ', firstSpace + 1);
            var site = command.Substring(firstSpace + 1, secondSpace - firstSpace - 1);
            Regex term;

            try
            {
                term = new Regex(command.Substring(secondSpace + 1), RegexOptions.CultureInvariant);
            }
            catch (Exception)
            {
                return new ReplyMessage("`Unable to remove term. Invalid regex.`");
            }

            if (term.IsFlakRegex())
            {
                return new ReplyMessage("`ReDoS detected (term not removed).`");
            }

            if (Config.WhiteFilters[filterInfo].Terms.Contains(term, site))
            {
                return new ReplyMessage("`Term not found.`");
            }

            Config.WhiteFilters[filterInfo].RemoveTerm(term, site);

            return new ReplyMessage("`Term removed!`");
        }

        //private static ReplyMessage RemoveBQTTerm(string command)
        //{
        //    var removeCommand = command.Remove(0, 9);
        //    var term = new Regex(removeCommand.Substring(removeCommand.IndexOf(' ') + 1), RegexOptions.Compiled);

        //    switch (removeCommand.ToLowerInvariant()[0])
        //    {
        //        case 'o':
        //        {
        //            if (!GlobalInfo.BlackFilters[FilterClass.QuestionTitleBlackOff].Terms.Contains(term)) { return new ReplyMessage("`Blacklist term does not exist.`"); }

        //            GlobalInfo.Stats.TotalCheckedPosts -= GlobalInfo.BlackFilters[FilterClass.QuestionTitleBlackOff].Terms.GetRealTerm(term).CaughtCount;
        //            GlobalInfo.BlackFilters[FilterClass.QuestionTitleBlackOff].RemoveTerm(term);

        //            break;
        //        }

        //        case 's':
        //        {
        //            if (!GlobalInfo.BlackFilters[FilterClass.QuestionTitleBlackSpam].Terms.Contains(term)) { return new ReplyMessage("`Blacklist term does not exist.`"); }
                    
        //            GlobalInfo.Stats.TotalCheckedPosts -= GlobalInfo.BlackFilters[FilterClass.QuestionTitleBlackSpam].Terms.GetRealTerm(term).CaughtCount;
        //            GlobalInfo.BlackFilters[FilterClass.QuestionTitleBlackSpam].RemoveTerm(term);

        //            break;
        //        }

        //        case 'l':
        //        {
        //            if (!GlobalInfo.BlackFilters[FilterClass.QuestionTitleBlackLQ].Terms.Contains(term)) { return new ReplyMessage("`Blacklist term does not exist.`"); }

        //            GlobalInfo.Stats.TotalCheckedPosts -= GlobalInfo.BlackFilters[FilterClass.QuestionTitleBlackLQ].Terms.GetRealTerm(term).CaughtCount;
        //            GlobalInfo.BlackFilters[FilterClass.QuestionTitleBlackLQ].RemoveTerm(term);

        //            break;
        //        }

        //        case 'n':
        //        {
        //            if (!GlobalInfo.BlackFilters[FilterClass.QuestionTitleBlackName].Terms.Contains(term)) { return new ReplyMessage("`Blacklist term does not exist.`"); }

        //            GlobalInfo.Stats.TotalCheckedPosts -= GlobalInfo.BlackFilters[FilterClass.QuestionTitleBlackName].Terms.GetRealTerm(term).CaughtCount;
        //            GlobalInfo.BlackFilters[FilterClass.QuestionTitleBlackName].RemoveTerm(term);

        //            break;
        //        }
        //    }

            

        //    return new ReplyMessage("`Blacklist term removed.`");
        //}

        //private static ReplyMessage RemoveWQTTerm(string command)
        //{
        //    var removeCommand = command.Remove(0, 9);
        //    var firstSpace = command.IndexOf(' ');
        //    var secondSpace = command.IndexOf(' ', firstSpace + 1);
        //    var term = new Regex(command.Substring(secondSpace + 1));
        //    var site = command.Substring(firstSpace + 1, secondSpace - firstSpace - 1);

        //    switch (removeCommand.ToLowerInvariant()[0])
        //    {
        //        case 'o':
        //        {
        //            if (!GlobalInfo.WhiteFilters[FilterClass.QuestionTitleWhiteOff].Terms.Contains(term, site)) { return new ReplyMessage("`Whitelist term does not exist.`"); }

        //            GlobalInfo.WhiteFilters[FilterClass.QuestionTitleWhiteOff].RemoveTerm(new Term(FilterClass.QuestionTitleWhiteOff, term, 0, site));

        //            break;
        //        }

        //        case 's':
        //        {
        //            if (!GlobalInfo.WhiteFilters[FilterClass.QuestionTitleWhiteSpam].Terms.Contains(term, site)) { return new ReplyMessage("`Whitelist term does not exist.`"); }

        //            GlobalInfo.WhiteFilters[FilterClass.QuestionTitleWhiteSpam].RemoveTerm(new Term(FilterClass.QuestionTitleWhiteSpam, term, 0, site));

        //            break;
        //        }

        //        case 'l':
        //        {
        //            if (!GlobalInfo.WhiteFilters[FilterClass.QuestionTitleWhiteLQ].Terms.Contains(term, site)) { return new ReplyMessage("`Whitelist term does not exist.`"); }

        //            GlobalInfo.WhiteFilters[FilterClass.QuestionTitleWhiteLQ].RemoveTerm(new Term(FilterClass.QuestionTitleWhiteLQ, term, 0, site));

        //            break;
        //        }
                
        //        case 'n':
        //        {
        //            if (!GlobalInfo.WhiteFilters[FilterClass.QuestionTitleWhiteName].Terms.Contains(term, site)) { return new ReplyMessage("`Whitelist term does not exist.`"); }

        //            GlobalInfo.WhiteFilters[FilterClass.QuestionTitleWhiteName].RemoveTerm(new Term(FilterClass.QuestionTitleWhiteName, term, 0, site));

        //            break;
        //        }
        //    }

        //    return new ReplyMessage("`Whitelist term removed.`");
        //}

        //private static ReplyMessage RemoveBQBTerm(string command)
        //{
        //    var removeCommand = command.Remove(0, 9);
        //    var term = new Regex(removeCommand.Substring(removeCommand.IndexOf(' ') + 1), RegexOptions.Compiled);

        //    switch (removeCommand.ToLowerInvariant()[0])
        //    {
        //        case 'o':
        //        {
        //            if (!GlobalInfo.BlackFilters[FilterClass.QuestionBodyBlackOff].Terms.Contains(term)) { return new ReplyMessage("`Blacklist term does not exist.`"); }

        //            GlobalInfo.Stats.TotalCheckedPosts -= GlobalInfo.BlackFilters[FilterClass.QuestionBodyBlackOff].Terms.GetRealTerm(term).CaughtCount;
        //            GlobalInfo.BlackFilters[FilterClass.QuestionBodyBlackOff].RemoveTerm(term);

        //            break;
        //        }

        //        case 's':
        //        {
        //            if (!GlobalInfo.BlackFilters[FilterClass.QuestionBodyBlackSpam].Terms.Contains(term)) { return new ReplyMessage("`Blacklist term does not exist.`"); }

        //            GlobalInfo.Stats.TotalCheckedPosts -= GlobalInfo.BlackFilters[FilterClass.QuestionBodyBlackSpam].Terms.GetRealTerm(term).CaughtCount;
        //            GlobalInfo.BlackFilters[FilterClass.QuestionBodyBlackSpam].RemoveTerm(term);

        //            break;
        //        }

        //        case 'l':
        //        {
        //            if (!GlobalInfo.BlackFilters[FilterClass.QuestionBodyBlackLQ].Terms.Contains(term)) { return new ReplyMessage("`Blacklist term does not exist.`"); }

        //            GlobalInfo.Stats.TotalCheckedPosts -= GlobalInfo.BlackFilters[FilterClass.QuestionBodyBlackLQ].Terms.GetRealTerm(term).CaughtCount;
        //            GlobalInfo.BlackFilters[FilterClass.QuestionBodyBlackLQ].RemoveTerm(term);

        //            break;
        //        }
        //    }

        //    return new ReplyMessage("`Blacklist term removed.`");
        //}

        //private static ReplyMessage RemoveWQBTerm(string command)
        //{
        //    var removeCommand = command.Remove(0, 9);
        //    var firstSpace = command.IndexOf(' ');
        //    var secondSpace = command.IndexOf(' ', firstSpace + 1);
        //    var term = new Regex(command.Substring(secondSpace + 1));
        //    var site = command.Substring(firstSpace + 1, secondSpace - firstSpace - 1);

        //    switch (removeCommand.ToLowerInvariant()[0])
        //    {
        //        case 'o':
        //        {
        //            if (!GlobalInfo.WhiteFilters[FilterClass.QuestionBodyWhiteOff].Terms.Contains(term, site)) { return new ReplyMessage("`Whitelist term does not exist.`"); }

        //            GlobalInfo.WhiteFilters[FilterClass.QuestionBodyWhiteOff].RemoveTerm(new Term(FilterClass.QuestionBodyWhiteOff, term, 0, site));

        //            break;
        //        }

        //        case 's':
        //        {
        //            if (!GlobalInfo.WhiteFilters[FilterClass.QuestionBodyWhiteSpam].Terms.Contains(term, site)) { return new ReplyMessage("`Whitelist term does not exist.`"); }

        //            GlobalInfo.WhiteFilters[FilterClass.QuestionBodyWhiteSpam].RemoveTerm(new Term(FilterClass.QuestionBodyWhiteSpam, term, 0, site));

        //            break;
        //        }

        //        case 'l':
        //        {
        //            if (!GlobalInfo.WhiteFilters[FilterClass.QuestionBodyWhiteLQ].Terms.Contains(term, site)) { return new ReplyMessage("`Whitelist term does not exist.`"); }

        //            GlobalInfo.WhiteFilters[FilterClass.QuestionBodyWhiteLQ].RemoveTerm(new Term(FilterClass.QuestionBodyWhiteLQ, term, 0, site));

        //            break;
        //        }
        //    }

        //    return new ReplyMessage("`Whitelist term removed.`");
        //}

        //private static ReplyMessage RemoveBATerm(string command)
        //{
        //    var removeCommand = command.Remove(0, 8);
        //    var term = new Regex(removeCommand.Substring(removeCommand.IndexOf(' ') + 1), RegexOptions.Compiled);

        //    switch (removeCommand.ToLowerInvariant()[0])
        //    {
        //        case 'o':
        //        {
        //            if (!GlobalInfo.BlackFilters[FilterClass.AnswerBlackOff].Terms.Contains(term)) { return new ReplyMessage("`Blacklist term does not exist.`"); }

        //            GlobalInfo.Stats.TotalCheckedPosts -= GlobalInfo.BlackFilters[FilterClass.AnswerBlackOff].Terms.GetRealTerm(term).CaughtCount;
        //            GlobalInfo.BlackFilters[FilterClass.AnswerBlackOff].RemoveTerm(term);

        //            break;
        //        }

        //        case 's':
        //        {
        //            if (!GlobalInfo.BlackFilters[FilterClass.AnswerBlackSpam].Terms.Contains(term)) { return new ReplyMessage("`Blacklist term does not exist.`"); }

        //            GlobalInfo.Stats.TotalCheckedPosts -= GlobalInfo.BlackFilters[FilterClass.AnswerBlackSpam].Terms.GetRealTerm(term).CaughtCount;
        //            GlobalInfo.BlackFilters[FilterClass.AnswerBlackSpam].RemoveTerm(term);

        //            break;
        //        }

        //        case 'l':
        //        {
        //            if (!GlobalInfo.BlackFilters[FilterClass.AnswerBlackLQ].Terms.Contains(term)) { return new ReplyMessage("`Blacklist term does not exist.`"); }

        //            GlobalInfo.Stats.TotalCheckedPosts -= GlobalInfo.BlackFilters[FilterClass.AnswerBlackLQ].Terms.GetRealTerm(term).CaughtCount;
        //            GlobalInfo.BlackFilters[FilterClass.AnswerBlackLQ].RemoveTerm(term);

        //            break;
        //        }

        //        case 'n':
        //        {
        //            if (!GlobalInfo.BlackFilters[FilterClass.AnswerBlackName].Terms.Contains(term)) { return new ReplyMessage("`Blacklist term does not exist.`"); }

        //            GlobalInfo.Stats.TotalCheckedPosts -= GlobalInfo.BlackFilters[FilterClass.AnswerBlackName].Terms.GetRealTerm(term).CaughtCount;
        //            GlobalInfo.BlackFilters[FilterClass.AnswerBlackName].RemoveTerm(term);

        //            break;
        //        }
        //    }

        //    return new ReplyMessage("`Blacklist term removed.`");
        //}

        //private static ReplyMessage RemoveWATerm(string command)
        //{
        //    var removeCommand = command.Remove(0, 8);
        //    var firstSpace = command.IndexOf(' ');
        //    var secondSpace = command.IndexOf(' ', firstSpace + 1);
        //    var term = new Regex(command.Substring(secondSpace + 1));
        //    var site = command.Substring(firstSpace + 1, secondSpace - firstSpace - 1);

        //    switch (removeCommand.ToLowerInvariant()[0])
        //    {
        //        case 'o':
        //        {
        //            if (!GlobalInfo.WhiteFilters[FilterClass.AnswerWhiteOff].Terms.Contains(term)) { return new ReplyMessage("`Whitelist term does not exist.`"); }

        //            GlobalInfo.WhiteFilters[FilterClass.AnswerWhiteOff].RemoveTerm(new Term(FilterClass.AnswerWhiteOff, term, 0, site));

        //            break;
        //        }

        //        case 's':
        //        {
        //            if (!GlobalInfo.WhiteFilters[FilterClass.AnswerWhiteSpam].Terms.Contains(term)) { return new ReplyMessage("`Whitelist term does not exist.`"); }

        //            GlobalInfo.WhiteFilters[FilterClass.AnswerWhiteSpam].RemoveTerm(new Term(FilterClass.AnswerWhiteSpam, term, 0, site));

        //            break;
        //        }

        //        case 'l':
        //        {
        //            if (!GlobalInfo.WhiteFilters[FilterClass.AnswerWhiteLQ].Terms.Contains(term)) { return new ReplyMessage("`Whitelist term does not exist.`"); }

        //            GlobalInfo.WhiteFilters[FilterClass.AnswerWhiteLQ].RemoveTerm(new Term(FilterClass.AnswerWhiteLQ, term, 0, site));

        //            break;
        //        }

        //        case 'n':
        //        {
        //            if (!GlobalInfo.WhiteFilters[FilterClass.AnswerWhiteName].Terms.Contains(term)) { return new ReplyMessage("`Whitelist term does not exist.`"); }

        //            GlobalInfo.WhiteFilters[FilterClass.AnswerWhiteName].RemoveTerm(new Term(FilterClass.AnswerWhiteName, term, 0, site));

        //            break;
        //        }
        //    }

        //    return new ReplyMessage("`Whitelist term removed.`");
        //}

        # endregion

        # region Edit term commands.

        private static ReplyMessage EditBlackTerm(string command)
        {
            var filterInfo = CommandParser.ParseFilterConfig(command);
            var startIndex = command.IndexOf(' ') + 1;
            var delimiterIndex = command.IndexOf("¬¬¬", StringComparison.Ordinal);
            Regex oldTerm;
            Regex newTerm;

            try
            {
                oldTerm = new Regex(command.Substring(startIndex, delimiterIndex - startIndex), RegexOptions.CultureInvariant);
                newTerm = new Regex(command.Remove(0, delimiterIndex + 3), regexOptions);
            }
            catch (Exception)
            {
                return new ReplyMessage("`Unable to add term. Invalid regex.`");
            }

            if (oldTerm.IsFlakRegex() || newTerm.IsFlakRegex())
            {
                return new ReplyMessage("`ReDoS detected (term not updated).`");
            }

            if (!Config.BlackFilters[filterInfo].Terms.Contains(oldTerm))
            {
                return new ReplyMessage("`Old term not found.`");
            }

            Config.BlackFilters[filterInfo].EditTerm(oldTerm, newTerm);

            return new ReplyMessage("`Term updated!`");
        }

        private static ReplyMessage EditWhiteTerm(string command)
        {
            var filterInfo = CommandParser.ParseFilterConfig(command);

            var firstSpace = command.IndexOf(' ');
            var secondSpace = command.IndexOf(' ', firstSpace + 1);
            var delimiter = command.IndexOf("¬¬¬", StringComparison.Ordinal);

            var site = command.Substring(firstSpace + 1, secondSpace - firstSpace - 1);
            Regex oldTerm;
            Regex newTerm;

            try
            {
                oldTerm = new Regex(command.Substring(secondSpace + 1, delimiter - secondSpace - 1), RegexOptions.CultureInvariant);
                newTerm = new Regex(command.Remove(0, delimiter + 3), regexOptions);
            }
            catch (Exception)
            {
                return new ReplyMessage("`Unable to add term. Invalid regex.`");
            }

            if (oldTerm.IsFlakRegex() || newTerm.IsFlakRegex())
            {
                return new ReplyMessage("`ReDoS detected (term not updated).`");
            }

            if (!Config.WhiteFilters[filterInfo].Terms.Contains(oldTerm))
            {
                return new ReplyMessage("`Old term not found.`");
            }

            Config.WhiteFilters[filterInfo].EditTerm(oldTerm, newTerm, site);

            return new ReplyMessage("`Term updated!`");
        }

        //private static ReplyMessage EditBQTTerm(string command)
        //{
        //    var editCommand = command.Remove(0, 10);

        //    if (!editCommand.StartsWith("off") && !editCommand.StartsWith("spam") && !editCommand.StartsWith("lq") && !editCommand.StartsWith("name")) { return new ReplyMessage("`Command not recognised.`"); }

        //    var startIndex = command.IndexOf(' ') + 1;
        //    var delimiterIndex = command.IndexOf("¬¬¬", StringComparison.Ordinal);
        //    var oldTerm = new Regex(command.Substring(startIndex, delimiterIndex - startIndex));
        //    var newTerm = new Regex(command.Remove(0, delimiterIndex + 3), RegexOptions.Compiled);

        //    if (newTerm.IsFlakRegex()) { return new ReplyMessage("`ReDoS detected (term not updated).`"); }

        //    switch (editCommand.ToLowerInvariant()[0])
        //    {
        //        case 'o':
        //        {
        //            if (!GlobalInfo.BlackFilters[FilterClass.QuestionTitleBlackOff].Terms.Contains(oldTerm)) { return new ReplyMessage("`Blacklist term does not exist.`"); }

        //            GlobalInfo.BlackFilters[FilterClass.QuestionTitleBlackOff].EditTerm(oldTerm, newTerm);

        //            break;
        //        }

        //        case 's':
        //        {
        //            if (!GlobalInfo.BlackFilters[FilterClass.QuestionTitleBlackSpam].Terms.Contains(oldTerm)) { return new ReplyMessage("`Blacklist term does not exist.`"); }

        //            GlobalInfo.BlackFilters[FilterClass.QuestionTitleBlackSpam].EditTerm(oldTerm, newTerm);

        //            break;
        //        }

        //        case 'l':
        //        {
        //            if (!GlobalInfo.BlackFilters[FilterClass.QuestionTitleBlackLQ].Terms.Contains(oldTerm)) { return new ReplyMessage("`Blacklist term does not exist.`"); }

        //            GlobalInfo.BlackFilters[FilterClass.QuestionTitleBlackLQ].EditTerm(oldTerm, newTerm);

        //            break;
        //        }

        //        case 'n':
        //        {
        //            if (!GlobalInfo.BlackFilters[FilterClass.QuestionTitleBlackName].Terms.Contains(oldTerm)) { return new ReplyMessage("`Blacklist term does not exist.`"); }

        //            GlobalInfo.BlackFilters[FilterClass.QuestionTitleBlackName].EditTerm(oldTerm, newTerm);

        //            break;
        //        }
        //    }

        //    return new ReplyMessage("`Term updated.`");
        //}

        //private static ReplyMessage EditBQBTerm(string command)
        //{
        //    var editCommand = command.Remove(0, 10);

        //    if (!editCommand.StartsWith("off") && !editCommand.StartsWith("spam") && !editCommand.StartsWith("lq")) { return new ReplyMessage("`Command not recognised.`"); }

        //    var startIndex = command.IndexOf(' ') + 1;
        //    var delimiterIndex = command.IndexOf("¬¬¬", StringComparison.Ordinal);
        //    var oldTerm = new Regex(command.Substring(startIndex, delimiterIndex - startIndex));
        //    var newTerm = new Regex(command.Remove(0, delimiterIndex + 3), RegexOptions.Compiled);

        //    if (newTerm.IsFlakRegex()) { return new ReplyMessage("`ReDoS detected (term not updated).`"); }

        //    switch (editCommand.ToLowerInvariant()[0])
        //    {
        //        case 'o':
        //        {
        //            if (!GlobalInfo.BlackFilters[FilterClass.QuestionBodyBlackOff].Terms.Contains(oldTerm)) { return new ReplyMessage("`Blacklist term does not exist.`"); }

        //            GlobalInfo.BlackFilters[FilterClass.QuestionBodyBlackOff].EditTerm(oldTerm, newTerm);

        //            break;
        //        }

        //        case 's':
        //        {
        //            if (!GlobalInfo.BlackFilters[FilterClass.QuestionBodyBlackSpam].Terms.Contains(oldTerm)) { return new ReplyMessage("`Blacklist term does not exist.`"); }

        //            GlobalInfo.BlackFilters[FilterClass.QuestionBodyBlackSpam].EditTerm(oldTerm, newTerm);

        //            break;
        //        }

        //        case 'l':
        //        {
        //            if (!GlobalInfo.BlackFilters[FilterClass.QuestionBodyBlackLQ].Terms.Contains(oldTerm)) { return new ReplyMessage("`Blacklist term does not exist.`"); }

        //            GlobalInfo.BlackFilters[FilterClass.QuestionBodyBlackLQ].EditTerm(oldTerm, newTerm);

        //            break;
        //        }
        //    }

        //    return new ReplyMessage("`Term updated.`");
        //}

        //private static ReplyMessage EditWQTTerm(string command)
        //{
        //    var editCommand = command.Remove(0, 10);

        //    if (!editCommand.StartsWith("off") && !editCommand.StartsWith("spam") && !editCommand.StartsWith("lq") && !editCommand.StartsWith("name")) { return new ReplyMessage("`Command not recognised.`"); }

        //    var firstSpace = command.IndexOf(' ');
        //    var secondSpace = command.IndexOf(' ', firstSpace + 1);
        //    var delimiter = command.IndexOf("¬¬¬", StringComparison.Ordinal);

        //    var site = command.Substring(firstSpace + 1, secondSpace - firstSpace - 1);
        //    var oldTerm = new Regex(command.Substring(secondSpace + 1, delimiter - secondSpace - 1));
        //    var newTerm = new Regex(command.Remove(0, delimiter + 3), RegexOptions.Compiled);

        //    if (newTerm.IsFlakRegex()) { return new ReplyMessage("`ReDoS detected (term not updated).`"); }

        //    switch (editCommand.ToLowerInvariant()[0])
        //    {
        //        case 'o':
        //        {
        //            if (!GlobalInfo.WhiteFilters[FilterClass.QuestionTitleWhiteOff].Terms.Contains(oldTerm, site)) { return new ReplyMessage("`Whitelist term does not exist.`"); }

        //            GlobalInfo.WhiteFilters[FilterClass.QuestionTitleWhiteOff].EditTerm(oldTerm, newTerm, site);

        //            break;
        //        }

        //        case 's':
        //        {
        //            if (!GlobalInfo.WhiteFilters[FilterClass.QuestionTitleWhiteSpam].Terms.Contains(oldTerm, site)) { return new ReplyMessage("`Whitelist term does not exist.`"); }

        //            GlobalInfo.WhiteFilters[FilterClass.QuestionTitleWhiteSpam].EditTerm(oldTerm, newTerm, site);

        //            break;
        //        }

        //        case 'l':
        //        {
        //            if (!GlobalInfo.WhiteFilters[FilterClass.QuestionTitleWhiteLQ].Terms.Contains(oldTerm, site)) { return new ReplyMessage("`Whitelist term does not exist.`"); }

        //            GlobalInfo.WhiteFilters[FilterClass.QuestionTitleWhiteLQ].EditTerm(oldTerm, newTerm, site);

        //            break;
        //        }

        //        case 'n':
        //        {
        //            if (!GlobalInfo.WhiteFilters[FilterClass.QuestionTitleWhiteName].Terms.Contains(oldTerm, site)) { return new ReplyMessage("`Whitelist term does not exist.`"); }

        //            GlobalInfo.WhiteFilters[FilterClass.QuestionTitleWhiteName].EditTerm(oldTerm, newTerm, site);

        //            break;
        //        }
        //    }

        //    return new ReplyMessage("`Term updated.`");
        //}

        //private static ReplyMessage EditWQBTerm(string command)
        //{
        //    var editCommand = command.Remove(0, 10);

        //    if (!editCommand.StartsWith("off") && !editCommand.StartsWith("spam") && !editCommand.StartsWith("lq")) { return new ReplyMessage("`Command not recognised.`"); }

        //    var firstSpace = command.IndexOf(' ');
        //    var secondSpace = command.IndexOf(' ', firstSpace + 1);
        //    var delimiter = command.IndexOf("¬¬¬", StringComparison.Ordinal);

        //    var site = command.Substring(firstSpace + 1, secondSpace - firstSpace - 1);
        //    var oldTerm = new Regex(command.Substring(secondSpace + 1, delimiter - secondSpace - 1));
        //    var newTerm = new Regex(command.Remove(0, delimiter + 3), RegexOptions.Compiled);

        //    if (newTerm.IsFlakRegex()) { return new ReplyMessage("`ReDoS detected (term not updated).`"); }

        //    switch (editCommand.ToLowerInvariant()[0])
        //    {
        //        case 'o':
        //        {
        //            if (!GlobalInfo.WhiteFilters[FilterClass.QuestionBodyWhiteOff].Terms.Contains(oldTerm, site)) { return new ReplyMessage("`Whitelist term does not exist.`"); }

        //            GlobalInfo.WhiteFilters[FilterClass.QuestionBodyWhiteOff].EditTerm(oldTerm, newTerm, site);

        //            break;
        //        }

        //        case 's':
        //        {
        //            if (!GlobalInfo.WhiteFilters[FilterClass.QuestionBodyWhiteSpam].Terms.Contains(oldTerm, site)) { return new ReplyMessage("`Whitelist term does not exist.`"); }

        //            GlobalInfo.WhiteFilters[FilterClass.QuestionBodyWhiteSpam].EditTerm(oldTerm, newTerm, site);

        //            break;
        //        }

        //        case 'l':
        //        {
        //            if (!GlobalInfo.WhiteFilters[FilterClass.QuestionBodyWhiteLQ].Terms.Contains(oldTerm, site)) { return new ReplyMessage("`Whitelist term does not exist.`"); }

        //            GlobalInfo.WhiteFilters[FilterClass.QuestionBodyWhiteLQ].EditTerm(oldTerm, newTerm, site);

        //            break;
        //        }
        //    }

        //    return new ReplyMessage("`Term updated.`");
        //}

        //private static ReplyMessage EditBATerm(string command)
        //{
        //    var editCommand = command.Remove(0, 9);

        //    if (!editCommand.StartsWith("off") && !editCommand.StartsWith("spam") && !editCommand.StartsWith("lq") && !editCommand.StartsWith("name")) { return new ReplyMessage("`Command not recognised.`"); }

        //    var startIndex = command.IndexOf(' ') + 1;
        //    var delimiterIndex = command.IndexOf("¬¬¬", StringComparison.Ordinal);
        //    var oldTerm = new Regex(command.Substring(startIndex, delimiterIndex - startIndex));
        //    var newTerm = new Regex(command.Remove(0, delimiterIndex + 3), RegexOptions.Compiled);

        //    if (newTerm.IsFlakRegex()) { return new ReplyMessage("`ReDoS detected (term not updated).`"); }

        //    switch (editCommand.ToLowerInvariant()[0])
        //    {
        //        case 'o':
        //        {
        //            if (!GlobalInfo.BlackFilters[FilterClass.AnswerBlackOff].Terms.Contains(oldTerm)) { return new ReplyMessage("`Blacklist term does not exist.`"); }

        //            GlobalInfo.BlackFilters[FilterClass.AnswerBlackOff].EditTerm(oldTerm, newTerm);

        //            break;
        //        }

        //        case 's':
        //        {
        //            if (!GlobalInfo.BlackFilters[FilterClass.AnswerBlackSpam].Terms.Contains(oldTerm)) { return new ReplyMessage("`Blacklist term does not exist.`"); }

        //            GlobalInfo.BlackFilters[FilterClass.AnswerBlackSpam].EditTerm(oldTerm, newTerm);

        //            break;
        //        }

        //        case 'l':
        //        {
        //            if (!GlobalInfo.BlackFilters[FilterClass.AnswerBlackLQ].Terms.Contains(oldTerm)) { return new ReplyMessage("`Blacklist term does not exist.`"); }

        //            GlobalInfo.BlackFilters[FilterClass.AnswerBlackLQ].EditTerm(oldTerm, newTerm);

        //            break;
        //        }

        //        case 'n':
        //        {
        //            if (!GlobalInfo.BlackFilters[FilterClass.AnswerBlackName].Terms.Contains(oldTerm)) { return new ReplyMessage("`Blacklist term does not exist.`"); }

        //            GlobalInfo.BlackFilters[FilterClass.AnswerBlackName].EditTerm(oldTerm, newTerm);

        //            break;
        //        }
        //    }

        //    return new ReplyMessage("`Term updated.`");
        //}

        //private static ReplyMessage EditWATerm(string command)
        //{
        //    var editCommand = command.Remove(0, 9);

        //    if (!editCommand.StartsWith("off") && !editCommand.StartsWith("spam") && !editCommand.StartsWith("lq") && !editCommand.StartsWith("name")) { return new ReplyMessage("`Command not recognised.`"); }

        //    var firstSpace = command.IndexOf(' ');
        //    var secondSpace = command.IndexOf(' ', firstSpace + 1);
        //    var delimiter = command.IndexOf("¬¬¬", StringComparison.Ordinal);

        //    var site = command.Substring(firstSpace + 1, secondSpace - firstSpace - 1);
        //    var oldTerm = new Regex(command.Substring(secondSpace + 1, delimiter - secondSpace - 1));
        //    var newTerm = new Regex(command.Remove(0, delimiter + 3), RegexOptions.Compiled);

        //    if (newTerm.IsFlakRegex()) { return new ReplyMessage("`ReDoS detected (term not updated).`"); }

        //    switch (editCommand.ToLowerInvariant()[0])
        //    {
        //        case 'o':
        //        {
        //            if (!GlobalInfo.WhiteFilters[FilterClass.AnswerWhiteOff].Terms.Contains(oldTerm, site)) { return new ReplyMessage("`Whitelist term does not exist.`"); }

        //            GlobalInfo.WhiteFilters[FilterClass.AnswerWhiteOff].EditTerm(oldTerm, newTerm, site);

        //            break;
        //        }

        //        case 's':
        //        {
        //            if (!GlobalInfo.WhiteFilters[FilterClass.AnswerWhiteSpam].Terms.Contains(oldTerm, site)) { return new ReplyMessage("`Whitelist term does not exist.`"); }

        //            GlobalInfo.WhiteFilters[FilterClass.AnswerWhiteSpam].EditTerm(oldTerm, newTerm, site);

        //            break;
        //        }

        //        case 'l':
        //        {
        //            if (!GlobalInfo.WhiteFilters[FilterClass.AnswerWhiteLQ].Terms.Contains(oldTerm, site)) { return new ReplyMessage("`Whitelist term does not exist.`"); }

        //            GlobalInfo.WhiteFilters[FilterClass.AnswerWhiteLQ].EditTerm(oldTerm, newTerm, site);

        //            break;
        //        }

        //        case 'n':
        //        {
        //            if (!GlobalInfo.WhiteFilters[FilterClass.AnswerWhiteName].Terms.Contains(oldTerm, site)) { return new ReplyMessage("`Whitelist term does not exist.`"); }

        //            GlobalInfo.WhiteFilters[FilterClass.AnswerWhiteName].EditTerm(oldTerm, newTerm, site);

        //            break;
        //        }
        //    }

        //    return new ReplyMessage("`Term updated.`");
        //}

        # endregion

        # region FP/TP(A) commands

        private static ReplyMessage FalsePositive()
        {
            if (analysis.Type == PostType.BadTagUsed) { return new ReplyMessage(""); }

            if (analysis.Type == PostType.Spam)
            {
                var questionReport = new Regex(@"^\*\*(Low Quality|Spam|Offensive)\*\* \*\*Q\*\*|\*\*Bad Tag Used\*\*", RegexOptions.CultureInvariant);

                if (questionReport.IsMatch(room[message.ParentID].Content))
                {
                    var p = PostFetcher.GetQuestion(post.Url);
                    var info = PostAnalyser.AnalyseAnswer(p);

                    if (info.Type == PostType.LowQuality)
                    {
                        var newMessage = MessageGenerator.GetPostReport(info, p, true);
                        var postedMessage = Config.PrimaryRoom.PostMessage("**Low Quality** " + newMessage);

                        Stats.PostedReports.Add(new Report
                        {
                            Message = postedMessage,
                            Post = p,
                            Analysis = info
                        });
                    }
                }
                else
                {
                    var p = PostFetcher.GetAnswer(post.Url);
                    var info = PostAnalyser.AnalyseAnswer(p);

                    if (info.Type == PostType.LowQuality)
                    {
                        var newMessage = MessageGenerator.GetPostReport(info, p);
                        var postedMessage = Config.PrimaryRoom.PostMessage("**Low Quality** " + newMessage);

                        Stats.PostedReports.Add(new Report
                        {
                            Message = postedMessage,
                            Post = p,
                            Analysis = info
                        });
                    }
                }
            }

            Config.Core.RegisterFP(post, analysis);

            return room.EditMessage(message.ParentID, "---" + room[message.ParentID].Content + "---") ? new ReplyMessage("") : new ReplyMessage("`FP acknowledged.`");
        }

        private static ReplyMessage TruePositive(string command)
        {
            string postBack = null;

            if (command.ToLowerInvariant().StartsWith("tpa"))
            {
                var m = room[message.ParentID].Content;

                if (analysis.Type == PostType.Offensive || command.ToLowerInvariant() == "tpa clean")
                {
                    m = ReportCleaner.GetCleanReport(message.ParentID);
                }

                foreach (var secondaryRoom in Config.SecondaryRooms)
                {
                    var postedMessage = secondaryRoom.PostMessage(m);

                    Stats.PostedReports.Add(new Report
                    {
                        Message = postedMessage,
                        Post = post,
                        Analysis = analysis
                    });
                }

                postBack = " ***TPA Acknowledged***.";
            }

            if (analysis.Type == PostType.Spam)
            {
                Stats.Spammers.Add(new Spammer(post.Site, post.AuthorName));
            }

            if (analysis.Type != PostType.BadTagUsed)
            {
                Config.Core.RegisterTP(post, analysis);
            }

            var reportMessage = room[message.ParentID].Content;
            reportMessage = reportMessage.Remove(reportMessage.Length - 1);

            return room.EditMessage(message.ParentID, reportMessage + (postBack ?? " ***TP Acknowledged***.")) ? new ReplyMessage("") : new ReplyMessage("`TP acknowledged.`");
        }

        # endregion

        # region Auto commands

        private static ReplyMessage AutoBlackTerm(string command)
        {
            var filterInfo = CommandParser.ParseFilterConfig(command);
            var startIndex = command.IndexOf(' ') + 1;
            var persistence = char.ToLowerInvariant(command[startIndex - 2]) == 'p';
            Regex term;

            try
            {
                term = new Regex(command.Substring(startIndex, command.Length - startIndex), RegexOptions.CultureInvariant);
            }
            catch (Exception)
            {
                return new ReplyMessage("`Unable to toggle auto flag. Invalid regex.`");
            }

            if (term.IsFlakRegex())
            {
                return new ReplyMessage("`ReDoS detected (auto not toggled).`");
            }

            if (!Config.BlackFilters[filterInfo].Terms.Contains(term))
            {
                return new ReplyMessage("`Term not found.`");
            }

            var isAuto = Config.BlackFilters[filterInfo].Terms.GetRealTerm(term).IsAuto;

            Config.BlackFilters[filterInfo].SetAuto(term, !isAuto, persistence);

            return new ReplyMessage("`Auto toggled (now " + !isAuto + ")!`");
        }

        //private static ReplyMessage AutoBQTTerm(string command)
        //{
        //    var editCommand = command.Remove(0, 10);

        //    if (!editCommand.StartsWith("off") && !editCommand.StartsWith("spam") && !editCommand.StartsWith("lq") && !editCommand.StartsWith("name")) { return new ReplyMessage("`Command not recognised.`"); }

        //    var startIndex = command.IndexOf(' ') + 1;
        //    var persistence = command[startIndex - 2] == 'p' || command[startIndex - 2] == 'P';
        //    var term = new Regex(command.Substring(startIndex, command.Length - startIndex));

        //    if (editCommand.StartsWith("off"))
        //    {
        //        if (!GlobalInfo.BlackFilters[FilterClass.QuestionTitleBlackOff].Terms.Contains(term)) { return new ReplyMessage("`Blacklist term does not exist.`"); }

        //        var isAuto = GlobalInfo.BlackFilters[FilterClass.QuestionTitleBlackOff].Terms.First(t => t.Regex.ToString() == term.ToString()).IsAuto;

        //        GlobalInfo.BlackFilters[FilterClass.QuestionTitleBlackOff].SetAuto(term, !isAuto, persistence);

        //        return new ReplyMessage("`Auto toggled (now " + !isAuto + ").`");
        //    }

        //    if (editCommand.StartsWith("spam"))
        //    {
        //        if (!GlobalInfo.BlackFilters[FilterClass.QuestionTitleBlackSpam].Terms.Contains(term)) { return new ReplyMessage("`Blacklist term does not exist.`"); }

        //        var isAuto = GlobalInfo.BlackFilters[FilterClass.QuestionTitleBlackSpam].Terms.First(t => t.Regex.ToString() == term.ToString()).IsAuto;

        //        GlobalInfo.BlackFilters[FilterClass.QuestionTitleBlackSpam].SetAuto(term, !isAuto, persistence);

        //        return new ReplyMessage("`Auto toggled (now " + !isAuto + ").`");
        //    }

        //    if (editCommand.StartsWith("lq"))
        //    {
        //        if (!GlobalInfo.BlackFilters[FilterClass.QuestionTitleBlackLQ].Terms.Contains(term)) { return new ReplyMessage("`Blacklist term does not exist.`"); }

        //        var isAuto = GlobalInfo.BlackFilters[FilterClass.QuestionTitleBlackLQ].Terms.First(t => t.Regex.ToString() == term.ToString()).IsAuto;

        //        GlobalInfo.BlackFilters[FilterClass.QuestionTitleBlackLQ].SetAuto(term, !isAuto, persistence);

        //        return new ReplyMessage("`Auto toggled (now " + !isAuto + ").`");
        //    }

        //    if (editCommand.StartsWith("name"))
        //    {
        //        if (!GlobalInfo.BlackFilters[FilterClass.QuestionTitleBlackName].Terms.Contains(term)) { return new ReplyMessage("`Blacklist term does not exist.`"); }

        //        var isAuto = GlobalInfo.BlackFilters[FilterClass.QuestionTitleBlackName].Terms.First(t => t.Regex.ToString() == term.ToString()).IsAuto;

        //        GlobalInfo.BlackFilters[FilterClass.QuestionTitleBlackName].SetAuto(term, !isAuto, persistence);

        //        return new ReplyMessage("`Auto toggled (now " + !isAuto + ").`");
        //    }

        //    return new ReplyMessage("`Command not recognised.`");
        //}

        //private static ReplyMessage AutoBQBTerm(string command)
        //{
        //    var editCommand = command.Remove(0, 10);

        //    if (!editCommand.StartsWith("off") && !editCommand.StartsWith("spam") && !editCommand.StartsWith("lq")) { return new ReplyMessage("`Command not recognised.`"); }

        //    var startIndex = command.IndexOf(' ') + 1;
        //    var persistence = command[startIndex - 2] == 'p' || command[startIndex - 2] == 'P';
        //    var term = new Regex(command.Substring(startIndex, command.Length - startIndex));

        //    if (editCommand.StartsWith("off"))
        //    {
        //        if (!GlobalInfo.BlackFilters[FilterClass.QuestionBodyBlackOff].Terms.Contains(term)) { return new ReplyMessage("`Blacklist term does not exist.`"); }

        //        var isAuto = GlobalInfo.BlackFilters[FilterClass.QuestionBodyBlackOff].Terms.First(t => t.Regex.ToString() == term.ToString()).IsAuto;

        //        GlobalInfo.BlackFilters[FilterClass.QuestionBodyBlackOff].SetAuto(term, !isAuto, persistence);

        //        return new ReplyMessage("`Auto toggled (now " + !isAuto + ").`");
        //    }

        //    if (editCommand.StartsWith("spam"))
        //    {
        //        if (!GlobalInfo.BlackFilters[FilterClass.QuestionBodyBlackSpam].Terms.Contains(term)) { return new ReplyMessage("`Blacklist term does not exist.`"); }

        //        var isAuto = GlobalInfo.BlackFilters[FilterClass.QuestionBodyBlackSpam].Terms.First(t => t.Regex.ToString() == term.ToString()).IsAuto;

        //        GlobalInfo.BlackFilters[FilterClass.QuestionBodyBlackSpam].SetAuto(term, !isAuto, persistence);

        //        return new ReplyMessage("`Auto toggled (now " + !isAuto + ").`");
        //    }

        //    if (editCommand.StartsWith("lq"))
        //    {
        //        if (!GlobalInfo.BlackFilters[FilterClass.QuestionBodyBlackLQ].Terms.Contains(term)) { return new ReplyMessage("`Blacklist term does not exist.`"); }

        //        var isAuto = GlobalInfo.BlackFilters[FilterClass.QuestionBodyBlackLQ].Terms.First(t => t.Regex.ToString() == term.ToString()).IsAuto;

        //        GlobalInfo.BlackFilters[FilterClass.QuestionBodyBlackLQ].SetAuto(term, !isAuto, persistence);

        //        return new ReplyMessage("`Auto toggled (now " + !isAuto + ").`");
        //    }

        //    return new ReplyMessage("`Term updated.`");
        //}

        //private static ReplyMessage AutoBATerm(string command)
        //{
        //    var editCommand = command.Remove(0, 9);

        //    if (!editCommand.StartsWith("off") && !editCommand.StartsWith("spam") && !editCommand.StartsWith("lq") && !editCommand.StartsWith("name")) { return new ReplyMessage("`Command not recognised.`"); }

        //    var startIndex = command.IndexOf(' ') + 1;
        //    var persistence = command[startIndex - 2] == 'p' || command[startIndex - 2] == 'P';
        //    var term = new Regex(command.Substring(startIndex, command.Length - startIndex));

        //    if (editCommand.StartsWith("off"))
        //    {
        //        if (!GlobalInfo.BlackFilters[FilterClass.AnswerBlackOff].Terms.Contains(term)) { return new ReplyMessage("`Blacklist term does not exist.`"); }

        //        var isAuto = GlobalInfo.BlackFilters[FilterClass.AnswerBlackOff].Terms.First(t => t.Regex.ToString() == term.ToString()).IsAuto;

        //        GlobalInfo.BlackFilters[FilterClass.AnswerBlackOff].SetAuto(term, !isAuto, persistence);

        //        return new ReplyMessage("`Auto toggled (now " + !isAuto + ").`");
        //    }

        //    if (editCommand.StartsWith("spam"))
        //    {
        //        if (!GlobalInfo.BlackFilters[FilterClass.AnswerBlackSpam].Terms.Contains(term)) { return new ReplyMessage("`Blacklist term does not exist.`"); }

        //        var isAuto = GlobalInfo.BlackFilters[FilterClass.AnswerBlackSpam].Terms.First(t => t.Regex.ToString() == term.ToString()).IsAuto;

        //        GlobalInfo.BlackFilters[FilterClass.AnswerBlackSpam].SetAuto(term, !isAuto, persistence);

        //        return new ReplyMessage("`Auto toggled (now " + !isAuto + ").`");
        //    }

        //    if (editCommand.StartsWith("lq"))
        //    {
        //        if (!GlobalInfo.BlackFilters[FilterClass.AnswerBlackLQ].Terms.Contains(term)) { return new ReplyMessage("`Blacklist term does not exist.`"); }

        //        var isAuto = GlobalInfo.BlackFilters[FilterClass.AnswerBlackLQ].Terms.First(t => t.Regex.ToString() == term.ToString()).IsAuto;

        //        GlobalInfo.BlackFilters[FilterClass.AnswerBlackLQ].SetAuto(term, !isAuto, persistence);

        //        return new ReplyMessage("`Auto toggled (now " + !isAuto + ").`");
        //    }

        //    if (editCommand.StartsWith("name"))
        //    {
        //        if (!GlobalInfo.BlackFilters[FilterClass.AnswerBlackName].Terms.Contains(term)) { return new ReplyMessage("`Blacklist term does not exist.`"); }

        //        var isAuto = GlobalInfo.BlackFilters[FilterClass.AnswerBlackName].Terms.First(t => t.Regex.ToString() == term.ToString()).IsAuto;

        //        GlobalInfo.BlackFilters[FilterClass.AnswerBlackName].SetAuto(term, !isAuto, persistence);

        //        return new ReplyMessage("`Auto toggled (now " + !isAuto + ").`");
        //    }

        //    return new ReplyMessage("`Term updated.`");
        //}

        # endregion

        private static ReplyMessage AddTag(string command)
        {
            var tagCommand = command.Remove(0, 8);
            var spaceCount = tagCommand.Count(c => c == ' ');

            if (spaceCount != 1 && spaceCount != 2) { return new ReplyMessage("`Command not recognised.`"); }

            var site = tagCommand.Substring(0, tagCommand.IndexOf(" ", StringComparison.Ordinal));
            var metaPost = "";
            string tag;

            if (spaceCount == 2)
            {
                tag = tagCommand.Substring(site.Length + 1, tagCommand.IndexOf(" ", site.Length + 1, StringComparison.Ordinal) - 1 - site.Length);

                metaPost = tagCommand.Substring(tagCommand.LastIndexOf(" ", StringComparison.Ordinal) + 1);
            }
            else
            {
                tag = tagCommand.Remove(0, tagCommand.IndexOf(" ", StringComparison.Ordinal) + 1);
            }

            if (Config.BadTags.Tags.ContainsKey(site) && Config.BadTags.Tags[site].ContainsKey(tag)) { return new ReplyMessage("`Tag already exists.`"); }

            Config.BadTags.AddTag(site, tag, metaPost);

            return new ReplyMessage("`Tag added!`");
        }

        private static ReplyMessage RemoveTag(string command)
        {
            var tagCommand = command.Remove(0, 8);

            if (tagCommand.Count(c => c == ' ') != 1) { return new ReplyMessage("`Command not recognised.`"); }

            var site = tagCommand.Substring(0, tagCommand.IndexOf(" ", StringComparison.Ordinal));
            var tag = tagCommand.Remove(0, tagCommand.IndexOf(" ", StringComparison.Ordinal) + 1);

            if (Config.BadTags.Tags.ContainsKey(site))
            {
                if (Config.BadTags.Tags[site].ContainsKey(tag))
                {
                    Config.BadTags.RemoveTag(site, tag);

                    return new ReplyMessage("`Tag removed!`");
                }

                return new ReplyMessage("`Tag not found.`");
            }

            return new ReplyMessage("`Site not found.`");
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
            var newReport = Regex.Replace(Stats.PostedReports.First(r => r.Message.ID == message.ParentID).Message.Content, @"\*\* \(\d*(\.\d)?\%\)\:", "?**:");

            foreach (var secondaryRoom in Config.SecondaryRooms)
            {
                var postedMessage = secondaryRoom.PostMessage(newReport);

                Stats.PostedReports.Add(new Report
                {
                    Message = postedMessage,
                    Post = post,
                    Analysis = analysis
                });
            }

            return new ReplyMessage("");
        }

        # endregion

        # region Owner commands.

        private static ReplyMessage SetStatus(string command)
        {
            var newStatus = command.Remove(0, 10).Trim();

            Config.Status = newStatus;

            return new ReplyMessage("`Status updated!`");
        }

        private static ReplyMessage AddUser(string command)
        {
            var id = int.Parse(command.Replace("add-user", "").Trim());

            if (Config.UserAccess.PrivUsers.Contains(id)) { return new ReplyMessage("`User already has command access.`"); }

            Config.UserAccess.AddPrivUser(id);

            return new ReplyMessage("`User added!`");
        }

        private static ReplyMessage BanUser(string command)
        {
            var id = command.Replace("ban-user", "").Trim();

            if (Config.BannedUsers.IsUserBanned(id)) { return new ReplyMessage("`User is already banned.`"); }

            return new ReplyMessage(Config.BannedUsers.AddUser(id) ? "`User banned!`" : "`Warning: the banned users file is missing (unable to add user). All commands have been disabled until the issue has been resolved.`");
        }

        private static ReplyMessage SetAccuracyThreshold(string command)
        {
            if (command.IndexOf(" ", StringComparison.Ordinal) == -1 || command.All(c => !Char.IsDigit(c))) { return new ReplyMessage("`Command not recognised.`"); }

            var newLimit = command.Remove(0, 10);

            Config.AccuracyThreshold = float.Parse(newLimit, CultureInfo.InvariantCulture);

            return new ReplyMessage("`Accuracy threshold updated.`");
        }

        private static ReplyMessage FullScan()
        {
            if (Config.FullScanEnabled)
            {
                Config.FullScanEnabled = false;

                return new ReplyMessage("`Full scan disabled.`");
            }

            Config.FullScanEnabled = true;

            return new ReplyMessage("`Full scan enabled.`");
        }

        private static ReplyMessage ResumeBot()
        {
            Config.IsRunning = true;

            return new ReplyMessage("`Phamhilator™ resumed.`");
        }

        private static ReplyMessage PauseBot()
        {
            Config.IsRunning = false;

            return new ReplyMessage("`Phamhilator™ paused.`");
        }

        # endregion
    }
}
