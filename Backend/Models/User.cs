using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace Server.Models
{
    public class User : IdentityUser
    {
        public virtual List<Task> Tasks { get; set; } = new();
        public virtual List<Chat> Chats { get; set; } = new();
        public virtual List<ChatUsers> ChatUsers { get; set; } = new();

        public virtual UserProfile UserProfile { get; set; }
    }
}
