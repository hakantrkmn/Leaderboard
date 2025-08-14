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

    public async Task<UserResponse?> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var user = await _userRepository.LoginAsync(request.Username, request.Password, ct);
        if (user is null) return null;
        return new UserResponse(user.Id, user.Username, user.RegistrationDate, user.PlayerLevel, user.TrophyCount);
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

    public async Task<UserResponse?> RegisterAsync(RegisterUserRequest request, CancellationToken ct = default)
    {
        var existingUser = await _userRepository.GetByUsernameAsync(request.Username, ct);
        if (existingUser is not null) return null;
        var user = new User
        {
            Username = request.Username,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            DeviceId = request.DeviceId,
        };
        await _userRepository.RegisterAsync(user, ct);
        if (user is null) return null;
        return new UserResponse(user.Id, user.Username, user.RegistrationDate, user.PlayerLevel, user.TrophyCount);
    }
}