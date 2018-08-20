using XamarinClient.Enum;

namespace XamarinClient.Models
{
    public class Result
    {
        public bool IsSuccess { get; set; }
        public ErrorTypes Message { get; set; }
    }
}
