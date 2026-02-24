# Implementation Plan: Accounting Service Core

**Branch**: `001-accounting-service-core` | **Date**: 2025-12-05 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `/specs/001-accounting-service-core/spec.md`

**Note**: This template is filled in by the `/speckit.plan` command. See `.specify/templates/commands/plan.md` for the execution workflow.

## Summary

The Accounting Service Core provides centralized financial record management for the MALIEV microservice ecosystem. It ingests financial events from Sales, Procurement, Inventory, and Payroll services, transforms them into double-entry accounting journal entries, maintains the chart of accounts, enforces financial period controls, tracks tax obligations, performs reconciliation, generates standard financial reports, and maintains comprehensive audit trails. The service guarantees data integrity through atomic transaction processing with rollback guarantees, prevents duplicate event processing via event ID tracking, and provides comprehensive observability through metrics, structured logging, and distributed tracing.

**Technical Approach**: ASP.NET Core web service with PostgreSQL persistence, RabbitMQ event-driven integration, Redis caching for performance, and full compliance with MALIEV constitution including .NET Aspire ServiceDefaults integration, Testcontainers-based real infrastructure testing, and OpenTelemetry observability.

## Technical Context

**Language/Version**: C# / .NET 10.0
**Primary Dependencies**:
- Maliev.Aspire.ServiceDefaults (via NuGet from GitHub Packages)
- Npgsql.EntityFrameworkCore.PostgreSQL (Database ORM)
- MassTransit.RabbitMQ (Event messaging)
- Microsoft.AspNetCore.OpenApi / Scalar.AspNetCore (API documentation)
- AspNetCore.HealthChecks.UI.Client (Health monitoring)

**Storage**: PostgreSQL for general ledger, chart of accounts, journal entries, audit trails, tax components; Redis for distributed caching and idempotency registry

**Testing**: xUnit, Moq, Testcontainers.PostgreSql, Testcontainers.RabbitMQ, Testcontainers.Redis (real infrastructure per constitution)

**Target Platform**: Linux containers (Docker), Kubernetes deployment, ASP.NET Core 10.0 runtime

**Project Type**: Web API microservice with event-driven integration

**Performance Goals**:
- Process 10,000 transaction events per hour minimum
- Event processing latency under 5 minutes (normal operations)
- Financial report generation under 2 minutes for any completed period
- Audit trail queries under 3 seconds for individual transactions
- 99.9% uptime for transaction ingestion

**Constraints**:
- Atomic transaction processing (all-or-nothing, no partial journal entries)
- Immutability of posted journal entries (100% prevention of modification/deletion)
- Idempotency guarantee via event ID tracking
- 7+ years historical data retention with full integrity
- Balanced debits/credits with zero tolerance for imbalance

**Scale/Scope**:
- Support 200+ distinct chart of accounts entries
- Handle multiple concurrent fiscal years and periods
- Track transactions across 4+ source systems (Sales, Procurement, Inventory, Payroll)
- Support time-based, account-based, customer/supplier filtering in reports
- Maintain complete audit trails for 100% of transactions

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

### Pre-Design Evaluation

