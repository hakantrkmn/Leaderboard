using Leaderboard.LeaderBoard.DTO;
using Leaderboard.LeaderBoard.Models;

namespace Leaderboard.LeaderBoard.Interfaces;

public interface ILeaderboardService
{
	Task SubmitAsync(Guid userId, SubmitMatchRequest request, CancellationToken ct = default);
	Task<IReadOnlyList<LeaderboardEntryResponse>?> GetTopAsync(GameMode gameMode, int n, CancellationToken ct = default);
	Task<LeaderboardEntryResponse?> GetMyStandingAsync(Guid userId, GameMode gameMode, CancellationToken ct = default);
	Task<IReadOnlyList<LeaderboardEntryResponse>> GetAroundMeAsync(Guid userId, GameMode gameMode, int k, CancellationToken ct = default);
}


