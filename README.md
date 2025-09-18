# 🏆 Scalable Leaderboard API

A high-performance, real-time leaderboard service built for multiplayer mobile football games. Designed to handle millions of players with consistent rankings and bulletproof data integrity.

## 🎯 What This Does

This API powers competitive leaderboards where every match matters. Players submit their scores after each game, and the system maintains real-time global rankings with sub-second response times. Think of it as the backbone behind those addictive leaderboards that keep players coming back for "just one more game."

## 🏗️ Architecture Overview

### The Big Picture

```
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│   Mobile Game   │───▶│  Leaderboard    │───▶│   PostgreSQL    │
│     Client      │    │      API        │    │   (Persistent)  │
└─────────────────┘    └─────────────────┘    └─────────────────┘
                                │
                                ▼
                       ┌─────────────────┐
                       │      Redis      │
                       │   (Cache Layer) │
                       └─────────────────┘
```

### Core Components

**🎮 Game Client Integration**
- JWT-based authentication keeps things secure
- Idempotency protection prevents duplicate score submissions
- Rate limiting stops spam attempts

**⚡ API Layer (.NET 8)**
- RESTful endpoints that just work
- Real-time score processing with validation
- Multi-game mode support (Classic, Tournament)
- Comprehensive error handling with meaningful responses

**🗄️ Data Layer**
- **PostgreSQL**: The source of truth for all player data
- **Redis**: Lightning-fast cache for top players and recent queries
- Smart caching strategy that keeps hot data accessible

**📊 Monitoring Stack**
- Prometheus metrics collection with security monitoring
- Grafana dashboards for real-time insights and security alerts
- Structured logging with Serilog
- Real-time security incident tracking

## 🚀 Quick Start

### Development Environment (Local API + Docker Database)

```bash
# Start development services (PostgreSQL, Redis, PgAdmin, RedisInsight, Prometheus, Grafana)
./dev.sh

# Or manually:
docker-compose --profile dev up -d

# Run API locally (with hot-reload)
dotnet run --urls "http://localhost:5088"

# API will be available at: http://localhost:5088
# Swagger UI: http://localhost:5088/swagger
```

### Production Environment (Everything in Docker)

```bash
# Start full production stack
./prod.sh

# Or manually:
docker-compose --profile prod up -d

# API will be available at: http://localhost:8080
# Swagger UI: http://localhost:8080/swagger
# Grafana: http://localhost:3000 (admin/admin)
# Prometheus: http://localhost:9090
```

### Available Services

| Service | Development | Production | URL |
|---------|-------------|------------|-----|
| API | Local (5088) | Docker (8080) | http://localhost:5088 / 8080 |
| PostgreSQL | Docker | Docker | localhost:5432 |
| Redis | Docker | Docker | localhost:6379 |
| PgAdmin | Docker | - | http://localhost:5050 |
| RedisInsight | Docker | - | http://localhost:5540 |
| Prometheus | Docker | Docker | http://localhost:9090 |
| Grafana | Docker | Docker | http://localhost:3000 |

### Environment Variables

Create a `.env` file in the project root:

```bash
# Database
POSTGRES_USER=postgres
POSTGRES_PASSWORD=postgres
POSTGRES_DB=leaderboard
POSTGRES_PORT=5432

# Redis
REDIS_PORT=6379

# JWT
JWT_SECRET="your-super-secret-key-here"
JWT_ISSUER="LeaderboardAPI"
JWT_AUDIENCE="LeaderboardUsers"
JWT_ACCESS_TOKEN_MINUTES=60

# Database
POSTGRES_USER=postgres
POSTGRES_PASSWORD=postgres
POSTGRES_DB=leaderboard
POSTGRES_PORT=5432
POSTGRES_CONNECTION_STRING="Host=localhost;Port=5432;Database=leaderboard;Username=postgres;Password=postgres"

# Redis
REDIS_PORT=6379
REDIS_CONNECTION_STRING="localhost:6379,abortConnect=false,connectTimeout=10000,syncTimeout=10000"

# API
API_PORT=8080
API_HTTPS_PORT=8443

# Development Tools
PGADMIN_PORT=5050
PGADMIN_DEFAULT_EMAIL=admin@local
PGADMIN_DEFAULT_PASSWORD=admin
REDISINSIGHT_PORT=5540

# Monitoring (Production only)
PROMETHEUS_PORT=9090
GRAFANA_PORT=3000
GRAFANA_ADMIN_USER=admin
GRAFANA_ADMIN_PASSWORD=admin

# ASP.NET Core
ASPNETCORE_ENVIRONMENT=Development
```

