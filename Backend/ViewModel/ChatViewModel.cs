using Server.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Server.ViewModel
{
    public class ChatViewModel
    {
        public Guid Id { get; set; }
        public byte[] Image { get; set; }
        public string Name { get; set; }
        public ChatTypes Type { get; set; }
        public ChatRoles Role { get; set; }
    }
}
