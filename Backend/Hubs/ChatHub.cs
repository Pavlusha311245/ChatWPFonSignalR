using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Text;
using System.Threading.Tasks;

namespace Server.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        byte[] file;

        public override async Task OnConnectedAsync()
        {
            await Clients.All.SendAsync("Notify", $"Подключён {Context.UserIdentifier}");
        }

        public async Task Send(string message, byte[] file)
        {
            this.file = file;
            //var ext = Encoding.UTF8.GetString(file[0..5]);

            var user = Context.User.Identity.Name;
            var username = Context.UserIdentifier;

            await Clients.All.SendAsync("Receive", username, message);
        }

        [Authorize(Roles = "Administration")]
        public async Task AddToChat(string connectionID, string groupName)
        {
            await Groups.AddToGroupAsync(connectionID, groupName);
            await Clients.Group(groupName).SendAsync("Receive", "Added new user");
        }
    }
}
