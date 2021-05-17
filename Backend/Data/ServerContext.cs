using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Server.Models;

namespace Server.Data
{
    public class ServerContext : IdentityDbContext<User>
    {
        public ServerContext(DbContextOptions<ServerContext> options) : base(options)
        {
            if (!Database.CanConnect())
            {
                Database.Migrate();
            }                
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            
        }
    }
}
