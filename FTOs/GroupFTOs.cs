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
        public Guid MutedBy { get; set; }
        public DateTime? MutedUntil { get; set; }

        public MemberFto(GroupMember member)
        {
            UserId = member.UserId;
            FirstName = member.User.FirstName;
            LastName = member.User.LastName;
            Role = member.Role;
            IsBlocked = member.IsBlocked;
            MutedUntil = member.MutedUntil;
            MutedBy = member.MutedBy;
        }
    }

    public record GroupFTO(string Name, string Description, List<MemberFto>? MemberList);
}
