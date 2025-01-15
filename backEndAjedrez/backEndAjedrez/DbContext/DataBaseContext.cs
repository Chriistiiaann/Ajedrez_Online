namespace backEndAjedrez.DbContext;

using backEndAjedrez.Models;
using Microsoft.EntityFrameworkCore;

public class DataBaseContext : DbContext
{

    private const string DATABASE_PATH = "chess.db";     

    public DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        string baseDir = AppDomain.CurrentDomain.BaseDirectory;

        optionsBuilder.UseSqlite($"DataSource={baseDir}{DATABASE_PATH}");
    }
}

