﻿using Client.Models;
using Microsoft.EntityFrameworkCore;

namespace Client.Data
{
    class UserContext : DbContext
    {
        public DbSet<Token> Tokens { get; set; }

        public UserContext()
        {
            Database.EnsureCreated();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Filename=Users.db");
        }
    }
}
