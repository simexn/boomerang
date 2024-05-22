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


        public DbSet<Chat> Chats { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
        public DbSet<ChatUser> ChatUsers { get; set; }
        public DbSet<BannedChatUser> BannedChatUsers { get; set; }
        public DbSet<ChatAdmin> ChatAdmins { get; set; }
        public DbSet<ChatEvent> ChatEvents { get; set; }
        public DbSet<Friendship> Friendships { get; set; }
        public DbSet<BlockedUser> BlockedUsers { get; set; }


        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<ChatUser>()
                .HasKey(x => new { x.ChatId, x.UserId });

            builder.Entity<ChatUser>()
                .HasOne(cu => cu.Chat)
                .WithMany(c => c.Users)
                .HasForeignKey(cu => cu.ChatId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ChatAdmin>()
                .HasKey(ca => new { ca.ChatId, ca.UserId });

            builder.Entity<ChatAdmin>()
                .HasOne(ca => ca.Chat)
                .WithMany(c => c.Admins)
                .HasForeignKey(ca => ca.ChatId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<BannedChatUser>()
                .HasKey(bc => new { bc.ChatId, bc.UserId });

            builder.Entity<BannedChatUser>()
                .HasOne(bc => bc.Chat)
                .WithMany(c => c.BannedChatUsers)
                .HasForeignKey(bc => bc.ChatId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Friendship>()
                .HasKey(f => new { f.UserId, f.FriendId });

            builder.Entity<Friendship>()
             .HasOne(f => f.User)
             .WithMany()
             .HasForeignKey(f => f.UserId)
             .OnDelete(DeleteBehavior.Restrict); // Prevent cascade delete

            builder.Entity<Friendship>()
                .HasOne(f => f.Friend)
                .WithMany()
                .HasForeignKey(f => f.FriendId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<BlockedUser>()
                .HasKey(b => new { b.BlockedById, b.BlockedId });

            builder.Entity<BlockedUser>()
               .HasOne(b => b.BlockedBy)
               .WithMany()
               .HasForeignKey(b => b.BlockedById)
               .OnDelete(DeleteBehavior.Restrict); // Specify ON DELETE NO ACTION for BlockedBy

            builder.Entity<BlockedUser>()
                .HasOne(b => b.Blocked)
                .WithMany()
                .HasForeignKey(b => b.BlockedId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ChatEvent>()
                .HasOne(ce => ce.Chat)
                .WithMany()
                .HasForeignKey(ce => ce.ChatId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Message>()
                .HasOne(m => m.Chat)
                .WithMany(c => c.Messages)
                .HasForeignKey(m => m.ChatId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
