# Multi-stage build for .NET 8 application
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 443

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Install Node.js and npm
RUN apt-get update && apt-get install -y \
    curl \
    && curl -fsSL https://deb.nodesource.com/setup_18.x | bash - \
    && apt-get install -y nodejs \
    && apt-get clean \
    && rm -rf /var/lib/apt/lists/*

# Copy project file and restore dependencies
COPY ["Leaderboard.csproj", "./"]
RUN dotnet restore "Leaderboard.csproj"

# Copy source code and build
COPY . .

# Build TypeScript files
RUN npm install && npm run build:ts

# Build .NET application
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

# Copy TypeScript build output
COPY --from=build /src/Scripts/dist ./Scripts/dist

# Create non-root user for security
RUN adduser --disabled-password --gecos "" appuser && chown -R appuser /app
USER appuser

# Health check handled by Docker Compose

# Start the application
ENTRYPOINT ["dotnet", "Leaderboard.dll"]
