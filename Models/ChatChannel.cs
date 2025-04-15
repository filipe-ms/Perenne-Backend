namespace perenne.Models
{
    public class ChatChannel
    {
        public int Id { get; set; }

        public Guid PublicId { get; set; } = new Guid();
        public Guid FirstUserId { get; set; }
        public Guid SecondUserId { get; set; }

        public required virtual User FirstUser { get; set; }
        public required virtual User SecondUser { get; set; }
        public List<ChatMessage> Messages { get; set; } = new();
    }
}