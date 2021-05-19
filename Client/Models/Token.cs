using System;

namespace Client.Models
{
    class Token
    {
        public int Id { get; set; }
        public string Value { get; set; }
        public DateTime ExpireDate { get; set; }

        //Foreign key
        public string UserId { get; set; }
        public User User { get; set; }
    }
}
