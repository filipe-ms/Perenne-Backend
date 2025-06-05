namespace perenne.DTOs
{
    public class TokenGenerationRequest(Guid userId, string email)
    {
        public Guid UserId { get; set; } = userId;
        public string Email { get; set; } = email;
    }
}
