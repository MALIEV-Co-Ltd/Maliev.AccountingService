# Development Quickstart: Accounting Service

**Feature**: `001-accounting-service-core`
**Date**: 2025-12-05
**Purpose**: Get the Accounting Service running locally for development

## Prerequisites

- **.NET 10.0 SDK** - [Download](https://dotnet.microsoft.com/download/dotnet/10.0)
- **Docker Desktop** - [Download](https://www.docker.com/products/docker-desktop)
- **PostgreSQL Client (psql)** - For database inspection
- **Git** - For cloning repository
- **NuGet Credentials** - Access to GitHub Packages for Maliev.Aspire.ServiceDefaults

## Quick Start (5 Minutes)

### 1. Clone and Configure

```bash
# Clone repository
git clone https://github.com/maliev/Maliev.AccountingService.git
cd Maliev.AccountingService

# Configure GitHub Packages NuGet credentials
export NUGET_USERNAME="your-github-username"
export NUGET_PASSWORD="your-github-pat-with-read:packages"

# Restore packages
dotnet restore
```

### 2. Start Infrastructure

```bash
# Start PostgreSQL, Redis, RabbitMQ via Docker Compose
docker-compose up -d

# Verify services are running
docker-compose ps
```

**docker-compose.yml** (example):
```yaml
version: '3.8'
services:
  postgres:
    image: postgres:16-alpine
    environment:
      POSTGRES_DB: accounting_dev
      POSTGRES_USER: accounting_user
      POSTGRES_PASSWORD: dev_password
    ports:
      - "5432:5432"
    volumes:
      - postgres-data:/var/lib/postgresql/data

  redis:
    image: redis:7-alpine
    ports:
      - "6379:6379"
    command: redis-server --maxmemory 256mb --maxmemory-policy volatile-lru

  rabbitmq:
    image: rabbitmq:3-management-alpine
    environment:
      RABBITMQ_DEFAULT_USER: accounting
      RABBITMQ_DEFAULT_PASS: dev_password
    ports:
      - "5672:5672"
      - "15672:15672"  # Management UI

volumes:
  postgres-data:
```

### 3. Configure Connection Strings

**Option A: User Secrets** (Recommended for local development)
```bash
cd src/Maliev.AccountingService.Api

dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:ServiceDbContext" "Host=localhost;Database=accounting_dev;Username=accounting_user;Password=dev_password"
dotnet user-secrets set "ConnectionStrings:redis" "localhost:6379"
dotnet user-secrets set "ConnectionStrings:rabbitmq" "amqp://accounting:dev_password@localhost:5672"
```

**Option B: appsettings.Development.json**
```json
{
  "ConnectionStrings": {
    "ServiceDbContext": "Host=localhost;Database=accounting_dev;Username=accounting_user;Password=dev_password",
    "redis": "localhost:6379",
    "rabbitmq": "amqp://accounting:dev_password@localhost:5672"
  }
}
```

### 4. Apply Database Migrations

```bash
cd src/Maliev.AccountingService.Api

# Install EF Core tools (if not already installed)
dotnet tool install --global dotnet-ef

# Apply migrations
dotnet ef database update

# Verify tables created
psql -h localhost -U accounting_user -d accounting_dev -c "\dt"
```

### 5. Run the Service

```bash
cd src/Maliev.AccountingService.Api

dotnet run
```

**Service should start on**: `https://localhost:5001`

### 6. Verify Service is Running

Open your browser:

- **Scalar API Docs**: `https://localhost:5001/accounting/scalar/v1`
- **Health Check**: `https://localhost:5001/accounting/health`
- **Metrics**: `https://localhost:5001/accounting/metrics`
- **RabbitMQ Management**: `http://localhost:15672` (accounting/dev_password)

## Testing

### Run All Tests

```bash
# From repository root
dotnet test

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"

# Run specific test project
dotnet test tests/Maliev.AccountingService.Tests
```

### Integration Tests

Integration tests automatically:
- Start Testcontainers for PostgreSQL, Redis, RabbitMQ
- Apply migrations to test database
- Seed test data
- Clean up after tests

**Note**: First test run may take 2-3 minutes to download Docker images.

### Manual API Testing

```bash
# Create chart of account (requires JWT token)
curl -X POST https://localhost:5001/api/v1/chart-of-accounts \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "accountNumber": "1200",
    "name": "Accounts Receivable",
    "type": "Asset",
    "category": "Current Assets"
  }'

# Get trial balance
curl -X GET "https://localhost:5001/api/v1/reports/trial-balance?periodId=YOUR_PERIOD_ID" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

## Development Workflow

### 1. Create a Feature Branch

```bash
git checkout -b feature/your-feature-name
```

### 2. Make Changes

- Entities: `src/Maliev.AccountingService.Api/Models/`
- Services: `src/Maliev.AccountingService.Api/Services/`
- Controllers: `src/Maliev.AccountingService.Api/Controllers/`
- Tests: `tests/Maliev.AccountingService.Tests/`

### 3. Create Migration (If Schema Changed)

```bash
cd src/Maliev.AccountingService.Api

dotnet ef migrations add YourMigrationName

# Review generated migration in Migrations/ folder
# Apply migration
dotnet ef database update
```

### 4. Run Tests

```bash
dotnet test
```

### 5. Commit and Push

```bash
git add .
git commit -m "feat: your feature description"
git push origin feature/your-feature-name
```

## Debugging

### Visual Studio Code

Create `.vscode/launch.json`:
```json
{
  "version": "0.2.0",
  "configurations": [
    {
      "name": ".NET Core Launch (web)",
      "type": "coreclr",
      "request": "launch",
      "preLaunchTask": "build",
      "program": "${workspaceFolder}/src/Maliev.AccountingService.Api/bin/Debug/net10.0/Maliev.AccountingService.Api.dll",
      "args": [],
      "cwd": "${workspaceFolder}/src/Maliev.AccountingService.Api",
      "env": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    }
  ]
}
```

### Visual Studio 2025

- Set `Maliev.AccountingService.Api` as startup project
- Press F5 to start debugging

### Common Issues

**Issue**: "Connection refused" to PostgreSQL
```bash
# Check if PostgreSQL is running
docker-compose ps

# Check logs
docker-compose logs postgres
```

**Issue**: "Package Maliev.Aspire.ServiceDefaults not found"
```bash
# Verify NuGet credentials
echo $NUGET_USERNAME
echo $NUGET_PASSWORD

# Check nuget.config
cat nuget.config

# Clear NuGet cache
dotnet nuget locals all --clear
dotnet restore
```

**Issue**: Migration fails with "relation already exists"
```bash
# Drop and recreate database
psql -h localhost -U accounting_user -d postgres -c "DROP DATABASE accounting_dev;"
psql -h localhost -U accounting_user -d postgres -c "CREATE DATABASE accounting_dev;"
dotnet ef database update
```

## Seeding Test Data

```bash
cd src/Maliev.AccountingService.Api

# Run seed script (create if needed)
dotnet run -- --seed-data
```

**Example seed data**:
- Fiscal Year 2025
- 12 monthly periods
- Standard chart of accounts (Assets, Liabilities, Equity, Revenue, Expenses)
- Sample transactions for testing reports

## Monitoring Development Metrics

Access OpenTelemetry metrics:
```bash
curl http://localhost:5001/accounting/metrics
```

View traces (if using Jaeger):
```bash
docker run -d --name jaeger \
  -p 16686:16686 \
  -p 4318:4318 \
  jaegertracing/all-in-one:latest

# Access Jaeger UI: http://localhost:16686
```

## Next Steps

1. **Read the spec**: [spec.md](./spec.md)
2. **Review data model**: [data-model.md](./data-model.md)
3. **Check API contracts**: [contracts/](./contracts/)
4. **Understand research decisions**: [research.md](./research.md)
5. **Start implementation**: See [tasks.md](./tasks.md) after running `/speckit.tasks`

## Getting Help

- **Documentation**: See specs/001-accounting-service-core/
- **Architecture questions**: Review research.md and plan.md
- **Constitution compliance**: See .specify/memory/constitution.md
- **GitHub Issues**: https://github.com/maliev/Maliev.AccountingService/issues