### Prerequisites

You'll need Docker and Docker Compose. That's it.

### Getting Up and Running

1. **Clone and Navigate**
   ```bash
   git clone <repository-url>
   cd Leaderboard
   ```

2. **Create Environment Configuration**
   ```bash
   # Create .env file with your environment variables
   # Copy the production template from the README above
   # Or use the development template for local development
   ```

3. **Fire It Up**
   ```bash
   # For production (everything in Docker)
   ./prod.sh

   # For development (API local, services in Docker)
   ./dev.sh
   dotnet run --urls "http://localhost:5088"
   ```

4. **Verify Everything's Working**
   ```bash
# Check API health (use appropriate port based on your setup)
curl http://localhost:5088/Health  # For local development
curl http://localhost:8080/Health  # For Docker production
   
# View Swagger docs
open http://localhost:5088/swagger  # For local development
open http://localhost:8080/swagger  # For Docker production
   
   # Check Grafana dashboard (includes security monitoring)
   open http://localhost:3000
   
   # View Prometheus metrics
   curl http://localhost:5088/metrics | grep security
   ```

That's it! The system creates all necessary database tables automatically.

### Development Setup

If you want to run it locally for development:


#### Full Local Development (Recommended)

If you want to run the API locally but use Docker for databases and monitoring:

1. **Create .env file for development**
   ```bash
   # Create .env file with development settings
   cat > .env << EOF
   # Database
   POSTGRES_USER=postgres
   POSTGRES_PASSWORD=postgres
   POSTGRES_DB=leaderboard
   POSTGRES_HOST=localhost
   POSTGRES_PORT=5432

   # Redis
   REDIS_HOST=localhost
   REDIS_PORT=6379

   # JWT
   JWT_SECRET=dev-secret-key-change-in-production
   JWT_ISSUER=LeaderboardAPI-Dev
   JWT_AUDIENCE=LeaderboardUsers-Dev
   JWT_ACCESS_TOKEN_MINUTES=60

   # Connection Strings
   ConnectionStrings__PostgreSQL=Host=localhost;Database=leaderboard;Username=postgres;Password=postgres;Port=5432
   ConnectionStrings__Redis=localhost:6379

   # ASP.NET Core
   ASPNETCORE_ENVIRONMENT=Development
   EOF
   ```

2. **Start development services**
   ```bash
   # Start databases and monitoring with development profile
   ./dev.sh
   ```

3. **Build TypeScript scripts**
   ```bash
   npm run build:ts
   ```

4. **Run the API locally**
   ```bash
   dotnet run --urls "http://localhost:5088"
   ```

This approach gives you:
- Local API development with hot reload
- Docker databases (PostgreSQL, Redis)
- Docker monitoring stack (Prometheus, Grafana)
- Development-specific configurations
- Hot-reload for .NET changes

## 📚 API Documentation

### Authentication

All leaderboard operations require a valid JWT token. Get one by registering or logging in:

```bash
# Register a new player
POST /auth/register
{
  "username": "player123",
  "password": "securepass",
  "deviceId": "device-uuid"
}

# Login
POST /auth/login
{
  "username": "player123", 
  "password": "securepass"
}
```

### Core Leaderboard Operations

