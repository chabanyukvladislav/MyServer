using System;

namespace XamarinClient.Models
{
	public class User
	{
        public Guid Id { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public bool IsAdmin { get; set; }
	}
}