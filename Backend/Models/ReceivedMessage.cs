using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Server.Models
{
    public class Doc
    {
        public string FileName { get; set; }
        public string Extension { get; set; }
        public string Hash { get; set; }
        public byte[] Content { get; set; }
    }

    public class ReceivedTask
    {
        public string Remark { get; set; }
        public DateTime DeadLine { get; set; }
        public bool Done { get; set; }
        public string SavePath { get; set; }
    }

    [NotMapped]
    public class ReceivedMessage
    {
        public Guid ChatID { get; set; }
        public string MessageText { get; set; }
        public ReceivedTask Task { get; set; }
        public List<Doc> Documents { get; set; }
    }
}
