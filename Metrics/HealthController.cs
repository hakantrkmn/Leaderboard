using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using Leaderboard.DB;

namespace Leaderboard.Metrics;

[ApiController]
[Route("[controller]")]
public class HealthController : ControllerBase
{
    private readonly DBContext _dbContext;
    private readonly IConnectionMultiplexer? _redis;

    public HealthController(DBContext dbContext, IConnectionMultiplexer? redis = null)
    {
        _dbContext = dbContext;
        _redis = redis;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var health = new
        {
            status = "healthy",
            timestamp = DateTime.UtcNow,
            services = new
            {
                database = await CheckDatabaseAsync(),
                redis = await CheckRedisAsync()
            }
        };

        var isHealthy = health.services.database && health.services.redis;
        return isHealthy ? Ok(health) : StatusCode(503, health);
    }

    private async Task<bool> CheckDatabaseAsync()
    {
        try
        {
            await _dbContext.Database.CanConnectAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    private async Task<bool> CheckRedisAsync()
    {
        if (_redis == null)
        {
            return false;
        }

        try
        {
            var db = _redis.GetDatabase();
            await db.PingAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }
}
