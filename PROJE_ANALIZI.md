# 🏆 Leaderboard API Projesi - Detaylı Analiz

## 📋 Proje Genel Bakış

Bu proje, .NET 8 ile geliştirilmiş yüksek performanslı bir leaderboard API'sidir. Multiplayer mobil futbol oyunları için tasarlanmış, milyonlarca oyuncuyu destekleyebilen, gerçek zamanlı sıralama sistemi.

### 🎯 Ana Özellikler
- **Gerçek zamanlı sıralama** sistemi
- **Multi-game mode** desteği (Classic, Tournament)
- **JWT tabanlı authentication**
- **Redis cache** ile yüksek performans
- **PostgreSQL** veritabanı
- **Prometheus + Grafana** monitoring
- **Docker** containerization
- **Security features**: Idempotency, rate limiting, timestamp validation
- **JavaScript script engine** (Jint) ile dinamik bonus hesaplama

## 🏗️ Proje Mimarisi

### Teknoloji Stack
- **Framework**: .NET 8 Web API
- **Database**: PostgreSQL 16
- **Cache**: Redis 7
- **ORM**: Entity Framework Core 8
- **Authentication**: JWT Bearer Token
- **Monitoring**: Prometheus + Grafana
- **Logging**: Serilog
- **Script Engine**: Jint (JavaScript runtime)
- **Container**: Docker + Docker Compose

### Proje Yapısı
```
Leaderboard/
├── Auth/                    # Authentication modülü
├── Users/                   # Kullanıcı yönetimi
├── LeaderBoard/            # Ana leaderboard işlemleri
├── Scripts/                # JavaScript script engine
├── DB/                     # Veritabanı context
├── Extensions/             # Service registration extensions
├── Filters/                # Custom action filters
├── Middleware/             # Custom middleware
├── Metrics/                # Prometheus metrics
├── Migrations/             # EF Core migrations
└── monitoring/             # Prometheus & Grafana configs
```

## 🗄️ Veritabanı Şeması

### 1. Users Tablosu
```sql
CREATE TABLE "Users" (
    "Id" uuid PRIMARY KEY,
    "Username" varchar(50) NOT NULL UNIQUE,
    "PasswordHash" text NOT NULL,
    "DeviceId" varchar(128) NOT NULL,
    "RegistrationDate" timestamp with time zone DEFAULT (NOW() AT TIME ZONE 'utc'),
    "PlayerLevel" integer DEFAULT 1,
    "TrophyCount" integer DEFAULT 0
);
```

**Indexes:**
- `IX_Users_Username` (UNIQUE)

### 2. Leaderboard Tablosu
```sql
CREATE TABLE "Leaderboard" (
    "UserId" uuid NOT NULL,
    "Score" bigint NOT NULL,
    "UpdatedAtUtc" timestamp with time zone DEFAULT (NOW() AT TIME ZONE 'utc'),
    "RegistrationDateUtc" timestamp with time zone NOT NULL DEFAULT (NOW() AT TIME ZONE 'utc'),
    "PlayerLevel" integer DEFAULT 1,
    "TrophyCount" integer DEFAULT 0,
    "GameMode" integer NOT NULL DEFAULT 1,
    CONSTRAINT "PK_Leaderboard" PRIMARY KEY ("UserId", "GameMode")
);
```

**Indexes:**
- `IX_Leaderboard_GameMode_Ranking` (GameMode, Score DESC, RegistrationDateUtc, PlayerLevel DESC, TrophyCount DESC)
- `IX_Leaderboard_User_GameMode` (UserId, GameMode)

**Foreign Key:**
- `Leaderboard.UserId` → `Users.Id` (CASCADE DELETE)

### 3. GameMode Enum
```csharp
public enum GameMode
{
    Classic = 1,
    Tournament = 2
}
```

### 4. LeaderboardAroundRow (View/Query Result)
```csharp
public class LeaderboardAroundRow
{
    public Guid UserId { get; set; }
    public long Score { get; set; }
    public int Rn { get; set; } // Row number for ranking
}
```

## 🔐 Authentication & Authorization

### JWT Token Yapısı
- **Secret Key**: Environment variable'dan alınır
- **Expiration**: 24 saat (configurable)
- **Claims**: UserId, Username
- **Algorithm**: HMAC SHA256

### Auth Endpoints
- `POST /auth/register` - Yeni kullanıcı kaydı
- `POST /auth/login` - Kullanıcı girişi

### Request/Response DTOs
```csharp
// LoginRequest
{
    "username": "string",
    "password": "string"
}

// RegisterUserRequest
{
    "username": "string",
    "password": "string",
    "deviceId": "string"
}

// AuthResponse
{
    "token": "string",
    "expiresAt": "datetime",
    "user": {
        "id": "guid",
        "username": "string",
        "playerLevel": "int",
        "trophyCount": "int"
    }
}
```

