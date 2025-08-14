using Leaderboard.Users.Models;

namespace Leaderboard.Auth.Interfaces;

public interface ITokenService
{
    (string Token, DateTime ExpiresAtUtc) CreateAccessToken(User user);
}