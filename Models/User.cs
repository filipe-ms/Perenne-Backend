using System.ComponentModel.DataAnnotations;

namespace perenne.Models
{
    public class User : Entity
    {
        [Required, MinLength(4), MaxLength(100)]
        public required string Email { get; set; }

        [Required, MinLength(6), MaxLength(100)]
        public required string Password { get; set; }

        // Rodar para false no futuro,
        // quando houver sistema de validação por e-mail.
        public bool IsValidated { get; set; } = true; 

        public bool IsBanned { get; set; } = false;

        [Required, MinLength(2), MaxLength(100)]
        public required string FirstName { get; set; }

        [Required, MinLength(2), MaxLength(100)]
        public required string LastName { get; set; }

        [Required, StringLength(11)]
        public required string CPF { get; set; }

        [Required]
        public SystemRole SystemRole { get; set; } = SystemRole.User;

        public DateTime? DateOfBirth { get; set; }

        [MinLength(10), MaxLength(20)]
        public string? PhoneNumber { get; set; }

        [MinLength(2), MaxLength(255)]
        public string? Address { get; set; }

        [MaxLength(1000)]
        public string? ProfilePictureUrl { get; set; }

        [MaxLength(3000)]
        public string? Bio { get; set; }

        // Navigation property
        public virtual List<GroupMember> Groups { get; set; } = new();
        public virtual ICollection<ChatChannel> PrivateChatChannelsAsUser1 { get; set; } = new List<ChatChannel>();
        public virtual ICollection<ChatChannel> PrivateChatChannelsAsUser2 { get; set; } = new List<ChatChannel>();
    }
}
