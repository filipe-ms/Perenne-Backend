using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace perenne.Models
{
    [Index(
        nameof(GroupId), 
        nameof(UserId), 
        IsUnique = true)]
    public class GroupMember
    {
        public GroupRole Role { get; set; } = GroupRole.Member;
        public bool IsBlocked { get; set; } = false;
        public DateTime? MutedUntil { get; set; } = null;
        public Guid MutedBy { get; set; } = Guid.Empty;


        // Join Date
        public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

        // Foreign keys
        [Required]
        public required Guid GroupId { get; init; }

        [Required]
        public required Guid UserId { get; init; }

        // Navigation properties
        public required User User { get; set; }
        public required Group Group { get; set; }

    }
}