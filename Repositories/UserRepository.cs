using Microsoft.EntityFrameworkCore;
using perenne.Data;
using perenne.Models;

namespace perenne.Repositories;

public class UserRepository
{
    private readonly ApplicationDbContext _context;

    public UserRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> AddUserAsync(User user)
    {
        await _context.Users.AddAsync(user);
        var saved = await _context.SaveChangesAsync();
        return saved > 0;
    }

    public async Task<bool> UserExistsAsync(string email, string cpf)
    {
        return await _context.Users.AnyAsync(u => u.Email == email || u.CPF == cpf);
    }
}
