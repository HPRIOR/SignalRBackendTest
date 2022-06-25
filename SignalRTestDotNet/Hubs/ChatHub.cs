using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.SignalR;
using SignalRTestDotNet.GameContextNs;
using SignalRTestDotNet.DOAs;
using SignalRTestDotNet.Extensions;

namespace SignalRTestDotNet.Hubs;

public class ChatHub : Hub
{
    private readonly GameContext _gameContext;

    // will get from config in the real thing
    private const string _clientUrl = "http://localhost:8080/";

    public ChatHub(GameContextNs.GameContext gameContext)
    {
        _gameContext = gameContext;
    }

    public async Task NewMessage(long username, string message)
    {
        Console.WriteLine("adding to db");
        await Clients.All.SendAsync("messageReceived", username, message);
    }

    public async Task AddPlayers(List<string> countries, string sessionId, string adminId)
    {
        // should accessed through a repository interface in real thing
        // check if session and admin exist and are valid 

        var session = await _gameContext.FindAsync<Session>(sessionId);

        if (session is null || session.AdminId != adminId)
        {
            // Return some error back to client
            return;
        }

        // get player DAO
        var playerDaOs = countries
            .Select(country =>
            {
                var playerId = Guid.NewGuid().ToString();

                return new PlayerDAO
                {
                    Url = _clientUrl + "?session=" + sessionId + "&player=" + playerId,
                    Country = country,
                    SessionId = sessionId,
                    PlayerId = playerId
                };
            }).ToList(); // needs to list otherwise will be lazily evaluated and GUID diff each time!
        //
        // save player information in db
        var players = playerDaOs.Select(playerDao => playerDao.AsPlayer());
        _gameContext.AddRange(players);
        await _gameContext.SaveChangesAsync();


        // send back DAO:
        await Clients.Caller.SendAsync("playersAdded", playerDaOs);
    }

    public async Task VerifyPlayerSession(string sessionId, string playerId)
    {
        var session = await _gameContext.FindAsync<Session>(sessionId);
        var player = await _gameContext.FindAsync<Player>(playerId);
        Console.WriteLine(session);
        Console.WriteLine(player);
        if (session is null || player is null || session.SessionId != player.PlayerId)
        {
            await Clients.Caller.SendAsync("verifiedPlayerSession", false);
            return;
        }

        await Clients.Caller.SendAsync("verifiedPlayerSession", true);
    }
}