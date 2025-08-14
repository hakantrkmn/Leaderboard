using Leaderboard.Users.DTO;
using Leaderboard.Auth.DTO;

namespace Leaderboard.Auth.Interfaces;

public interface IAuthService
{
    Task<AuthResponse?> LoginAsync(LoginRequest request, CancellationToken ct = default);
    Task<AuthResponse?> RegisterAsync(RegisterUserRequest request, CancellationToken ct = default);
}