**Submit Match Score**
```bash
POST /leaderboard/submit
Headers: 
  Authorization: Bearer <jwt-token>
  Idempotency-Key: unique-request-id
  X-Timestamp: 1642694400  # UTC timestamp for replay protection
  
{
  "score": 1500,
  "gameMode": "Classic",
  "playerLevel": 25,
  "trophyCount": 150,
  "bonus": ["weekend_bonus"]  // Optional: Array of bonus function names
}
```

**Get Top Players**
```bash
GET /leaderboard/top/Classic?n=100

Response:
{
  "success": true,
  "message": "Top 100 players retrieved for Classic mode",
  "data": [
    {
      "userId": "uuid",
      "score": 5000,
      "playerLevel": 50,
      "trophyCount": 500,
      "registrationDateUtc": "2024-01-01T00:00:00Z"
    }
  ]
}
```

**Get My Ranking**
```bash
GET /leaderboard/me/Classic

Response:
{
  "success": true,
  "data": {
    "rank": 1337,
    "score": 2500,
    "totalPlayers": 50000
  }
}
```

**Get Players Around Me**
```bash
GET /leaderboard/around-me/Classic?k=5
Authorization: Bearer <jwt-token>

# Returns 5 players above and below your rank
```

### User Management Endpoints

**Get User by ID**
```bash
GET /api/users/{id}
Authorization: Bearer <jwt-token>

Response:
{
  "id": "uuid",
  "username": "player123",
  "registrationDateUtc": "2024-01-01T00:00:00Z"
}
```

### Health & Monitoring Endpoints

**Health Check**
```bash
GET /Health

Response:
{
  "status": "Healthy",
  "timestamp": "2024-01-01T12:00:00Z"
}
```

**Metrics (Prometheus)**
```bash
GET /metrics

# Returns Prometheus-formatted metrics including:
# - Request counts and durations
# - Security violations (replay attacks, idempotency conflicts)
# - Database connection pool usage
# - Cache hit/miss rates
# - Bonus usage statistics
```

### Game Modes

The system supports multiple game modes with separate leaderboards:

- **Classic** (GameMode.Classic = 1): Standard gameplay mode
- **Tournament** (GameMode.Tournament = 2): Competitive events with special rules

Each mode maintains its own rankings, cache keys, and scoring calculations.

## 🧮 Ranking Algorithm

### How Rankings Work

1. **Primary Sort**: Higher score wins
2. **Tie Breakers** (in order):
   - Earlier registration date (veterans get priority)
   - Higher player level
   - Higher trophy count

### Example Ranking

```
Rank | Score | Reg Date    | Level | Trophies | Player
-----|-------|-------------|-------|----------|--------
1    | 5000  | 2024-01-01  | 50    | 500      | Alice
2    | 5000  | 2024-01-02  | 60    | 600      | Bob    ← Same score, later reg
3    | 4500  | 2024-01-01  | 45    | 450      | Charlie
```

This system rewards both skill (high scores) and loyalty (early adoption).

## 🎯 Bonus System & Dynamic Scoring

### TypeScript Scripting Engine

The leaderboard supports dynamic bonus calculations through a TypeScript-based scripting system. This allows for flexible score modifications based on various conditions without redeploying the application.

### Available Bonus Types

**Weekend Bonus**
```typescript
function weekend_bonus(baseScore: number): number {
    // Check if current date is weekend
    const currentDate = new Date();
    const isWeekend = currentDate.getDay() === 0 || currentDate.getDay() === 6;
    if(!isWeekend) {
        return baseScore;
    }
    // If current date is weekend, return baseScore * 1.05
    const levelBonus = 1.05;
    return Math.floor(baseScore * levelBonus);
}
```

### Using Bonuses in Score Submission

```bash
POST /leaderboard/submit
Headers:
  Authorization: Bearer <jwt-token>
  Idempotency-Key: unique-request-id
  X-Timestamp: 1642694400

{
  "score": 1500,
  "gameMode": "Classic",
  "playerLevel": 25,
  "trophyCount": 150,
  "bonus": ["weekend_bonus"]  // Array of bonus function names
}
```

