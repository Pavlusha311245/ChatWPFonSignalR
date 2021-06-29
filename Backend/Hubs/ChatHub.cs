using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Server.Data;
using Server.ViewModel;
using System;
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
        public RoleManager<IdentityRole> roleManager;
        public ServerContext db;
        public IMapper mapper;

        public ChatHub(IWebHostEnvironment hostEnvironment,
            IConfiguration configuration,
            UserManager<Models.User> userManager,
            RoleManager<IdentityRole> roleManager,
            ServerContext db,
            IMapper mapper)
        {
            appEnv = hostEnvironment;
            this.configuration = configuration;
            this.userManager = userManager;
            this.roleManager = roleManager;
            this.db = db;
            this.mapper = mapper;
        }

        public override async Task OnConnectedAsync()
        {

            var user = await userManager.FindByEmailAsync(Context.UserIdentifier);
            user.Chats.AddRange(db.Chats.Where(c => c.Type == Models.ChatTypes.Single).Where(c => !c.Users.Contains(user)).ToList());
            var chats = mapper.Map<List<Models.Chat>, List<ChatViewModel>>(user.Chats
                .Where(c => c.ChatUsers.Where(c => c.UserID == user.Id).First().Role != Models.ChatRoles.Owner).ToList());
            var tasks = mapper.Map<List<Models.Task>, List<TaskViewModel>>(user.Tasks);
            foreach (var chat in chats)
            {
                if (chat.Type == Models.ChatTypes.Group)
                {
                    await Groups.AddToGroupAsync(Context.ConnectionId, chat.Name);
                }
            }

            await Clients.Caller.SendAsync("Connected",
                tasks,
                chats);

            if (Context.User.IsInRole(Models.UserRoles.Administration))
                await Clients.Caller.SendAsync("AdminPrivileges");
        }

        public async Task SendMessage(Guid chatID, string message)
        {
            var chat = db.Chats.Find(chatID);
            var receiver = chat.Users.FirstOrDefault().Email;
            var user = await userManager.FindByEmailAsync(Context.UserIdentifier);
            if (chat.Type == Models.ChatTypes.Single)
                await Clients.Users(receiver, user.Email).SendAsync("Receive", user.Email, message);

            if (chat.Type == Models.ChatTypes.Group)
            {
                await Clients.Group(chat.Name).SendAsync("Receive", user.Email, message);
            }

            db.Messages.Add(new Models.Message
            {
                ChatID = chat.Id,
                MessageText = message,
                SenderID = user.Id
            });

            await db.SaveChangesAsync();
        }

        private static async Task SaveFile(List<Models.Document> documents)
        {
            foreach (var document in documents)
                if (!File.Exists(document.SavePath))
                    using (FileStream stream = new(appEnv.ContentRootPath + document.SavePath, FileMode.Create))
                        await stream.WriteAsync(document.Content);
        }

        [Authorize(Roles = "Administration")]
        public async Task AddToChat(string connectionID, string groupName)
        {
            await Groups.AddToGroupAsync(connectionID, groupName);
            await Clients.Group(groupName).SendAsync("Receive", "Added new user");
        }
    }
}
