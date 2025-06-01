using perenne.Models;

namespace perenne.DTOs
{
    public record UserLoginResponseDto
    {
        public User? User { get; init; }
        public string? Token { get; init; }

        public UserLoginResponseDto(User user, string token)
        {
            this.User = user;
            this.Token = token;
        }
    }
}
