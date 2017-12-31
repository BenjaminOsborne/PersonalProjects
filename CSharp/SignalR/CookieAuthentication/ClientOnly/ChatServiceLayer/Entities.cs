using System;
using System.Collections.Generic;
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
        public Message(int id, DateTime messageTime, MessageRoute route, string content, ImmutableList<ReadState> readStates)
        {
            Id = id;
            MessageTime = messageTime;
            Route = route;
            Content = content;
            ReadStates = readStates;
        }

        public int Id { get; }
        public DateTime MessageTime { get; }
        public MessageRoute Route { get; }
        public string Content { get; }
        public ImmutableList<ReadState> ReadStates { get; }

        public Message CloneAsReadFor(string user)
        {
            var updated = new ReadState(user, true);
            var exist = ReadStates.FirstOrDefault(x => x.User == user);
            var states = exist != null ? ReadStates.Replace(exist, updated) : ReadStates.Add(updated);
            return new Message(Id, MessageTime, Route, Content, states);
        }
    }

    public class ReadState
    {
        public ReadState(string user, bool hasRead)
        {
            User = user;
            HasRead = hasRead;
        }

        public string User { get; }
        public bool HasRead { get; }
    }
}
