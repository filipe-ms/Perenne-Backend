using System.ComponentModel.DataAnnotations;

namespace perenne.DTOs
{
    public record GuestLoginDto
    {
        [Required(ErrorMessage = "O campo Email é obrigatório.")]
        [EmailAddress(ErrorMessage = "O formato do Email é inválido.")]
        public string? Email { get; init; }

        [Required(ErrorMessage = "O campo Senha é obrigatório.")]
        public string? Password { get; init; } = default!;
    }

    public record GuestRegisterDto
    {
        [Required(ErrorMessage = "O campo Email é obrigatório.")]
        [EmailAddress(ErrorMessage = "O formato do Email é inválido.")]
        public required string Email { get; init; }

        [Required(ErrorMessage = "O campo Senha é obrigatório.")]
        [MinLength(6, ErrorMessage = "A Senha deve ter no mínimo 6 caracteres.")]
        public required string Password { get; init; }

        [Required(ErrorMessage = "O campo Nome é obrigatório.")]
        [MinLength(2, ErrorMessage = "O Nome deve ter no mínimo 2 caracteres.")]
        [MaxLength(100, ErrorMessage = "O Nome deve ter no máximo 100 caracteres.")]
        public required string FirstName { get; init; }

        [Required(ErrorMessage = "O campo Sobrenome é obrigatório.")]
        [MinLength(2, ErrorMessage = "O Sobrenome deve ter no mínimo 2 caracteres.")]
        [MaxLength(100, ErrorMessage = "O Sobrenome deve ter no máximo 100 caracteres.")]
        public required string LastName { get; init; }

        [Required(ErrorMessage = "O campo CPF é obrigatório.")]
        [StringLength(11, MinimumLength = 11, ErrorMessage = "O CPF deve ter exatamente 11 dígitos.")]
        public required string CPF { get; init; }
    }
}