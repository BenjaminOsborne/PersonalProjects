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
        public Message(string user, string text)
        {
            User = user;
            Text = text;
        }

        public string User { get; }
        public string Text { get; }
    }
}
