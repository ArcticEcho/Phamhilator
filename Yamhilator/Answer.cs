﻿namespace Yamhilator
 {
     public class Answer : Post
     {
         public Answer(string url, string title, string body, string site, int score, string authorName, string authorLink, int authorRep)
         {
             Url = url;
             Title = title;
             Body = body;
             Site = site;
             Score = score;
             AuthorName = authorName;
             AuthorLink = authorLink;
             AuthorRep = authorRep;
         }
     }
 }