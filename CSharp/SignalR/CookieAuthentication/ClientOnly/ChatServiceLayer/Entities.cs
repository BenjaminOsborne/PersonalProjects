using System;
using System.Collections.Immutable;

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

    public class ConverationGroup
    {
        public ConverationGroup(string groupName, ImmutableList<string> users)
        {
            GroupName = groupName;
            Users = users;
        }

        public string GroupName { get; }
        public ImmutableList<string> Users { get; }
    }

    public class MessageRoute
    {
        public MessageRoute(ConverationGroup group, string sender)
        {
            Group = group;
            Sender = sender;
        }

        public ConverationGroup Group { get; }
        public string Sender { get; }
    }

    public class Message
    {
        public Message(Guid messageId, DateTime messageTime, MessageRoute route, string content)
        {
            MessageId = messageId;
            MessageTime = messageTime;
            Route = route;
            Content = content;
        }

        public Guid MessageId { get; }
        public DateTime MessageTime { get; }
        public MessageRoute Route { get; }
        public string Content { get; }
    }
}
