using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Backend.Models
{
    public class ApplicationUser : IdentityUser<int>
    {
        [Key]
        public override int Id { get; set; }
        [Required]
        public override string UserName { get; set; }
        public string? ProfilePictureUrl { get; set; }
        [Required]
        public DateTime AccountCreatedDate { get; set; }
        [Required]
        public string Status { get; set; }
        public string? Bio { get; set; }
        [Required]
        public bool IsAdmin { get; set; }



    }
}
