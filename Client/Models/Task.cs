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
        public List<Document> Documents { get; set; } = new();
    }
}
