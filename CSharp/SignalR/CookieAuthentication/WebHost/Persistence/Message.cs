using System;
using System.Collections.Generic;
using System.Data.Entity;

namespace WebHost.Persistence
{
    public class ChatsContext : DbContext
    {
        public ChatsContext() : base("ChatsDatabase")
        {
        }

        public DbSet<ConversationGroup> ConversationGroups { get; set; }
        public DbSet<ConversationUser> ConversationUsers { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<MessageReads> MessageReads { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            //Primary Keys
            modelBuilder.Entity<ConversationGroup>()
                .HasKey(u => u.Id);

            modelBuilder.Entity<ConversationUser>()
                .HasKey(x => x.Id);

            modelBuilder.Entity<Message>()
                .HasKey(u => u.Id);

            modelBuilder.Entity<MessageReads>()
                .HasKey(x => x.Id);

            //Foreign Keys
            modelBuilder.Entity<ConversationUser>()
                .HasRequired(x => x.ConversationGroup)
                .WithMany(x => x.UserConversations)
                .HasForeignKey(x => x.ConversationGroupId);

            modelBuilder.Entity<Message>()
                .HasRequired(c => c.ConversationGroup)
                .WithMany(x => x.Messages)
                .HasForeignKey(c => c.ConversationGroupId);

            modelBuilder.Entity<MessageReads>()
                .HasRequired(c => c.Message)
                .WithMany(x => x.MessageReads)
                .HasForeignKey(c => c.MessageId);
        }
    }

    public class ConversationGroup
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public virtual ICollection<ConversationUser> UserConversations { get; set; }

        public virtual ICollection<Message> Messages { get; set; }
    }

    public class ConversationUser
    {
        public int Id { get; set; }

        public int ConversationGroupId { get; set; }
        public virtual ConversationGroup ConversationGroup { get; set; }

        public string User { get; set; }
    }

    public class Message
    {
        public int Id { get; set; }

        public DateTimeOffset MessageTime { get; set; }

        public int ConversationGroupId { get; set; }

        public virtual ConversationGroup ConversationGroup { get; set; }

        public string Sender { get; set; }

        public string Content { get; set; }

        public virtual ICollection<MessageReads> MessageReads { get; set; }
    }

    public class MessageReads
    {
        public int Id { get; set; }

        public int MessageId { get; set; }
        public virtual Message Message { get; set; }

        public string User { get; set; }
        public bool HasRead { get; set; }
    }
}