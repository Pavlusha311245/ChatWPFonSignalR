using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Server.Data;
using System.Text;
using System.Threading.Tasks;

namespace Server.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        public Models.Message Message { get; set; }

        public override async Task OnConnectedAsync()
        {
            await Clients.All.SendAsync("Notify", $"Подключён {Context.UserIdentifier}");
        }

        public async Task Send(Models.Message message)
        {
            if (message.Task != null)
            {

            }

            await Clients.All.SendAsync("Receive", Context.UserIdentifier, message.MessageText);
        }

        [Authorize(Roles = "Administration")]
        public async Task AddToChat(string connectionID, string groupName)
        {
            await Groups.AddToGroupAsync(connectionID, groupName);
            await Clients.Group(groupName).SendAsync("Receive", "Added new user");
        }
    }
}
