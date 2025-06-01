using System.ComponentModel.DataAnnotations;

namespace perenne.DTOs
{
    public record AddGroupMemberDto
    {
        public Guid UserId { get; init; }
        public Guid GroupId { get; init; }
    }

    public record GroupDeleteDto
    {
        [Required(ErrorMessage = "O ID do grupo é obrigatório.")]
        public Guid GroupId { get; init; }
    }

    public record GroupCreateDto
    {
        [Required(ErrorMessage = "O campo Nome é obrigatório.")]
        [MinLength(3, ErrorMessage = "O nome deve ter ao menos 3 caracteres.")]
        public required string Name { get; init; }
        public string Description { get; init; } = string.Empty;
    }

    public record GroupSummaryDto
    {
        public Guid Id { get; init; }
        public required string Name { get; init; }
        public string Description { get; init; } = string.Empty;
        public int MemberCount { get; init; }
    }
}
