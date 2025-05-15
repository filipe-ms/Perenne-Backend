using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using perenne.Models;

public class Group : Entity
{
    [Required, MinLength(4), MaxLength(100)]
    public required string Name { get; set; }

    [MinLength(2), MaxLength(500)]
    public string? Description { get; set; }

    public required ChatChannel ChatChannel { get; set; }

    public required Feed Feed { get; set; }
    public virtual List<GroupMember> Members { get; set; } = new List<GroupMember>();
}