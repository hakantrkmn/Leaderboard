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
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            var response = await _authService.LoginAsync(request);
            return Ok(new { 
                success = true, 
                message = "Login successful",
                data = response 
            });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { 
                success = false, 
                message = ex.Message 
            });
        }
        catch
        {
            return BadRequest(new { 
                success = false, 
                message = "Login failed" 
            });
        }
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterUserRequest request)
    {
        try
        {
            var response = await _authService.RegisterAsync(request);
            return Ok(new { 
                success = true, 
                message = "Registration successful",
                data = response 
            });
        }
        catch (UnauthorizedAccessException ex)
        {
            return BadRequest(new { 
                success = false, 
                message = ex.Message 
            });
        }
        catch
        {
            return BadRequest(new { 
                success = false, 
                message = "Registration failed" 
            });
        }
    }
}