using perenne.Models;

namespace perenne.Repositories
{
    public interface IUserRepository
    {
        Task<User> CreateUserAsync(User user);
        Task<User> GetUserByEmailAsync(string email);
        Task<User> GetUserByIdAsync(Guid id);
        Task<IEnumerable<Group>> GetGroupsByUserIdAsync(Guid userId);
        Task<bool> UpdateUserRoleInSystemAsync(Guid userId, SystemRole newRole);
        Task<IEnumerable<User>> GetAllUsersAsync();

        Task<User> UpdateUserAsync(User user);
    }
}