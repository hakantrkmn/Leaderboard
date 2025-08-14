using System.Reflection;
using Leaderboard.Filters;
using Microsoft.Extensions.DependencyInjection;
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
			c.SwaggerDoc("v1", new OpenApiInfo { Title = "rune-case", Version = "v1" });
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
			
			c.OperationFilter<IdempotencyHeaderOperationFilter>();
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
