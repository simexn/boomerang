using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Backend.Models;
using Microsoft.AspNetCore.Identity;
using System.Reflection.Emit;

namespace Backend.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole<int>, int>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }
   

        public DbSet<Chat> Chats {  get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
        public DbSet<ChatUser> ChatUsers { get; set; }
        public DbSet<ChatAdmin> ChatAdmins { get; set; }
        public DbSet<ChatEvent> ChatEvents { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {

            base.OnModelCreating(builder);
            builder.Entity<ChatUser>()
                .HasKey(x => new { x.ChatId, x.UserId });

            builder.Entity<ChatAdmin>()
                .HasKey(ca => new { ca.ChatId, ca.UserId });
        }

    }
}
