using System.ComponentModel.DataAnnotations;

namespace Leaderboard.Users.DTO;

public class LoginRequest
{
	[Required, MinLength(3), MaxLength(50)]
	public string Username { get; set; } = null!;

	[Required, MinLength(6), MaxLength(100)]
	public string Password { get; set; } = null!;
}


