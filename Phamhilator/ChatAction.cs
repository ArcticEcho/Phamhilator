using ChatExchangeDotNet;



namespace Phamhilator
{
    public class ChatAction
    {
        public Room Room { get; private set; }
        public MessageHandler.MessagePostedCallBack Action { get; private set; }



        public ChatAction(Room room, MessageHandler.MessagePostedCallBack action)
        {
            Action = action;
            Room = room;
        }
    }
}
