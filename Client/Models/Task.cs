using System;
using System.Collections.Generic;

namespace Client.Models
{
    public class Task
    {
        public int Id { get; set; }
        public string Remark { get; set; }
        public DateTime DeadLine { get; set; }
        public List<Document> Documents { get; set; } = new();
    }
}
