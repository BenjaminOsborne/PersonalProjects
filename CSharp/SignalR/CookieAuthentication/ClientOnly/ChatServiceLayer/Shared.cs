using System;

namespace ChatServiceLayer.Shared
{
    public class ConversationGroup
    {
        public int? Id { get; set; }
        public string Name { get; set; }
        public string[] Users { get; set; }
    }

    public class MessageRoute
    {
        public ConversationGroup Group { get; set; }
        public string Sender { get; set; }
    }

    public class Message
    {
        public int? Id { get; set; }
        public DateTime MessageTime { get; set; }

        public MessageRoute Route { get; set; }
        public string Content { get; set; }
    }

    public class MessageSendInfo
    {
        public MessageRoute Route { get; set; }
        public string Content { get; set; }
    }

    public class ChatHistories
    {
        public ChatHistory[] Histories { get; set; }
    }

    public class ChatHistory
    {
        public ConversationGroup ConversationGroup { get; set; }
        public Message[] Messages { get; set; }
    }
}
