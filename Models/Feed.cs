using perenne.Models;
using System.ComponentModel.DataAnnotations;

public class Feed : Entity
{
    public List<Post> Posts { get; set; } = new List<Post>();

    // Foreign Key
    [Required]
    public Guid GroupId { get; set; }

    // Navigation Property
    [Required]
    public Group Group { get; set; } = null!;
}
