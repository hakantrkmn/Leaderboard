using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Leaderboard.LeaderBoard.DTO;
using Leaderboard.LeaderBoard.Interfaces;
using Leaderboard.LeaderBoard.Models;
using Leaderboard.Filters;
using Microsoft.AspNetCore.RateLimiting;
using Leaderboard.Scripts;
using Leaderboard.Metrics;

namespace Leaderboard.LeaderBoard.Controllers;

[ApiController]
[Route("leaderboard")]
public class LeaderboardController : ControllerBase
{
	private readonly ILeaderboardService _service;
    private readonly IScriptEngineService _scriptEngine;

	public LeaderboardController(ILeaderboardService service, IScriptEngineService scriptEngine)
	{
		_service = service;
		_scriptEngine = scriptEngine;
	}

    [Authorize]
    [HttpPost("submit")]
	[Idempotency(ttlSeconds: 300, headerName: "Idempotency-Key")]
	[TimestampValidation(maxAgeMinutes: 10, maxFutureMinutes: 2)]
	[EnableRateLimiting("submit")]
	public async Task<IActionResult> Submit([FromBody] SubmitMatchRequest request, CancellationToken ct)
	{
		long? bonus = null;
		try 
		{
			var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
			if (!Guid.TryParse(userIdStr, out var userId)) 
				return Unauthorized(new { success = false, message = "Invalid user token" });
			//check if current date is weekend

				if(request.Bonus is not null) 
				{
					foreach (var bonusType in request.Bonus)
					{
						if(_scriptEngine.HasFunction("calculator", bonusType))
						{
							var bonusAmount = _scriptEngine.ExecuteScript<int>("calculator", bonusType, request.Score);
							bonus = bonusAmount; // bonus değişkenini set et
							request.Score = bonusAmount;
							AppMetrics.BonusUsageTotal.WithLabels(bonusType, request.GameMode.ToString()).Inc();
							AppMetrics.BonusAmountHistogram.WithLabels(bonusType, request.GameMode.ToString()).Observe(bonusAmount);
						}
					}
				}
				
			await _service.SubmitAsync(userId, request, ct);
			return Ok(new { 
				success = true, 
				message = $"Score {request.Score} successfully submitted for {request.GameMode} mode",
				bonus = bonus ?? 0,
				data = new { 
					score = request.Score, 
					gameMode = request.GameMode.ToString(),
					submittedAt = DateTime.UtcNow 
				}
			});
		}
		catch (Exception ex)
		{
			return BadRequest(new { success = false, message = ex.Message });
		}
	}

	[HttpGet("top/{gameMode}")]
	[ActionMetric]
	[EnableRateLimiting("top")]
	public async Task<IActionResult> Top(
		GameMode gameMode, 
		[FromQuery] int n = 100, 
		CancellationToken ct = default)
	{
		try {
			n = Math.Clamp(n, 1, 1000);
			var top = await _service.GetTopAsync(gameMode, n, ct);
			return Ok(new { 
				success = true, 
				message = $"Top {top?.Count ?? 0} players retrieved for {gameMode} mode",
				data = top,
				metadata = new { 
					gameMode = gameMode.ToString(), 
					requestedCount = n,
					actualCount = top?.Count ?? 0
				}
			});
		}
		catch (Exception ex)
		{
			return StatusCode(500, new { success = false, message = "Failed to get top scores", error = ex.Message });
		}
	}

	[Authorize]
	[HttpGet("me/{gameMode}")]
	public async Task<IActionResult> Me(GameMode gameMode, CancellationToken ct)
	{
		try 
		{
			var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
			if (!Guid.TryParse(userIdStr, out var userId)) 
				return Unauthorized(new { success = false, message = "Invalid user token" });
			
			var me = await _service.GetMyStandingAsync(userId, gameMode, ct);
			if (me is null)
				return Ok(new { 
					success = false, 
					message = $"No ranking found for user in {gameMode} mode",
					data = (object?)null
				});
			
			return Ok(new { 
				success = true, 
				message = $"Your ranking in {gameMode} mode: #{me.Rank}",
				data = me,
				metadata = new { gameMode = gameMode.ToString() }
			});
		}
		catch (Exception ex)
		{
			return StatusCode(500, new { success = false, message = "Failed to get user ranking", error = ex.Message });
		}
	}

	[Authorize]
	[HttpGet("around-me/{gameMode}")]
	public async Task<IActionResult> AroundMe(
		GameMode gameMode, 
		[FromQuery] int k = 5, 
		CancellationToken ct = default)
	{
		try 
		{
			var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
			if (!Guid.TryParse(userIdStr, out var userId)) 
				return Unauthorized(new { success = false, message = "Invalid user token" });
			
			k = Math.Clamp(k, 1, 50);
			var res = await _service.GetAroundMeAsync(userId, gameMode, k, ct);
			return Ok(new { 
				success = true, 
				message = $"Found {res.Count} players around your position in {gameMode} mode",
				data = res,
				metadata = new { 
					gameMode = gameMode.ToString(), 
					range = k,
					foundCount = res.Count 
				}
			});
		}
		catch (Exception ex)
		{
			return StatusCode(500, new { success = false, message = "Failed to get players around you", error = ex.Message });
		}
	}




}


