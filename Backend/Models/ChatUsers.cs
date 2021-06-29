using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Server.Models
{
    public enum ChatRoles
    {
        User,
        Moderator,
        Admin,
        Owner
    }

    public class ChatUsers
    {
        public Guid ChatID { get; set; }
        public virtual Chat Chat { get; set; }

        public string UserID { get; set; }
        public virtual User User { get; set; }

        public ChatRoles Role { get; set; }
    }
}
