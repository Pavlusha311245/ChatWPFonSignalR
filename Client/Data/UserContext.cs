using Client.Models;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Client.Data
{
    class UserContext : DbContext
    {
        public const string DEFAULTDATABASE = "Users.db";

        public DbSet<Token> Tokens { get; set; }
        public DbSet<User> Users { get; set; }

        private readonly string dbFile = DEFAULTDATABASE;
        private SqliteConnection connection;

        public UserContext(string databaseFile)
        {
            if (!string.IsNullOrEmpty(databaseFile)) dbFile = databaseFile;
        }

        public UserContext(SqliteConnection sqliteConnection)
        {
            if (!string.IsNullOrEmpty(sqliteConnection?.DataSource)) dbFile = sqliteConnection.DataSource;
            connection = sqliteConnection;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            connection ??= InitializeSQLiteConnection(dbFile);
            optionsBuilder.UseSqlite(connection);
        }

        private static SqliteConnection InitializeSQLiteConnection(string databaseFile)
        {
            var connectionString = new SqliteConnectionStringBuilder
            {
                DataSource = databaseFile,
                Password = "12345678"
            };

            return new SqliteConnection(connectionString.ToString());
        }
    }
}
