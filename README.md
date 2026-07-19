# Maliev Accounting Service

[![Build Status](https://img.shields.io/badge/Build-Passing-success)](https://github.com/ORGANIZATION/Maliev.AccountingService)
[![.NET Version](https://img.shields.io/badge/.NET-10.0-blue)](https://dotnet.microsoft.com/download/dotnet/10.0)
[![Database](https://img.shields.io/badge/Database-PostgreSQL%2018-blue)](https://www.postgresql.org/)

Central financial record management system providing double-entry bookkeeping, fiscal period management, and comprehensive financial reporting.

**Role in MALIEV Architecture**: Consolidates and manages all financial records across the platform. It serves as the central system of record for financial accounting data, ensuring GAAP compliance and providing immutable audit trails.

---

## 🏗️ Architecture & Tech Stack

- **Framework**: ASP.NET Core 10.0 (C# 13)
- **Database**: PostgreSQL 18 with Entity Framework Core 10.x
- **Distributed Cache**: Redis 7.x (Idempotency tracking & reporting)
- **Messaging**: RabbitMQ via MassTransit
- **API Documentation**: OpenAPI 3.1 + Scalar UI
- **Observability**: OpenTelemetry (Metrics, Traces, Logging)

---

## ⚖️ Constitution Rules

This service strictly adheres to the platform development mandates:

### Banned Libraries
To maintain high performance and low complexity, the following are **NOT** used:
- ❌ **AutoMapper**: Explicit manual mapping only.
- ❌ **FluentValidation**: Standard Data Annotations (`[Required]`, `[EmailAddress]`) only.
- ❌ **FluentAssertions**: Standard xUnit `Assert` methods only.
- ❌ **In-memory Test DB**: All integration tests use **Testcontainers** with real PostgreSQL 18.

### Mandatory Practices
- ✅ **TreatWarningsAsErrors**: Enabled in all `.csproj` files.
- ✅ **XML Documentation**: Required on all public methods and properties.
- ✅ **No Secrets in Code**: All sensitive configuration injected via environment variables.
- ✅ **No Test Config in Program.cs**: Test configuration in test fixtures only.
- ✅ **IAM Integration**: Self-registers permissions with the IAM Service using GCP-style naming: `{service}.{resource}.{action}`.

---

## ✨ Key Features

- **Double-Entry Bookkeeping**: Maintains balanced journal entries with strict validation across the GL.
- **Chart of Accounts**: Hierarchical account structure management with flexible categorization.
- **Fiscal Period Management**: Full control over fiscal years and periods with automated closing routines.
- **Financial Reporting**: Real-time generation of trial balance, balance sheet, and income statements.
- **Audit Traceability**: Implementation of immutable audit logs for every financial operation.

---

## 🚀 Quick Start

### Prerequisites
- .NET 10.0 SDK
- Docker Desktop (for infrastructure)
- PostgreSQL 18 (Alpine)

### Local Development Setup

1. **Clone the repository**
```bash
git clone https://github.com/ORGANIZATION/Maliev.AccountingService.git
cd Maliev.AccountingService
```

2. **Spin up Infrastructure**
```bash
docker run --name accounting-db -e POSTGRES_PASSWORD=YOUR_PASSWORD -p 5432:5432 -d postgres:18-alpine
docker run --name accounting-redis -p 6379:6379 -d redis:7-alpine
```

3. **Configure Environment**
```powershell
# Windows PowerShell
$env:ConnectionStrings__AccountingDbContext="YOUR_POSTGRES_CONNECTION_STRING"
$env:ConnectionStrings__Cache="YOUR_REDIS_CONNECTION_STRING"
```

4. **Apply Migrations & Run**
```bash
dotnet ef database update --project Maliev.AccountingService.Api
dotnet run --project Maliev.AccountingService.Api
```

The service will be available at `http://localhost:5000/accounting`. Access the interactive documentation at `http://localhost:5000/accounting/scalar`.

---

## 📡 API Endpoints

All endpoints are prefixed with `/accounting/v1/`.

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/chart-of-accounts` | List and manage the chart of accounts |
| POST | `/journal-entries` | Create and post new journal entries |
| GET | `/reports/balance-sheet` | Generate the latest balance sheet |
| POST | `/periods/close` | Close an active fiscal period |

---

## 🏥 Health & Monitoring

Standardized health probes for Kubernetes orchestration:
- **Liveness**: `GET /accounting/liveness`
- **Readiness**: `GET /accounting/readiness` (Checks DB and Redis connectivity)
- **Metrics**: `GET /accounting/metrics` (Prometheus format)

---

## 🧪 Testing

We prioritize reliable tests over mock-heavy unit tests.

```bash
# Run all tests using Testcontainers
dotnet test --verbosity normal
```

- **Integration Tests**: Use real PostgreSQL 18 containers.
- **Contract Tests**: Ensure API stability for consumers.

---

## 📦 Deployment

Infrastructure management is handled via GitOps patterns.

- **Docker Image**: `REGION-docker.pkg.dev/PROJECT_ID/REPOSITORY/maliev-accounting-service:{sha}`
- **Environments**: Development, Staging, Production

---

## 📄 License

Proprietary - © 2025 MALIEV Co., Ltd. All rights reserved.
