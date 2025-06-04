using perenne.Models;
using perenne.Utils;

namespace perenne.FTOs
{
    public record ProfileInfoFTO
    {
        public string Email { get; init; }
        public string FirstName { get; init; }
        public string LastName { get; init; }
        public string Role { get; init; }
        public string ProfilePictureUrl { get; init; }
        public List<string>? Groups { get; init; }
        public DateTime CreatedAt { get; init; }
        public bool IsBanned { get; init; }

        public ProfileInfoFTO(User user)
        {
            Email = user.Email;
            FirstName = user.FirstName;
            LastName = user.LastName;
            Role = user.SystemRole.EnumToName();
            ProfilePictureUrl = string.IsNullOrEmpty(user.ProfilePictureUrl) ? "" : user.ProfilePictureUrl;
            CreatedAt = user.CreatedAt;
            IsBanned = user.IsBanned;
        }
    }
}
