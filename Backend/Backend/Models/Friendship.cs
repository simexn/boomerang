using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models
{
    public class Friendship
    {
        [Required]
        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; }

        [Required]
        public int FriendId { get; set; }
        [ForeignKey("FriendId")]
        public ApplicationUser Friend { get; set; }

        [Required]
        public string Status { get; set; }
        public DateTime RequestSentDate { get; set; }
        public DateTime RequestRespondedDate { get; set; }

        public int? ChatId { get; set; }
        [ForeignKey("ChatId")]
        public Chat Chat { get; set; } 
    }
}