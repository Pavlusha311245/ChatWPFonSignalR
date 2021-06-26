using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Server.Models
{
    public class Document
    {
        public Guid Id { get; set; }
        public string SavePath { get; set; }
        public string Extension { get; set; }
        public string Hash { get; set; }
        public byte[] Content { get; set; }

        public virtual List<Task> Tasks { get; set; } = new();

        [Timestamp]
        public byte[] TimeStamp { get; set; }
    }
}
