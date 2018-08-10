using System;

namespace PeopleApp.Models
{
	public class User
	{
        public Guid Id { get; set; }
        public string Login { get; set; }
        public string Passsword { get; set; }
        public bool IsAdmin { get; set; }
	}
}