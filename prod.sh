#!/bin/bash

echo "🚀 Starting Production Environment..."
echo "📊 Services: PostgreSQL, Redis, API, Prometheus, Grafana"
echo ""

# First, stop development environment if running
echo "🛑 Stopping development environment if running..."
docker-compose --profile dev down

# Production profile - tüm servisler dahil API
docker-compose --profile prod up -d

echo ""
echo "✅ Production environment started!"
echo "🔗 API: http://localhost:8080"
echo "🔗 PostgreSQL: localhost:5432"
echo "🔗 Redis: localhost:6379"
echo "🔗 Prometheus: http://localhost:9090"
echo "🔗 Grafana: http://localhost:3000 (admin/admin)"
echo ""
echo "🛑 Stop with: docker-compose --profile prod down"
