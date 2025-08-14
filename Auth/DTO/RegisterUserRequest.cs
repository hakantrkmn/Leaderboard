using System.ComponentModel.DataAnnotations;

namespace Leaderboard.Auth.DTO;

public class RegisterUserRequest
{
	[Required, MinLength(3), MaxLength(50)]
	public string Username { get; set; } = null!;

	[Required, MinLength(6), MaxLength(100)]
	public string Password { get; set; } = null!;

	[Required, MaxLength(128)]
	public string DeviceId { get; set; } = null!;
}