using perenne.Models;

public class Feed : Entity
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid GroupId { get; set; }
    public required virtual Group Group { get; set; }
    public virtual List<Post> Posts { get; set; } = new List<Post>();
}