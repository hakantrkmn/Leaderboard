using DotNetEnv;
using Leaderboard.Extensions;
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
builder.Services.AddUsersModule(builder.Configuration);
builder.Services.AddAuthModule(builder.Configuration);
builder.Services.AddSwaggerWithJwtAndIdempotency();

builder.Services.AddJwtAuth(builder.Configuration);

var app = builder.Build();
app.UsePipeline();
app.Run();

