using Leaderboard.LeaderBoard.Models;

namespace Leaderboard.LeaderBoard.Interfaces;

public interface ILeaderboardRepository
{
	Task<LeaderboardEntry?> GetByUserIdAsync(Guid userId, CancellationToken ct = default);
	Task UpsertAsync(LeaderboardEntry entry, CancellationToken ct = default);
	Task<List<LeaderboardEntry>> GetTopAsync(int n, CancellationToken ct = default);
	Task<int?> GetUserRankAsync(Guid userId, CancellationToken ct = default);

	Task<List<LeaderboardAroundRow>> GetAroundMeAsync(Guid userId, int k, CancellationToken ct = default);
}


