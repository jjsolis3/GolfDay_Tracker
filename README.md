# GolfDay Tracker

A production-ready .NET 9 Web Application for Golf Club management — tracking live scoring, tournaments, season leagues, and player statistics across multiple clubs and locations.

## Features

| Module | Description |
|--------|-------------|
| **Live Scoring** | Real-time hole-by-hole scoring via SignalR — leaderboard updates instantly |
| **Day Events** | Single-day events with scorecards, pairings, tee times, and achievements |
| **Tournaments** | Multi-round tournaments with registration, handicaps, and final leaderboards |
| **Season Leagues** | Head-to-head league play with automated scheduling and deadline tracking |
| **Player Profiles** | Handicap index (WHS), stats, scoring trends, round history |
| **Multi-Club** | Players can belong to multiple clubs; each club manages independently |
| **Admin Panel** | Club managers control events, members, courses, leagues, and tournaments |
| **Stats & Achievements** | Longest drive, closest to pin, hole-in-ones, birdies, GIR, fairways |

## Architecture

```
GolfDay_Tracker/
├── src/
│   ├── GolfDay.Domain/          # Core entities, value objects, enums
│   ├── GolfDay.Application/     # Interfaces, service contracts
│   ├── GolfDay.Infrastructure/  # EF Core, Identity, service implementations
│   └── GolfDay.Web/             # ASP.NET Core 9, Razor Pages, SignalR
├── Dockerfile
├── docker-compose.yml
└── README.md
```

**Tech Stack:**
- .NET 9 / ASP.NET Core 9
- Entity Framework Core 9 + SQL Server
- ASP.NET Core Identity (authentication + roles)
- SignalR (real-time live scoring)
- Razor Pages + Bootstrap 5 + Chart.js
- Serilog (structured logging)
- Docker + Docker Compose

## Quick Start

### Prerequisites
- .NET 9 SDK
- SQL Server (or Docker)

### Development Setup

```bash
# 1. Clone the repo
git clone <repo-url>
cd GolfDay_Tracker

# 2. Set up user secrets (connection string)
cd src/GolfDay.Web
dotnet user-secrets set "ConnectionStrings:DefaultConnection" \
  "Server=(localdb)\mssqllocaldb;Database=GolfDayTracker_Dev;Trusted_Connection=True"

# 3. Run EF migrations (first time only)
dotnet tool install --global dotnet-ef
dotnet ef migrations add InitialCreate --project ../GolfDay.Infrastructure --startup-project .
dotnet ef database update --project ../GolfDay.Infrastructure --startup-project .

# 4. Run the application
dotnet run
# → https://localhost:5001
```

### Docker (Production)

```bash
# Copy and configure environment
cp .env.example .env
# Edit .env with your DB_PASSWORD

# Build and start
docker compose up -d

# Check logs
docker compose logs -f web
```

### Default Admin Account (Seeded)

| Field | Value |
|-------|-------|
| Email | admin@golfdaytracker.com |
| Password | Admin@123456 |

**Change this password immediately in production!**

## Database Configuration

The app auto-migrates on startup. For production, set the connection string via:
- Environment variable: `ConnectionStrings__DefaultConnection`
- Docker Compose: `docker-compose.yml` `environment` section
- Azure App Service: Application Settings

## Role System

| Role | Permissions |
|------|-------------|
| **Admin** | Full system access, all clubs |
| **ClubManager** | Manage their club's events, members, tournaments, leagues |
| **Member** | View events, enter scores, view stats |

## League Play

Season leagues support:
- **Round Robin** — every player faces every other player once
- **Single Elimination** — bracket-style playoff
- Match deadlines with overdue tracking (automated background service)
- Players self-schedule within the deadline window
- Standings auto-update after each match result

## Live Scoring

Live scoring uses **SignalR WebSockets**:
1. Scorer enters strokes per hole on the scorecard page
2. Score broadcasts to all connected viewers in real-time
3. Leaderboard auto-sorts after every update
4. Toast notifications show new scores

## Production Deployment

### Azure App Service + Azure SQL

```bash
# Using Azure CLI
az group create -n golfday-rg -l eastus
az sql server create -n golfday-sql -g golfday-rg -l eastus -u sqladmin -p <password>
az sql db create -n GolfDayTracker -s golfday-sql -g golfday-rg --edition Standard
az webapp create -n golfday-app -g golfday-rg --plan <plan-name> --runtime "DOTNET|9.0"
az webapp config connection-string set -n golfday-app -g golfday-rg \
  --settings DefaultConnection="<azure-sql-connection-string>" -t SQLAzure
```

### Health Check Endpoint

```
GET /health
```

Returns `Healthy` when the application and database are operational.

## Development Notes

- Razor Pages with `.cshtml` + `.cshtml.cs` code-behind
- EF migrations are in `GolfDay.Infrastructure/Migrations/`
- SignalR hub: `/hubs/scoring`
- Bootstrap Icons for all UI icons
- Chart.js for stats visualization
