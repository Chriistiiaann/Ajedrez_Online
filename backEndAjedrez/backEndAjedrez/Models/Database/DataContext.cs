namespace backEndAjedrez.Models.Database;

using backEndAjedrez.Models.Database.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;

public class DataContext : DbContext
{

    private const string DATABASE_PATH = "chess.db";

    public DbSet<User> Users { get; set; }
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        string baseDir = AppDomain.CurrentDomain.BaseDirectory;

        optionsBuilder.UseSqlite($"DataSource={baseDir}{DATABASE_PATH}");
    }
}

