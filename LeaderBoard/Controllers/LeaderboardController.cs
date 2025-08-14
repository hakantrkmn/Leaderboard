using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Leaderboard.LeaderBoard.DTO;
using Leaderboard.LeaderBoard.Interfaces;
using Leaderboard.Filters;

namespace Leaderboard.LeaderBoard.Controllers;

[ApiController]
[Route("leaderboard")]
public class LeaderboardController : ControllerBase
{
	private readonly ILeaderboardService _service;

	public LeaderboardController(ILeaderboardService service)
	{
		_service = service;
	}

    [Authorize]
    [HttpPost("submit")]
	[Idempotency(ttlSeconds: 30, headerName: "Idempotency-Key")]
	public async Task<IActionResult> Submit([FromBody] SubmitMatchRequest request, CancellationToken ct)
	{
		var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
		if (!Guid.TryParse(userIdStr, out var userId)) return Unauthorized();
		await _service.SubmitAsync(userId, request, ct);
		return NoContent();
	}

	[HttpGet("top")]
	public async Task<ActionResult<IReadOnlyList<LeaderboardEntryResponse>>> Top([FromQuery] int n = 100, CancellationToken ct = default)
	{
		try {
			n = Math.Clamp(n, 1, 1000);
			var top = await _service.GetTopAsync(n, ct);
			return Ok(top);
		}
		catch (Exception ex)
		{
			Console.WriteLine(ex.Message);
			return StatusCode(500, "Failed to get top scores");
		}
	}

	[Authorize]
	[HttpGet("me")]
	public async Task<ActionResult<LeaderboardEntryResponse>> Me(CancellationToken ct)
	{
		var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
		if (!Guid.TryParse(userIdStr, out var userId)) return Unauthorized();
		var me = await _service.GetMyStandingAsync(userId, ct);
		return me is null ? NotFound() : Ok(me);
	}




}


