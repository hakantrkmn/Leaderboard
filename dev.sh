#!/bin/bash

echo "🚀 Starting Development Environment..."
echo "📊 Services: PostgreSQL, Redis, PgAdmin, RedisInsight, Prometheus, Grafana"
echo ""

# First, stop production environment if running
echo "🛑 Stopping production environment if running..."
docker-compose --profile prod down

# Development profile - database'ler + monitoring
docker-compose --profile dev up -d

echo ""
echo "✅ Development environment started!"
echo "🔗 PostgreSQL: localhost:5432"
echo "🔗 Redis: localhost:6379"
echo "🔗 PgAdmin: http://localhost:5050"
echo "🔗 RedisInsight: http://localhost:5540"
echo "🔗 Prometheus: http://localhost:9090"
echo "🔗 Grafana: http://localhost:3000 (admin/admin)"
echo ""
echo "💻 Run your API locally with: dotnet run
# API will be available at: http://localhost:8080"
