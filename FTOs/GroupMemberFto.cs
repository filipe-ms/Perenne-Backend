namespace perenne.FTOs
{
    public class GroupMembershipFto
    {
        public Guid GroupId { get; set; }
        public required string GroupName { get; set; }
        public Guid UserId { get; set; }
        public required string UserFirstName { get; set; }
        public required string UserLastName { get; set; }
        public GroupRole RoleInGroup { get; set; } = GroupRole.Member;
        public DateTime JoinedAt { get; set; }
        public string Message { get; set; } = "Successfully joined the group.";
    }
}
