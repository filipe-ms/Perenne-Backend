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

        // Recuperação de senha
        Task<(bool Success, string? Token, string? ErrorMessage)> InitiatePasswordResetAsync(string email); 
        Task<(bool Success, string? ErrorMessage)> ResetPasswordAsync(string token, string newPassword); 
    }
}
