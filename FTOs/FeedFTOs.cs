using perenne.Models;

namespace perenne.FTOs
{
    public record PostFTO
    {
        public Guid Id { get; init; }
        public string Title { get; init; }
        public string Content { get; init; }
        public string? ImageUrl { get; init; }
        public int Likes { get; init; }
        public Guid Creator { get; init; }
        public DateTime CreatedAt { get; init; }

        public PostFTO(Post post)
        {
            Id = post.Id;
            Title = post.Title;
            Content = post.Content!;
            ImageUrl = post.ImageUrl;
            Likes = post.Likes;
            Creator = (Guid)post.CreatedById!;
            CreatedAt = post.CreatedAt;
        }
    }
}
