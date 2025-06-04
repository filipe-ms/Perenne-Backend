using System.ComponentModel.DataAnnotations;

namespace perenne.DTOs
{
    public record MemberRoleDTO(
        [Required(ErrorMessage ="ID de grupo vazio!")]
        string GroupIdString,
        [Required(ErrorMessage ="ID de usuário vazio!")]
        string UserIdString,
        [Required(ErrorMessage ="Campo de cargo vazio!")]
        string NewRoleString);
}
