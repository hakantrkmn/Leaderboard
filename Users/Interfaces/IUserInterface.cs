using Leaderboard.Users.DTO;


public interface IUsersService
{
    Task<UserResponse?> GetUserById(Guid id, CancellationToken ct = default);
}