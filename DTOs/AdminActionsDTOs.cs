using System.ComponentModel.DataAnnotations;

namespace perenne.DTOs
{
    public record MuteChatMemberDTO(
        [Required(ErrorMessage = "O ID do membro é obrigatório.")]
        string MemberIdString,
        [Required(ErrorMessage = "O ID do grupo é obrigatório.")]
        string GroupIdString,
        [Required(ErrorMessage = "A data de término do mute é obrigatória.")]
        int Minutes);

    public record UnmuteChatMemberDTO(
        [Required(ErrorMessage = "O ID do membro é obrigatório.")]
        string MemberIdString,
        [Required(ErrorMessage = "O ID do grupo é obrigatório.")]
        string GroupIdString);
}