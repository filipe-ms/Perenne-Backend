using perenne.Models;
using perenne.Utils;

namespace perenne.FTOs
{
    public record PostFTO
    {
        public Guid Id { get; init; }
        public string Title { get; init; }
        public string Content { get; init; }
        public string? ImageUrl { get; init; }
        public int Likes { get; init; }
        public string FirstName { get; init; }
        public string LastName { get; init; }
        public string Role { get; init; }
        public DateTime CreatedAt { get; init; }

        public PostFTO(Post post)
        {
            Id = post.Id;
            Title = post.Title;
            Content = post.Content!;
            ImageUrl = post.ImageUrl;
            Likes = post.Likes;
            FirstName = post.CreatedBy!.FirstName;
            LastName = post.CreatedBy!.LastName;
            Role = post.CreatedBy!.SystemRole.EnumToName();
            CreatedAt = post.CreatedAt;
        }
    }
}
