using perenne.Models;
using System.ComponentModel.DataAnnotations;

public class ChatChannel : Entity
{
    public List<ChatMessage> Messages { get; set; } = new();

    // Foreign Key
    [Required]
    public Guid GroupId { get; set; }

    // Navigation Property
    [Required]
    public Group Group { get; set; } = null!;
}
