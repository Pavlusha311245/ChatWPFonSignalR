using Server.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Server.ViewModel
{
    public class TaskViewModel
    {
        public Guid Id { get; set; }
        public string Remark { get; set; }
        public DateTime DeadLine { get; set; }
        public bool Done { get; set; }
        public List<Document> Documents { get; set; } = new();
    }
}
