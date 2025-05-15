using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using perenne.Models;

public class GroupMember : Entity
{
    public enum GroupRole
    {
        Member,
        Admin,
        Moderator
    }
    public required Guid UserId { get; set; }
    public virtual User User { get; set; } = null!;
    public required Guid GroupId { get; set; }
    public required virtual Group Group { get; set; } = null!;
    public required GroupRole Role { get; set; }
    public bool IsBlocked { get; set; } = false;
}