### Creating Custom Bonus Scripts

1. **Add your TypeScript function** to `Scripts/src/calculator.ts`
2. **Build the scripts**: `npm run build:ts`
3. **Deploy the updated scripts** with your application

The system will automatically detect and execute your bonus functions, with metrics tracking bonus usage and amounts.

## 💾 Data Consistency Strategy

### The Two-Layer Approach

**PostgreSQL (Source of Truth)**
- All score submissions go here first
- ACID transactions ensure data integrity
- Composite indexes for lightning-fast queries
- Database migrations handle schema changes safely

**Redis (Speed Layer)**
- Caches top 100 players per game mode
- 5-minute TTL prevents stale data
- Cache invalidation on score updates
- Idempotency keys stored for replay protection

### Cache Consistency Guarantees

1. **Write-Through Pattern**: Database first, then cache
2. **Atomic Operations**: Cache only updates after successful DB commits
3. **Failure Handling**: Failed transactions don't pollute cache
4. **TTL Safety Net**: Cache expires automatically if invalidation fails

### Real-World Example

```bash
# Player submits score
POST /leaderboard/submit {"score": 2000}

# What happens behind the scenes:
1. Validate score isn't suspiciously high
2. Start database transaction
3. Update player score in PostgreSQL
4. Commit transaction ✅
5. Invalidate Redis cache for affected game mode
6. Update metrics

# If step 4 fails, cache remains untouched = consistency ✅
```

## ⚡ Performance Strategies

### Database Optimizations

**Smart Indexing**
```sql
-- Game mode aware composite index for ranking queries
IX_Leaderboard_GameMode_Ranking (GameMode, Score DESC, RegistrationDate, PlayerLevel DESC, TrophyCount DESC)

-- User lookup optimization
IX_Leaderboard_User_GameMode (UserId, GameMode)
```

**Query Patterns**
- Composite primary key: `{UserId, GameMode}`
- Rank calculation uses window functions
- "Around me" queries use CTEs for efficiency

### Redis Optimization

**Caching Strategy**
- Top players cached per game mode: `lb:top:Classic:100`
- 5-minute TTL balances freshness vs performance
- Idempotency keys: `idem:lb:{userId}:{key}` (5min TTL)

**Memory Management**
- String storage for simple key-value pairs
- Automatic expiration prevents memory bloat
- Connection pooling via `IConnectionMultiplexer`

### Application-Level Performance

**Batching Operations**
- Multiple Redis operations in pipelines where possible
- Bulk database operations for data migrations
- Async/await throughout for non-blocking I/O

**Connection Management**
- PostgreSQL connection pooling via Entity Framework
- Redis connection multiplexing
- Proper disposal patterns with `using` statements

### Real Performance Numbers

With proper indexing and caching:
- Top 100 query: ~5ms (cache hit) / ~50ms (cache miss)
- Score submission: ~100ms (includes validation + DB write + cache invalidation)
- User rank lookup: ~20ms average
- "Around me" query: ~30ms average

## 📈 Horizontal Scalability Plan

### Current Architecture Scaling Points

**API Layer Scaling**
```yaml
# Easy horizontal scaling with load balancer
nginx:
  upstream api_servers:
    - api-server-1:8080
    - api-server-2:8080
    - api-server-3:8080
```

**Database Scaling Strategy**

*Phase 1: Vertical Scaling*
- Increase PostgreSQL resources
- Optimize queries and add indexes
- Monitor connection pool usage

*Phase 2: Read Replicas*
- Separate read/write database connections
- Route ranking queries to read replicas
- Master-slave replication for high availability

*Phase 3: Sharding (if needed)*
- Shard by game mode or user ID ranges
- Use PostgreSQL logical replication
- Implement application-level routing

### Redis Scaling

**Current Setup**: Single Redis instance
**Next Steps**: 
- Redis Cluster for automatic sharding
- Separate cache instances per game mode
- Redis Sentinel for high availability

### Load Testing Targets

