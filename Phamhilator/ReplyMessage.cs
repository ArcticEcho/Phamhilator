namespace Phamhilator
{
    public class ReplyMessage
    {
        public string Content { get; private set; }
        public bool Reply { get; private set; }



        public ReplyMessage(string content, bool reply = true)
        {
            Content = content;
            Reply = reply;
        }
    }
}
