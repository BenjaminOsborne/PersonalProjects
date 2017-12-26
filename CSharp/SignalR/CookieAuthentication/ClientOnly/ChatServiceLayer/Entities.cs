using System;

namespace ChatServiceLayer
{
    public class SessionKey
    {
        public SessionKey(Guid id)
        {
            Id = id;
        }

        public Guid Id { get; }
    }
    
    public class Message
    {
        public Message(Guid messageId, DateTime messageTime, string sender, string receiver, string text)
        {
            MessageId = messageId;
            MessageTime = messageTime;
            Sender = sender;
            Receiver = receiver;
            Text = text;
        }

        public Guid MessageId { get; }
        public DateTime MessageTime { get; }
        public string Sender { get; }
        public string Receiver { get; }
        public string Text { get; }
    }
}
