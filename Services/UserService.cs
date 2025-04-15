using perenne.DTOs;
using perenne.Models;
using perenne.Repositories;

namespace perenne.Services;

public class UserService
{
    private readonly UserRepository _userRepository;

    public UserService(UserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<bool> RegisterUserAsync(UserRegisterDto dto)
    {
        if (await _userRepository.UserExistsAsync(dto.Email, dto.CPF))
            return false;

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

        return await _userRepository.AddUserAsync(user);
    }
}
