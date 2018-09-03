using System;

namespace XamarinClient.Models
{
    [Serializable]
    public class Messager
    {
        public Guid Id { get; set; }
        public string Sms { get; set; }
        public string FromId { get; set; }
        public string FromNick { get; set; }
        public string ToId { get; set; }
        public DateTime Date { get; set; }
    }
}
