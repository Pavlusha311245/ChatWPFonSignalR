using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Server.Models
{
    public enum ChatTypes
    {
        Single,
        Group
    }

    public class Chat
    {
        public Guid Id { get; set; }
        public byte[] Image { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public ChatTypes Type { get; set; }

        public virtual List<Message> Messages { get; set; } = new();
        public virtual List<User> Users { get; set; } = new();
        public virtual List<ChatUsers> ChatUsers { get; set; } = new();

        [Timestamp]
        public byte[] TimeStamp { get; set; }
    }
}
