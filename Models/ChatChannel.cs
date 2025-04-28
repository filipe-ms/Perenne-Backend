using perenne.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

public class ChatChannel
{
    [Required, Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }
    public Guid GroupId { get; set; } // Non-nullable foreign key
    public Guid FirstUserId { get; set; }
    public Guid SecondUserId { get; set; }

    public required virtual User FirstUser { get; set; }
    public required virtual User SecondUser { get; set; }
    public List<ChatMessage> Messages { get; set; } = new();
    public required virtual Group Group { get; set; } // Non-nullable navigation
}