using perenne.Models;

namespace perenne.Repositories
{
    public interface IUserRepository
    {
        Task AddUserAsync(User user);
        Task<User?> GetUserByEmailAsync(string email);
        Task<User> GetUserByIdAsync(Guid id);
        Task<IEnumerable<Group>> GetGroupsByUserIdAsync(Guid userId);
    }
}