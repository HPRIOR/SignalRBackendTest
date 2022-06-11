using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using SignalRTestDotNet.GameContextNs;

namespace SignalRTestDotNet.Hubs
{
    public class ChatHub : Hub
    {
        private readonly GameContext _gameContext;

        public ChatHub(GameContextNs.GameContext gameContext)
        {
            _gameContext = gameContext;
        }


        public async Task NewMessage(long username, string message)
        {

            System.Console.WriteLine("adding to db");
            await Clients.All.SendAsync("messageReceived", username, message);
        }

        public async Task AddPlayers(List<string> players, string sessionId, string AdminId)
        {
            var playerUrls = new List<string>();

            await Clients.Caller.SendAsync("playersAdded", playerUrls);
        }


    }
}
