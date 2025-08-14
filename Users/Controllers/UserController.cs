using Leaderboard.Users.DTO;
using Leaderboard.Users.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

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
    [Authorize]
    public async Task<ActionResult<UserResponse>> GetUserById(Guid id, CancellationToken ct = default)
    {
        var user = await _usersService.GetUserById(id, ct);
        if (user is null)
        {
            return NotFound();
        }
        return Ok(user);
    }

}