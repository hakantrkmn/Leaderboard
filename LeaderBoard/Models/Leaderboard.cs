namespace Leaderboard.LeaderBoard.Models;

public enum GameMode
{
	Classic = 1,
	Tournament = 2
}

public class LeaderboardEntry
{
	public Guid UserId { get; set; }
	public long Score { get; set; }
	public DateTime UpdatedAtUtc { get; set; }
	public DateTime RegistrationDateUtc { get; set; }
	public int PlayerLevel { get; set; }
	public int TrophyCount { get; set; }
	
	public GameMode GameMode { get; set; } = GameMode.Classic;
}


