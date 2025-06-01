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
        return user == null ? throw new Exception("User not found") : user;
    }

    public async Task<IEnumerable<Group>> GetGroupsByUserIdAsync(Guid userId)
    {
        return await _userRepository.GetGroupsByUserIdAsync(userId);
    }

    public Guid ParseUserId(string? str)
    {
        if (string.IsNullOrEmpty(str))
            throw new ArgumentNullException($"[UserService] O parâmetro 'str' está nulo ou vazio. Um identificador de usuário é obrigatório.");
        if (!Guid.TryParse(str, out var guid))
            throw new ArgumentException($"[UserService] O valor fornecido não é um GUID válido.");
        return guid;
    }
}
