using System;
using System.Collections.Generic;

namespace Client.Models
{
    public class Task
    {
        public Guid Id { get; set; }
        public string Remark { get; set;}
        public DateTime DeadLine { get; set; }
        public bool Done { get; set; }
    }
}
