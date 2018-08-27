using System;
using XamarinClient.Enum;

namespace XamarinClient.Models
{
    public class LocalAction
    {
        public Guid Id { get; set; }
        public TypeOfActions Type { get; set; }
        public virtual People People { get; set; }
    }
}
