using System;
using System.ComponentModel.DataAnnotations;

namespace PeopleApp.Models
{
    public class People
    {
        public Guid Id { get; set; }
        [Required]
        public string Name { get; set; }
        public string Surname { get; set; }
        [Required]
        public string Phone { get; set; }
        [Required]
        public virtual User User { get; set; }
    }
}