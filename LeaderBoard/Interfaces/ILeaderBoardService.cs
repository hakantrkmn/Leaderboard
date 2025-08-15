using Leaderboard.LeaderBoard.DTO;

namespace Leaderboard.LeaderBoard.Interfaces;

public interface ILeaderboardService
{
	Task SubmitAsync(Guid userId, SubmitMatchRequest request, CancellationToken ct = default);
	Task<IReadOnlyList<LeaderboardEntryResponse>?> GetTopAsync(int n, CancellationToken ct = default);
	Task<LeaderboardEntryResponse?> GetMyStandingAsync(Guid userId, CancellationToken ct = default);

		Task<IReadOnlyList<LeaderboardEntryResponse>> GetAroundMeAsync(Guid userId, int k, CancellationToken ct = default);

}


