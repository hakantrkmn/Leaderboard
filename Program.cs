using DotNetEnv;
using Leaderboard.Extensions;
using Leaderboard.DB;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;
using Prometheus;
using System.Text.Json.Serialization;

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
})
.AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    options.JsonSerializerOptions.PropertyNamingPolicy = null; 
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



app.UseMiddleware<Leaderboard.Middleware.StructuredLoggingMiddleware>();


app.UsePipeline();
app.Run();

