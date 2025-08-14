namespace Leaderboard.LeaderBoard.DTO;

public record LeaderboardEntryResponse(Guid UserId, long Score, int Rank);


