using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Server.Models;

namespace Server.Data
{
    public class ServerContext : IdentityDbContext<User>
    {
        public DbSet<Task> Tasks { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<UserProfile> UserProfiles { get; set; }
        public DbSet<Chat> Chats { get; set; }

        public ServerContext(DbContextOptions<ServerContext> options) : base(options)
        {
            if (!Database.CanConnect())
            {
                Database.Migrate();
            }
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseLazyLoadingProxies();

            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder
                .Entity<Chat>()
                .HasMany(c => c.Users)
                .WithMany(u => u.Chats)
                .UsingEntity<ChatUsers>(
                   j => j
                    .HasOne(cu => cu.User)
                    .WithMany(u => u.ChatUsers)
                    .HasForeignKey(u => u.UserID),
                j => j
                    .HasOne(cu => cu.Chat)
                    .WithMany(u => u.ChatUsers)
                    .HasForeignKey(c => c.ChatID)
                    , j =>
                    {
                        j.Property(cu => cu.Role).HasDefaultValue(ChatRoles.User);
                        j.HasKey(cu => new { cu.ChatID, cu.UserID });
                        j.ToTable("ChatUser");
                    }
            );

            base.OnModelCreating(builder);
        }
    }
}
