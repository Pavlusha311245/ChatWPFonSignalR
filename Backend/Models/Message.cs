using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Server.Models
{
    public class Message
    {
        public int Id { get; set; }
        public string MessageText { get; set; }
        public Task Task { get; set; }
        
        public string UserID { get; set; }
        public User User { get; set; }

        [Timestamp]
        public byte[] Timestamp { get; set; }
    }
}