| Principle | Status | Evidence |
|-----------|--------|----------|
| **I. Service Autonomy** | ✅ PASS | Accounting Service owns its PostgreSQL database (AccountingDbContext), domain logic for journal entries/reconciliation, exposes only APIs/events. No direct DB access to other services. |
| **II. Explicit Contracts** | ✅ PASS | Will document all APIs via OpenAPI/Scalar. Event contracts versioned with RabbitMQ routing keys (maliev.accounting.v1.*). |
| **III. Test-First Development** | ✅ PASS | Tests will be authored immediately after spec approval before implementation. Using xUnit with real infrastructure. |
| **IV. Real Infrastructure Testing** | ✅ PASS | Using Testcontainers for PostgreSQL, RabbitMQ, and Redis - no in-memory substitutes. Matches constitution requirement exactly. |
| **V. Auditability & Observability** | ✅ PASS | Spec requires comprehensive audit trails (FR-022, FR-023), structured logging with correlation IDs (FR-040), distributed tracing (FR-041), and metrics (FR-039). |
| **VI. Security & Compliance** | ✅ PASS | JWT authentication via User Service integration (FR-025), role-based authorization (FR-024), audit retention meets 7-year requirement (SC-016). |
| **VII. Secrets Management** | ✅ PASS | Will use Google Secret Manager volume mount (AddGoogleSecretManagerVolume), no secrets in code. |
| **VIII. Zero Warnings Policy** | ✅ PASS | Build configuration will enforce TreatWarningsAsErrors=true. |
| **IX. Clean Project Artifacts** | ✅ PASS | Will include .gitignore and .dockerignore excluding build artifacts, specs, IDE files, and Test projects. |
| **X. Docker Best Practices** | ✅ PASS | Will use built-in app user, multi-stage build (sdk:10.0/aspnet:10.0), BuildKit secrets for NuGet, port 8080, health check endpoint. |
| **XI. Simplicity & Maintainability** | ✅ PASS | Service follows single responsibility (accounting domain), stateless API design, no over-engineering. |
| **XII. Business Metrics & Analytics** | ✅ PASS | Spec defines business metrics: event processing rates, transaction volumes, reconciliation accuracy (SC-010), financial close cycle time (SC-011), error rates. All tagged with service_name, version, environment. |
| **XIII. .NET Aspire Integration** | ✅ PASS | Will consume ServiceDefaults as NuGet package from GitHub Packages, include nuget.config, use BuildKit secrets, call AddServiceDefaults() and MapDefaultEndpoints(). |
| **XIV. Code Quality & Library Standards** | ✅ PASS | Will use extension methods for mapping (no AutoMapper), DataAnnotations for validation (no FluentValidation), standard xUnit Assert (no FluentAssertions). |

**Gate Result**: ✅ **PASS** - All constitutional requirements satisfied. No violations requiring justification.

## Project Structure

### Documentation (this feature)

```text
specs/001-accounting-service-core/
├── spec.md              # Feature specification
├── plan.md              # This file (/speckit.plan command output)
├── research.md          # Phase 0 output (/speckit.plan command)
├── data-model.md        # Phase 1 output (/speckit.plan command)
├── quickstart.md        # Phase 1 output (/speckit.plan command)
├── contracts/           # Phase 1 output (/speckit.plan command)
│   ├── openapi.yaml     # REST API contract
│   └── events.md        # RabbitMQ event schemas
├── checklists/          # Quality validation
│   └── requirements.md  # Spec quality checklist (already exists)
└── tasks.md             # Phase 2 output (/speckit.tasks command - NOT created by /speckit.plan)
```

### Source Code (repository root)

