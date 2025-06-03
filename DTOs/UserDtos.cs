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

    public record UserInfoDto
    {
        public string Email { get; init; }
        public bool IsValidated { get; init; }
        public bool IsBanned { get; init; }
        public string FirstName { get; init; }
        public string LastName { get; init; }
        public string CPF { get; init; }
        public int Role { get; init; } // Considere usar um Enum para Role se fizer sentido
        //public DateTime? DateOfBirth { get; init; }
        public string? ProfilePictureUrl { get; init; }
        public List<string> Groups { get; init; }

        // talvez listar projetos qnd e se for implementado?
        public DateTime CreatedAt { get; init; }
    }

}
