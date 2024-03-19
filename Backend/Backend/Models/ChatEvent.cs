using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models
{
    public class ChatEvent
    {
               
        public int Id { get; set; }
        
        public DateTime Timestamp { get; set; }
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
        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; }
        public int ChatId { get; set; }
        [ForeignKey("ChatId")]
        public Chat Chat { get; set; }
        
    }
}