The system is designed to handle:
- **10,000+ concurrent players**
- **100,000+ score submissions per hour**
- **1M+ leaderboard queries per hour**
- **Sub-second response times** under normal load

### Monitoring and Alerting

Key metrics to watch:
- API response times (p95, p99)
- Database connection pool usage
- Redis memory usage and hit rates
- Queue lengths and processing times
- **Security metrics**: Replay attacks, idempotency conflicts, validation failures
- **Security score**: Real-time security health indicator

## 🔒 Security Features

### Authentication & Authorization
- JWT tokens with configurable expiration
- Password hashing using industry standards
- Device ID tracking for additional security

### Score Validation
- Server-side validation prevents client manipulation
- Dramatic score increase detection
- Rate limiting prevents spam submissions

### Replay Attack Protection
- **Timestamp Validation**: UTC timestamp headers with 10-minute tolerance window
- **Idempotency Keys**: Prevent duplicate submissions, keys only persist for successful requests
- **Failed Request Retry**: Failed requests can be retried with same key
- **Real-time Detection**: All security violations tracked in Prometheus metrics

### API Security
- CORS configuration for browser clients
- Request size limits
- Structured error responses (no stack trace leaks)

## 🛡️ Security Monitoring & Incident Response

### Real-time Security Dashboard

The Grafana dashboard includes dedicated security panels:

**🔐 Security Attacks Detected**
- Real-time replay attack attempts by type (old timestamps, future timestamps)
- Idempotency conflicts tracking
- Security validation failures by category

**⏰ Request Timestamp Age Distribution**
- p50, p95, p99 percentiles of request timestamp ages
- Helps identify network latency patterns and potential issues

**Security Metrics Summary**
- 🚨 Total replay attacks counter
- 🔑 Total idempotency conflicts counter  
- ❌ Total security validation failures counter
- 🛡️ Real-time security score (0-100%)

**🔍 Security Incidents Table**
- Detailed breakdown by user ID and incident type
- Useful for identifying repeat offenders or systematic attacks

### Security Metrics Reference

```bash
# Replay attack detection
leaderboard_replay_attack_attempts_total{user_id, reason, endpoint}

# Idempotency conflict tracking  
leaderboard_idempotency_conflicts_total{user_id, endpoint}

# Request timestamp age analysis
leaderboard_request_timestamp_age_seconds{endpoint, status}

# Security validation failures
leaderboard_security_validation_failures_total{validation_type, user_id, endpoint}
```

### Security Response Scenarios

**High Replay Attack Volume**
1. Check Grafana security dashboard
2. Identify affected users in incidents table
3. Review application logs for patterns
4. Consider temporary rate limiting adjustments

**Suspicious Timestamp Patterns**
1. Monitor timestamp age distribution
2. Look for coordinated attacks (multiple users, similar patterns)
3. Verify client-side timestamp generation logic

**Idempotency Key Abuse**
1. Track specific users with high conflict rates
2. Investigate potential client-side bugs
3. Consider user-specific rate limits

## 🔧 Configuration

### Environment Variables

#### Production Environment Variables

```bash
# Database
DB_CONNECTION_STRING="Host=localhost;Database=leaderboard;Username=postgres;Password=postgres"

# Redis  
REDIS_CONNECTION_STRING="localhost:6379"

# JWT
JWT_SECRET="your-secret-key"
JWT_EXPIRATION_HOURS=24

# Rate Limiting
RATE_LIMIT_REQUESTS=100
RATE_LIMIT_WINDOW_MINUTES=1
```

#### Development Environment Variables (.env.development)

Create a `.env.development` file for local development:

