using System.ComponentModel.DataAnnotations;

namespace perenne.Models
{
    public class Group : Entity
    {
        [Required, MinLength(4), MaxLength(100)]
        public required string Name { get; set; }

        [MinLength(2), MaxLength(500)]
        public string? Description { get; set; }

        public bool IsPrivate { get; set; } = false; // New property

        public List<GroupMember> Members { get; set; } = [];
        public ChatChannel? ChatChannel { get; set; }
        public Feed? Feed { get; set; }

        // Navigation property for join requests
        public virtual ICollection<GroupJoinRequest> JoinRequests { get; set; } = [];
    }
}