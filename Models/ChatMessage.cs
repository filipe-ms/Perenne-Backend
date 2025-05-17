using System.ComponentModel.DataAnnotations;

namespace perenne.Models
{
    public class ChatMessage : Entity
    {
        [Required]
        public required string Message { get; set; } = string.Empty;
        public bool IsRead { get; set; } = false;
        public bool IsDelivered { get; set; } = false;
        public bool IsDeleted { get; set; } = false;

        // Foreign keys
        public required Guid ChatChannelId { get; set; }

        // Navigation properties
        public required ChatChannel ChatChannel { get; set; }
    }
}
