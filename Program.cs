using DotNetEnv;
using Leaderboard.Extensions;
using Leaderboard.DB;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;

try
{
    Env.Load();
}
catch (Exception)
{
    Console.WriteLine("Error loading .env file");
    throw;
}


var builder = WebApplication.CreateBuilder(args);
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console(new CompactJsonFormatter())
    .CreateLogger();

builder.Host.UseSerilog();

builder.Logging.ClearProviders();
builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddRedis(builder.Configuration);
builder.Services.AddPostgre(builder.Configuration);
builder.Services.AddAllModules(builder.Configuration);
builder.Services.AddRateLimitingPolicies();
builder.Services.AddSwaggerWithJwtAndIdempotency();

builder.Services.AddJwtAuth(builder.Configuration);

var app = builder.Build();

// Apply database migrations
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<DBContext>();
    try
    {
        dbContext.Database.Migrate();
        Log.Information("Database migrations applied successfully");
    }
    catch (Exception ex)
    {
        Log.Error(ex, "An error occurred while applying database migrations");
        throw;
    }
}

app.UsePipeline();
app.Run();