## 🏆 Leaderboard API Endpoints

### 1. Score Submission
```http
POST /leaderboard/submit
Authorization: Bearer <jwt-token>
Idempotency-Key: <unique-key>
X-Timestamp: <unix-timestamp>

{
    "score": 1500,
    "gameMode": "Classic",
    "playerLevel": 25,
    "trophyCount": 150,
    "bonus": ["weekend_bonus"]
}
```

**Security Features:**
- Idempotency key (5 dakika TTL)
- Timestamp validation (±10 dakika tolerance)
- Rate limiting
- JWT authentication

### 2. Top Players
```http
GET /leaderboard/top/Classic?n=100
```

**Response:**
```json
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

### 3. My Ranking
```http
GET /leaderboard/me/Classic
Authorization: Bearer <jwt-token>
```

### 4. Players Around Me
```http
GET /leaderboard/around-me/Classic?k=5
Authorization: Bearer <jwt-token>
```

## 🧮 Sıralama Algoritması

### Primary Sort Criteria
1. **Score** (DESC) - Yüksek skor önce
2. **RegistrationDateUtc** (ASC) - Erken kayıt önce (tie-breaker)
3. **PlayerLevel** (DESC) - Yüksek seviye önce (tie-breaker)
4. **TrophyCount** (DESC) - Yüksek trophy önce (tie-breaker)

### SQL Query Example
```sql
SELECT 
    UserId, Score, PlayerLevel, TrophyCount, RegistrationDateUtc,
    ROW_NUMBER() OVER (
        ORDER BY 
            Score DESC,
            RegistrationDateUtc ASC,
            PlayerLevel DESC,
            TrophyCount DESC
    ) as Rank
FROM Leaderboard 
WHERE GameMode = @gameMode
```

## ⚡ Performance Optimizasyonları

### Database Indexes
- **Composite Index**: GameMode + Score + RegistrationDate + PlayerLevel + TrophyCount
- **User Lookup**: UserId + GameMode
- **Unique Constraint**: Username

### Redis Caching Strategy
- **Cache Key Pattern**: `lb:top:{gameMode}:{count}`
- **TTL**: 5 dakika
- **Cache Invalidation**: Score submission sonrası
- **Idempotency Keys**: `idem:lb:{userId}:{key}` (5 dakika TTL)

### Connection Management
- **PostgreSQL**: Entity Framework connection pooling
- **Redis**: StackExchange.Redis connection multiplexing

## 🔧 Script Engine (Jint)

### JavaScript Calculator
```javascript
// Scripts/src/calculator.ts
function weekend_bonus(baseScore: number): number {
    const currentDate = new Date();
    const isWeekend = currentDate.getDay() === 0 || currentDate.getDay() === 6;
    if(!isWeekend) {
        return baseScore;
    }
    const levelBonus = 1.05;
    return Math.floor(baseScore * levelBonus);
}

(globalThis as any).weekend_bonus = weekend_bonus;
```

### Script Engine Features
- **Memory Limit**: 4MB
- **Recursion Limit**: 100
- **CLR Integration**: .NET types erişimi
- **Console Logging**: Serilog integration
- **Hot Reload**: Development'ta script değişiklikleri

## 📊 Monitoring & Metrics

### Prometheus Metrics
```csharp
// HTTP Metrics
- http_requests_total (method, path, status_code, status_class)
- http_request_duration_seconds
- http_requests_in_progress

// Business Metrics
- leaderboard_score_submissions_total (user_id, score_range)
- leaderboard_bonus_usage_total (bonus_type, game_mode)
- leaderboard_bonus_amount (bonus_type, game_mode)

// Security Metrics
- leaderboard_replay_attack_attempts_total (user_id, reason, endpoint)
- leaderboard_idempotency_conflicts_total (user_id, endpoint)
- leaderboard_request_timestamp_age_seconds (endpoint, status)
- leaderboard_security_validation_failures_total (validation_type, user_id, endpoint)

// Auth Metrics
- leaderboard_login_attempts_total (status)
- leaderboard_active_users
```

### Grafana Dashboard
- **API Overview**: Request rates, response times, error rates
- **Security Monitoring**: Replay attacks, idempotency conflicts
- **Business Metrics**: Score submissions, bonus usage
- **Database Performance**: Query times, connection pool usage

## 🛡️ Security Features

### 1. Idempotency Protection
- **Header**: `Idempotency-Key`
- **Storage**: Redis (5 dakika TTL)
- **Scope**: Per user, per endpoint
- **Conflict Detection**: Duplicate key rejection

### 2. Timestamp Validation
- **Header**: `X-Timestamp` (Unix timestamp)
- **Tolerance**: ±10 dakika
- **Purpose**: Replay attack prevention
- **Validation**: Server-side timestamp check

### 3. Rate Limiting
```csharp
// Submit endpoint: 100 requests/minute
// Top endpoint: 1000 requests/minute
// Login endpoint: 10 requests/minute
```

### 4. Score Validation
- **Dramatic Increase Detection**: Anormal skor artışları
- **Server-side Validation**: Client manipulation koruması
- **Range Validation**: Mantıklı skor aralıkları

## 🐳 Docker Infrastructure

### Services
```yaml
services:
  postgres:     # PostgreSQL 16
  redis:        # Redis 7
  pgadmin:      # PostgreSQL admin UI
  redisinsight: # Redis admin UI
  prometheus:   # Metrics collection
  grafana:      # Monitoring dashboard
  api:          # .NET 8 API
