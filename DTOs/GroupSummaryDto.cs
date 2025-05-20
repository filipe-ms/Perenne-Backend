namespace perenne.DTOs
{
    public class GroupSummaryDto
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public string? Description { get; set; }
        public Guid? ChatChannelId { get; set; }
        public int MemberCount { get; set; }
    }
}