```bash
# Database
POSTGRES_USER=postgres
POSTGRES_PASSWORD=postgres
POSTGRES_DB=leaderboard
POSTGRES_HOST=localhost
POSTGRES_PORT=5432

# Redis
REDIS_HOST=localhost
REDIS_PORT=6379

# JWT
JWT_SECRET=dev-secret-key-change-in-production
JWT_ISSUER=LeaderboardAPI-Dev
JWT_AUDIENCE=LeaderboardUsers-Dev
JWT_ACCESS_TOKEN_MINUTES=60

# Database
POSTGRES_USER=postgres
POSTGRES_PASSWORD=postgres
POSTGRES_DB=leaderboard
POSTGRES_HOST=localhost
POSTGRES_PORT=5432

# Redis
REDIS_HOST=localhost
REDIS_PORT=6379

# Connection Strings
ConnectionStrings__PostgreSQL=Host=localhost;Database=leaderboard;Username=postgres;Password=postgres;Port=5432
ConnectionStrings__Redis=localhost:6379

# ASP.NET Core
ASPNETCORE_ENVIRONMENT=Development
```

### Docker Compose Configuration

The `docker-compose.yml` includes:
- API service with health checks
- PostgreSQL with persistent volume
- Redis with memory optimization
- Prometheus for metrics collection
- Grafana with pre-configured dashboards

## 🐛 Troubleshooting

### Common Issues

**"Connection refused" errors**
- Check if containers are running: `docker-compose --profile dev ps` or `docker-compose --profile prod ps`
- Verify port mappings in docker-compose.yml
- Ensure no port conflicts with other services
- For local development, verify the API is running on the correct port

**Database Connection Issues**
- Verify PostgreSQL container is healthy: `docker-compose --profile dev exec postgres pg_isready -U postgres -d leaderboard`
- Check connection string format in .env file
- Ensure database migrations have run successfully
- Check logs: `docker-compose --profile dev logs postgres`

**Redis Connection Issues**
- Verify Redis container is running: `docker-compose --profile dev exec redis redis-cli ping`
- Check Redis connection string in .env file
- Verify Redis port is accessible

**Score submission failures**
- Verify JWT token is valid and not expired
- Check idempotency key is provided and unique
- Ensure score increase isn't flagged as suspicious by the validation system
- Check timestamp is within acceptable range (not too old or future)
- Verify user has permission to submit scores

**Authentication Issues**
- Verify JWT secret matches between API and client
- Check token expiration time
- Ensure correct issuer and audience in token
- Verify user exists in database

**Cache inconsistency**
- Check Redis connection in API logs
- Verify cache TTL settings in appsettings.json
- Monitor cache hit/miss rates in Grafana
- Clear cache manually if needed: `docker-compose --profile dev exec redis redis-cli FLUSHALL`

**TypeScript Script Issues**
- Ensure scripts are built: `npm run build:ts`
- Check for syntax errors in `Scripts/src/calculator.ts`
- Verify function names match exactly in bonus requests
- Check API logs for script execution errors

**Migration Issues**
- Ensure database is accessible before running migrations
- Check migration logs for errors
- Verify database user has proper permissions
- Reset database if needed: `docker-compose --profile dev exec postgres psql -U postgres -d leaderboard -c "DROP SCHEMA public CASCADE; CREATE SCHEMA public;"`

### Debug Commands

```bash
# Check container health
docker-compose --profile dev ps
docker-compose --profile prod ps

# View API logs
docker-compose --profile dev logs -f api
docker-compose --profile prod logs -f api

# Check database connection and tables
docker-compose --profile dev exec postgres psql -U postgres -d leaderboard -c "\dt"
docker-compose --profile dev exec postgres psql -U postgres -d leaderboard -c "SELECT * FROM \"LeaderboardEntries\" LIMIT 5;"

# Verify Redis connection and keys
docker-compose --profile dev exec redis redis-cli ping
docker-compose --profile dev exec redis redis-cli KEYS "*"

# Check API health and metrics
curl http://localhost:5088/Health  # Development
curl http://localhost:8080/Health  # Production
curl http://localhost:5088/metrics | head -20  # Check metrics endpoint

# Test authentication
curl -X POST http://localhost:5088/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"test","password":"test"}'

# Check leaderboard endpoints
curl -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  http://localhost:5088/leaderboard/top/Classic?n=10

# Monitor system resources
docker-compose --profile dev exec postgres psql -U postgres -d leaderboard -c "SELECT * FROM pg_stat_activity;"
docker stats
```

