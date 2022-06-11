using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace SignalRTestDotNet.GameContext;


public class GameContext : DbContext
{
    public DbSet<Session> Sessions { get; set; }
    public DbSet<Game> Games { get; set; }
    public DbSet<Player> Players { get; set; }
    public DbSet<Move> Moves { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseNpgsql("Host=my_host;Database=my_db;Password=my_pw");

}

public record Session
{
    string AdminId { get; init; }
    string SessionId { get; init; }
}

public record Game
{
    int CurrentTurn { get; init; }
    List<int> PlayedTurns { get; init; }

    Session Session { get; init; }
    string SessionId { get; init; }
}

public record Player
{
    string PlayerId { get; init; }

    Session Session { get; init; }
    string SessionId { get; init; }
}


public record Move
{
    string Content { get; init; }

    int Turn { get; init; }

    Session session { get; init; }
    string SessionId { get; init; }


}