```text
Maliev.AccountingService/
├── src/
│   └── Maliev.AccountingService.Api/
│       ├── Program.cs                        # Service bootstrap with ServiceDefaults
│       ├── AccountingDbContext.cs            # EF Core DbContext for accounting domain
│       ├── Controllers/                      # REST API endpoints
│       │   ├── ChartOfAccountsController.cs  # Account CRUD operations
│       │   ├── JournalEntriesController.cs   # Journal entry management
│       │   ├── PeriodsController.cs          # Fiscal period management
│       │   ├── ReconciliationController.cs   # Reconciliation tools
│       │   ├── ReportsController.cs          # Financial reporting
│       │   └── TaxController.cs              # Tax reporting
│       ├── Models/                           # Domain entities
│       │   ├── ChartOfAccount.cs             # Chart of accounts entity
│       │   ├── JournalEntry.cs               # Journal entry header
│       │   ├── JournalEntryLine.cs           # Journal entry line
│       │   ├── FinancialPeriod.cs            # Period entity
│       │   ├── FiscalYear.cs                 # Fiscal year entity
│       │   ├── TaxComponent.cs               # Tax tracking
│       │   ├── ReconciliationReport.cs       # Reconciliation result
│       │   ├── AuditTrailEntry.cs            # Audit log
│       │   ├── SubledgerTransaction.cs       # Source system transactions
│       │   ├── AdjustingEntryApproval.cs     # Adjusting entry workflow
│       │   └── ProcessedEventRegistry.cs     # Idempotency tracking
│       ├── DTOs/                             # Request/Response models
│       │   ├── Requests/                     # Incoming DTOs
│       │   └── Responses/                    # Outgoing DTOs
│       ├── Extensions/                       # Mapping extension methods
│       │   ├── ChartOfAccountExtensions.cs
│       │   ├── JournalEntryExtensions.cs
│       │   └── [OtherEntityExtensions].cs
│       ├── Services/                         # Business logic
│       │   ├── IEventProcessingService.cs
│       │   ├── EventProcessingService.cs     # Ingest events, create journal entries
│       │   ├── IJournalEntryService.cs
│       │   ├── JournalEntryService.cs        # Journal entry validation, posting
│       │   ├── IReconciliationService.cs
│       │   ├── ReconciliationService.cs      # Subledger reconciliation
│       │   ├── IReportingService.cs
│       │   ├── ReportingService.cs           # Financial report generation
│       │   ├── ITaxCalculationService.cs
│       │   └── TaxCalculationService.cs      # Tax calculation and reporting
│       ├── Consumers/                        # MassTransit event consumers
│       │   ├── InvoiceCreatedConsumer.cs     # From Sales service
│       │   ├── PaymentReceivedConsumer.cs    # From Sales service
│       │   ├── SupplierInvoiceConsumer.cs    # From Procurement service
│       │   ├── InventoryMovementConsumer.cs  # From Inventory service
│       │   └── PayrollProcessedConsumer.cs   # From Payroll service
│       ├── Events/                           # Published event models
│       │   └── TransactionPostedEvent.cs     # Notify downstream analytics
│       ├── Middleware/
│       │   └── ExceptionHandlingMiddleware.cs
│       └── Migrations/                       # EF Core migrations
│
├── tests/
│   └── Maliev.AccountingService.Tests/
│       ├── TestWebApplicationFactory.cs      # Custom factory with dynamic auth
│       ├── TestDatabaseFixture.cs            # Testcontainers PostgreSQL setup
│       ├── MockHttpMessageHandler.cs         # HTTP mocking (replaces WireMock)
│       ├── Integration/                      # API integration tests
│       │   ├── ChartOfAccountsTests.cs
│       │   ├── JournalEntryTests.cs
│       │   ├── PeriodManagementTests.cs
│       │   ├── ReconciliationTests.cs
│       │   ├── TaxReportingTests.cs
│       │   └── EventProcessingTests.cs       # End-to-end event flow tests
│       ├── Unit/                             # Service layer unit tests
│       │   ├── EventProcessingServiceTests.cs
│       │   ├── JournalEntryServiceTests.cs
│       │   ├── ReconciliationServiceTests.cs
│       │   ├── ReportingServiceTests.cs
│       │   └── TaxCalculationServiceTests.cs
│       └── Contract/                         # API contract validation
│           └── OpenApiContractTests.cs
│
├── Dockerfile                                # Multi-stage Docker build with BuildKit secrets
├── .dockerignore                             # Exclude build outputs, IDE files, specs, tests
├── nuget.config                              # GitHub Packages source with credential placeholders
├── appsettings.json                          # Configuration with ConnectionStrings (ServiceDbContext, redis, rabbitmq)
├── appsettings.Development.json
├── .gitignore                                # Exclude bin/, obj/, secrets, IDE files
└── README.md                                 # Service documentation
```

