using perenne.DTOs;
using perenne.Models;

namespace perenne.Interfaces
{
    public interface IUserService
    {
        Task RegisterUserAsync(UserRegisterDto dto);
        Task<User> LoginAsync(string email, string password);
        Task<User> GetUserByIdAsync(Guid id);
    }
}
