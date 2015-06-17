///*
// * Phamhilator. A .Net based bot network catching spam/low quality posts for Stack Exchange.
// * Copyright © 2015, ArcticEcho.
// *
// * This program is free software: you can redistribute it and/or modify
// * it under the terms of the GNU General Public License as published by
// * the Free Software Foundation, either version 3 of the License, or
// * (at your option) any later version.
// *
// * This program is distributed in the hope that it will be useful,
// * but WITHOUT ANY WARRANTY; without even the implied warranty of
// * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// * GNU General Public License for more details.
// *
// * You should have received a copy of the GNU General Public License
// * along with this program.  If not, see <http://www.gnu.org/licenses/>.
// */





//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using System.Text.RegularExpressions;



//namespace Phamhilator.Pham.Core
//{
//    public static class CommandParser
//    {
//        private const RegexOptions options = RegexOptions.Compiled | RegexOptions.CultureInvariant;
//        private static Regex filterConfigVaildCommand = new Regex(@"^[a-z]{3,}?\-[wb]\-(a|q[tb])\-", options);
//        private static Regex filterTypeClassStrip = new Regex(@"^[a-z]{3,}?\-[wb]\-", options);
//        private static Regex filterSubclassStrip = new Regex(@"^[a-z]{3,}?\-[wb]\-(a|q[bt])\-", options);
//        private static Regex filterConfigIsBlack = new Regex(@"^[a-z]{3,}?\-b", options);
//        private static Regex filterConfigIsQt = new Regex(@"^[a-z]{3,}?\-[wb]\-qt", options);
//        private static Regex filterConfigIsQb = new Regex(@"^[a-z]{3,}?\-[wb]\-qb", options);
//        private static Regex filterConfigIsA = new Regex(@"^[a-z]{3,}?\-[wb]\-a", options);



//        public static FilterConfig ParseFilterConfig(string command)
//        {
//            var lower = command.ToLowerInvariant();

//            if (!filterConfigVaildCommand.IsMatch(lower)) { throw new NotSupportedException(); }

//            var type = filterConfigIsBlack.IsMatch(lower) ? FilterType.Black : FilterType.White;
//            var filterTypeCommand = filterTypeClassStrip.Replace(lower, "").Substring(0, 2);
//            var filterSubclassCommand = filterSubclassStrip.Replace(lower, "").Substring(0, 2);
//            var classification = FilterClass.AnswerLQ;

//            switch (filterTypeCommand)
//            {
//                case "qt":
//                {
//                    classification = ParseQt(filterSubclassCommand);
//                    break;
//                }
//                case "qb":
//                {
//                    classification = ParseQb(filterSubclassCommand);
//                    break;
//                }
//                case "a-":
//                {
//                    classification = ParseA(filterSubclassCommand);
//                    break;
//                }
//            }

//            return new FilterConfig(classification, type);
//        }



//        private static FilterClass ParseQt(string command)
//        {
//            switch (command)
//            {
//                case "sp":
//                {
//                    return FilterClass.QuestionTitleSpam;
//                }
//                case "of":
//                {
//                    return FilterClass.QuestionTitleOff;
//                }
//                case "lq":
//                {
//                    return FilterClass.QuestionTitleLQ;
//                }
//                case "na":
//                {
//                    return FilterClass.QuestionTitleName;
//                }
//                default:
//                {
//                    throw new NotSupportedException();
//                }
//            }
//        }

//        private static FilterClass ParseQb(string command)
//        {
//            switch (command)
//            {
//                case "sp":
//                {
//                    return FilterClass.QuestionBodySpam;
//                }
//                case "of":
//                {
//                    return FilterClass.QuestionBodyOff;
//                }
//                case "lq":
//                {
//                    return FilterClass.QuestionBodyLQ;
//                }
//                default:
//                {
//                    throw new NotSupportedException();
//                }
//            }
//        }

//        private static FilterClass ParseA(string command)
//        {
//            switch (command)
//            {
//                case "sp":
//                {
//                    return FilterClass.AnswerSpam;
//                }
//                case "of":
//                {
//                    return FilterClass.AnswerOff;
//                }
//                case "lq":
//                {
//                    return FilterClass.AnswerLQ;
//                }
//                case "na":
//                {
//                    return FilterClass.AnswerName;
//                }
//                default:
//                {
//                    throw new NotSupportedException();
//                }
//            }
//        }
//    }
//}
