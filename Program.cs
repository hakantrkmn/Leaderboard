using DotNetEnv;
using Leaderboard.Extensions;
using Leaderboard.DB;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;
using System.Text.Json.Serialization;

try
{
    if (File.Exists(".env"))
    {
        Env.Load();
        Console.WriteLine("✅ .env file loaded successfully");
    }
    else
    {
        Console.WriteLine("ℹ️ .env file not found, using default configuration from appsettings");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"⚠️ Error loading .env file: {ex.Message}");
    Console.WriteLine("ℹ️ Continuing with default configuration");
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

try
{
    builder.Services.AddRedis(builder.Configuration);
    Console.WriteLine("✅ Redis services registered successfully");
}
catch (Exception ex)
{
    Console.WriteLine($"❌ Redis service registration failed: {ex.Message}");
    Console.WriteLine($"🔍 Redis connection string: {builder.Configuration.GetConnectionString("Redis")}");
}
builder.Services.AddPostgre(builder.Configuration);
builder.Services.AddAllModules(builder.Configuration);
builder.Services.AddRateLimitingPolicies();
builder.Services.AddSwaggerWithJwtAndIdempotency();

builder.Services.AddJwtAuth(builder.Configuration);

var app = builder.Build();


await DBMigration.EnsureDatabaseMigrated(app.Services);

app.UseMiddleware<Leaderboard.Middleware.StructuredLoggingMiddleware>();

app.UsePipeline();
app.Run();
