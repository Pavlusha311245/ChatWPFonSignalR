using System;

namespace Server.Models
{
    public class PersonalData
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Patronymic { get; set; }
        public DateTime Birthday { get; set; }

        public string UserID { get; set; }
        public User User { get; set; }
    }
}
