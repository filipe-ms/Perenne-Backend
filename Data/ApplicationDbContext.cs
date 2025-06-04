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
        public DbSet<GroupJoinRequest> GroupJoinRequests { get; set; }

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

            // GroupJoinRequest configuration
            modelBuilder.Entity<GroupJoinRequest>(entity =>
            {
                entity.HasKey(gjr => gjr.Id);

                entity.HasOne(gjr => gjr.User)
                    .WithMany(u => u.GroupJoinRequests)
                    .HasForeignKey(gjr => gjr.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(gjr => gjr.Group)
                    .WithMany(g => g.JoinRequests)
                    .HasForeignKey(gjr => gjr.GroupId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(gjr => gjr.HandledByUser)
                    .WithMany()
                    .HasForeignKey(gjr => gjr.HandledByUserId)
                    .IsRequired(false)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.Property(gjr => gjr.Status)
                    .HasConversion<string>();

                // Prevent duplicate pending requests from the same user for the same group
                entity.HasIndex(gjr => new { gjr.UserId, gjr.GroupId, gjr.Status })
                      .HasFilter("\"Status\" = 'Pending'")
                      .IsUnique();
            });

            // Enum conversion
            modelBuilder.Entity<GroupMember>()
                .Property(gm => gm.Role)
                .HasConversion<string>();

            // Relação ChatChannel → Messages
            modelBuilder.Entity<ChatChannel>()
                .HasMany(cc => cc.Messages)
                .WithOne(cm => cm.ChatChannel)
                .HasForeignKey(cm => cm.ChatChannelId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ChatChannel>()
                .HasOne(cc => cc.Group)
                .WithOne(g => g.ChatChannel)
                .HasForeignKey<ChatChannel>(cc => cc.GroupId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Cascade);

            // Novas relações para Chat Privado
            modelBuilder.Entity<ChatChannel>()
                .HasOne(cc => cc.User1)
                .WithMany()
                .HasForeignKey(cc => cc.User1Id)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ChatChannel>()
                .HasOne(cc => cc.User2)
                .WithMany()
                .HasForeignKey(cc => cc.User2Id)
                .OnDelete(DeleteBehavior.Restrict);

            // Novos Índices para ChatChannel
            modelBuilder.Entity<ChatChannel>()
                .HasIndex(cc => new { cc.User1Id, cc.User2Id, cc.IsPrivate })
                .IsUnique()
                .HasFilter("\"IsPrivate\" = true AND \"User1Id\" IS NOT NULL AND \"User2Id\" IS NOT NULL");

            modelBuilder.Entity<ChatChannel>()
                .HasIndex(cc => new { cc.GroupId, cc.IsPrivate })
                .IsUnique()
                .HasFilter("\"IsPrivate\" = false AND \"GroupId\" IS NOT NULL");

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

            modelBuilder.Entity<User>()
                .HasMany(u => u.PrivateChatChannelsAsUser1)
                .WithOne(cc => cc.User1)
                .HasForeignKey(cc => cc.User1Id)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<User>()
                .HasMany(u => u.PrivateChatChannelsAsUser2)
                .WithOne(cc => cc.User2)
                .HasForeignKey(cc => cc.User2Id)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}

// Mexeu aqui, faz migração
// dotnet ef migrations add [nome da migração]
// dotnet ef database update