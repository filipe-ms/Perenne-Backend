using System.ComponentModel.DataAnnotations;

namespace perenne.Models
{
    public enum UserRole
    {
        Admin,
        User,
        Guest
    }
    public class User : Entity
    {
        [Required, MinLength(4), MaxLength(100)]
        public required string Email { get; set; }

        [Required, MinLength(6), MaxLength(100)]
        public required string Password { get; set; }

        public bool IsValidated { get; set; } = true; //lembrar de rodar pra false

        public bool IsBanned { get; set; } = false;

        [Required, MinLength(2), MaxLength(100)]
        public required string FirstName { get; set; }

        [Required, MinLength(2), MaxLength(100)]
        public required string LastName { get; set; }

        [Required, StringLength(11)]
        public required string CPF { get; set; }

        [Required]
        public UserRole Role { get; set; } = UserRole.User;

        public DateTime? DateOfBirth { get; set; }

        [MinLength(10), MaxLength(20)]
        public string? PhoneNumber { get; set; }

        [MinLength(2), MaxLength(255)]
        public string? Address { get; set; }

        [MaxLength(1000)]
        public string? ProfilePictureUrl { get; set; }

        [MaxLength(3000)]
        public string? Bio { get; set; }

    }
}
