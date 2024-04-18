using Backend.Migrations;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Backend.Models
{
    public class Chat
    {
        public Chat()
        {
            // Initialize collections to prevent NullReferenceException
            Messages = new List<Message>();
            Users = new List<ChatUser>();
            Admins = new List<ChatAdmin>();
            BannedChatUsers = new List<BannedChatUser>();
        }
        [Key]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public bool IsGroup { get; set; } = true;
        public string? InviteCode { get; set; }
        [Required]
        public int CreatorId { get; set; }
        [ForeignKey("CreatorId")]
        // Ignore these properties when serializing to JSON
        [JsonIgnore]
        public virtual ApplicationUser Creator { get; set; }
        [JsonIgnore]
        public virtual ICollection<Message> Messages { get; set; }
        [JsonIgnore]
        public virtual ICollection<ChatUser> Users { get; set; }
        [JsonIgnore]
        public virtual ICollection<ChatAdmin> Admins { get; set; }
        [JsonIgnore]
        public virtual ICollection<BannedChatUser> BannedChatUsers { get; set; }
    }
}

