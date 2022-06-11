using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace SignalRTestDotNet.Hubs
{
    public class ChatHub : Hub
    {

        public ChatHub()
        {
        }


        public async Task NewMessage(long username, string message)
        {
            await Clients.All.SendAsync("messageReceived", username, message);
        }

        public async Task AddPlayers(List<string> players, string sessionId, string AdminId)
        {
            var playerUrls = new List<string>();
            // 
            await Clients.Caller.SendAsync("playersAdded", playerUrls);
        }


    }
}
