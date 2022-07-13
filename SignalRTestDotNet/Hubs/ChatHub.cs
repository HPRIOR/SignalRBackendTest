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

    /*
     * Groups will be associated with sessionIds.  Only admins will be able to send messages to the group
     * by using their admin id.
     */
    public async Task SendMessageToGroup(string adminId, string message)
    {

    }

    public async Task SendMessageToAdmin(string sessionId, string message)
    {

    }

    public async Task Debug()
    {
        // Console.WriteLine("Debug:");
        // Console.WriteLine($"user id: {Context.UserIdentifier}");
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
            }).ToList();

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

        if (session is null || player is null || session.SessionId != player.SessionId)
        {
            await Clients.Caller.SendAsync("verifiedPlayerSession", false);
            return;
        }
        Console.WriteLine("player verified");
        await Clients.Caller.SendAsync("verifiedPlayerSession", true);
    }
}
