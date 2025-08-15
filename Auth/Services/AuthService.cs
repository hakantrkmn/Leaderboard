using Leaderboard.Users.Interfaces;
using Leaderboard.Auth.DTO;
using Leaderboard.Auth.Interfaces;
using Leaderboard.Users.DTO;
using Leaderboard.Users.Models;
using Leaderboard.Metrics;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly ITokenService _tokenService;

    public AuthService(IUserRepository userRepository, ITokenService tokenService)
    {
        _userRepository = userRepository;
        _tokenService = tokenService;
    }

    public async Task<AuthResponse?> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var user = await _userRepository.GetByUsernameAsync(request.Username, ct);
        if (user is null) 
        {
            AppMetrics.LoginAttemptsTotal.WithLabels("failed").Inc();
            throw new UnauthorizedAccessException("Invalid username or password");
        }
        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            AppMetrics.LoginAttemptsTotal.WithLabels("failed").Inc();
            throw new UnauthorizedAccessException("Invalid username or password");
        }

        AppMetrics.LoginAttemptsTotal.WithLabels("success").Inc();
        var (token, expiresAt) = _tokenService.CreateAccessToken(user);
        return new AuthResponse(token, expiresAt);
    }

    public async Task<AuthResponse?> RegisterAsync(RegisterUserRequest request, CancellationToken ct = default)
    {
        var existingUser = await _userRepository.GetByUsernameAsync(request.Username, ct);
        if (existingUser is not null)
            throw new UnauthorizedAccessException("Username already exists");
        var user = new User{
            Username = request.Username,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            DeviceId = request.DeviceId,
        };
        await _userRepository.RegisterAsync(user, ct);
        var (token, expiresAt) = _tokenService.CreateAccessToken(user);
        return new AuthResponse(token, expiresAt);
    }
}