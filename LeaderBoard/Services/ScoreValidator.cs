using System.ComponentModel.DataAnnotations;
using Leaderboard.LeaderBoard.Interfaces;
using Leaderboard.LeaderBoard.DTO;
using LeaderBoard.Settings;
using Microsoft.Extensions.Options;

namespace Leaderboard.LeaderBoard.Services;

public class ScoreValidator : IScoreValidator
{
    private readonly ILeaderboardRepository _repo;
    private readonly GameSettings _gameSettings;
    private readonly ILogger<ScoreValidator> _logger;

    public ScoreValidator(ILeaderboardRepository repo, IOptions<GameSettings> gameSettings, ILogger<ScoreValidator> logger)
    {
        _repo = repo;
        _gameSettings = gameSettings.Value;
        _logger = logger;
    }

    public async Task ValidateAsync(Guid userId, SubmitMatchRequest request, CancellationToken ct)
    {
        var currentEntry = await _repo.GetByUserIdAsync(userId, request.GameMode, ct);
        var currentScore = currentEntry?.Score ?? 0;

        var maxIncrease = CalculateMaxAllowedIncrease(currentScore);
        if (request.Score > currentScore + maxIncrease)
        {
            _logger.LogWarning("Suspicious score increase detected for user {UserId}: {CurrentScore} -> {NewScore}",
                userId, currentScore, request.Score);
            throw new ValidationException($"Score increase too dramatic: {currentScore} -> {request.Score}. Max allowed: {maxIncrease}");
        }

        var lastSubmission = await GetLastSubmissionTime(currentEntry);
        if (lastSubmission.HasValue && DateTime.UtcNow - lastSubmission.Value < TimeSpan.FromMinutes(_gameSettings.SubmissionCooldownMinutes))
        {
            throw new ValidationException($"Too frequent score submissions. Please wait at least {_gameSettings.SubmissionCooldownMinutes} minute between submissions.");
        }
    }

    private long CalculateMaxAllowedIncrease(long currentScore)
    {
        var sortedTiers = _gameSettings.ScoreIncreaseTiers.OrderBy(t => t.Threshold).ToList();

        if (!sortedTiers.Any() || currentScore < sortedTiers.First().Threshold)
        {
            return _gameSettings.MaxAllowedIncreaseBase;
        }

        var applicableTier = _gameSettings.ScoreIncreaseTiers
            .OrderByDescending(t => t.Threshold)
            .First(t => currentScore >= t.Threshold);

        return (long)(currentScore * applicableTier.Factor);
    }

    private Task<DateTime?> GetLastSubmissionTime(Models.LeaderboardEntry? entry)
    {
        return Task.FromResult(entry?.UpdatedAtUtc);
    }
}
