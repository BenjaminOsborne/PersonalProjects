using System;
using System.Collections.Generic;
using System.Data.Entity;

namespace WebHost.Persistence
{
    public class ChatsContext : DbContext
    {
        public ChatsContext() : base("ChatsConnection")
        {
        }

        public DbSet<ConversationGroup> ConversationGroups { get; set; }

        public DbSet<Message> Messages { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ConversationGroup>()
                .HasKey(u => u.Id);

            modelBuilder.Entity<ConversationGroup>()
                .HasKey(u => u.Id);

            modelBuilder.Entity<Message>()
                .HasKey(u => u.Id);

            modelBuilder.Entity<Message>()
                .HasRequired(c => c.ConversationGroup)
                .WithMany(x => x.Messages)
                .HasForeignKey(c => c.ConversationGroupId);
        }
    }

    public class ConversationGroup
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string UsersJson { get; set; }

        public virtual ICollection<Message> Messages { get; set; }
    }

    public class Message
    {
        public int Id { get; set; }

        public DateTimeOffset MessageTime { get; set; }

        public int ConversationGroupId { get; set; }

        public virtual ConversationGroup ConversationGroup { get; set; }

        public string Sender { get; set; }

        public string Content { get; set; }
    }
}