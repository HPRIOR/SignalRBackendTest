using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace SignalRTestDotNet.GameContextNs;


public class GameContext : DbContext
{
    public DbSet<Session> Sessions { get; set; }
    public DbSet<Game> Games { get; set; }
    public DbSet<Player> Players { get; set; }
    public DbSet<Move> Moves { get; set; }
    public DbSet<Country> Countries { get; set; }

    public GameContext(DbContextOptions<GameContext> options) : base(options) { }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseNpgsql("Host=localhost:5432;Username=myusername;Password=mypassword;Database=myusername");

    protected override void OnModelCreating(ModelBuilder modelBuilder){
        modelBuilder.Entity<Session>().ToTable("Session");
        modelBuilder.Entity<Game>().ToTable("Game");
        modelBuilder.Entity<Player>().ToTable("Player");
        modelBuilder.Entity<Move>().ToTable("Move");
        modelBuilder.Entity<Country>().ToTable("Country");
    }

}

public record Session
{
    public string AdminId { get; init; }
    public string SessionId { get; init; }
}

public record Game
{
    public string GameId { get; init; }
    public int CurrentTurn { get; init; }
    public List<int> PlayedTurns { get; init; }

    public Session Session { get; init; }
    public string SessionId { get; init; }
}

public record Player
{
    public string PlayerId { get; init; }
    public Session Session { get; init; }
    public string SessionId { get; init; }
}


public record Move
{
    public string MoveId { get; init; }
    public string Content { get; init; }

    public int Turn { get; init; }

    public Session session { get; init; }
    public string SessionId { get; init; }

}

public record Country
{
    public string CountryId {get; init;}
    public string CountryName {get; init;}

    public Player player {get; init;}
    public string PlayerId {get; init;}
}

