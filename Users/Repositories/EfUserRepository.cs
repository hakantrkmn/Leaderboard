using Leaderboard.Users.Interfaces;
using Leaderboard.Users.Models;
using Leaderboard.DB;
using Microsoft.EntityFrameworkCore;

namespace Leaderboard.Users.Repositories;

public class EfUserRepository : IUserRepository
{
    private readonly DBContext _context;

    public EfUserRepository(DBContext context)
    {
        _context = context;
    }

    public async Task<User?> LoginAsync(string username, string password, CancellationToken ct = default)
    {
        var user = await GetByUsernameAsync(username, ct);
        if (user is null) return null;
        if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash)) return null;
        return user;
    }

    public async Task<User?> RegisterAsync(User user, CancellationToken ct = default)
    {
        await AddAsync(user, ct);
        return user;
    }

    public async Task AddAsync(User user, CancellationToken ct = default)
    {
        await _context.Users.AddAsync(user, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Id == id, ct);
    }

    public async Task<User?> GetByUsernameAsync(string username, CancellationToken ct = default)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Username == username, ct);
    }

    public Task<bool> UsernameExistsAsync(string username, CancellationToken ct = default)
    {
        return _context.Users.AnyAsync(u => u.Username == username, ct);
    }
}