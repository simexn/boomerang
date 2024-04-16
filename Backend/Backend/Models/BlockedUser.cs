using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models
{

    public class BlockedUser
    {
        [Required]
        public int BlockedById { get; set; }
        [ForeignKey("BlockedById")]
        public ApplicationUser BlockedBy { get; set; }
        [Required]
        public int BlockedId { get; set; }
        [ForeignKey("BlockedId")]
        public ApplicationUser Blocked { get; set; }
        [Required]

        public string BlockedOn { get; set;}
    }
}
