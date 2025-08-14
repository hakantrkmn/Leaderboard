using Leaderboard.Users.DTO;
using Leaderboard.Users.Services;
using Microsoft.AspNetCore.Mvc;

namespace Leaderboard.Users.Controllers;

[ApiController]
[Route("api/users")]
public class UserController : ControllerBase
{
    private readonly UsersService _usersService;

    public UserController(UsersService usersService)
    {
        _usersService = usersService;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<UserResponse>> GetUserById(Guid id, CancellationToken ct = default)
    {
        var user = await _usersService.GetUserById(id, ct);
        if (user is null)
        {
            return NotFound();
        }
        return Ok(user);
    }

    [HttpPost("login")]
    public async Task<ActionResult<UserResponse>> Login(LoginRequest request, CancellationToken ct = default)
    {
        var user = await _usersService.LoginAsync(request, ct);
        if (user is null)
        {
            return Unauthorized();
        }
        return Ok(user);
    }

    [HttpPost("register")]
    public async Task<ActionResult<UserResponse>> Register(RegisterUserRequest request, CancellationToken ct = default)
    {
        var user = await _usersService.RegisterAsync(request, ct);
        if (user is null)
        {
            return BadRequest();
        }
        return Ok(user);
    }
}