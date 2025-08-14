using System.ComponentModel.DataAnnotations;

namespace Leaderboard.LeaderBoard.DTO;

public class SubmitMatchRequest
{
	[Required]
	[Range(0, 1000000000)]
	public long Score { get; set; }
	public int? PlayerLevel { get; set; }
	public int? TrophyCount { get; set; }
}


