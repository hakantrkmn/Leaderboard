using System.Reflection;
using Leaderboard.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Leaderboard.Extensions;

public static class SwaggerExtensions
{
	public static IServiceCollection AddSwaggerWithJwtAndIdempotency(this IServiceCollection services)
	{
		services.AddEndpointsApiExplorer();
		services.AddSwaggerGen(c =>
		{
			c.SwaggerDoc("v1", new OpenApiInfo 
			{ 
				Title = "Leaderboard API - Multi-Game Mode Support", 
				Version = "v1",
				Description = "Scalable leaderboard API supporting Classic and Tournament game modes with real-time ranking"
			});
			
			c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
			{
				Name = "Authorization",
				In = ParameterLocation.Header,
				Type = SecuritySchemeType.Http,
				Scheme = "bearer",
				BearerFormat = "JWT",
				Description = "Enter: Bearer {your JWT token}"
			});
			
			c.AddSecurityRequirement(new OpenApiSecurityRequirement
			{
				{
					new OpenApiSecurityScheme
					{
						Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
					},
					Array.Empty<string>()
				}
			});
			
			c.MapType<Leaderboard.LeaderBoard.Models.GameMode>(() => new OpenApiSchema
			{
				Type = "string",
				Enum = new List<IOpenApiAny>
				{
					new OpenApiString("Classic"),
					new OpenApiString("Tournament")
				},
				Description = "Game mode: Classic for standard play, Tournament for competitive events"
			});
			
			c.OperationFilter<IdempotencyHeaderOperationFilter>();
			c.OperationFilter<TimestampHeaderOperationFilter>();
			c.OperationFilter<GameModeOperationFilter>();
		});
		return services;
	}
}


public class IdempotencyHeaderOperationFilter : IOperationFilter
{
	public void Apply(OpenApiOperation operation, OperationFilterContext context)
	{
		var hasIdempotency = context.MethodInfo.GetCustomAttribute<IdempotencyAttribute>() != null
			|| context.MethodInfo.DeclaringType?.GetCustomAttribute<IdempotencyAttribute>() != null;
		if (!hasIdempotency) return;

		operation.Parameters ??= new List<OpenApiParameter>();
		operation.Parameters.Add(new OpenApiParameter
		{
			Name = "Idempotency-Key",
			In = ParameterLocation.Header,
			Required = true,
			Schema = new OpenApiSchema { Type = "string", Format = "uuid" },
			Description = "Client-generated UUID to guarantee idempotent processing"
		});
	}
}

public class GameModeOperationFilter : IOperationFilter
{
	public void Apply(OpenApiOperation operation, OperationFilterContext context)
	{
		if (context.MethodInfo.DeclaringType?.Name == "LeaderboardController")
		{
			var methodName = context.MethodInfo.Name;
			
			switch (methodName)
			{
				case "Submit":
					operation.Summary = "Submit match score for specific game mode";
					operation.Description = "Submit a player's score for either Classic or Tournament mode. Each mode maintains separate leaderboards.";
					break;
					
				case "Top":
					operation.Summary = "Get top players for specific game mode";
					operation.Description = "Retrieve the highest-scoring players for the specified game mode. Results are cached for optimal performance.";
					break;
					
				case "Me":
					operation.Summary = "Get my ranking in specific game mode";
					operation.Description = "Get the authenticated user's current rank and score in the specified game mode.";
					break;
					
				case "AroundMe":
					operation.Summary = "Get players around my ranking";
					operation.Description = "Get players ranked above and below the authenticated user in the specified game mode.";
					break;
			}
		}
	}
}

public class TimestampHeaderOperationFilter : IOperationFilter
{
	public void Apply(OpenApiOperation operation, OperationFilterContext context)
	{
		// TimestampValidation attribute'u olan method'lara timestamp header ekle
		var hasTimestampValidation = context.MethodInfo.GetCustomAttributes(typeof(Leaderboard.Filters.TimestampValidationAttribute), false).Any();
		
		if (hasTimestampValidation)
		{
			operation.Parameters ??= new List<OpenApiParameter>();
			
			operation.Parameters.Add(new OpenApiParameter
			{
				Name = "X-Timestamp",
				In = ParameterLocation.Header,
				Required = true,
				Description = "UTC timestamp in Unix seconds format (e.g., 1642694400). Used for replay attack protection.",
				Schema = new OpenApiSchema
				{
					Type = "integer",
					Format = "int64",
					Example = new OpenApiLong(DateTimeOffset.UtcNow.ToUnixTimeSeconds())
				}
			});
		}
	}
}
