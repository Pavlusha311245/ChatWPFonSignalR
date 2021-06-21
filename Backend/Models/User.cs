using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace Server.Models
{
    public class User : IdentityUser
    {
        public List<Message> Messages { get; set; } = new();
        public List<Task> Tasks { get; set; } = new();
        public List<GroupChat> GroupChats { get; set; } = new();

        public PersonalData PersonalData { get; set; }
    }
}
