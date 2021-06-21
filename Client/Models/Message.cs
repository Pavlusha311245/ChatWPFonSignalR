using Client.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Server.Models
{
    public class Message
    {
        public string MessageText { get; set; }
        public Task Task { get; set; }
    }
}
