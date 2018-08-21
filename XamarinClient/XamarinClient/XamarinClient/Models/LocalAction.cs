using XamarinClient.Enum;

namespace XamarinClient.Models
{
    class LocalAction
    {
        public int Id { get; set; }
        public TypeOfActions Type { get; set; }
        public People People { get; set; }
    }
}
