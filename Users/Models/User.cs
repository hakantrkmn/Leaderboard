namespace Leaderboard.Users.Models;

public class User
{
    public Guid Id { get; set; }
    public string Username { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public string DeviceId { get; set; } = null!;
    public DateTime RegistrationDate { get; set; }
    public int PlayerLevel { get; set; } = 1;
    public int TrophyCount { get; set; } = 0;

}