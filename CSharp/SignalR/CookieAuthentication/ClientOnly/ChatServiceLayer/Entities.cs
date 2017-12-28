using System;
using System.Collections.Immutable;
using System.Linq;

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

    public class ConverationGroup : IEquatable<ConverationGroup>
    {
        #region Equality

        public bool Equals(ConverationGroup other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(Key, other.Key);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ConverationGroup)obj);
        }

        public override int GetHashCode() => Key.GetHashCode();

        public static bool operator ==(ConverationGroup left, ConverationGroup right) => Equals(left, right);

        public static bool operator !=(ConverationGroup left, ConverationGroup right) => !Equals(left, right);

        #endregion

        public static ConverationGroup Create(params string[] users) => new ConverationGroup(CollectionKey.CreateDistinctAndOrder(users));

        private ConverationGroup(CollectionKey<string> key)
        {
            Key = key;
            UsersFlat = string.Join(", ", key.Items);
        }

        public CollectionKey<string> Key { get; }

        public ImmutableList<string> Users => Key.Items;
        public string UsersFlat { get; }
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
