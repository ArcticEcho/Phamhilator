namespace Phamhilator
{
    public abstract class Post
    {
        public string Title { get; protected set; }
        public string AuthorName { get; protected set; }
        public string AuthorLink { get; protected set; }
        public string Url { get; protected set; }
        public string Site { get; protected set; }
        public string Body { get; protected set; }
    }
}
