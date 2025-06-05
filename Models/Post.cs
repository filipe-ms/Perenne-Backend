using System.ComponentModel.DataAnnotations;

namespace perenne.Models
{
    public class Post : Entity
    {
        [Required]
        public required string Title { get; set; } = string.Empty;

        public string? Content { get; set; } = string.Empty;

        public string? ImageUrl { get; set; }

        public int Likes { get; set; } = 0;

        // Foreign Keys
        [Required]
        public required Guid FeedId { get; set; }

        [Required]
        public Guid UserId { get; set; }

        // Navigation properties
        [Required]
        public Feed Feed { get; set; } = null!;

        [Required]
        public User User { get; set; } = null!;
    }
}
