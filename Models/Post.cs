using System.ComponentModel.DataAnnotations;

namespace perenne.Models
{
    public class Post : Entity
    {
        [Required]
        public required string Title { get; set; } = string.Empty;

        [Required]
        public required string Content { get; set; } = string.Empty;

        public string? ImageUrl { get; set; }

        // Foreign keys

        public Guid FeedId;

        // Navigation properties
        public Feed Feed { get; set; } = null!;
        public User User { get; set; } = null!;
    }
}