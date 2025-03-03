namespace backEndAjedrez.Models.Database;

using backEndAjedrez.Models.Database.Entities;
using Microsoft.EntityFrameworkCore;

public class DataContext : DbContext
{

    private const string DATABASE_PATH = "chess.db";

    public DbSet<User> Users { get; set; }
    public DbSet<Friend> Friends { get; set; }
    public DbSet<FriendRequest> FriendRequests { get; set; }
    public DbSet<MatchRequest> MatchRequests { get; set; }
    public DbSet<MatchHistory> MatchHistory { get; set; }
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        string baseDir = AppDomain.CurrentDomain.BaseDirectory;

        optionsBuilder.UseSqlite($"DataSource={baseDir}{DATABASE_PATH}");
    }
}

