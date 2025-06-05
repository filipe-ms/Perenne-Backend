using Microsoft.EntityFrameworkCore;
using perenne.Data;
using perenne.Models;

namespace perenne.Repositories;

public class UserRepository(ApplicationDbContext _context) : IUserRepository
{
    public async Task<bool> CreateUserAsync(User user)
    {
        if (user == null) throw new ArgumentNullException(nameof(user));
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();
        return true;
    }
    public async Task<User> GetUserByEmailAsync(string email)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        return user == null ? throw new KeyNotFoundException($"User with email {email} not found.") : user;
    }
    public async Task<User> GetUserByIdAsync(Guid id)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
        return user == null ? throw new KeyNotFoundException($"User with ID {id} not found.") : user;
    }
    public async Task<IEnumerable<Group>> GetGroupsByUserIdAsync(Guid userId)
    {
        var user = await _context.Users
            .AsNoTracking()
            .Include(u => u.Groups)
                .ThenInclude(gm => gm.Group)
                    .ThenInclude(g => g.ChatChannel)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null || user.Groups == null)
        {
            return Enumerable.Empty<Group>();
        }

        return user.Groups.Select(gm => gm.Group).ToList();
    }

    // Recuperação de senha
    public async Task<User?> GetUserByPasswordResetTokenAsync(string token)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.PasswordResetToken == token);
    }

    public async Task<bool> UpdateUserAsync(User user)
    {
        if (user == null) throw new ArgumentNullException(nameof(user));
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
        return true;
    }
}