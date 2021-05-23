using System.ComponentModel.DataAnnotations;

namespace Server.Models
{
    public class Document
    {
        public int Id { get; set; }
        public string SavePath { get; set; }
        public string Extension { get; set; }
        public string Hash { get; set; }
        public byte[] Content { get; set; }

        [Timestamp]
        public byte[] TimeStamp { get; set; }
    }
}
