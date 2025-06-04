using perenne.Models;
using System.ComponentModel.DataAnnotations.Schema;

public class ChatChannel : Entity
{
    public List<ChatMessage> Messages { get; set; } = new();

    // Chat de Grupos
    public Guid? GroupId { get; set; }
    public Group? Group { get; set; }

    // Chat privado
    public bool IsPrivate { get; set; } = false;
    public Guid? User1Id { get; set; }
    [ForeignKey(nameof(User1Id))]
    public User? User1 { get; set; }

    public Guid? User2Id { get; set; }
    [ForeignKey(nameof(User2Id))]
    public User? User2 { get; set; }
}
