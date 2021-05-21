using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Text;
using System.Threading.Tasks;

namespace Server.Hubs
{
        public class ChatHub : Hub
    {
        byte[] file;

        [Authorize]
        public async Task Send(string message, byte[] file)
        {
            this.file = file;
            //var ext = Encoding.UTF8.GetString(file[0..5]);

            var user = Context.User;
            var username = Context.User.Identity.Name;

            await Clients.All.SendAsync("Receive", username, message);
        }
                
        public override async Task OnConnectedAsync()
        {
            await Clients.All.SendAsync("Notify", $"Подключён {Context.UserIdentifier}");
        }
    }
}
