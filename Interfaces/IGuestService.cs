using perenne.DTOs;
using perenne.Models;

namespace perenne.Interfaces
{
    public interface IGuestService
    {
        Task<bool> CreateUserAsync(User user);
        Task<User> UserLoginAsync(string email, string password);
    }
}
