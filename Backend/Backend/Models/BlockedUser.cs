using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models
{

    public class BlockedUser
    {
        public int BlockedById { get; set; }
        [ForeignKey("BlockedById")]
        public ApplicationUser BlockedBy { get; set; }
        public int BlockedId { get; set; }
        [ForeignKey("BlockedId")]
        public ApplicationUser Blocked { get; set; }

        public string blockedOn { get; set;}
    }
}
