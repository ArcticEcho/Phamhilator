using System;
using System.Collections.Generic;
using System.Net;
using CsQuery;



namespace Phamhilator
{
    public class Question : Post
    {
        public bool PopulateExtraDataFailed { get; private set; }
        public List<string> Tags { get; private set; }
        public string Html { get; private set; }



        public Question(string url, string title, string site, string authorName, string authorLink, List<string> tags)
        {
            Url = url;
            Title = title;
            Site = site;
            AuthorName = authorName;
            AuthorLink = authorLink;
            Tags = tags;

            if (GlobalInfo.FullScanEnabled)
            {
                PopulateExtraData();
            }
        }

        public Question(string url, string title, string body, string site, int score, string authorName, string authorLink, int authorRep, List<string> tags)
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
        }



        private void PopulateExtraData()
        {
            try
            {
                Html = new StringDownloader().DownloadString(Url);
                var dom = CQ.Create(Html);

                Body = WebUtility.HtmlDecode(dom[".post-text"].Html().Trim());
                Score = int.Parse(dom[".vote-count-post"].Html());
                AuthorRep = PostRetriever.ParseRep(dom[".reputation-score"].Html());
            }
            catch (Exception)
            {
                PopulateExtraDataFailed = true;
            }
        }
    }
}