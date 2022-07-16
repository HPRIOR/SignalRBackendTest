using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using SignalRTestDotNet.GameContextNs;
using SignalRTestDotNet.DOAs;
using SignalRTestDotNet.Extensions;
using Microsoft.AspNetCore.Http.Extensions;

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
            return;
        }

        var session = await _gameContext.FindAsync<Session>(sessionId);
        var player = await _gameContext.FindAsync<Player>(playerId);

        if (session is null || player is null || session.SessionId != player.SessionId)
        {
            await AbortSessionWith("No matching session or player found");
            return;
        }

        Console.WriteLine("player verified");
        await Clients.Caller.SendAsync("verifiedSession", "player");
    }

    private async Task VerifyAdminSession(String sessionId, string adminId, string userId)
    {
        if (adminId != userId)
        {
            await AbortSessionWith("No matching user id for admin id");
            return;
        }
        
        var session = await _gameContext.FindAsync<Session>(sessionId);
        if (session is null || session.AdminId != adminId)
        {
            await AbortSessionWith("No matching session for admin");
            return;
        }

        await Clients.Caller.SendAsync("verifiedSession", "Admin");
    }

    public override async Task OnConnectedAsync()
    {
        var playerQuery = Context.GetHttpContext()!.Request.Query["player"];
        var sessionQuery = Context.GetHttpContext()!.Request.Query["session"];
        var adminCookie = Context.GetHttpContext()?.Request.Cookies["AdminId"];
        var sessionCookie = Context.GetHttpContext()?.Request.Cookies["SessionId"];
        var userId = Context.UserIdentifier;

        var isPlayerSession = (!String.IsNullOrEmpty(sessionQuery) && !String.IsNullOrEmpty(sessionQuery));
        if (isPlayerSession)
        {
            if (userId != playerQuery) return;
            await VerifyPlayerSession(sessionQuery, playerQuery, userId);
        }
        else
        {
            await VerifyAdminSession(sessionCookie, adminCookie, userId);
        }

        await base.OnConnectedAsync();
    }


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
        var _1 = Context.GetHttpContext().Request.Cookies["AdminId"];
        var _2 = Context.GetHttpContext().Request.Cookies["SessionId"];
        var _3 = Context.GetHttpContext().Request.Cookies["Anything"];
        var _ = Context.GetHttpContext().Response.GetTypedHeaders().SetCookie.First(cookie => cookie.Name == "AdminId");

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
}
