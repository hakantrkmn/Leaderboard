using Microsoft.AspNetCore.Mvc;
using Leaderboard.Auth.Interfaces;
using Leaderboard.Auth.DTO;
using Microsoft.AspNetCore.RateLimiting;

namespace Leaderboard.Auth.Controller;

[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    [EnableRateLimiting("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var response = await _authService.LoginAsync(request);
        if (response is null)
            return Unauthorized();
        return Ok(response);
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterUserRequest request)
    {
        var response = await _authService.RegisterAsync(request);
        if (response is null)
            return BadRequest();
        return Ok(response);
    }
}