using System;

namespace PeopleApp.Models
{
	public class People
	{
        public Guid Id { get; set; }
        public string Name { get; set; }
	    public string Surname { get; set; }
	    public string Phone { get; set; }
    }
}