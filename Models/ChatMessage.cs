namespace perenne.Models
{
    public class ChatMessage : Entity
    {
        public string Message { get; set; } = string.Empty;
        public bool IsRead { get; set; } = false;
        public bool IsDelivered { get; set; } = false;
        public bool Deleted { get; set; } = false;
    }
}
