using perenne.Interfaces;
using perenne.Models;
using perenne.Repositories;

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

    public Guid ParseUserId(string? str)
    {
        if (string.IsNullOrEmpty(str))
            throw new ArgumentNullException($"[UserService] O parâmetro 'str' está nulo ou vazio. Um identificador de usuário é obrigatório.");
        if (!Guid.TryParse(str, out var guid))
            throw new ArgumentException($"[UserService] O valor fornecido não é um GUID válido.");
        return guid;
    }
}
