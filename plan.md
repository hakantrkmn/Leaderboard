# Scalable and Consistent Leaderboard API - .NET Implementation

## Scenario
A multiplayer mobile football game with millions of players requires a central backend service to maintain a global leaderboard after each match result submission.

The leaderboard must:
- Reflect real-time changes
- Ensure consistent ranking
- Scale under high load
- Prevent manipulation or loss of score data

## Problem Statement
Design and implement a Leaderboard API Service with the following requirements.

---

## Functional Requirements

### 1. User Management
- **User Registration**
  - Accept: `username`, `password`, `device_id`
  - Persist user securely (password hashing)
- **Login**
  - Accept: `username`, `password`
  - Return: JWT or token usable for authenticated requests

---

### 2. Leaderboard System
- **Match Result Submission Endpoint**
  - Accept: user scores and optional metadata (`player_level`, `trophy_count`)
  - Update player’s score in the leaderboard
- **Ranking Logic**
  - Higher score = better rank
  - Tie-breaker order: `registration_date` > `player_level` > `trophy_count`
- **Leaderboard Read Endpoint**
  - Return top **N** number of players
  - Return current user’s rank and surrounding context

---

### 3. Data Consistency and Performance
- Use **Redis** for caching top leaderboard entries (e.g., top 100)
- Use **PostgreSQL** for persistent score storage

---

### 4. Security
- Prevent score tampering via authenticated API requests and server-side validation
- Do not blindly trust scores from the client

---

## Technical Stack Requirements
- **Primary Language:** C# (.NET 6+ / ASP.NET Core Web API)
- **Databases:**
  - PostgreSQL (persistence)
  - Redis (caching layer)
- **Containerization:** Docker (include Dockerfile and docker-compose)
- **Optional Tools:**
  - Nakama knowledge is a plus
  - Lua / TypeScript / .NET for bonus scripting or integration

---

## Expectations and Deliverables

### Code Requirements
- Clean, idiomatic C# code
- RESTful or gRPC APIs
- Dockerized setup for easy deployment
- Redis and PostgreSQL defined in `docker-compose.yml`

---

### Documentation (`README.md`)
- Architecture overview
- Setup and usage instructions
- API documentation (Swagger or Markdown)
- Ranking algorithm explanation
- Data consistency strategy (Redis eviction, DB failover handling)
- Performance strategies (PostgreSQL indexing, Redis TTLs, batching, etc.)
- Horizontal scalability plan

---

### Additional Requirements
**Security**
- Token-based authentication (JWT)
- Data validation
- Protection from replay attacks

**Monitoring and Logging**
- Structured logging
- Notes on how to integrate centralized logging and monitoring (e.g., Prometheus/Grafana)

**Extensibility**
- Support configurable ranking logic and multiple game modes

---

### Bonus Points
- Lua, TypeScript, or .NET-based dynamic rule modules
- Integration with Nakama’s RPC, storage, or matchmaking APIs

---

## Submission Guidelines
- Submit the solution in a public or private Git repository (GitHub, GitLab, etc.)
- Ensure all dependencies are documented or included
- Bonus: Include a short demo video or screenshots showing functionality
