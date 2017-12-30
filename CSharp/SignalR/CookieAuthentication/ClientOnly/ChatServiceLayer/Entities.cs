using System;
using System.Collections.Generic;
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

    public class ConversationGroup
    {
        public static CollectionKey<string> CreateUsersKey(IEnumerable<string> users)
        {
            return CollectionKey.CreateDistinctAndOrder(users);
        }

        public static ConversationGroup CreateFromExisting(int id, string name, IEnumerable<string> users)
        {
            var key = CreateUsersKey(users);
            return new ConversationGroup(id, name, key);
        }
        
        private ConversationGroup(int id, string name, CollectionKey<string> usersKey)
        {
            Id = id;
            Name = name;
            UsersKey = usersKey;
        }

        public int Id { get; }
        public string Name { get; }
        public CollectionKey<string> UsersKey { get; }
        public ImmutableList<string> Users => UsersKey.Items;
    }

    public class MessageRoute
    {
        public MessageRoute(ConversationGroup group, string sender)
        {
            Group = group;
            Sender = sender;
        }

        public ConversationGroup Group { get; }
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
