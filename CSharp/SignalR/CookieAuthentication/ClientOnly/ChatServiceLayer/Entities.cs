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

    public class User
    {
        public User(string name)
        {
            Name = name;
        }

        public string Name { get; }
    }
}
