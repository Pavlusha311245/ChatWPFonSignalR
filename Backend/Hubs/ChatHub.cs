using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Server.Hubs
{

    public class ChatHub : Hub
    {
        byte[] file;

        public async Task Send(string username, string message, byte[] file)
        {
            this.file = file;
            await Clients.All.SendAsync("Receive", username, message);
        }
    }
}
