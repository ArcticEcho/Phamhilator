namespace Phamhilator
{
    public class ReplyMessage
    {
        public string Content { get; private set; }
        public bool IsReply { get; private set; }



        public ReplyMessage(string content, bool isReply = true)
        {
            Content = content;
            IsReply = isReply;
        }
    }
}
