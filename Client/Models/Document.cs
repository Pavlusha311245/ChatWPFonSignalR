using System.ComponentModel.DataAnnotations;

namespace Client.Models
{
    public class Document
    {
        public string FileName { get; set; }
        public string Extension { get; set; }
        public string Hash { get; set; }
        public byte[] Content { get; set; }
    }
}
