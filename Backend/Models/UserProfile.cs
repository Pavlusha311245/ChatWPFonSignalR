using System;

namespace Server.Models
{
    public class UserProfile
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Patronymic { get; set; }
        public DateTime Birthday { get; set; }

        public string UserID { get; set; }
        public virtual User User { get; set; }
    }
}
