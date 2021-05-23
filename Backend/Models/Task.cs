using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Server.Models
{
    public class Task
    {
        public int Id { get; set; }
        public string Remark { get; set; }
        public DateTime DeadLine { get; set; }
        public bool Done { get; set; }
        public List<Document> Documents { get; set; } = new();
        public List<User> Users { get; set; } = new();

        public int MessageId { get; set; }
        public Message Message { get; set; }

        [Timestamp]
        public byte[] Timestamp { get; set; }
    }
}
