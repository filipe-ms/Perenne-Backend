using perenne.DTOs;
using perenne.Models;

namespace perenne.Services
{
    public interface IUserService
    {
        Task RegisterUserAsync(UserRegisterDto dto);
        Task<User> LoginAsync(string email, string password);
    }
}
