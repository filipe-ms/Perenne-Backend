public class FeedChannel
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid GroupId { get; set; }
    public required virtual Group Group { get; set; }
}