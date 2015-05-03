/*
 * Phamhilator. A .Net based bot network catching spam/low quality posts for Stack Exchange.
 * Copyright © 2015, ArcticEcho.
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */





using System;
using System.Collections.Generic;
using System.Net;



namespace Yam.Core
{
    public class Question : Post
    {
        public List<string> Tags { get; private set; }
        internal string Html { get; private set; }



        public Question(string url, string site, string title, string body, int score, string authorName, string authorLink, int authorRep, List<string> tags, string html)
        {
            Url = url;
            Title = title;
            Body = body;
            Site = site;
            Score = score;
            AuthorName = authorName;
            AuthorLink = authorLink;
            AuthorRep = authorRep;
            Tags = tags;
            Html = html;
        }
    }
}