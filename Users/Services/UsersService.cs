using Leaderboard.Users.DTO;
using Leaderboard.Users.Interfaces;
using Leaderboard.Users.Models;

namespace Leaderboard.Users.Services;

public class UsersService : IUsersService
{
    private readonly IUserRepository _userRepository;

    public UsersService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<UserResponse?> GetUserById(Guid id , CancellationToken ct = default)
    {
        var user = await _userRepository.GetByIdAsync(id, ct);
        if (user is null)
        {
            return null;
        }
        return new UserResponse(user.Id, user.Username, user.RegistrationDate, user.PlayerLevel, user.TrophyCount);
    }

}