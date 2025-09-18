using System.Text.Json;
using StackExchange.Redis;
using Leaderboard.Users.Interfaces;
using Leaderboard.Users.Models;
using Microsoft.EntityFrameworkCore;
using Leaderboard.DB;
using Leaderboard.LeaderBoard.DTO;
using Leaderboard.LeaderBoard.Interfaces;
using Leaderboard.LeaderBoard.Models;
using System.ComponentModel.DataAnnotations;
using Leaderboard.Metrics;
using Microsoft.Extensions.Options;
using LeaderBoard.Settings;

namespace Leaderboard.LeaderBoard.Services;

public class LeaderboardService : ILeaderboardService
{
    private readonly ILeaderboardRepository _repo;
    private readonly IDatabase _redis;
    private readonly IUserRepository _userRepository;
    private readonly DBContext _db;
    private readonly ILogger<LeaderboardService> _logger;
    private readonly GameSettings _gameSettings;
    private readonly IScoreValidator _scoreValidator;

    public LeaderboardService(
        ILeaderboardRepository repo, 
        IConnectionMultiplexer mux, 
        IUserRepository userRepository, 
        DBContext db,
        ILogger<LeaderboardService> logger,
        IOptions<GameSettings> gameSettings,
        IScoreValidator scoreValidator)
    {
        _repo = repo;
        _redis = mux.GetDatabase();
        _userRepository = userRepository;
        _db = db;
        _logger = logger;
        _gameSettings = gameSettings.Value;
        _scoreValidator = scoreValidator;
    }

	public async Task SubmitAsync(Guid userId, SubmitMatchRequest request, CancellationToken ct = default)
	{
        await _scoreValidator.ValidateAsync(userId, request, ct);
        
        await using var tx = await _db.Database.BeginTransactionAsync(ct);
        try
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId, ct);
            if (user is null) throw new InvalidOperationException("User not found");

            if (request.PlayerLevel.HasValue) user.PlayerLevel = request.PlayerLevel.Value;
            if (request.TrophyCount.HasValue) user.TrophyCount = request.TrophyCount.Value;
            await _db.SaveChangesAsync(ct);

            var entry = new LeaderboardEntry
            {
                UserId = userId,
                Score = request.Score,
                UpdatedAtUtc = DateTime.UtcNow,
                RegistrationDateUtc = user.RegistrationDate,
                PlayerLevel = user.PlayerLevel,
                TrophyCount = user.TrophyCount,
                GameMode = request.GameMode
            };
            await _repo.UpsertAsync(entry, ct);
            
            await tx.CommitAsync(ct);
            
            await _redis.KeyDeleteAsync($"lb:top:{request.GameMode}:{_gameSettings.TopCacheSize}");
            
            var scoreRange = GetScoreRange((int)request.Score);
            AppMetrics.ScoreSubmissionsTotal.WithLabels(userId.ToString(), scoreRange).Inc();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Transaction failed while submitting score for user {UserId}. Rolling back.", userId);
            await tx.RollbackAsync(ct);
            throw;
        }
	}

    private string GetScoreRange(int score)
    {
        for (int i = 0; i < _gameSettings.ScoreRanges.Length; i++)
        {
            if (score < _gameSettings.ScoreRanges[i])
            {
                // Assuming ScoreRangeLabels has one more element than ScoreRanges for the "else" case
                if (i < _gameSettings.ScoreRangeLabels.Length)
                    return _gameSettings.ScoreRangeLabels[i];
            }
        }
        
        // Return the last label for scores greater than or equal to the last range
        if (_gameSettings.ScoreRangeLabels.Length > _gameSettings.ScoreRanges.Length)
            return _gameSettings.ScoreRangeLabels[^1];

        // Fallback in case of configuration mismatch
        return "unknown";
    }

    public async Task<IReadOnlyList<LeaderboardEntryResponse>?> GetTopAsync(GameMode gameMode, int n, CancellationToken ct = default)
	{
        try {
        n = Math.Clamp(n, 1, _gameSettings.TopCacheSize);
        var cacheKey = $"lb:top:{gameMode}:{_gameSettings.TopCacheSize}";
        var cached = await _redis.StringGetAsync(cacheKey);
        if (cached.HasValue)
        {
            var cachedResult = JsonSerializer.Deserialize<List<LeaderboardEntryResponse>>(cached!);
            if (cachedResult is not null)
            {
                if (cachedResult.Count >= n) return cachedResult.GetRange(0, n);
                return cachedResult;
            }
        }

        var list = await _repo.GetTopAsync(gameMode, _gameSettings.TopCacheSize, ct);
        var responses = new List<LeaderboardEntryResponse>(list.Count);
        for (int i = 0; i < list.Count; i++)
            responses.Add(new LeaderboardEntryResponse(list[i].UserId, list[i].Score, i + 1));

        var json = JsonSerializer.Serialize(responses);
        await _redis.StringSetAsync(cacheKey, json, TimeSpan.FromSeconds(_gameSettings.DefaultCacheExpirationSeconds));
        return responses.Count >= n ? responses.GetRange(0, n) : responses;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            throw new Exception("Failed to get top scores", ex);
        }
	}

	public async Task<LeaderboardEntryResponse?> GetMyStandingAsync(Guid userId, GameMode gameMode, CancellationToken ct = default)
	{
		var rank = await _repo.GetUserRankAsync(userId, gameMode, ct);
		var entry = await _repo.GetByUserIdAsync(userId, gameMode, ct);
		return (rank is null || entry is null) ? null : new LeaderboardEntryResponse(userId, entry.Score, rank.Value);
	}

	public async Task<IReadOnlyList<LeaderboardEntryResponse>> GetAroundMeAsync(Guid userId, GameMode gameMode, int k, CancellationToken ct = default)
	{
		k = Math.Clamp(k, 1, _gameSettings.AroundMeMaxRank);
		var rows = await _repo.GetAroundMeAsync(userId, gameMode, k, ct);
		return rows.Select(r => new LeaderboardEntryResponse(r.UserId, r.Score, r.Rn)).ToList();
	}


}


