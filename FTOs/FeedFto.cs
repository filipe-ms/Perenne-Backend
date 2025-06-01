namespace perenne.FTOs
{
    public record PostFto
    {
        public Guid Id { get; init; }
        public string Title { get; init; }
        public string Content { get; init; }
        public string? ImageUrl { get; init; }
        public Guid Creator { get; init; }
        public DateTime CreatedAt { get; init; }
    }
}
