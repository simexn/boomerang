using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Backend.Models
{
    public class Chat
    {

        public Chat()
        {
            Messages = new List<Message>();
            Users = new List<ChatUser>();
            Admins = new List<ChatAdmin>();
        }
        [Key]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public bool IsGroup { get; set; } = true;

        [Required]
        public string InviteCode { get; set; }

        public int? CreatorId { get; set; }
        [ForeignKey("CreatorId")]
        [JsonIgnore]
        public virtual ApplicationUser Creator { get; set; }
        [JsonIgnore]
        public virtual ICollection<Message> Messages { get; set; }
        [JsonIgnore]
        public virtual ICollection<ChatUser> Users { get; set; }
        [JsonIgnore]
        public virtual ICollection<ChatAdmin> Admins { get; set; }
    }
}
