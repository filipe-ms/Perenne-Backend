using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

public class Group : Entity
{
    [Required, MinLength(4), MaxLength(100)]
    public required string Name { get; set; }

    [MinLength(2), MaxLength(500)]
    public string? Description { get; set; }
    public List<GroupMember> Members { get; set; } = new();
    public ChatChannel? ChatChannel { get; set; }
    public Feed? Feed { get; set; }
}