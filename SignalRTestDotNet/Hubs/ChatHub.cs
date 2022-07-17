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

    public async Task<bool> VerifyPlayerSession(string sessionId, string playerId, string userId)
    {
        if (playerId != userId)
        {
            await AbortSessionWith("User id does not match player id");
            return false;
        }

        var session = await _gameContext.FindAsync<Session>(sessionId);
        var player = await _gameContext.FindAsync<Player>(playerId);

        if (session is null || player is null || session.SessionId != player.SessionId)
        {
            await AbortSessionWith("No matching session or player found");
            return false;
        }

        Console.WriteLine("player verified");
        await Clients.Caller.SendAsync("verifiedSession", "Player");
        return true;
    }

    private async Task<bool> VerifyAdminSession(String sessionId, string adminId, string userId)
    {
        if (adminId != userId)
        {
            await AbortSessionWith("No matching user id for admin id");
            return false;
        }

        var session = await _gameContext.FindAsync<Session>(sessionId);
        if (session is null || session.AdminId != adminId)
        {
            await AbortSessionWith("No matching session for admin");
            return false;
        }

        await Clients.Caller.SendAsync("verifiedSession", "Admin");
        return true;
    }

    public override async Task OnConnectedAsync()
    {
        var playerQuery = Context.GetHttpContext()!.Request.Query["player"];
        var sessionQuery = Context.GetHttpContext()!.Request.Query["session"];
        var userId = Context.UserIdentifier;

        var isPlayerSession = (!String.IsNullOrEmpty(sessionQuery) && !String.IsNullOrEmpty(sessionQuery));
        if (isPlayerSession)
        {
            if (userId != playerQuery) return;
            var verified = await VerifyPlayerSession(sessionQuery, playerQuery, userId);
            if (verified)
            {
                Console.WriteLine("Adding player to group");
                await Groups.AddToGroupAsync(Context.UserIdentifier, sessionQuery);
            }
        }
        else // verify admin
        {
            var adminCookie = Context.GetHttpContext()?.Request.Cookies["AdminId"];
            var sessionCookie = Context.GetHttpContext()?.Request.Cookies["SessionId"];
            var verified = await VerifyAdminSession(sessionCookie, adminCookie, userId);
            if (verified){
                Console.WriteLine("Adding admin to group");
                await Groups.AddToGroupAsync(Context.UserIdentifier, sessionCookie);
            }
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
    public async Task SendMessageToGroup(string sessionId, string message)
    {
        await Clients.Group(sessionId).SendAsync("SendMessage", message);
    }

    public async Task SendMessageToUser(string playerId, string message)
    {
        await Clients.User(playerId).SendAsync("SendMessage", message);
    }

    public async Task SendMessageToAdmin(string sessionId, string message)
    {
        System.Console.WriteLine("message admin");
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
