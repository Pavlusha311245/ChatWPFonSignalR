using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Server.Models;

namespace Server.Data
{
    public class ServerContext : IdentityDbContext<User>
    {
        public DbSet<Task> Tasks { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<PersonalData> PersonalDatas { get; set; }

        public ServerContext(DbContextOptions<ServerContext> options) : base(options)
        {
            if (!Database.CanConnect())
            {
                Database.Migrate();
            }
        }
    }
}
