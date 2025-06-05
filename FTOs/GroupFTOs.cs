using perenne.Models;

namespace perenne.FTOs
{
    public record GroupCreateFTO
    {
        public string Name { get; init; }
        public string Description { get; init; }
        public bool IsPrivate { get; set; } = false;

        public GroupCreateFTO(Group group)
        {
            Name = group.Name;
            Description = group.Description!;
            IsPrivate = group.IsPrivate;
        }
    }

    public record MemberFTO
    {
        public Guid UserId { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public GroupRole Role { get; set; }
        public bool IsBlocked { get; set; }
        public Guid MutedBy { get; set; }
        public DateTime? MutedUntil { get; set; }

        public MemberFTO(GroupMember member)
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

    public record GroupFTO(string Name, string Description, List<MemberFTO>? MemberList);
}
