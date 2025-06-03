using Microsoft.EntityFrameworkCore;
using perenne.Models;

namespace perenne.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }


        public DbSet<User> Users { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<GroupMember> GroupMembers { get; set; }
        public DbSet<ChatChannel> ChatChannels { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }
        public DbSet<Feed> Feed { get; set; }
        public DbSet<Post> Posts { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User configuration
            modelBuilder.Entity<User>()
                .HasIndex(u => u.CPF)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            // Group configuration
            modelBuilder.Entity<Group>()
                .HasIndex(g => g.Name)
                .IsUnique();

            modelBuilder.Entity<Group>()
                .HasOne(g => g.ChatChannel)
                .WithOne(cc => cc.Group)
                .HasForeignKey<ChatChannel>(cc => cc.GroupId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Group>()
                .HasOne(g => g.Feed)
                .WithOne(f => f.Group)
                .HasForeignKey<Feed>(f => f.GroupId)
                .OnDelete(DeleteBehavior.Cascade);

            // GroupMember configuration
            modelBuilder.Entity<GroupMember>()
                .HasKey(gm => new { gm.UserId, gm.GroupId });

            // Relationship: GroupMember → User (Many-to-One)
            modelBuilder.Entity<GroupMember>()
                .HasOne(gm => gm.User)
                .WithMany(u => u.Groups)
                .HasForeignKey(gm => gm.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Relationship: GroupMember → Group (Many-to-One)
            modelBuilder.Entity<GroupMember>()
                .HasOne(gm => gm.Group)
                .WithMany(g => g.Members)
                .HasForeignKey(gm => gm.GroupId)
                .OnDelete(DeleteBehavior.Cascade);

            // Enum conversion
            modelBuilder.Entity<GroupMember>()
                .Property(gm => gm.Role)
                .HasConversion<string>();

            // ChatChannel configuration
            modelBuilder.Entity<ChatChannel>()
                .HasMany(cc => cc.Messages)
                .WithOne(cm => cm.ChatChannel)
                .HasForeignKey(cm => cm.ChatChannelId)
                .OnDelete(DeleteBehavior.Cascade);

            // Feed configuration
            modelBuilder.Entity<Feed>()
                .HasMany(f => f.Posts)
                .WithOne(p => p.Feed)
                .HasForeignKey(p => p.FeedId)
                .OnDelete(DeleteBehavior.Cascade);

            // Post configuration
            modelBuilder.Entity<Post>()
                .HasOne(p => p.User)
                .WithMany()
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure enum conversions
            modelBuilder.Entity<GroupMember>()
                .Property(gm => gm.Role)
                .HasConversion<string>();

            modelBuilder.Entity<User>()
                .Property(u => u.SystemRole)
                .HasConversion<string>();

            // Entity base class audit tracking configuration
            modelBuilder.Entity<User>()
                .HasMany<User>()
                .WithOne(e => e.CreatedBy)
                .HasForeignKey("CreatedById")
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<User>()
                .HasMany<User>()
                .WithOne(e => e.UpdatedBy)
                .HasForeignKey("UpdatedById")
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}

// dotnet ef migrations add [nome da migração]
// dotnet ef database update