```

### Environment Variables
```bash
# Database
POSTGRES_USER=postgres
POSTGRES_PASSWORD=postgres
POSTGRES_DB=leaderboard
POSTGRES_PORT=5432

# Redis
REDIS_PORT=6379

# JWT
JWT_SECRET=your-secret-key
JWT_EXPIRATION_HOURS=24

# API
API_PORT=8080
```

## 📈 Scaling Strategy

### Current Architecture
- **Single API Instance**: Docker container
- **Single PostgreSQL**: Master database
- **Single Redis**: Cache layer
- **Load Balancer Ready**: Horizontal scaling hazır

### Future Scaling Points
1. **API Layer**: Multiple instances behind load balancer
2. **Database**: Read replicas for ranking queries
3. **Redis**: Redis Cluster for cache sharding
4. **Monitoring**: Distributed tracing (Jaeger)

## 🔄 Development Workflow

### Local Development
```bash
# 1. Start infrastructure
docker-compose -f docker-compose-dev.yaml up -d

# 2. Build TypeScript scripts
npm run build:ts

# 3. Run API locally
dotnet run --urls "http://localhost:5088"

# 4. Access services
# API: http://localhost:5088
# Swagger: http://localhost:5088/swagger
# Grafana: http://localhost:3000
# Prometheus: http://localhost:9090
```

### Database Migrations
```bash
# Create migration
dotnet ef migrations add MigrationName

# Apply migrations
dotnet ef database update

# Or auto-migration in code
await DBMigration.EnsureDatabaseMigrated(app.Services);
```

## 📝 Logging Strategy

### Serilog Configuration
```csharp
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "LeaderboardAPI")
    .Enrich.WithProperty("Environment", environment)
    .Enrich.WithMachineName()
    .Enrich.WithThreadId()
    .WriteTo.Console(new CompactJsonFormatter())
    .WriteTo.File(new CompactJsonFormatter(), "logs/leaderboard-.json", 
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 7)
    .CreateLogger();
```

### Log Structure
- **Structured Logging**: JSON format
- **Context Enrichment**: Request ID, user ID, environment
- **File Rotation**: Daily rotation, 7 gün retention
- **Console Output**: Development için

## 🎯 Business Logic

### Score Submission Flow
1. **Authentication**: JWT token validation
2. **Idempotency Check**: Redis'te key kontrolü
3. **Timestamp Validation**: Replay attack kontrolü
4. **Bonus Calculation**: JavaScript engine ile bonus hesaplama
5. **Database Update**: PostgreSQL'e score kaydetme
6. **Cache Invalidation**: Redis cache temizleme
7. **Metrics Update**: Prometheus metrics güncelleme

### Ranking Calculation
1. **Cache Check**: Redis'te top players var mı?
2. **Database Query**: Composite index ile hızlı sıralama
3. **Window Function**: ROW_NUMBER() ile rank hesaplama
4. **Cache Update**: Sonuçları Redis'e kaydetme

### Security Monitoring
1. **Real-time Detection**: Prometheus metrics ile anlık izleme
2. **Grafana Alerts**: Security dashboard'da görselleştirme
3. **Incident Tracking**: User ID bazında saldırı takibi
4. **Response Automation**: Rate limiting ile otomatik koruma

## 🔍 API Response Patterns

### Standard Response Format
```json
{
    "success": true,
    "message": "Operation completed successfully",
    "data": { /* response data */ },
    "metadata": { /* additional info */ }
}
```

### Error Response Format
```json
{
    "success": false,
    "message": "Error description",
    "error": "Detailed error info (development only)"
}
```

### Pagination (Future)
```json
{
    "success": true,
    "data": [ /* items */ ],
    "pagination": {
        "page": 1,
        "pageSize": 100,
        "totalItems": 50000,
        "totalPages": 500
    }
}
```

Bu analiz, projenin tüm teknik detaylarını, veritabanı şemasını, API yapısını ve özelliklerini kapsamlı bir şekilde açıklamaktadır. NestJS ile yeniden implement ederken bu dokümanı referans olarak kullanabilirsiniz.