**Structure Decision**: Single microservice project (Maliev.AccountingService.Api) following MALIEV constitution. The service is self-contained with its own database schema, domain logic, and API surface. Uses standard ASP.NET Core Web API structure with Controllers for synchronous HTTP endpoints and MassTransit Consumers for asynchronous RabbitMQ event processing. Separates concerns into Models (entities), Services (business logic), DTOs (API contracts), Extensions (mapping), and Consumers (event handlers). Tests organized by type (Integration/Unit/Contract) with Testcontainers for real infrastructure testing per constitution Principle IV.

## Complexity Tracking

> **Fill ONLY if Constitution Check has violations that must be justified**

*No violations detected - this section intentionally left empty.*

## Phase 0: Research & Technical Decisions

*See [research.md](./research.md) for detailed analysis.*

**Key Research Topics**:
1. Double-entry bookkeeping validation patterns in C#
2. PostgreSQL transaction isolation levels for atomic journal entry creation
3. RabbitMQ consumer idempotency patterns with event ID deduplication
4. EF Core optimistic concurrency for preventing period closing race conditions
5. Exponential backoff retry policy configuration in MassTransit
6. OpenTelemetry distributed tracing context propagation across RabbitMQ
7. Redis-based processed event registry with TTL for idempotency
8. Financial report query optimization strategies (materialized views vs. aggregation)
9. Audit trail immutability guarantees with EF Core (shadow properties, temporal tables)
10. Chart of accounts hierarchical structure representation (adjacency list vs. nested sets)

## Phase 1: Design Artifacts

### Data Model

*See [data-model.md](./data-model.md) for complete entity relationship diagram and field specifications.*

**Core Entities**:
- ChartOfAccount (chart of accounts catalog)
- JournalEntry (transaction header)
- JournalEntryLine (debit/credit lines)
- FinancialPeriod (monthly/quarterly periods)
- FiscalYear (annual accounting cycles)
- TaxComponent (VAT tracking)
- ReconciliationReport (subledger vs. GL comparison)
- AuditTrailEntry (immutable change log)
- SubledgerTransaction (source system events)
- AdjustingEntryApproval (closed period adjustments)
- ProcessedEventRegistry (idempotency tracking)

### API Contracts

*See [contracts/](./contracts/) for complete OpenAPI specification and event schemas.*

**REST Endpoints**:
- `POST /api/v1/chart-of-accounts` - Create account
- `GET /api/v1/chart-of-accounts` - List accounts with hierarchy
- `PUT /api/v1/chart-of-accounts/{id}` - Update account
- `DELETE /api/v1/chart-of-accounts/{id}` - Deactivate account
- `POST /api/v1/journal-entries` - Create draft entry
- `POST /api/v1/journal-entries/{id}/post` - Post entry to ledger
- `GET /api/v1/journal-entries` - Query entries with filters
- `POST /api/v1/periods` - Create fiscal period
- `POST /api/v1/periods/{id}/close` - Close period
- `POST /api/v1/reconciliation` - Run reconciliation
- `GET /api/v1/reconciliation/{id}` - Get reconciliation report
- `GET /api/v1/reports/trial-balance` - Generate trial balance
- `GET /api/v1/reports/balance-sheet` - Generate balance sheet
- `GET /api/v1/reports/income-statement` - Generate income statement
- `GET /api/v1/reports/cash-flow` - Generate cash flow statement
- `GET /api/v1/tax/vat-report` - Generate VAT report

**RabbitMQ Event Subscriptions** (Consumed):
- `maliev.sales.v1.invoice.created`
- `maliev.sales.v1.payment.received`
- `maliev.procurement.v1.supplier-invoice.received`
- `maliev.inventory.v1.stock-movement.recorded`
- `maliev.payroll.v1.payroll.processed`

**RabbitMQ Event Publications** (Published):
- `maliev.accounting.v1.transaction.posted`
- `maliev.accounting.v1.period.closed`
- `maliev.accounting.v1.reconciliation.completed`

### Development Quickstart

