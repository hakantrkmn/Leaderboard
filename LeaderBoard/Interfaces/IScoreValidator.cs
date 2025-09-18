using Leaderboard.LeaderBoard.DTO;

namespace Leaderboard.LeaderBoard.Interfaces;

public interface IScoreValidator
{
    Task ValidateAsync(Guid userId, SubmitMatchRequest request, CancellationToken ct = default);
}