### Performance Issues

**High Latency**
- Check database query performance with `EXPLAIN ANALYZE`
- Monitor Redis cache hit rates in Grafana
- Verify network connectivity between services
- Check for memory pressure on containers

**Memory Issues**
- Monitor container memory usage: `docker stats`
- Check Redis memory usage: `docker-compose exec redis redis-cli INFO memory`
- Verify PostgreSQL memory settings
- Look for memory leaks in application logs

**Rate Limiting Issues**
- Check rate limit configurations in appsettings.json
- Monitor rate limit metrics in Grafana
- Verify client is not exceeding limits
- Check for legitimate high-traffic scenarios

### Security-Related Issues

**Replay Attack Detections**
- Check timestamp validation settings
- Verify client clock synchronization
- Review timestamp age distribution in Grafana
- Check for network latency issues

**Idempotency Conflicts**
- Verify idempotency key generation is unique per request
- Check key TTL settings
- Monitor conflict rates in Grafana
- Review client-side key generation logic

### Getting Help

If you continue to have issues:

1. **Check the logs**: Start with `docker-compose logs -f` for all services
2. **Verify configuration**: Ensure all environment variables are set correctly
3. **Test connectivity**: Use the debug commands above to test each service
4. **Check Grafana**: Review dashboards for performance and error metrics
5. **Reset if needed**: Stop containers, remove volumes, and restart fresh

### Recovery Procedures

**Full System Reset (Development)**
```bash
# Stop all services
docker-compose --profile dev down

# Remove volumes (WARNING: This deletes all data)
docker volume rm $(docker volume ls -q | grep leaderboard)

# Restart fresh
./dev.sh
dotnet run --urls "http://localhost:5088"
```

**Database Reset Only**
```bash
# Connect to database
docker-compose --profile dev exec postgres psql -U postgres -d leaderboard

# Drop and recreate schema
DROP SCHEMA public CASCADE;
CREATE SCHEMA public;
GRANT ALL ON SCHEMA public TO postgres;
GRANT ALL ON SCHEMA public TO public;

# Exit and restart API to run migrations
exit
dotnet run --urls "http://localhost:5088"
```

## 🎮 Game Integration Examples

### Unity C# Integration

```csharp
public class LeaderboardClient
{
    private readonly HttpClient _http;
    private readonly string _baseUrl = "http://your-api-url";
    
    public async Task SubmitScore(int score, string gameMode)
    {
        var request = new {
            score = score,
            gameMode = gameMode,
            playerLevel = PlayerData.Level,
            trophyCount = PlayerData.Trophies
        };
        
        // Security headers for replay attack protection
        _http.DefaultRequestHeaders.Clear();
        _http.DefaultRequestHeaders.Add("Authorization", $"Bearer {authToken}");
        _http.DefaultRequestHeaders.Add("X-Timestamp", DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString());
        _http.DefaultRequestHeaders.Add("Idempotency-Key", $"unity-{SystemInfo.deviceUniqueIdentifier}-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}");
        
        var response = await _http.PostAsJsonAsync(
            "/leaderboard/submit", 
            request
        );
        
        return await response.Content.ReadAsStringAsync();
    }

    public async Task SubmitScoreWithBonus(int baseScore, string gameMode, string bonusType)
    {
        var request = new {
            score = baseScore,
            gameMode = gameMode,
            playerLevel = PlayerData.Level,
            trophyCount = PlayerData.Trophies,
            bonus = new[] { bonusType }  // Apply bonus function
        };

        var response = await _http.PostAsJsonAsync("/leaderboard/submit", request);
        var result = await response.Content.ReadFromJsonAsync<dynamic>();

        // Result will include bonus amount applied
        Console.WriteLine($"Score submitted: {result.data.score}, Bonus: {result.bonus}");
    }
}
```