﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;



namespace Phamhilator
{
    public static class CommandParser
    {
        private const RegexOptions options = RegexOptions.Compiled | RegexOptions.CultureInvariant;
        private static Regex filterConfigVaildCommand = new Regex(@"^[a-z]{3,}?\-[wb]\-(a|q[tb])\-", options);
        private static Regex filterTypeClassStrip = new Regex(@"^[a-z]{3,}?\-[wb]\-", options);
        private static Regex filterSubclassStrip = new Regex(@"^[a-z]{3,}?\-[wb]\-(a|q[bt])\-", options);
        private static Regex filterConfigIsBlack = new Regex(@"^[a-z]{3,}?\-b", options);
        private static Regex filterConfigIsQt = new Regex(@"^[a-z]{3,}?\-[wb]\-qt", options);
        private static Regex filterConfigIsQb = new Regex(@"^[a-z]{3,}?\-[wb]\-qb", options);
        private static Regex filterConfigIsA = new Regex(@"^[a-z]{3,}?\-[wb]\-a", options);



        public static FilterConfig ParseFilterConfig(string command)
        {
            var lower = command.ToLowerInvariant();

            if (!filterConfigVaildCommand.IsMatch(lower)) { throw new NotSupportedException(); }

            var type = filterConfigIsBlack.IsMatch(lower) ? FilterType.Black : FilterType.White;
            var filterTypeCommand = filterTypeClassStrip.Replace(lower, "").Substring(0, 2);
            var filterSubclassCommand = filterSubclassStrip.Replace(lower, "");
            var classification = FilterClass.AnswerLQ;

            switch (filterTypeCommand)
            {
                case "qt":
                {
                    classification = ParseQt(filterSubclassCommand);
                    break;
                }
                case "qb":
                {
                    classification = ParseQt(filterSubclassCommand);
                    break;
                }
                case "a-":
                {
                    classification = ParseQt(filterSubclassCommand);
                    break;
                }
            }

            return new FilterConfig(classification, type);
        }



        private static FilterClass ParseQt(string command)
        {
            switch (command)
            {
                case "spam":
                {
                    return FilterClass.QuestionTitleSpam;
                }
                case "off":
                {
                    return FilterClass.QuestionTitleOff;
                }
                case "lq":
                {
                    return FilterClass.QuestionTitleLQ;
                }
                case "name":
                {
                    return FilterClass.QuestionTitleName;
                }
                default:
                {
                    throw new NotSupportedException();
                }
            }
        }

        private static FilterClass ParseQb(string command)
        {
            switch (command)
            {
                case "spam":
                {
                    return FilterClass.QuestionBodySpam;
                }
                case "off":
                {
                    return FilterClass.QuestionBodyOff;
                }
                case "lq":
                {
                    return FilterClass.QuestionBodyLQ;
                }
                default:
                {
                    throw new NotSupportedException();
                }
            }
        }

        private static FilterClass ParseA(string command)
        {
            switch (command)
            {
                case "spam":
                {
                    return FilterClass.AnswerSpam;
                }
                case "off":
                {
                    return FilterClass.AnswerOff;
                }
                case "lq":
                {
                    return FilterClass.AnswerLQ;
                }
                case "name":
                {
                    return FilterClass.AnswerName;
                }
                default:
                {
                    throw new NotSupportedException();
                }
            }
        }
    }
}
