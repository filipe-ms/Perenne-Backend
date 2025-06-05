using System.ComponentModel.DataAnnotations;

namespace perenne.DTOs
{
    public class ForgotPasswordRequestDto
    {
        [Required]
        [EmailAddress]
        public required string Email { get; set; }
    }

    public class ResetPasswordRequestDto
    {
        [Required]
        public required string Token { get; set; }

        [Required]
        [MinLength(6)] // Mantenha uma validação mínima para a nova senha
        public required string NewPassword { get; set; }
    }
}