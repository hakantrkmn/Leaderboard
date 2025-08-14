using Leaderboard.Users.DTO;

namespace Leaderboard.Auth.DTO;

public record AuthResponse(string AccessToken, DateTime ExpiresAtUtc);