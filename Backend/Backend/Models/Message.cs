using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models
{
    public class Message
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public int FromUserId { get; set; }
        [ForeignKey("FromUserId")]
        public virtual ApplicationUser FromUser { get; set; }

        [Required]
        public string Text { get; set; }
        [Required]
        public DateTime Timestamp { get; set; }

        public bool IsEdited { get; set; }
        public bool IsDeleted { get; set; }
        public string? FileUrl { get; set; }

        [Required]
        public int ChatId { get; set; }
        [ForeignKey("ChatId")]
        public Chat Chat { get; set; }
    }
}
