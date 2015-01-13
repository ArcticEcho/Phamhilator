﻿namespace Phamhilator
 {
     public class Answer : Post
     {
         public Answer(string url, string excerpt, string body, string site, int score, string authorName, string authorLink, int authorRep)
         {
             Url = url;
             Title = excerpt;
             Body = body;
             Site = site;
             Score = score;
             AuthorName = authorName;
             AuthorLink = authorLink;
             AuthorRep = authorRep;
         }
     }
 }