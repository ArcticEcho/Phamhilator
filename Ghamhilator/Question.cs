using System;
using System.Collections.Generic;
using System.Net;



namespace Ghamhilator
{
    public class Question : Post
    {
        public List<string> Tags { get; private set; }
        internal string Html { get; private set; }



        public Question(string url, string site, string body, string title, int score, string authorName, string authorLink, int authorRep, List<string> tags, string html)
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