using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Server.Models;
using Server.Seeders;

namespace Server.Data
{
    public class ServerContext : IdentityDbContext<User>
    {
        public ServerContext(DbContextOptions<ServerContext> options) : base(options)
        {
            if (!Database.CanConnect())
            {
                Database.Migrate();
                DatabaseSeeder.Run();
            }

            DatabaseSeeder.Run();
        }        
    }
}
