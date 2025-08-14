# Multi-stage build for .NET 8 application
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project file and restore dependencies
COPY ["Leaderboard.csproj", "./"]
RUN dotnet restore "Leaderboard.csproj"

# Copy source code and build
COPY . .
RUN dotnet build "Leaderboard.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "Leaderboard.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Migration stage (optional - if you want separate migration)
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS migration
WORKDIR /src
COPY ["Leaderboard.csproj", "./"]
RUN dotnet restore "Leaderboard.csproj"
COPY . .
RUN dotnet ef migrations list --project Leaderboard.csproj

# Final runtime stage
FROM base AS final
WORKDIR /app

# Copy published application
COPY --from=publish /app/publish .

# Create non-root user for security
RUN adduser --disabled-password --gecos "" appuser && chown -R appuser /app
USER appuser

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD curl -f http://localhost/health || exit 1

# Start the application
ENTRYPOINT ["dotnet", "Leaderboard.dll"]
