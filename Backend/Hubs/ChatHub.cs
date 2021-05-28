using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Server.Data;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Server.Hubs
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class ChatHub : Hub
    {
        public Models.Message Message { get; set; }
        public static IWebHostEnvironment appEnv;
        public IConfiguration configuration;
        public UserManager<Models.User> userManager;

        public ChatHub(IWebHostEnvironment hostEnvironment, IConfiguration configuration, UserManager<Models.User> userManager)
        {
            appEnv = hostEnvironment;
            this.configuration = configuration;
            this.userManager = userManager;
        }

        public override async Task OnConnectedAsync()
        {
            await Clients.All.SendAsync("Connected", $"Соединение с сервером установлено");
        }

        private static async void SaveFile(List<Models.Document> documents)
        {
            foreach (var document in documents)
                if (!File.Exists(document.SavePath))
                    using (FileStream stream = new(appEnv.ContentRootPath + document.SavePath, FileMode.Create))
                        await stream.WriteAsync(document.Content);
        }

        public async Task SendToUser(Models.Message message, string user)
        {
            if (message.Task != null)
                SaveFile(message.Task.Documents);

            await Clients.Client(user).SendAsync("Receive", Context.User.Identity.Name, message.MessageText);
        }

        public async Task SendToEveryone(Models.Message message)
        {
            DbContextOptionsBuilder<ServerContext> optionsBuilder = new();
            var options = optionsBuilder
                .UseSqlServer(configuration.GetConnectionString("ServerContext"))
                .Options;

            using (ServerContext db = new(options))
            {
                var user = await userManager.FindByEmailAsync(Context.UserIdentifier);

                if (message.Task != null)
                {
                    message.Task.UserId = user.Id;

                    foreach (var document in message.Task.Documents)
                        document.SavePath = "/Resources/Files/" + document.Hash + document.Extension;

                    SaveFile(message.Task.Documents);
                }
                
                message.UserID = user.Id;                

                await db.Messages.AddAsync(message);
                await db.SaveChangesAsync();

                await Clients.All.SendAsync("Receive", Context.User.Identity.Name, message.MessageText);
            }
        }

        [Authorize(Roles = "Administration")]
        public async Task AddToChat(string connectionID, string groupName)
        {
            await Groups.AddToGroupAsync(connectionID, groupName);
            await Clients.Group(groupName).SendAsync("Receive", "Added new user");
        }
    }
}
