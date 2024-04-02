using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models
{
    public class Friendship
    {
        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; }

        public int FriendId { get; set; }
        [ForeignKey("FriendId")]
        public ApplicationUser Friend { get; set; }

        public string Status { get; set; }
        public DateTime RequestSentDate { get; set; }
        public DateTime RequestRespondedDate { get; set; }

        public int? ChatId { get; set; } // Nullable ChatId
        [ForeignKey("ChatId")]
        public Chat Chat { get; set; } // Navigation property for the Chat
    }
}