using System.ComponentModel.DataAnnotations;

namespace perenne.DTOs
{
    public class GroupCreateDto
    {
        [Required, MinLength(3)]
        public required string Name { get; set; }
        public string? Description { get; set; }
    }
}
