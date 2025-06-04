using perenne.Models;

namespace perenne.FTOs
{
    public record MemberFto
    {
        public Guid UserId { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public GroupRole Role { get; set; }
        public bool IsBlocked { get; set; }
        public bool IsMuted { get; set; }

        public MemberFto(GroupMember member)
        {
            UserId = member.UserId;
            FirstName = member.User.FirstName;
            LastName = member.User.LastName;
            Role = member.Role;
            IsBlocked = member.IsBlocked;
            IsMuted = member.IsMuted;
        }
    }

    public record GroupFTO(string Name, string Description, List<MemberFto>? MemberList);
}
