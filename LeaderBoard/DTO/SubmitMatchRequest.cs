using System.ComponentModel.DataAnnotations;
using Leaderboard.LeaderBoard.Models;

namespace Leaderboard.LeaderBoard.DTO;

public class SubmitMatchRequest
{
	[Required]
	[Range(0, 1000000000)]
	public long Score { get; set; }
	public int? PlayerLevel { get; set; }
	public int? TrophyCount { get; set; }

	public string[]? Bonus { get; set; }
	
	public GameMode GameMode { get; set; } = GameMode.Classic;
}


