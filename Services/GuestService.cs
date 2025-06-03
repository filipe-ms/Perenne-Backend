using perenne.Interfaces;
using perenne.Models;
using perenne.Repositories;

namespace perenne.Services
{
    public class GuestService(IUserRepository _userRepository) : IGuestService
    {
        public async Task<bool> CreateUserAsync(User user)
        {
            return await _userRepository.CreateUserAsync(user);
        }
        public async Task<User> UserLoginAsync(string email, string password)
        {
            var user = await _userRepository.GetUserByEmailAsync(email);
            if (user == null || user.Password != password)
                throw new Exception("Invalid Email or Password");
            return user;
        }
    }
}
