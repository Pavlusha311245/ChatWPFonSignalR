using Client.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Server.Models
{
    public class SenderMessage
    {
        public Guid ChatID { get; set; }
        public string MessageText { get; set; }
        public Task Task { get; set; }
        public List<Document> Documents { get; set; }
    }
}
