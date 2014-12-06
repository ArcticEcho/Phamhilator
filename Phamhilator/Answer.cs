namespace Phamhilator
{
	public class Answer : Post
	{
		public int Score { get; private set; }
        public string Body { get; private set; }
        public int AuthorRep { get; private set; }



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

        public Answer(string url)
        {
            Url = url;
            Body = "";
        }

	    public Answer()
	    {

            Body = "";
	    }
	}
}
