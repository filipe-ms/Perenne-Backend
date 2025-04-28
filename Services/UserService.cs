using perenne.DTOs;
using perenne.Models;
using perenne.Repositories;

namespace perenne.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;

    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task RegisterUserAsync(UserRegisterDto dto)
    {
        var user = new User
        {
            Email = dto.Email,
            Password = dto.Password,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            CPF = dto.CPF,
            IsValidated = false,
            CreatedAt = DateTime.UtcNow
        };

        try { await _userRepository.AddUserAsync(user); } 
        catch { throw new Exception("User with this email or CPF already exists."); }
    }

    public async Task<User> LoginAsync(string email, string password)
    {
        var user = await _userRepository.GetUserByEmailAsync(email);
        if (user == null || user.Password != password) 
            throw new Exception("Invalid Email or Password");
        return user;
    }
}
