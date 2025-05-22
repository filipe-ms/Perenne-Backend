namespace perenne.FTOs
{
    public class MemberFto
    {
        public Guid UserId { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public GroupRole RoleInGroup { get; set; }
        public bool IsBlocked { get; set; }
        public bool IsMutedInGroupChat { get; set; }
    }

    public record class GetGroupByIdFto
    {
        public required string Name { get; set; }
        public string? Description { get; set; }
        public List<MemberFto>? MemberList { get; set; }
    }
}
