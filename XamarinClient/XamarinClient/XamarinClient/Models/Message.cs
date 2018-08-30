using System;

namespace XamarinClient.Models
{
    class Message
    {
        public Guid Id { get; set; }
        public string Sms { get; set; }
        public string From { get; set; }
        public string To { get; set; }
    }
}
