using System.ComponentModel.DataAnnotations;

namespace perenne.DTOs
{
    public record UserRegisterDto
    {
        [Required, EmailAddress]
        public required string Email { get; init; }

        [Required, MinLength(6)]
        public required string Password { get; init; }

        [Required, MinLength(2), MaxLength(100)]
        public required string FirstName { get; init; }

        [Required, MinLength(2), MaxLength(100)]
        public required string LastName { get; init; }

        [Required, StringLength(11)]
        public required string CPF { get; init; }
    }
}
