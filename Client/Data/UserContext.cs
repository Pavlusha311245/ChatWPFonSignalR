using Client.Models;
using Microsoft.EntityFrameworkCore;

namespace Client.Data
{
    class UserContext : DbContext
    {
        public DbSet<Token> Tokens { get; set; }
        public DbSet<User> Users { get; set; }

        public UserContext()
        {
            if (!Database.CanConnect())
                Database.EnsureCreated();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Filename=Users.db");
        }
    }
}
