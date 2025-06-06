using Microsoft.EntityFrameworkCore;
using perenne.Data;
using perenne.Models;

namespace perenne.Repositories;

public class UserRepository(ApplicationDbContext context) : IUserRepository
{
    public async Task<User> CreateUserAsync(User user)
    {
        ArgumentNullException.ThrowIfNull(user);
        var entry = await context.Users.AddAsync(user);
        await context.SaveChangesAsync();
        return entry.Entity;
    }
    public async Task<User> GetUserByEmailAsync(string email)
    {
        var user = await context.Users.FirstOrDefaultAsync(u => u.Email == email);
        return user == null ? throw new KeyNotFoundException($"User with email {email} not found.") : user;
    }
    public async Task<User> GetUserByIdAsync(Guid id)
    {
        var user = await context.Users.FirstOrDefaultAsync(u => u.Id == id);
        return user == null ? throw new KeyNotFoundException($"User with ID {id} not found.") : user;
    }
    public async Task<IEnumerable<Group>> GetGroupsByUserIdAsync(Guid userId)
    {
        var user = await context.Users
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

    public async Task<bool> UpdateUserRoleInSystemAsync(Guid userId, SystemRole newRole)
    {
        var user = await context.Users.FirstOrDefaultAsync(u => u.Id == userId) ?? throw new KeyNotFoundException($"Usuário com ID {userId} não encontrado.");
        user.SystemRole = newRole;
        context.Users.Update(user);
        await context.SaveChangesAsync();
        return true;
    }

    public async Task<User> UpdateUserAsync(User user)
    {
        ArgumentNullException.ThrowIfNull(user);
        var existingUser = await context.Users.FirstOrDefaultAsync(u => u.Id == user.Id) ?? throw new KeyNotFoundException($"Usuário com ID {user.Id} não encontrado.");
        existingUser.FirstName = user.FirstName;
        existingUser.LastName = user.LastName;
        existingUser.Bio = user.Bio;
        context.Users.Update(existingUser);
        await context.SaveChangesAsync();
        return existingUser;
    }

    public async Task<IEnumerable<User>> GetAllUsersAsync()
    {
        var users = await context.Users.ToListAsync();
        if (users == null || users.Count == 0) return [];
        return users;
    }
}