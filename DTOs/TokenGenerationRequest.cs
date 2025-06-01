namespace perenne.DTOs
{
    public class TokenGenerationRequest
    {
        public Guid UserId { get; set; }
        public string Email { get; set; }
        public TokenGenerationRequest(Guid userId, string email)
        {
            UserId = userId;
            Email = email;
        }
    }
}