*See [quickstart.md](./quickstart.md) for complete setup and development workflow.*

**Prerequisites**: .NET 10.0 SDK, Docker Desktop, PostgreSQL client, NuGet credentials for GitHub Packages

**Local Development Steps**:
1. Clone repository and restore NuGet packages (including Maliev.Aspire.ServiceDefaults)
2. Configure ConnectionStrings in appsettings.Development.json or user secrets
3. Run `dotnet ef database update` to apply migrations
4. Start dependencies (PostgreSQL, Redis, RabbitMQ) via Docker Compose
5. Run `dotnet run --project src/Maliev.AccountingService.Api`
6. Access Scalar API documentation at `https://localhost:5001/accounting/scalar/v1`
7. Access health checks at `https://localhost:5001/accounting/health`
8. Access metrics at `https://localhost:5001/accounting/metrics`

**Testing**:
1. Run all tests: `dotnet test`
2. Run with coverage: `dotnet test --collect:"XPlat Code Coverage"`
3. Tests automatically start Testcontainers for PostgreSQL, Redis, RabbitMQ

## Post-Design Constitution Re-Check

*GATE: Verify design artifacts comply with constitution.*

| Principle | Status | Evidence |
|-----------|--------|----------|
| **I. Service Autonomy** | ✅ PASS | Design confirms AccountingDbContext owns all entities. No cross-service DB access in contracts. |
| **II. Explicit Contracts** | ✅ PASS | contracts/openapi.yaml provides complete API documentation. Event schemas documented in contracts/events.md with versioned routing keys. |
| **III. Test-First Development** | ✅ PASS | Test structure defined in project layout with Integration/Unit/Contract separation. Tests will be written immediately after spec approval. |
| **IV. Real Infrastructure Testing** | ✅ PASS | TestDatabaseFixture uses Testcontainers.PostgreSql, Testcontainers.Redis, Testcontainers.RabbitMQ - confirmed in project structure. |
| **V. Auditability & Observability** | ✅ PASS | AuditTrailEntry entity captures all changes. Services will use ILogger with correlation IDs. OpenTelemetry via ServiceDefaults. |
| **VI. Security & Compliance** | ✅ PASS | JWT authentication via AddJwtAuthentication(), authorization on controllers, 7-year audit retention via AuditTrailEntry. |
| **VII. Secrets Management** | ✅ PASS | Program.cs calls AddGoogleSecretManagerVolume() first, uses environment variables for ConnectionStrings. |
| **VIII. Zero Warnings Policy** | ✅ PASS | Will configure TreatWarningsAsErrors in .csproj. |
| **IX. Clean Project Artifacts** | ✅ PASS | .gitignore and .dockerignore defined in structure excluding bin/, obj/, specs/, tests/. |
| **X. Docker Best Practices** | ✅ PASS | Dockerfile structure uses app user, multi-stage build, BuildKit secrets for NuGet, port 8080, health check at /accounting/liveness. |
| **XI. Simplicity & Maintainability** | ✅ PASS | Design uses extension methods for mapping (no AutoMapper), direct service injection (no complex abstractions), stateless controllers. |
| **XII. Business Metrics & Analytics** | ✅ PASS | Metrics defined: transaction_processing_rate, event_ingestion_count, reconciliation_discrepancy_count, report_generation_duration. Tagged with service_name="accounting", version, environment. |
| **XIII. .NET Aspire Integration** | ✅ PASS | Program.cs structure calls AddServiceDefaults() first, MapDefaultEndpoints(servicePrefix: "accounting"), nuget.config includes GitHub Packages source. |
| **XIV. Code Quality & Library Standards** | ✅ PASS | Extensions/ folder for explicit mapping, DataAnnotations on DTOs for validation, xUnit Assert in tests (no banned libraries). |

**Gate Result**: ✅ **PASS** - Design fully compliant with constitution. Ready to proceed to implementation.

## Implementation Phases

