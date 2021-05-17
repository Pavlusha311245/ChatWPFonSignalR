using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;

namespace Server.Hubs
{

    public class ChatHub : Hub
    {
        public async Task Send(string username, string message)
        {
            await Clients.All.SendAsync("Receive", username, message);
        }
    }
}
