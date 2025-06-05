using System.ComponentModel.DataAnnotations;

namespace perenne.DTOs
{
    public record GroupCreateDTO
    {
        [Required(ErrorMessage = "O campo Nome é obrigatório.")]
        [MinLength(3, ErrorMessage = "O nome deve ter ao menos 3 caracteres.")]
        public required string Name { get; init; }
        public string Description { get; init; } = string.Empty;
        public bool IsPrivate { get; set; } = false;
    }
    public record GroupDeleteDTO
    {
        [Required(ErrorMessage = "O ID do grupo é obrigatório.")]
        public required string GroupIdString { get; init; }
    }

    public record GroupSummaryDTO
    {
        public Guid Id { get; init; }
        public required string Name { get; init; }
        public string Description { get; init; } = string.Empty;
        public int MemberCount { get; init; }
    }

    public record GroupUpdateDTO(
        [Required(ErrorMessage = "O ID do grupo é obrigatório.")]
        string GroupIdString,
        string? NewNameString,
        string? NewDescriptionString
        );

    // DTO para o join request
    public record JoinGroupRequestDto(string? Message);
}
