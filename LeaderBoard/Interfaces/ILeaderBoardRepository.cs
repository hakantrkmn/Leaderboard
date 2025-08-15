using Leaderboard.LeaderBoard.Models;

namespace Leaderboard.LeaderBoard.Interfaces;

public interface ILeaderboardRepository
{
	Task<LeaderboardEntry?> GetByUserIdAsync(Guid userId, GameMode gameMode, CancellationToken ct = default);
	Task UpsertAsync(LeaderboardEntry entry, CancellationToken ct = default);
	Task<List<LeaderboardEntry>> GetTopAsync(GameMode gameMode, int n, CancellationToken ct = default);
	Task<int?> GetUserRankAsync(Guid userId, GameMode gameMode, CancellationToken ct = default);
	Task<List<LeaderboardAroundRow>> GetAroundMeAsync(Guid userId, GameMode gameMode, int k, CancellationToken ct = default);
}


