using System;

namespace PeopleApp.Models
{
    public class Message
    {
        public Guid Id { get; set; }
        public string Sms { get; set; }
        public virtual User From { get; set; }
        public virtual User To { get; set; }
    }
}
