using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Leaderboard.Auth.Interfaces;
using Leaderboard.Users.Models;

namespace Leaderboard.Auth.Services;

public class TokenService : ITokenService
{
    private readonly string _secret;
    private readonly string _issuer;
    private readonly string _audience;
    private readonly int _accessTokenMinutes;

    public TokenService()
    {
        _secret = Environment.GetEnvironmentVariable("JWT_SECRET") ?? "dev-insecure-secret-change";
        _issuer = Environment.GetEnvironmentVariable("JWT_ISSUER") ?? "LeaderboardAPI";
        _audience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? "LeaderboardUsers";
        _accessTokenMinutes = int.TryParse(Environment.GetEnvironmentVariable("JWT_ACCESS_TOKEN_MINUTES"), out var minutes) ? minutes : 60;
    }

    public (string Token, DateTime ExpiresAtUtc) CreateAccessToken(User user)
    {
        var expires = DateTime.UtcNow.AddMinutes(_accessTokenMinutes);
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.UniqueName, user.Username),
        };

        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: expires,
            signingCredentials: creds
        );

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
        return (tokenString, expires);
    }
}


