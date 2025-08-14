using Leaderboard.Users.DTO;


public interface IUsersService
{
    Task<UserResponse?> LoginAsync(LoginRequest request, CancellationToken ct = default);
    Task<UserResponse?> GetUserById(Guid id, CancellationToken ct = default);
    Task<UserResponse?> RegisterAsync(RegisterUserRequest request, CancellationToken ct = default);
}