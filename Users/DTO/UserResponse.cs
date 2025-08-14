namespace Leaderboard.Users.DTO;

public record UserResponse(Guid Id, string Username, DateTime RegistrationDate, int PlayerLevel, int TrophyCount);