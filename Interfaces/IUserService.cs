using perenne.Models;

namespace perenne.Interfaces
{
    public interface IUserService
    {
        Task<User> GetUserByIdAsync(Guid id);
        Task<IEnumerable<Group>> GetGroupsByUserIdAsync(Guid userId);
        Guid ParseUserId(string? str);
        Task<bool> UpdateUserRoleInSystemAsync(Guid userId, SystemRole newRole);
    }
}