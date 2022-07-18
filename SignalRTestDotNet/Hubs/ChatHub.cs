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
    public async Task AbortSessionWith(string reason)
    {
        await Clients.Caller.SendAsync("couldNotConnect", reason);
        Context.Abort();
    }

    public async Task VerifyPlayerSession(string sessionId, string playerId, string userId)
    {
        if (playerId != userId)
        {
            await AbortSessionWith("User id does not match player id");
        }

        var session = await _gameContext.FindAsync<Session>(sessionId);
        var player = await _gameContext.FindAsync<Player>(playerId);

        if (session is null || player is null || session.SessionId != player.SessionId)
        {
            await AbortSessionWith("No matching session or player found");
        }

        Console.WriteLine("player verified");
        await Clients.Caller.SendAsync("verifiedSession", "Player");
    }

    private async Task VerifyAdminSession(String sessionId, string adminId, string userId)
    {
        if (adminId != userId)
        {
            await AbortSessionWith("No matching user id for admin id");
        }

        var session = await _gameContext.FindAsync<Session>(sessionId);
        if (session is null || session.AdminId != adminId)
        {
            await AbortSessionWith("No matching session for admin");
        }

        await Clients.Caller.SendAsync("verifiedSession", "Admin");
    }

    public override async Task OnConnectedAsync()
    {
        var playerQuery = Context.GetHttpContext()!.Request.Query["player"];
        var sessionQuery = Context.GetHttpContext()!.Request.Query["session"];
        var userId = Context.UserIdentifier;

        var isPlayerSession =  !string.IsNullOrEmpty(sessionQuery) && !string.IsNullOrEmpty(sessionQuery) ;
        if (isPlayerSession)
        {
            if (userId != playerQuery) return;
            await VerifyPlayerSession(sessionQuery, playerQuery, userId);
        }
        else // verify admin
        {
            var adminCookie = Context.GetHttpContext()?.Request.Cookies["AdminId"];
            var sessionCookie = Context.GetHttpContext()?.Request.Cookies["SessionId"];
            await VerifyAdminSession(sessionCookie, adminCookie, userId);
        }

        await base.OnConnectedAsync();
    }


    private readonly GameContext _gameContext;

    // will get from config in the real thing
    private const string ClientUrl = "http://localhost:8080/";

    public ChatHub(GameContext gameContext)
    {
        _gameContext = gameContext;
    }

    public async Task NewMessage(long username, string message)
    {
        Console.WriteLine("adding to db");
        await Clients.All.SendAsync("messageReceived", username, message);
    }

    public async Task SendMessageToPlayers(string sessionId, string message)
    {
        _gameContext
            .Players
            .Where(player => player.SessionId == sessionId)
            .ToList()
            .ForEach(async player => { await Clients.User(player.PlayerId).SendAsync("SendMessage", message); });

        await Clients.Group(sessionId).SendAsync("SendMessage", message);
    }

    public async Task SendMessageToUser(string playerId, string message)
    {
        await Clients.User(playerId).SendAsync("SendMessage", message);
    }

    public async Task SendMessageToAdmin(string sessionId, string message)
    {
        var session = await _gameContext.FindAsync<Session>(sessionId);
        if (session is not null)
        {
            await Clients.User(session.AdminId).SendAsync("SendMessage", message);
        }
    }

    public async Task Debug()
    {
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
                    Url = ClientUrl + "?session=" + sessionId + "&player=" + playerId,
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
}
