using Microsoft.EntityFrameworkCore;
using Leaderboard.DB;
using Leaderboard.LeaderBoard.Interfaces;
using Leaderboard.LeaderBoard.Models;
using System.Data;

namespace Leaderboard.LeaderBoard.Repositories;

public class EfLeaderboardRepository : ILeaderboardRepository
{
	private readonly DBContext _db;

	public EfLeaderboardRepository(DBContext db) => _db = db;

	public Task<LeaderboardEntry?> GetByUserIdAsync(Guid userId, GameMode gameMode, CancellationToken ct = default) =>
		_db.Leaderboard.AsNoTracking().FirstOrDefaultAsync(e => e.UserId == userId && e.GameMode == gameMode, ct);

	public async Task UpsertAsync(LeaderboardEntry entry, CancellationToken ct = default)
	{
		var existing = await _db.Leaderboard.FirstOrDefaultAsync(e => e.UserId == entry.UserId && e.GameMode == entry.GameMode, ct);
		if (existing is null)
		{
			_db.Leaderboard.Add(entry);
		}
		else
		{
			existing.Score = entry.Score;
			existing.UpdatedAtUtc = entry.UpdatedAtUtc;
			existing.PlayerLevel = entry.PlayerLevel;
			existing.TrophyCount = entry.TrophyCount;
		}
		await _db.SaveChangesAsync(ct);
	}

    public async Task<List<LeaderboardEntry>> GetTopAsync(GameMode gameMode, int n, CancellationToken ct = default)
	{
        return await _db.Leaderboard
            .AsNoTracking()
            .Where(e => e.GameMode == gameMode)
            .OrderByDescending(e => e.Score)
            .ThenBy(e => e.RegistrationDateUtc)
            .ThenByDescending(e => e.PlayerLevel)
            .ThenByDescending(e => e.TrophyCount)
            .Take(n)
            .ToListAsync(ct);
	}

	public async Task<int?> GetUserRankAsync(Guid userId, GameMode gameMode, CancellationToken ct = default)
	{
		var target = await _db.Leaderboard.AsNoTracking().FirstOrDefaultAsync(e => e.UserId == userId && e.GameMode == gameMode, ct);
		if (target is null) return null;
        var higher = await _db.Leaderboard.AsNoTracking()
            .Where(e => e.GameMode == gameMode)
            .CountAsync(e =>
                e.Score > target.Score ||
                (e.Score == target.Score && e.RegistrationDateUtc < target.RegistrationDateUtc) ||
                (e.Score == target.Score && e.RegistrationDateUtc == target.RegistrationDateUtc && e.PlayerLevel > target.PlayerLevel) ||
                (e.Score == target.Score && e.RegistrationDateUtc == target.RegistrationDateUtc && e.PlayerLevel == target.PlayerLevel && e.TrophyCount > target.TrophyCount)
            , ct);
        return higher + 1;
	}

	public async Task<List<LeaderboardAroundRow>> GetAroundMeAsync(Guid userId, GameMode gameMode, int k, CancellationToken ct = default)
    {
        var sql = @"WITH ranked AS (
  SELECT
    ""UserId"",
    ""Score"",
    ""RegistrationDateUtc"",
    ""PlayerLevel"",
    ""TrophyCount"",
    ROW_NUMBER() OVER (
      ORDER BY ""Score"" DESC, ""RegistrationDateUtc"" ASC, ""PlayerLevel"" DESC, ""TrophyCount"" DESC
    ) AS rn
  FROM ""Leaderboard""
  WHERE ""GameMode"" = @p1
),
me AS (
  SELECT rn AS my_rn FROM ranked WHERE ""UserId"" = @p0
)
SELECT r.""UserId"" AS ""UserId"", r.""Score"" AS ""Score"", r.rn AS ""Rn""
FROM ranked r CROSS JOIN me
WHERE r.rn BETWEEN GREATEST(me.my_rn - @p2, 1) AND me.my_rn + @p2
ORDER BY r.rn;";

        return await _db.LeaderboardAroundRows
            .FromSqlRaw(sql, userId, (int)gameMode, k)
            .AsNoTracking()
            .ToListAsync(ct);
    }

   
}


