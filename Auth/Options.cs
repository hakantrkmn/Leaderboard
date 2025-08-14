namespace Leaderboard.Auth.Options;

public class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Secret { get; set; } = null!;
    public string Issuer { get; set; } = "LeaderboardAPI";
    public string Audience { get; set; } = "LeaderboardUsers";
    public int AccessTokenMinutes { get; set; } = 60;
}