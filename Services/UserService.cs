using perenne.Interfaces;
using perenne.Models;
using perenne.Repositories;
using System.Security.Claims;

namespace perenne.Services;

public class UserService(IUserRepository userRepository) : IUserService
{
    public async Task<User> GetUserByIdAsync(Guid id)
    {
        var user = await userRepository.GetUserByIdAsync(id);
        return user ?? throw new Exception("User not found");
    }

    public async Task<IEnumerable<Group>> GetGroupsByUserIdAsync(Guid userId)
    {
        return await userRepository.GetGroupsByUserIdAsync(userId);
    }

    public async Task<bool> UpdateUserRoleInSystemAsync(Guid userId, SystemRole newRole)
    {
        return await userRepository.UpdateUserRoleInSystemAsync(userId, newRole);
    }

    public Guid ParseUserId(string? userIdString)
    {
        if (string.IsNullOrWhiteSpace(userIdString))
            throw new ArgumentNullException(nameof(userIdString), "[UserService] O parâmetro 'userIdString' está nulo ou vazio. Um identificador de usuário é obrigatório.");

        if (!Guid.TryParse(userIdString, out var guid))
            throw new ArgumentException("[UserService] O valor fornecido não é um GUID válido.", nameof(userIdString));

        return guid;
    }

    public async Task<IEnumerable<User>> GetAllUsersAsync()
    {
        return await userRepository.GetAllUsersAsync();
    }
}
