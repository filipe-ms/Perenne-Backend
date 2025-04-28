using perenne.Models;

namespace perenne.DTOs
{
    public class TokenGenerationRequest
    {
        public required Guid UserId { get; set; }
        public required string Email { get; set; }
    }
}
