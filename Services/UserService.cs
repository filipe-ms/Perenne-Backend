using perenne.DTOs;
using perenne.Models;
using perenne.Repositories;
using perenne.Interfaces;

namespace perenne.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;

    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<User> GetUserByIdAsync(Guid id)
    {
        var user = await _userRepository.GetUserByIdAsync(id);
        if (user == null) 
            throw new Exception("User not found");
        return user;
    }

    public async Task<IEnumerable<Group>> GetGroupsByUserIdAsync(Guid userId)
    {
        return await _userRepository.GetGroupsByUserIdAsync(userId);
    }

    public Guid ParseUserId(string? str) 
    {
        if (string.IsNullOrEmpty(str))
            throw new BadHttpRequestException($"[{nameof(ParseUserId)}] String não pode ser nula ou vazia.");
        if (!Guid.TryParse(str, out var guid))
            throw new BadHttpRequestException($"[{nameof(ParseUserId)}] String não é um GUID válido.");

        return guid;
    }
}
