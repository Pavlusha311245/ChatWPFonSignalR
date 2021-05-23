using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace Server.Models
{
    public class User : IdentityUser
    {
        public List<Task> Tasks { get; set; } = new();

        public PersonalData PersonalData { get; set; }
    }
}
