using System;

namespace ChatServiceLayer.Shared
{
    public class ConversationGroup
    {
        public string GroupName { get; set; }
        public string[] Users { get; set; }
    }

    public class MessageRoute
    {
        public ConversationGroup Group { get; set; }
        public string Sender { get; set; }
    }

    public class Message
    {
        public Guid MessageId { get; set; }
        public DateTime MessageTime { get; set; }

        public MessageRoute Route { get; set; }
        public string Content { get; set; }
    }

    public class MessageSendInfo
    {
        public MessageRoute Route { get; set; }
        public string Content { get; set; }
    }
}
