# Maliev Accounting Service

Central financial record management system for the MALIEV microservice ecosystem.

## Overview

The Accounting Service consolidates, interprets, and manages all financial records across MALIEV services. It serves as the central system of record for financial accounting data, ensuring that every transaction is captured, validated, and stored according to proper accounting principles.

## Features

- **Financial Transaction Recording**: Automatically capture transactions from Sales, Procurement, Inventory, and Payroll
- **Double-Entry Bookkeeping**: Maintains balanced journal entries with strict validation
- **Chart of Accounts Management**: Hierarchical account structure with categorization
- **Financial Period Management**: Fiscal year and period controls with closing routines
- **Tax Tracking**: VAT and tax obligation tracking with reporting
- **Financial Reconciliation**: Automated discrepancy detection between subledgers and GL
- **Financial Reporting**: Trial balance, balance sheet, income statement, cash flow
- **Comprehensive Audit Trail**: Immutable audit logs for all financial operations

## Technology Stack

- **.NET 10.0**: ASP.NET Core Web API
- **PostgreSQL**: Primary database with EF Core
- **RabbitMQ**: Event-driven integration via MassTransit
- **Redis**: Distributed caching and idempotency tracking
- **Docker**: Containerized deployment
- **xUnit**: Testing framework with Testcontainers

## Project Structure

```
Maliev.AccountingService/
├── src/
│   └── Maliev.AccountingService.Api/     # Main API project
│       ├── Controllers/                   # REST API endpoints
│       ├── Models/                        # Domain entities
│       ├── Services/                      # Business logic
│       ├── Consumers/                     # RabbitMQ event consumers
│       ├── DTOs/                          # Request/Response models
│       ├── Extensions/                    # Mapping extensions
│       ├── Middleware/                    # Custom middleware
│       └── AccountingDbContext.cs         # EF Core DbContext
├── tests/
│   └── Maliev.AccountingService.Tests/   # Test project
├── specs/                                 # Feature specifications
├── Dockerfile                             # Multi-stage build
└── nuget.config                          # NuGet configuration
```

## Getting Started

### Prerequisites

- .NET 10.0 SDK
- Docker Desktop
- PostgreSQL 16+ (or use Docker)
- RabbitMQ (or use Docker)
- Redis (or use Docker)

### Local Development

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd Maliev.AccountingService
   ```

2. **Configure connection strings**

   Update `src/Maliev.AccountingService.Api/appsettings.Development.json` or use user secrets:
   ```json
   {
     "ConnectionStrings": {
       "ServiceDbContext": "Host=localhost;Port=5432;Database=accounting_service_dev;Username=postgres;Password=postgres",
       "Redis": "localhost:6379",
       "RabbitMQ": "amqp://guest:guest@localhost:5672/"
     }
   }
   ```

3. **Start infrastructure services**
   ```bash
   docker-compose up -d postgres rabbitmq redis
   ```

4. **Run database migrations** (when available)
   ```bash
   cd src/Maliev.AccountingService.Api
   dotnet ef database update
   ```

5. **Run the application**
   ```bash
   dotnet run --project src/Maliev.AccountingService.Api
   ```

6. **Access the API**
   - API Documentation: `https://localhost:5001/openapi/v1.json`
   - Health Check: `https://localhost:5001/accounting/health`
   - Liveness: `https://localhost:5001/accounting/liveness`

### Running Tests

```bash
dotnet test
```

## Development Status

### ✅ Phase 1: Setup (Complete)
- [x] Project structure created
- [x] .NET 10.0 Web API initialized
- [x] NuGet packages added (PostgreSQL, MassTransit, Testcontainers)
- [x] Configuration files (appsettings.json, appsettings.Development.json)
- [x] .gitignore and .dockerignore configured
- [x] Dockerfile created
- [x] nuget.config with GitHub Packages
- [x] Test project initialized

### 🔄 Phase 2: Foundational (In Progress)
- [x] Program.cs with basic infrastructure
- [x] AccountingDbContext created
- [x] Health checks configured
- [x] MassTransit configured
- [ ] Middleware pipeline
- [ ] Exception handling
- [ ] Entity models
- [ ] EF Core migrations

### ⏳ Phase 3-11: Pending
- User Story 1: Financial Transaction Recording
- User Story 2: Chart of Accounts Management
- User Story 3: Financial Period Management
- User Story 4: Reconciliation and Validation
- User Story 5: Tax Tracking
- User Story 6: Financial Reporting
- User Story 7: Audit Trail
- User Story 8: Event Integration
- Polish and cross-cutting concerns

## Build and Deploy

### Build

```bash
dotnet build
```

**Note**: Project enforces `TreatWarningsAsErrors=true` - all builds must have zero warnings.

### Docker Build

```bash
docker build -t maliev-accounting-service:latest .
```

### Docker Run

```bash
docker run -p 8080:8080 \
  -e ConnectionStrings__ServiceDbContext="Host=host.docker.internal;..." \
  -e ConnectionStrings__Redis="host.docker.internal:6379" \
  -e ConnectionStrings__RabbitMQ="amqp://guest:guest@host.docker.internal:5672/" \
  maliev-accounting-service:latest
```

## Architecture

### Database Schema

See [specs/001-accounting-service-core/data-model.md](specs/001-accounting-service-core/data-model.md) for complete data model.

**Core Entities**:
- ChartOfAccount
- JournalEntry / JournalEntryLine
- FinancialPeriod / FiscalYear
- TaxComponent
- AuditTrailEntry
- ReconciliationReport
- SubledgerTransaction

### Event-Driven Integration

**Consumed Events**:
- `maliev.sales.v1.invoice.created`
- `maliev.sales.v1.payment.received`
- `maliev.procurement.v1.supplier-invoice.received`
- `maliev.inventory.v1.stock-movement.recorded`
- `maliev.payroll.v1.payroll.processed`

**Published Events**:
- `maliev.accounting.v1.transaction.posted`
- `maliev.accounting.v1.period.closed`
- `maliev.accounting.v1.reconciliation.completed`

## Contributing

See [specs/001-accounting-service-core/tasks.md](specs/001-accounting-service-core/tasks.md) for the complete implementation plan.

## License

Proprietary - MALIEV Organization
