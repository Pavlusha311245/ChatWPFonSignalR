using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Server.Models
{
    public class Message
    {
        public Guid Id { get; set; }
        public string MessageText { get; set; }

        public string SenderID { get; set; }
        public virtual User Sender { get; set; }
        public Guid ChatID { get; set; }
        public virtual Chat Chat { get; set; }

        [Timestamp]
        public byte[] Timestamp { get; set; }
    }
}
