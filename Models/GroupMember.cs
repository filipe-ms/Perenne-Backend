using Microsoft.EntityFrameworkCore;
using perenne.Models;
using System.ComponentModel.DataAnnotations;
public enum GroupRole
{
    Member,
    Admin,
    Moderator
}

[Index(nameof(GroupId), nameof(UserId), IsUnique = true)]
public class GroupMember
{
    [Required]
    public required GroupRole Role { get; set; }
    public bool IsBlocked { get; set; } = false;
    public bool IsMutedInGroupChat { get; set; } = false;

    // Audit fields

    [Required]
    public required DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public Guid? CreatedById { get; set; }
    public Guid? UpdatedById { get; set; }

    // Foreign keys
    [Required]
    public required Guid GroupId { get; set; }

    [Required]
    public required Guid UserId { get; set; }

    // Navigation properties
    public required User User { get; set; }
    public required Group Group { get; set; }

    
}