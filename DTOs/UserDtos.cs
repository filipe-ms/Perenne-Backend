using System.ComponentModel.DataAnnotations;

namespace perenne.DTOs
{
    public record SystemRoleDTO(
        [Required(ErrorMessage ="ID de usuário vazio!")]
        string UserIdString,
        [Required(ErrorMessage ="Campo de cargo vazio!")]
        string NewRoleString);

    public record EditUserDTO(
        string? FirstName,
        string? LastName,
        string? Bio,
        string? ProfilePictureURL);
}
