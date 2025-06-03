using perenne.Models;

namespace perenne.FTOs
{
    public record GroupJoinedFTO
    {
        public Guid GroupId { get; init; }
        public string GroupName { get; init; }
        public GroupRole Role { get; init; }
        public DateTime JoinedAt { get; init; }

        public GroupJoinedFTO(GroupMember member) 
        {
            GroupId = member.GroupId;
            GroupName = member.Group.Name;
            Role = member.Role;
            JoinedAt = member.CreatedAt;
        }
    }
}
