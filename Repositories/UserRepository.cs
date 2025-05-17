using Microsoft.EntityFrameworkCore;
using perenne.Data;
using perenne.Models;

namespace perenne.Repositories;

public class UserRepository(ApplicationDbContext context) : IUserRepository
{
    public async Task AddUserAsync(User user)
    {
        await context.Users.AddAsync(user);
        await context.SaveChangesAsync();
    }

    public async Task<User?> GetUserByEmailAsync(string email) =>
        await context.Users.FirstOrDefaultAsync(u => u.Email == email);

    public async Task<User?> GetUserByIdAsync(Guid id)
    {
        return await context.Users.FirstOrDefaultAsync(u => u.Id == id);
    }
        
}
