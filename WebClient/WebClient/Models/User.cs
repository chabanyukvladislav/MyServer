using System.ComponentModel.DataAnnotations;

namespace WebClient.Models
{
    public class User
    {
        [Key]
        public string UserId { get; set; }
        public string Name { get; set; }
        public string Nickname { get; set; }
        public string Picture { get; set; }
    }
}
