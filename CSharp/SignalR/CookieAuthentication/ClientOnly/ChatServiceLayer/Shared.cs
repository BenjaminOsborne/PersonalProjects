using System;

namespace ChatServiceLayer.Shared
{
    public class Message
    {
        public Guid MessageId { get; set; }
        public DateTime MessageTime { get; set; }
        public string Sender { get; set; }
        public string Receiver { get; set; }
        public string Text { get; set; }
    }
}
