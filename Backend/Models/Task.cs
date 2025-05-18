using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Server.Models
{
    public class Task
    {
        public Guid Id { get; set; }
        public string Remark { get; set; }
        public DateTime DeadLine { get; set; }
        public bool Done { get; set; }

        public Guid MessageId { get; set; }
        public virtual Message Message { get; set; }

        public virtual List<Document> Documents { get; set; } = new();
        public virtual List<User> Users { get; set; } = new();

        [Timestamp]
        public byte[] Timestamp { get; set; }
    }
}
