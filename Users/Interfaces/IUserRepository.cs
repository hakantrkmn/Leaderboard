using Leaderboard.Users.Models;

namespace Leaderboard.Users.Interfaces;

public interface IUserRepository
{
	Task<bool> UsernameExistsAsync(string username, CancellationToken ct = default);
	Task AddAsync(User user, CancellationToken ct = default);
	Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default);
	Task<User?> GetByUsernameAsync(string username, CancellationToken ct = default);
	Task<User?> LoginAsync(string username, string password, CancellationToken ct = default);
	Task<User?> RegisterAsync(User user, CancellationToken ct = default);
}