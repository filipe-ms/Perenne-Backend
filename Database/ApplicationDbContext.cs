using Microsoft.EntityFrameworkCore;
using perenne.Models;

namespace perenne.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>()
                .HasIndex(u => u.CPF)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasOne<User>()
                .WithMany()
                .HasForeignKey("CreatedById")
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<User>()
                .HasOne<User>()
                .WithMany()
                .HasForeignKey("UpdatedById")
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Group>(entity =>
            {
                entity.HasOne(g => g.ChatChannelId)
                    .WithOne(c => c.Group)
                    .HasForeignKey<ChatChannel>(c => c.GroupId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(g => g.FeedChannelId)
                    .WithOne(f => f.Group)
                    .HasForeignKey<FeedChannel>(f => f.GroupId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure unique constraints for channels
            modelBuilder.Entity<ChatChannel>()
                .HasIndex(c => c.GroupId)
                .IsUnique();

            modelBuilder.Entity<FeedChannel>()
                .HasIndex(f => f.GroupId)
                .IsUnique();
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<ChatChannel> ChatChannels { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<Feed> Feeds { get; set; }


    }
}