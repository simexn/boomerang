using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Backend.Models
{
    public class ChatUser
    {
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; }
        
        public int ChatId { get; set; }

        [ForeignKey("ChatId")]
        [JsonIgnore]
        public Chat Chat { get; set; }

       
    }
}
