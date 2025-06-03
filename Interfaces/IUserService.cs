using perenne.DTOs;
using perenne.Models;

namespace perenne.Interfaces
{
    public interface IUserService
    {
        Task<User> GetUserByIdAsync(Guid id);
        Task<IEnumerable<Group>> GetGroupsByUserIdAsync(Guid userId);
        Guid ParseUserId(string? str);
        Task<UserInfoDto?> GetUserInfoAsync(Guid userId);


    }
}
