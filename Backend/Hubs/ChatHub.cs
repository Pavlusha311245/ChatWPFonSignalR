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

        public async Task SendMessage(Models.ReceivedMessage receivedMessage)
        {
            var chat = db.Chats.Find(receivedMessage.ChatID);
            var receiver = chat.Users.FirstOrDefault();
            var user = await userManager.FindByEmailAsync(Context.UserIdentifier);

            if (receivedMessage.Documents != null)
            {
                await SaveFiles(receivedMessage.Documents);
            }

            var task = new Models.Task
            {
                DeadLine = receivedMessage.Task.DeadLine,
                Done = receivedMessage.Task.Done,
                Remark = receivedMessage.Task.Remark,
            };

            var createdTask = await db.Tasks.AddAsync(task);

            if (chat.Type == Models.ChatTypes.Single)
            {
                createdTask.Entity.Users.Add(receiver);
                await Clients.Users(receiver.Email, user.Email).SendAsync("Receive", user.Email, receivedMessage.MessageText);
            }

            if (chat.Type == Models.ChatTypes.Group)
            {
                createdTask.Entity.Users.AddRange(chat.Users);
                await Clients.Group(chat.Name).SendAsync("Receive", user.Email, receivedMessage.MessageText);
            }            

            var message = db.Messages.Add(new Models.Message
            {
                ChatID = chat.Id,
                MessageText = receivedMessage.MessageText,
                SenderID = user.Id
            });

            createdTask.Entity.MessageId = message.Entity.Id;

            await db.SaveChangesAsync();
        }

        private static async Task SaveFiles(List<Models.Doc> documents)
        {
            foreach (var document in documents)
                if (!File.Exists("/Resources/Files/" + document.Hash + document.Extension))
                    using (FileStream stream = new(appEnv.ContentRootPath + "/Resources/Files/" + document.Hash + document.Extension, FileMode.Create))
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
