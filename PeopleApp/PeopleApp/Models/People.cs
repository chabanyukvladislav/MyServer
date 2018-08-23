using System;
using System.ComponentModel.DataAnnotations;

namespace PeopleApp.Models
{
    public class People
    {
        public Guid Id { get; set; }
        [Required(ErrorMessage = "Must be")]
        public string Name { get; set; }
        public string Surname { get; set; }
        [Required(ErrorMessage = "Must be")]
        public string Phone { get; set; }
        public virtual User User { get; set; }
    }
}