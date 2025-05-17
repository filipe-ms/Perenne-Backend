using perenne.Models;
using System.ComponentModel.DataAnnotations;
public class ChatChannel : Entity
{
    [Required]
    public List<ChatMessage> Messages { get; set; } = new();

    // Foreign Key
    public Guid GroupId { get; set; }

    // Navigation Property
    [Required]
    public Group? Group { get; set; }
}