### Phase 2: Task Generation

*Use `/speckit.tasks` command to generate tasks.md with dependency-ordered implementation tasks.*

**Expected task categories**:
1. Infrastructure setup (DbContext, migrations, ServiceDefaults integration)
2. Core domain entities (ChartOfAccount, JournalEntry, FinancialPeriod)
3. Event processing pipeline (Consumers, EventProcessingService, idempotency)
4. Business services (JournalEntryService, ReconciliationService, TaxCalculationService)
5. API controllers (REST endpoints with authorization)
6. Financial reporting (ReportingService, query optimization)
7. Observability (metrics, logging, tracing instrumentation)
8. Testing (Integration tests with Testcontainers, Unit tests, Contract tests)
9. Documentation (API docs, deployment guide, runbook)
10. CI/CD pipeline (GitHub Actions, Docker build with BuildKit secrets)

### Phase 3: Implementation

*Execute tasks from tasks.md in dependency order. Each task includes acceptance criteria tied to spec requirements.*

### Phase 4: Validation

*Run all tests, verify metrics endpoints, validate OpenAPI contract, confirm constitutional compliance.*

## Success Criteria Mapping

*Links implementation plan elements to spec success criteria for validation.*

| Success Criterion | Implementation Element | Validation Method |
|-------------------|------------------------|-------------------|
| SC-001: 5 min event processing | EventProcessingService, exponential backoff retry | Performance test measuring end-to-end latency |
| SC-002: 100% balanced entries | JournalEntryService validation logic | Unit test + DB constraint check |
| SC-003: 10 min reconciliation | ReconciliationService with optimized queries | Integration test with sample data |
| SC-004: 2 min report generation | ReportingService with query optimization | Performance test for each report type |
| SC-005: 100% audit trails | AuditTrailEntry + EF change tracking | Integration test verifying audit capture |
| SC-006: 5 min tax reports | TaxCalculationService aggregation | Integration test with multi-period data |
| SC-007: 10k events/hour | MassTransit consumer throughput | Load test with concurrent events |
| SC-008: Immutable posted entries | JournalEntry status check, EF validation | Unit test attempting modification |
| SC-009: 99.9% uptime | Health checks, resilience policies | Chaos testing with infrastructure failures |
| SC-010: 50% reconciliation improvement | Automated discrepancy detection | Comparison test vs. manual process |
| SC-011: 40% faster close cycle | Period closing automation | Timed test vs. baseline |
| SC-012: 3 sec audit queries | Indexed queries on AuditTrailEntry | Performance test with large dataset |
| SC-013: 200+ accounts support | Chart of accounts capacity test | Integration test creating 200+ accounts |
| SC-014: 100% error capture | Dead-letter queue routing | Integration test with intentional failures |
| SC-015: 2 sec report access | Authorization + query performance | Integration test with auth token |
| SC-016: 7 year retention | Data archival strategy, query tests | Integration test querying old data |
| SC-017: End-to-end tracing | OpenTelemetry Activity propagation | Trace inspection across RabbitMQ |
| SC-018: 1 min metric granularity | Prometheus metrics emission | Metrics endpoint inspection |
| SC-019: 95% transient retry success | Exponential backoff configuration | Failure injection test with recovery |

## Notes

- **Event Schema Evolution**: Use RabbitMQ routing key versioning (v1, v2) for breaking changes. Consumers support multiple versions during transition period.
- **Performance Optimization**: Consider materialized views for frequently accessed reports after initial implementation validates query patterns.
- **Audit Trail Storage**: Evaluate separate audit database or table partitioning if audit volume exceeds 10M entries.
- **Idempotency Registry TTL**: Configure Redis TTL based on maximum expected retry window (e.g., 24 hours) to balance memory vs. duplicate detection.
- **Multi-Currency Support**: Currently out of scope per spec assumptions. Future phase would require currency conversion service integration and multi-currency financial reporting.
