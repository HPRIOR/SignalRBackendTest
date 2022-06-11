using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.SignalR;
using SignalRTestDotNet.GameContextNs;
using Newtonsoft.Json;

namespace SignalRTestDotNet.Hubs;

public record PlayerDAO
{
    [JsonProperty("country")]
    public string Country { get; init; }

    [JsonProperty("url")]
    public string Url { get; init; }

    [JsonProperty("sessionId")]
    public string SessionId { get; init; }

    [JsonProperty("playerId")]
    public string PlayerId { get; init; }
}


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

        System.Console.WriteLine("adding to db");
        await Clients.All.SendAsync("messageReceived", username, message);
    }

    public async Task AddPlayers(List<string> countries, string sessionId, string AdminId)
    {
        // should accces through a repository interface in real thing
        // check if session and admin exist and are valid 
        var session = await _gameContext.FindAsync(typeof(Session), "sessionId");

        // get player DAO
        

        // save player information in db



        // send back DAO:
        var players = countries.Select(country =>
        {
            var playerId = Guid.NewGuid().ToString();

            return new PlayerDAO
            {
                Url = _clientUrl + "?session=" + sessionId + "&player=" + playerId,
                Country = country,
                SessionId = sessionId,
                PlayerId = playerId
            };
        });
        await Clients.Caller.SendAsync("playersAdded", players);
    }


}
