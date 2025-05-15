namespace perenne.Models
{
    public class Post : Entity
    {
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public Guid FeedId { get; set; }
        public virtual Feed Feed { get; set; } = null!;
        public Guid UserId { get; set; }
        public virtual User User { get; set; } = null!;
    }
}