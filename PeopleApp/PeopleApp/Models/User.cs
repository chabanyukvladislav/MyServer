using System;
using System.ComponentModel.DataAnnotations;

namespace PeopleApp.Models
{
    public class User
    {
        public Guid Id { get; set; }
        [Required(ErrorMessage = "Must be")]
        public string Login { get; set; }
        [Required(ErrorMessage = "Must be")]
        public string Password { get; set; }
        public bool IsAdmin { get; set; }
    }
}