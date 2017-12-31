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

    public class ConversationGroup : IEquatable<ConversationGroup>
    {
        #region Equality

        public bool Equals(ConversationGroup other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Id == other.Id && string.Equals(Name, other.Name) && Equals(UsersKey, other.UsersKey);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ConversationGroup) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Id;
                hashCode = (hashCode * 397) ^ Name.GetHashCode();
                hashCode = (hashCode * 397) ^ UsersKey.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(ConversationGroup left, ConversationGroup right) => Equals(left, right);

        public static bool operator !=(ConversationGroup left, ConversationGroup right) => !Equals(left, right);

        #endregion

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
        public Message(int id, DateTime messageTime, MessageRoute route, string content)
        {
            Id = id;
            MessageTime = messageTime;
            Route = route;
            Content = content;
        }

        public int Id { get; }
        public DateTime MessageTime { get; }
        public MessageRoute Route { get; }
        public string Content { get; }
    }
}
