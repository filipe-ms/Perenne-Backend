using System.ComponentModel.DataAnnotations;

namespace perenne.DTOs
{
    public class PostDTO
    {
        [Required(ErrorMessage = "O campo Título é obrigatório.")]
        [MinLength(2, ErrorMessage = "O Título deve ter no mínimo 2 caracteres.")]
        public string? Title { get; set; }
        public string? Content { get; set; } = string.Empty;
        public string? ImageUrl { get; set; } = string.Empty;
    }

    public class EditPostDTO
    {
        public string? PostIdString { get; set; }
        public string? Title { get; set; }
        public string? Content { get; set; } = string.Empty;
        public string? ImageUrl { get; set; } = string.Empty;
    }
}
