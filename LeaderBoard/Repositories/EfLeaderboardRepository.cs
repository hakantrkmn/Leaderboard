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

	public Task<LeaderboardEntry?> GetByUserIdAsync(Guid userId, CancellationToken ct = default) =>
		_db.Leaderboard.AsNoTracking().FirstOrDefaultAsync(e => e.UserId == userId, ct);

	public async Task UpsertAsync(LeaderboardEntry entry, CancellationToken ct = default)
	{
		var existing = await _db.Leaderboard.FirstOrDefaultAsync(e => e.UserId == entry.UserId, ct);
		if (existing is null)
		{
			_db.Leaderboard.Add(entry);
		}
		else
		{
			existing.Score = entry.Score;
			existing.UpdatedAtUtc = entry.UpdatedAtUtc;
		}
		await _db.SaveChangesAsync(ct);
	}

    public async Task<List<LeaderboardEntry>> GetTopAsync(int n, CancellationToken ct = default)
	{
        return await _db.Leaderboard
            .AsNoTracking()
            .OrderByDescending(e => e.Score)
            .ThenBy(e => e.RegistrationDateUtc)
            .ThenByDescending(e => e.PlayerLevel)
            .ThenByDescending(e => e.TrophyCount)
            .Take(n)
            .ToListAsync(ct);
	}

	public async Task<int?> GetUserRankAsync(Guid userId, CancellationToken ct = default)
	{
		var target = await _db.Leaderboard.AsNoTracking().FirstOrDefaultAsync(e => e.UserId == userId, ct);
		if (target is null) return null;
        var higher = await _db.Leaderboard.AsNoTracking()
            .CountAsync(e =>
                e.Score > target.Score ||
                (e.Score == target.Score && e.RegistrationDateUtc < target.RegistrationDateUtc) ||
                (e.Score == target.Score && e.RegistrationDateUtc == target.RegistrationDateUtc && e.PlayerLevel > target.PlayerLevel) ||
                (e.Score == target.Score && e.RegistrationDateUtc == target.RegistrationDateUtc && e.PlayerLevel == target.PlayerLevel && e.TrophyCount > target.TrophyCount)
            , ct);
        return higher + 1;
	}

   
}


