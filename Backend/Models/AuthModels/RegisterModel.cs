using System;
using System.ComponentModel.DataAnnotations;

namespace Server.Models
{
    public class RegisterModel
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress]
        public string Email { get; set; }

        [Required(ErrorMessage = "Phone is required")]
        [Phone]
        public string Phone { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [StringLength(50, ErrorMessage = "Incorrect password (min 8 characters, digits and letters with lower and upper case)", MinimumLength = 8)]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required(ErrorMessage = "Name is required")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Surname is required")]
        public string Surname { get; set; }

        [Required(ErrorMessage = "Patronymic is required")]
        public string Patronymic { get; set; }

        [Required(ErrorMessage = "Age is required")]
        public DateTime Birthday { get; set; }
    }
}
