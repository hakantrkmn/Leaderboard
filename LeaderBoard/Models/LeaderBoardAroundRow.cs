namespace Leaderboard.LeaderBoard.Models;

// Keyless projection for raw SQL around-me query
public class LeaderboardAroundRow
{
	public Guid UserId { get; set; }
	public long Score { get; set; }
	public int Rn { get; set; }
}


