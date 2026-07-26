using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.Models
{
    public enum ChatRoles
    {
        User,
        Moderator,
        Admin
    }

    public enum ChatTypes
    {
        Single,
        Group
    }

    public class Chat
    {
        public Guid Id { get; set; }
        public byte[] Image { get; set; }
        public string Name { get; set; }
        public ChatTypes Type { get; set; }
        public ChatRoles Role { get; set; }
    }
}
