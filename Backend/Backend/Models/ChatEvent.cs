using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models
{
    public class ChatEvent
    {
               
        public int Id { get; set; }

        [Required]
        public DateTime Timestamp { get; set; }
        [Required]
        public EventType Event { get; set; }

        public enum EventType
        {
            UserJoined,
            UserLeft,
            UserKicked,
            UserBanned,
            UserPromoted,
            UserDemoted,
            OwnershipTransferred,
            UserUnbanned
        }

        // Navigation properties
        [Required]
        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; }
        [Required]
        public int ChatId { get; set; }
        [ForeignKey("ChatId")]
        public Chat Chat { get; set; }
        
    }
}
