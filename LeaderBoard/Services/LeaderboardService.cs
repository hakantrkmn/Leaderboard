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

namespace Leaderboard.LeaderBoard.Services;

public class LeaderboardService : ILeaderboardService
{
    private readonly ILeaderboardRepository _repo;
    private readonly IDatabase _redis;
    private readonly IUserRepository _userRepository;
    private readonly DBContext _db;
    private readonly ILogger<LeaderboardService> _logger;
	private const int TopCacheSize = 100;

    public LeaderboardService(
        ILeaderboardRepository repo, 
        IConnectionMultiplexer mux, 
        IUserRepository userRepository, 
        DBContext db,
        ILogger<LeaderboardService> logger)
    {
        _repo = repo;
        _redis = mux.GetDatabase();
        _userRepository = userRepository;
        _db = db;
        _logger = logger;
    }

	public async Task SubmitAsync(Guid userId, SubmitMatchRequest request, CancellationToken ct = default)
	{
        await ValidateScoreSubmission(userId, request, ct);
        
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
            
            await _redis.KeyDeleteAsync($"lb:top:{request.GameMode}:{TopCacheSize}");
            
            var scoreRange = GetScoreRange((int)request.Score);
            AppMetrics.ScoreSubmissionsTotal.WithLabels(userId.ToString(), scoreRange).Inc();
        }
        catch
        {
            throw;
        }
	}

    private static string GetScoreRange(int score)
    {
        return score switch
        {
            < 1000 => "0-999",
            < 5000 => "1000-4999",
            < 10000 => "5000-9999",
            < 50000 => "10000-49999",
            < 100000 => "50000-99999",
            _ => "100000+"
        };
    }

    public async Task<IReadOnlyList<LeaderboardEntryResponse>?> GetTopAsync(GameMode gameMode, int n, CancellationToken ct = default)
	{
        try {
        n = Math.Clamp(n, 1, TopCacheSize);
        var cacheKey = $"lb:top:{gameMode}:{TopCacheSize}";
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

        var list = await _repo.GetTopAsync(gameMode, TopCacheSize, ct);
        var responses = new List<LeaderboardEntryResponse>(list.Count);
        for (int i = 0; i < list.Count; i++)
            responses.Add(new LeaderboardEntryResponse(list[i].UserId, list[i].Score, i + 1));

        var json = JsonSerializer.Serialize(responses);
        await _redis.StringSetAsync(cacheKey, json, TimeSpan.FromSeconds(30));
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

    private async Task ValidateScoreSubmission(Guid userId, SubmitMatchRequest request, CancellationToken ct)
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

        var lastSubmission = await GetLastSubmissionTime(userId, request.GameMode, ct);
        if (lastSubmission.HasValue && DateTime.UtcNow - lastSubmission.Value < TimeSpan.FromMinutes(1))
        {
            throw new ValidationException("Too frequent score submissions. Please wait at least 1 minute between submissions.");
        }

    }

    private long CalculateMaxAllowedIncrease(long currentScore)
    {
        if (currentScore < 1000) return 500;
        if (currentScore < 10000) return (long)(currentScore * 0.5); 
        if (currentScore < 100000) return (long)(currentScore * 0.3); 
        return (long)(currentScore * 0.2); 
    }

    private async Task<DateTime?> GetLastSubmissionTime(Guid userId, GameMode gameMode, CancellationToken ct)
    {
        var entry = await _repo.GetByUserIdAsync(userId, gameMode, ct);
        return entry?.UpdatedAtUtc;
    }
	public async Task<IReadOnlyList<LeaderboardEntryResponse>> GetAroundMeAsync(Guid userId, GameMode gameMode, int k, CancellationToken ct = default)
	{
		k = Math.Clamp(k, 1, 50);
		var rows = await _repo.GetAroundMeAsync(userId, gameMode, k, ct);
		return rows.Select(r => new LeaderboardEntryResponse(r.UserId, r.Score, r.Rn)).ToList();
	}


}


