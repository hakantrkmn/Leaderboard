using DotNetEnv;
using Leaderboard.Extensions;
using Leaderboard.DB;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;
using Prometheus;

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
    .Enrich.WithProperty("Application", "LeaderboardAPI")
    .Enrich.WithProperty("Environment", builder.Environment.EnvironmentName)
    .Enrich.WithMachineName()
    .Enrich.WithThreadId()
    .WriteTo.Console(new CompactJsonFormatter())
    .WriteTo.File(new CompactJsonFormatter(), "logs/leaderboard-.json", 
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 7)
    .CreateLogger();

builder.Host.UseSerilog();

builder.Logging.ClearProviders();
builder.Services.AddControllers(options =>
{
    options.Filters.Add<Leaderboard.Filters.LoggingActionFilter>();
});


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddRedis(builder.Configuration);
builder.Services.AddPostgre(builder.Configuration);
builder.Services.AddAllModules(builder.Configuration);
builder.Services.AddRateLimitingPolicies();
builder.Services.AddSwaggerWithJwtAndIdempotency();

builder.Services.AddJwtAuth(builder.Configuration);

var app = builder.Build();

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

app.UseMiddleware<Leaderboard.Middleware.StructuredLoggingMiddleware>();


app.UsePipeline();
app.Run();

