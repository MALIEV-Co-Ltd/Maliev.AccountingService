# Tasks: Accounting Service Core

**Input**: Design documents from `/specs/001-accounting-service-core/`
**Prerequisites**: plan.md (required), spec.md (required), research.md, data-model.md, contracts/

**Tests**: Included per Constitution Principle III (Test-First Development)

**Organization**: Tasks grouped by user story to enable independent implementation and testing.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Path Conventions

- **Single microservice project**: `src/Maliev.AccountingService.Api/`, `tests/Maliev.AccountingService.Tests/`
- Paths based on plan.md structure

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Project initialization and basic structure

- [X] T001 Create repository structure per plan.md with src/, tests/, Dockerfile, nuget.config
- [X] T002 Initialize .NET 10.0 Web API project in src/Maliev.AccountingService.Api/
- [X] T003 [P] Add NuGet packages: Maliev.Aspire.ServiceDefaults, Npgsql.EntityFrameworkCore.PostgreSQL, MassTransit.RabbitMQ
- [X] T004 [P] Configure .gitignore excluding bin/, obj/, secrets, IDE files per Constitution
- [X] T005 [P] Configure .dockerignore excluding build outputs, IDE files, specs/, tests/ per Constitution
- [X] T006 [P] Create nuget.config with GitHub Packages source and credential placeholders per Constitution XIII
- [X] T007 Create appsettings.json with ConnectionStrings structure (ServiceDbContext, redis, rabbitmq)
- [X] T008 Create appsettings.Development.json with local development settings
- [X] T009 Create Dockerfile with multi-stage build, BuildKit secrets, app user per Constitution X
- [X] T010 [P] Create .csproj with TreatWarningsAsErrors=true per Constitution VIII
- [X] T011 [P] Initialize test project Maliev.AccountingService.Tests with xUnit, Moq, Testcontainers
- [ ] T011a [P] Create bulk import CLI command in Maliev.AccountingService.Api/Commands/BulkImportCommand.cs for initial chart of accounts and opening balances from CSV/JSON (supports FR-038)

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core infrastructure that MUST be complete before ANY user story can be implemented

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [X] T012 Create Program.cs with AddServiceDefaults(), AddGoogleSecretManagerVolume() per implementation guidelines
- [X] T013 Configure PostgreSQL DbContext in Program.cs using AddPostgresDbContext<AccountingDbContext>
- [X] T014 Configure Redis distributed cache in Program.cs using AddRedisDistributedCache
- [X] T015 Configure MassTransit with RabbitMQ in Program.cs using AddMassTransitWithRabbitMq
- [X] T016 Add JWT authentication in Program.cs using AddJwtAuthentication()
- [X] T017 Add CORS configuration in Program.cs using AddDefaultCors()
- [X] T018 Add API versioning in Program.cs (DefaultApiVersion 1.0)
- [X] T019 Add OpenAPI/Scalar in Program.cs using AddOpenApi and Scalar.AspNetCore
- [X] T020 Add health checks in Program.cs using AddHealthChecks() with DbContext check
- [X] T021 Configure middleware pipeline in Program.cs (ExceptionHandling, HTTPS, CORS, Auth, RateLimiter)
- [X] T022 Map default endpoints in Program.cs using MapDefaultEndpoints(servicePrefix: "accounting")
- [X] T023 Map API documentation in Program.cs using MapApiDocumentation(servicePrefix: "accounting")
- [X] T024 Create ExceptionHandlingMiddleware in src/Maliev.AccountingService.Api/Middleware/ExceptionHandlingMiddleware.cs
- [X] T025 Create AccountingDbContext in src/Maliev.AccountingService.Api/AccountingDbContext.cs with DbSets for all entities
- [X] T026 Configure DbContext OnModelCreating with entity configurations (indexes, constraints, relationships)
- [X] T027 Create TestWebApplicationFactory in tests/Maliev.AccountingService.Tests/TestWebApplicationFactory.cs with dynamic RSA key generation
- [X] T028 Create TestDatabaseFixture in tests/Maliev.AccountingService.Tests/TestDatabaseFixture.cs using Testcontainers.PostgreSql
- [X] T029 Create MockHttpMessageHandler in tests/Maliev.AccountingService.Tests/MockHttpMessageHandler.cs for HTTP mocking

**Checkpoint**: Foundation ready - user story implementation can now begin in parallel

---

## Phase 3: User Story 1 - Financial Transaction Recording (Priority: P1) 🎯 MVP

**Goal**: Automatically capture and record financial transactions from Sales, Procurement, Inventory, Payroll, transforming them into double-entry journal entries

**Independent Test**: Trigger sample financial events (invoice, payment, expense) and verify journal entries appear in ledger with balanced debits/credits and audit trails

### Tests for User Story 1

> **NOTE: Write these tests FIRST, ensure they FAIL before implementation**

- [X] T030 [P] [US1] Create EventProcessingTests in tests/Integration/EventProcessingTests.cs for end-to-end event flow validation
- [X] T031 [P] [US1] Write test: InvoiceCreated event creates balanced journal entry with AR debit, Revenue credit, VAT credit
- [X] T032 [P] [US1] Write test: PaymentReceived event creates journal entry with Cash debit, AR credit
- [X] T033 [P] [US1] Write test: SupplierInvoice event creates journal entry with Expense debit, VAT debit, AP credit
- [X] T034 [P] [US1] Write test: InventoryMovement event creates journal entry with Inventory debit, AP/Cash credit
- [X] T035 [P] [US1] Write test: PayrollProcessed event creates journal entry with Payroll Expense debit, liabilities credits
- [X] T036 [P] [US1] Write test: Event processing creates audit trail entry for each transaction
- [X] T037 [P] [US1] Write test: Duplicate event ID is rejected (idempotency check)

### Implementation for User Story 1

#### Entities & Database

- [X] T038 [P] [US1] Create JournalEntry entity in src/Models/JournalEntry.cs with status, period, amounts, row version
- [X] T039 [P] [US1] Create JournalEntryLine entity in src/Models/JournalEntryLine.cs with account, debit/credit, references
- [X] T040 [P] [US1] Create TaxComponent entity in src/Models/TaxComponent.cs with tax type, rate, amounts
- [X] T041 [P] [US1] Create SubledgerTransaction entity in src/Models/SubledgerTransaction.cs with source system, transaction details
- [X] T042 [P] [US1] Create AuditTrailEntry entity in src/Models/AuditTrailEntry.cs with entity tracking, before/after values
- [X] T043 [P] [US1] Create ProcessedEventRegistry entity in src/Models/ProcessedEventRegistry.cs with event ID, journal entry reference
- [X] T044 [US1] Create initial migration for User Story 1 entities using dotnet ef migrations add InitialSchema
- [X] T045 [US1] Add database constraints: balanced entry check, debit/credit validation, prevent posted entry modification trigger
- [X] T046 [US1] Add database indexes: journal entry period/status, processed event registry event ID, audit trail entity/timestamp

#### DTOs & Extensions

- [X] T047 [P] [US1] Create JournalEntryResponse DTO in src/DTOs/Responses/JournalEntryResponse.cs
- [X] T048 [P] [US1] Create CreateJournalEntryRequest DTO in src/DTOs/Requests/CreateJournalEntryRequest.cs with DataAnnotations validation
- [X] T049 [P] [US1] Create JournalEntryExtensions in src/Extensions/JournalEntryExtensions.cs with ToResponse(), ToEntity() mapping methods
- [X] T050 [P] [US1] Create event DTOs: InvoiceCreatedEvent, PaymentReceivedEvent, SupplierInvoiceEvent, InventoryMovementEvent, PayrollProcessedEvent in src/Events/

#### Services

- [X] T051 [US1] Create IEventIdempotencyService interface in src/Services/IEventIdempotencyService.cs
- [X] T052 [US1] Implement RedisEventIdempotencyService in src/Services/RedisEventIdempotencyService.cs with 24h TTL tracking
- [X] T053 [US1] Create IAuditService interface in src/Services/IAuditService.cs
- [X] T054 [US1] Implement AuditService in src/Services/AuditService.cs with append-only audit trail creation
- [X] T055 [US1] Create IEventProcessingService interface in src/Services/IEventProcessingService.cs
- [X] T056 [US1] Implement EventProcessingService in src/Services/EventProcessingService.cs with event-to-journal-entry transformation logic
- [X] T057 [US1] Add atomic transaction processing in EventProcessingService using Serializable isolation level
- [X] T058 [US1] Add double-entry balance validation in EventProcessingService (total debits = total credits)
- [X] T059 [US1] Add tax calculation logic in EventProcessingService for VAT input/output
- [X] T060 [US1] Add audit trail generation in EventProcessingService for each processed event

#### Event Consumers

- [X] T061 [P] [US1] Create InvoiceCreatedConsumer in src/Consumers/InvoiceCreatedConsumer.cs with idempotency check, distributed tracing
- [X] T062 [P] [US1] Create PaymentReceivedConsumer in src/Consumers/PaymentReceivedConsumer.cs with idempotency check, distributed tracing
- [X] T063 [P] [US1] Create SupplierInvoiceConsumer in src/Consumers/SupplierInvoiceConsumer.cs with idempotency check, distributed tracing
- [X] T064 [P] [US1] Create InventoryMovementConsumer in src/Consumers/InventoryMovementConsumer.cs with idempotency check, distributed tracing
- [X] T065 [P] [US1] Create PayrollProcessedConsumer in src/Consumers/PayrollProcessedConsumer.cs with idempotency check, distributed tracing
- [X] T066 [US1] Configure MassTransit exponential backoff retry (3 retries, 2s/4s/8s, max 60s) for transient errors
- [X] T067 [US1] Configure MassTransit dead-letter queue routing for permanent errors (validation failures)
- [X] T068 [US1] Configure MassTransit circuit breaker (15 failures in 1 min, 5 min reset)

#### Observability

- [X] T069 [P] [US1] Add OpenTelemetry Activity instrumentation in EventProcessingService with span tags (event.id, journal.entry.id)
- [X] T070 [P] [US1] Add metrics emission: event_ingestion_count, transaction_processing_rate, processing_latency_ms
- [X] T071 [P] [US1] Add structured logging with correlation IDs in all consumers and services

#### API Controllers

- [X] T071a [US1] Create JournalEntriesController in src/Controllers/JournalEntriesController.cs with [ApiController] and [Authorize]
- [X] T071b [P] [US1] Implement GET /api/v1/journal-entries endpoint with filtering by date range, account, transaction type, customer, supplier (supports FR-032)
- [X] T071c [P] [US1] Implement POST /api/v1/journal-entries endpoint for manual draft journal entry creation
- [X] T071d [P] [US1] Implement POST /api/v1/journal-entries/{id}/post endpoint for posting draft entries to ledger
- [X] T071e [US1] Add role-based authorization: accountant role required for journal entry operations

#### Integration

- [X] T072 [US1] Register all services in Program.cs DI container (IEventProcessingService, IEventIdempotencyService, IAuditService)
- [X] T073 [US1] Configure RabbitMQ routing keys: maliev.sales.v1.invoice.created, maliev.sales.v1.payment.received, maliev.procurement.v1.supplier-invoice.received, maliev.inventory.v1.stock-movement.recorded, maliev.payroll.v1.payroll.processed
- [X] T074 [US1] Run all User Story 1 tests and verify they pass

**Checkpoint**: User Story 1 complete - Can ingest events and create balanced journal entries with audit trails

---

## Phase 4: User Story 2 - Chart of Accounts Management (Priority: P1)

**Goal**: Define and maintain company's chart of accounts with proper categorization and hierarchy for consistent transaction classification

**Independent Test**: Create, update, organize accounts (Assets, Liabilities, Equity, Revenue, Expenses), verify transactions can only post to valid active accounts

### Tests for User Story 2

- [X] T075 [P] [US2] Create ChartOfAccountsTests in tests/Integration/ChartOfAccountsTests.cs
- [X] T076 [P] [US2] Write test: Create new account with type and category, verify added to chart
- [X] T077 [P] [US2] Write test: Update account category, verify future reporting reflects change
- [X] T078 [P] [US2] Write test: Deactivate account, verify visible in historical reports but not available for new transactions
- [X] T079 [P] [US2] Write test: Query account hierarchy, verify parent-child relationships displayed correctly
- [X] T080 [P] [US2] Write test: Duplicate account number rejected with error

### Implementation for User Story 2

#### Entities & Database

- [X] T081 [P] [US2] Create ChartOfAccount entity in src/Models/ChartOfAccount.cs with account number, name, type, category, parent, is_active
- [X] T082 [US2] Create migration for ChartOfAccount table with self-referencing foreign key for hierarchy
- [ ] T083 [US2] Add database constraints: account type enum check, account number format regex, unique account number
- [X] T084 [US2] Add database indexes: account number unique, parent account ID, type, is_active partial index

#### DTOs & Extensions

- [X] T085 [P] [US2] Create ChartOfAccountResponse DTO in src/DTOs/Responses/ChartOfAccountResponse.cs
- [X] T086 [P] [US2] Create CreateChartOfAccountRequest DTO in src/DTOs/Requests/CreateChartOfAccountRequest.cs with DataAnnotations
- [X] T087 [P] [US2] Create UpdateChartOfAccountRequest DTO in src/DTOs/Requests/UpdateChartOfAccountRequest.cs
- [X] T088 [P] [US2] Create ChartOfAccountExtensions in src/Extensions/ChartOfAccountExtensions.cs with ToResponse(), ToEntity()

#### Services

- [X] T089 [US2] Create IChartOfAccountsService interface in src/Services/IChartOfAccountsService.cs
- [X] T090 [US2] Implement ChartOfAccountsService in src/Services/ChartOfAccountsService.cs with CRUD operations
- [X] T091 [US2] Add hierarchical query using PostgreSQL recursive CTE in ChartOfAccountsService
- [X] T092 [US2] Add validation: prevent deactivating account with transactions in open periods
- [X] T093 [US2] Add validation: parent account must be same or higher-level type
- [X] T094 [US2] Add audit logging for all chart of accounts changes

#### API Controllers

- [X] T095 [US2] Create ChartOfAccountsController in src/Controllers/ChartOfAccountsController.cs with [ApiController] and [Authorize]
- [X] T096 [P] [US2] Implement POST /accounting/v1/chart-of-accounts endpoint for creating accounts
- [X] T097 [P] [US2] Implement GET /accounting/v1/chart-of-accounts endpoint for listing accounts with hierarchy and filters
- [X] T098 [P] [US2] Implement PUT /accounting/v1/chart-of-accounts/{id} endpoint for updating accounts
- [X] T099 [P] [US2] Implement DELETE /accounting/v1/chart-of-accounts/{id} endpoint for deactivating accounts
- [X] T100 [US2] Add role-based authorization: financial_controller role required for chart modifications

#### Integration

- [X] T101 [US2] Update JournalEntryLine to reference ChartOfAccount entity
- [X] T102 [US2] Add validation in EventProcessingService: verify account exists and is active before creating journal lines
- [ ] T103 [US2] Create SeedDataExtensions.cs with SeedStandardChartOfAccounts() method, called from Program.cs when --seed-data arg present (Assets, Liabilities, Equity, Revenue, Expenses structure)
- [X] T104 [US2] Run all User Story 2 tests and verify they pass

**Checkpoint**: User Stories 1 AND 2 complete - Can manage chart of accounts and transactions reference valid accounts

---

## Phase 5: User Story 3 - Financial Period Management and Closing (Priority: P2)

**Goal**: Define fiscal periods, manage opening/closing cycles, lock closed periods to prevent unauthorized changes for audit compliance

**Independent Test**: Create fiscal year/period structures, post transactions to open periods, close periods, verify closed periods reject new transactions except authorized adjustments

### Tests for User Story 3

- [ ] T105 [P] [US3] Create PeriodManagementTests in tests/Integration/PeriodManagementTests.cs
- [ ] T106 [P] [US3] Write test: Configure fiscal year with monthly periods, verify all created in open status
- [ ] T107 [P] [US3] Write test: Close period with balanced transactions, verify marked closed and rejects new postings
- [ ] T108 [P] [US3] Write test: Attempt post to closed period, verify rejection with period locked error
- [ ] T109 [P] [US3] Write test: Create adjusting entry with approval for closed period, verify allowed with audit trail
- [ ] T110 [P] [US3] Write test: Query multiple periods, verify open/closed status clearly indicated

### Implementation for User Story 3

#### Entities & Database

- [ ] T111 [P] [US3] Create FiscalYear entity in src/Models/FiscalYear.cs with name, start/end dates, period structure, is_active
- [ ] T112 [P] [US3] Create FinancialPeriod entity in src/Models/FinancialPeriod.cs with name, dates, status, closed_by, row_version
- [ ] T113 [P] [US3] Create AdjustingEntryApproval entity in src/Models/AdjustingEntryApproval.cs with journal entry reference, requester, approver, reason
- [ ] T114 [US3] Create migration for FiscalYear, FinancialPeriod, AdjustingEntryApproval tables
- [ ] T115 [US3] Add database constraints: end date > start date, period status enum check, only one active fiscal year, no overlapping periods
- [ ] T116 [US3] Add database indexes: fiscal year dates, financial period fiscal year ID, period status, period dates
- [ ] T117 [US3] Update JournalEntry to add foreign key to FinancialPeriod

#### DTOs & Extensions

- [ ] T118 [P] [US3] Create FiscalYearResponse, FinancialPeriodResponse DTOs in src/DTOs/Responses/
- [ ] T119 [P] [US3] Create CreateFiscalYearRequest, CreateFinancialPeriodRequest DTOs in src/DTOs/Requests/
- [ ] T120 [P] [US3] Create extensions: FiscalYearExtensions, FinancialPeriodExtensions in src/Extensions/

#### Services

- [ ] T121 [US3] Create IPeriodManagementService interface in src/Services/IPeriodManagementService.cs
- [ ] T122 [US3] Implement PeriodManagementService in src/Services/PeriodManagementService.cs with period CRUD and close operations
- [ ] T123 [US3] Add period closing logic with SELECT FOR UPDATE pessimistic locking to prevent concurrent closes
- [ ] T124 [US3] Add validation: cannot close period with draft journal entries
- [ ] T125 [US3] Add adjusting entry approval workflow in PeriodManagementService
- [ ] T126 [US3] Add audit logging for period status changes

#### API Controllers

- [ ] T127 [US3] Create PeriodsController in src/Controllers/PeriodsController.cs with [ApiController] and [Authorize]
- [ ] T128 [P] [US3] Implement POST /api/v1/periods endpoint for creating periods
- [ ] T129 [P] [US3] Implement GET /api/v1/periods endpoint for listing periods
- [ ] T130 [P] [US3] Implement POST /api/v1/periods/{id}/close endpoint for closing periods
- [ ] T131 [US3] Add role-based authorization: financial_controller role required for period operations

#### Integration

- [ ] T132 [US3] Update EventProcessingService to validate period is open before creating journal entries
- [ ] T133 [US3] Update EventProcessingService to assign journal entry to correct period based on transaction date
- [ ] T134 [US3] Add concurrency handling in PeriodManagementService for DbUpdateConcurrencyException and NpgsqlException (lock not available)
- [ ] T135 [US3] Run all User Story 3 tests and verify they pass

**Checkpoint**: User Stories 1, 2, AND 3 complete - Can manage periods, transactions respect period status

---

## Phase 6: User Story 4 - Financial Reconciliation and Validation (Priority: P2)

**Goal**: Automated tools to detect discrepancies between subledgers and GL, unbalanced entries, orphaned transactions, mismatched payments

**Independent Test**: Create intentional discrepancies (unbalanced entries, orphaned payments, mismatched amounts), verify reconciliation identifies errors with sufficient detail

### Tests for User Story 4

- [ ] T136 [P] [US4] Create ReconciliationTests in tests/Integration/ReconciliationTests.cs
- [ ] T137 [P] [US4] Write test: Subledger AR $50k vs GL $48.5k, verify reconciliation identifies $1.5k discrepancy with transaction list
- [ ] T138 [P] [US4] Write test: Unbalanced journal entry (debits ≠ credits), verify flagged and prevented from posting
- [ ] T139 [P] [US4] Write test: Payment references nonexistent invoice, verify identified as orphaned with alert
- [ ] T140 [P] [US4] Write test: Invoice $2k vs payment $1.8k, verify mismatch flagged as $200 underpaid
- [ ] T141 [P] [US4] Write test: Access reconciliation history, verify all runs visible with resolution status

### Implementation for User Story 4

#### Entities & Database

- [ ] T142 [P] [US4] Create ReconciliationReport entity in src/Models/ReconciliationReport.cs with type, run_at, totals, variance, status, discrepancy_details
- [ ] T143 [US4] Create migration for ReconciliationReport table
- [ ] T144 [US4] Add database constraints: variance equals subledger_total - general_ledger_total
- [ ] T145 [US4] Add database indexes: period ID + reconciliation type, status partial index for discrepancies

#### DTOs & Extensions

- [ ] T146 [P] [US4] Create ReconciliationReportResponse DTO in src/DTOs/Responses/ReconciliationReportResponse.cs
- [ ] T147 [P] [US4] Create RunReconciliationRequest DTO in src/DTOs/Requests/RunReconciliationRequest.cs
- [ ] T148 [P] [US4] Create ReconciliationExtensions in src/Extensions/ReconciliationExtensions.cs

#### Services

- [ ] T149 [US4] Create IReconciliationService interface in src/Services/IReconciliationService.cs
- [ ] T150 [US4] Implement ReconciliationService in src/Services/ReconciliationService.cs with subledger vs GL comparison logic
- [ ] T151 [US4] Add Accounts Receivable reconciliation: compare SubledgerTransaction (Sales) totals with JournalEntry AR account balance
- [ ] T152 [US4] Add Accounts Payable reconciliation: compare SubledgerTransaction (Procurement) totals with JournalEntry AP account balance
- [ ] T153 [US4] Add Inventory reconciliation: compare SubledgerTransaction (Inventory) totals with JournalEntry Inventory account balance
- [ ] T154 [US4] Add unbalanced entry detection: query JournalEntry where total_debit ≠ total_credit
- [ ] T155 [US4] Add orphaned transaction detection: find SubledgerTransaction with no matching journal_entry_id
- [ ] T156 [US4] Add payment-to-invoice matching: compare SubledgerTransaction payment amount with invoice amount
- [ ] T157 [US4] Add discrepancy flagging logic: create alerts (not automatic corrections) per clarification decision
- [ ] T158 [US4] Add audit logging for all reconciliation runs

#### API Controllers

- [ ] T159 [US4] Create ReconciliationController in src/Controllers/ReconciliationController.cs with [ApiController] and [Authorize]
- [ ] T160 [P] [US4] Implement POST /api/v1/reconciliation endpoint for running reconciliation
- [ ] T161 [P] [US4] Implement GET /api/v1/reconciliation/{id} endpoint for retrieving reconciliation report
- [ ] T162 [P] [US4] Implement GET /api/v1/reconciliation endpoint for listing reconciliation history
- [ ] T163 [US4] Add role-based authorization: accountant role required for reconciliation operations

#### Integration

- [ ] T164 [US4] Update EventProcessingService to create SubledgerTransaction record for each ingested event
- [ ] T165 [US4] Add notification mechanism in ReconciliationService: log alerts, emit metrics for discrepancies
- [ ] T166 [US4] Run all User Story 4 tests and verify they pass

**Checkpoint**: User Stories 1-4 complete - Can detect and report financial discrepancies

---

## Phase 7: User Story 5 - Tax Tracking and Reporting (Priority: P2)

**Goal**: Automatically track VAT and tax obligations, distinguish input vs output tax, provide aggregated tax summaries for filing

**Independent Test**: Process transactions with various tax components, generate tax reports, verify VAT output/input/net calculated correctly

### Tests for User Story 5

- [ ] T167 [P] [US5] Create TaxReportingTests in tests/Integration/TaxReportingTests.cs
- [ ] T168 [P] [US5] Write test: Customer invoice $1k + 15% VAT, verify $150 recorded as VAT output tax
- [ ] T169 [P] [US5] Write test: Supplier invoice $500 + 15% VAT, verify $75 recorded as VAT input tax
- [ ] T170 [P] [US5] Write test: Generate VAT report for March 2025, verify output/input/net totals with transaction details
- [ ] T171 [P] [US5] Write test: VAT-exempt invoice, verify zero VAT recorded and excluded from output tax
- [ ] T172 [P] [US5] Write test: Tax rate change April 1, verify correct rate applied based on transaction date

### Implementation for User Story 5

#### Services

- [ ] T173 [US5] Create ITaxCalculationService interface in src/Services/ITaxCalculationService.cs
- [ ] T174 [US5] Implement TaxCalculationService in src/Services/TaxCalculationService.cs with VAT calculation logic
- [ ] T175 [US5] Add tax rate lookup by transaction date (support rate changes over time)
- [ ] T176 [US5] Add VAT output tax calculation: sales invoices tax tracking
- [ ] T177 [US5] Add VAT input tax calculation: supplier invoices tax tracking
- [ ] T178 [US5] Add VAT-exempt transaction handling (zero tax rate, exclude from calculations)
- [ ] T179 [US5] Add tax report generation: aggregate TaxComponent by period and tax type
- [ ] T180 [US5] Add net tax payable/receivable calculation: output tax - input tax

#### DTOs

- [ ] T181 [P] [US5] Create VATReportResponse DTO in src/DTOs/Responses/VATReportResponse.cs
- [ ] T182 [P] [US5] Create GenerateVATReportRequest DTO in src/DTOs/Requests/GenerateVATReportRequest.cs

#### API Controllers

- [ ] T183 [US5] Create TaxController in src/Controllers/TaxController.cs with [ApiController] and [Authorize]
- [ ] T184 [P] [US5] Implement GET /api/v1/tax/vat-report endpoint for VAT report generation
- [ ] T185 [US5] Add role-based authorization: tax_compliance_officer role required for tax reports

#### Integration

- [ ] T186 [US5] Update EventProcessingService to call TaxCalculationService when creating journal entries with taxable amounts
- [ ] T187 [US5] Ensure TaxComponent entities created for all taxable journal entry lines
- [ ] T188 [US5] Associate TaxComponent with correct reporting period (FinancialPeriod)
- [ ] T189 [US5] Run all User Story 5 tests and verify they pass

**Checkpoint**: User Stories 1-5 complete - Can track and report tax obligations

---

## Phase 8: User Story 6 - Financial Reporting and Dashboards (Priority: P3)

**Goal**: Access standard financial reports (trial balance, balance sheet, income statement, cash flow) with flexible filtering

**Independent Test**: Record representative transactions, generate each report type, verify figures accurately reflect underlying data with proper classifications

### Tests for User Story 6

- [ ] T190 [P] [US6] Create ReportingTests in tests/Integration/ReportingTests.cs
- [ ] T191 [P] [US6] Write test: Generate trial balance, verify each account shows debit/credit balance and totals equal
- [ ] T192 [P] [US6] Write test: Generate balance sheet as of Dec 31, verify Assets = Liabilities + Equity with current/non-current categorization
- [ ] T193 [P] [US6] Write test: Generate income statement for Q4, verify revenues, expenses by category, net income
- [ ] T194 [P] [US6] Write test: Generate cash flow statement for 2024, verify operating/investing/financing activities with net cash change
- [ ] T195 [P] [US6] Write test: Filter income statement by customer, verify only customer-specific revenues/expenses shown
- [ ] T196 [P] [US6] Write test: Unauthorized user attempts access, verify denial and audit log entry

### Implementation for User Story 6

#### Services

- [ ] T197 [US6] Create IReportingService interface in src/Services/IReportingService.cs
- [ ] T198 [US6] Implement ReportingService in src/Services/ReportingService.cs with report generation logic
- [ ] T199 [US6] Add trial balance generation: aggregate JournalEntryLine by account with debit/credit totals
- [ ] T200 [US6] Add balance sheet generation: categorize accounts by type (Asset/Liability/Equity) with current/non-current classification
- [ ] T201 [US6] Add income statement generation: aggregate Revenue and Expense accounts by category for time period
- [ ] T202 [US6] Add cash flow statement generation: categorize cash movements by operating/investing/financing activities
- [ ] T203 [US6] Add report filtering: time period, account, customer, supplier filters
- [ ] T204 [US6] Add query optimization: indexed queries on journal_entry_lines with period_id + status + account_id composite index
- [ ] T205 [US6] Add distributed tracing instrumentation with span tags (report.type, report.rows, report.generation.ms)

#### DTOs

- [ ] T206 [P] [US6] Create TrialBalanceResponse, BalanceSheetResponse, IncomeStatementResponse, CashFlowResponse DTOs in src/DTOs/Responses/
- [ ] T207 [P] [US6] Create GenerateReportRequest DTOs in src/DTOs/Requests/

#### API Controllers

- [ ] T208 [US6] Create ReportsController in src/Controllers/ReportsController.cs with [ApiController] and [Authorize]
- [ ] T209 [P] [US6] Implement GET /api/v1/reports/trial-balance endpoint
- [ ] T210 [P] [US6] Implement GET /api/v1/reports/balance-sheet endpoint
- [ ] T211 [P] [US6] Implement GET /api/v1/reports/income-statement endpoint
- [ ] T212 [P] [US6] Implement GET /api/v1/reports/cash-flow endpoint
- [ ] T213 [US6] Add role-based authorization: finance_executive role required for financial reports
- [ ] T214 [US6] Add audit logging for all report access attempts

#### Integration

- [ ] T215 [US6] Run all User Story 6 tests and verify they pass
- [ ] T216 [US6] Validate report generation completes within 2 minutes per SC-004

**Checkpoint**: User Stories 1-6 complete - Can generate all standard financial reports

---

## Phase 9: User Story 7 - Audit Trail and Data Integrity (Priority: P2)

**Goal**: Comprehensive audit trails showing who/what/when for each transaction, guarantees posted entries cannot be altered/deleted

**Independent Test**: Perform operations (create, modify, post, attempt delete), verify audit trail logs all actions with user/timestamp/before-after, verify immutability prevents unauthorized modifications

### Tests for User Story 7

- [ ] T217 [P] [US7] Create AuditTrailTests in tests/Integration/AuditTrailTests.cs
- [ ] T218 [P] [US7] Write test: Create draft journal entry, verify audit trail shows creator, timestamp, initial values
- [ ] T219 [P] [US7] Write test: Modify draft entry, verify audit trail shows each modification with before/after values
- [ ] T220 [P] [US7] Write test: Post entry to ledger, attempt modification, verify rejection and unauthorized attempt logged
- [ ] T221 [P] [US7] Write test: Adjusting entry for closed period, verify audit trail documents authorization and approver
- [ ] T222 [P] [US7] Write test: Query audit trail by transaction ID, verify complete chronological history with all actors
- [ ] T223 [P] [US7] Write test: Access historical audit data from 7 years ago, verify full integrity

### Implementation for User Story 7

#### Database

- [ ] T224 [US7] Add database migration to revoke UPDATE/DELETE permissions on audit_trail table (append-only enforcement)
- [ ] T225 [US7] Add database trigger on journal_entries to prevent modification/deletion when status = Posted
- [ ] T226 [US7] Add database indexes on audit_trail: entity_type + entity_id + timestamp composite, user_id, correlation_id

#### Services

- [ ] T227 [US7] Update AuditService to capture before/after snapshots using JsonSerializer for all entity changes
- [ ] T228 [US7] Add audit trail query methods in AuditService: GetByEntityAsync, GetByUserAsync, GetByCorrelationIdAsync
- [ ] T229 [US7] Add HTTP context integration in AuditService to capture IP address and correlation ID from distributed trace

#### Integration

- [ ] T230 [US7] Update all services (EventProcessingService, ChartOfAccountsService, PeriodManagementService) to call AuditService for all data changes
- [ ] T231 [US7] Update JournalEntryService (if created) to prevent modification of posted entries with explicit status check
- [ ] T232 [US7] Add EF Core SaveChanges interceptor to automatically create audit trail entries for all entity changes
- [ ] T233 [US7] Run all User Story 7 tests and verify they pass
- [ ] T234 [US7] Validate audit trail queries complete within 3 seconds per SC-012

**Checkpoint**: User Stories 1-7 complete - Full audit trail and immutability guarantees in place

---

## Phase 10: User Story 8 - Event-Driven Integration with Microservices (Priority: P3)

**Goal**: Subscribe to financial events via message queuing, process reliably, publish results to downstream analytics, handle failures gracefully

**Independent Test**: Publish sample events to queues, verify consumption, journal entry creation, result publication, graceful failure handling (retries, DLQ, error notifications)

### Tests for User Story 8

- [ ] T235 [P] [US8] Create integration tests using Testcontainers.RabbitMQ in tests/Integration/EventProcessingTests.cs
- [ ] T236 [P] [US8] Write test: Publish InvoiceCreated event to queue, verify consumption, journal entry creation, message acknowledgment
- [ ] T237 [P] [US8] Write test: Publish malformed event, verify error logged, moved to DLQ, other events continue processing
- [ ] T238 [P] [US8] Write test: Journal entry posted successfully, verify TransactionPosted event published to analytics queue
- [ ] T239 [P] [US8] Write test: Accounting service unavailable, messages accumulate, service resumes from last acknowledged position on recovery
- [ ] T240 [P] [US8] Write test: Access service metrics, verify event consumption rates, processing latency, error rates, queue depths visible

### Implementation for User Story 8

#### Event Publishing

- [ ] T241 [P] [US8] Create TransactionPostedEvent in src/Events/TransactionPostedEvent.cs with journal entry details and accounts affected
- [ ] T242 [P] [US8] Create PeriodClosedEvent in src/Events/PeriodClosedEvent.cs with period details and transaction counts
- [ ] T243 [P] [US8] Create ReconciliationCompletedEvent in src/Events/ReconciliationCompletedEvent.cs with reconciliation results

#### Services

- [ ] T244 [US8] Update EventProcessingService to publish TransactionPostedEvent after successful journal entry posting
- [ ] T245 [US8] Update PeriodManagementService to publish PeriodClosedEvent after period closure
- [ ] T246 [US8] Update ReconciliationService to publish ReconciliationCompletedEvent after reconciliation run

#### MassTransit Configuration

- [ ] T247 [US8] Configure MassTransit InMemoryOutbox and EntityFrameworkOutbox in Program.cs for at-least-once delivery
- [ ] T248 [US8] Configure RabbitMQ exchange and queue durability settings (durable, non-auto-delete)
- [ ] T249 [US8] Configure consumer prefetch count to 10 for optimal throughput
- [ ] T250 [US8] Add W3C Trace Context propagation in MassTransit using PropagateActivityContext()

#### Observability

- [ ] T251 [P] [US8] Add OpenTelemetry MassTransit instrumentation with AddSource("MassTransit")
- [ ] T252 [P] [US8] Add metrics: rabbitmq_consumer_rate, rabbitmq_processing_latency, rabbitmq_dlq_count, rabbitmq_error_rate
- [ ] T253 [P] [US8] Add alerting thresholds: DLQ count > 10, processing latency > 5 min, idempotency cache miss rate > 5%

#### Integration

- [ ] T254 [US8] Configure all RabbitMQ routing keys per contracts/events.md (maliev.accounting.v1.transaction.posted, etc.)
- [ ] T255 [US8] Run all User Story 8 tests and verify they pass
- [ ] T256 [US8] Validate event processing handles 10,000 events/hour per SC-007

**Checkpoint**: All 8 user stories complete - Full event-driven integration operational

---

## Phase 11: Polish & Cross-Cutting Concerns

**Purpose**: Improvements affecting multiple user stories

### Documentation

- [ ] T257 [P] Create README.md with service overview, architecture diagram, getting started guide
- [ ] T258 [P] Create API documentation review: verify Scalar endpoints accurate per contracts/openapi.yaml
- [ ] T259 [P] Create deployment guide in docs/DEPLOYMENT.md with Kubernetes manifests, environment variables
- [ ] T260 [P] Create runbook in docs/RUNBOOK.md with common operations, troubleshooting, monitoring

### Business Metrics & Analytics

- [ ] T261 [P] Add business metrics per Constitution XII: transaction_count, reconciliation_accuracy_improvement, financial_close_cycle_time_reduction
- [ ] T262 [P] Tag all metrics with service_name="accounting", version, environment, region
- [ ] T263 [P] Validate metrics exposed at /accounting/metrics endpoint in Prometheus format
- [ ] T264 [P] Create metrics validation test in tests/Integration/MetricsTests.cs

### Performance Optimization

- [ ] T265 [P] Review query performance for financial reports: add materialized views if any report exceeds 2-minute target
- [ ] T266 [P] Review database indexes: ensure composite indexes cover all report queries
- [ ] T267 [P] Add database connection pooling configuration for optimal concurrency
- [ ] T268 [P] Run load test: validate 10,000 events/hour throughput per SC-007

### Data Retention & Archival

- [ ] T268a [P] Implement data archival policy: move journal entries, audit trails, and reconciliation reports older than 1 year to cold storage tier (supports FR-029)
- [ ] T268b [P] Create archival verification script in src/Scripts/VerifyArchivalIntegrity.ps1 to validate archived data integrity and recoverability
- [ ] T268c [P] Add archival metrics: archived_records_count, archival_operation_duration_ms, archival_storage_size_bytes
- [ ] T268d [US7] Validate historical audit data remains accessible and retrievable with full integrity for 7+ years per SC-016 and FR-029

### Security Hardening

- [ ] T269 [P] Validate all sensitive endpoints require authentication and role-based authorization
- [ ] T270 [P] Verify no secrets in code, all credentials from Google Secret Manager per Constitution VII
- [ ] T271 [P] Run security scan: verify no SQL injection, XSS, or OWASP Top 10 vulnerabilities
- [ ] T272 [P] Validate HTTPS redirection enforced in Program.cs per implementation guidelines

### Code Quality

- [ ] T273 [P] Run build: verify zero warnings per Constitution VIII
- [ ] T274 [P] Run code formatter: ensure consistent style across all files
- [ ] T275 [P] Remove unused files, outdated docs per Constitution IX
- [ ] T276 [P] Verify .dockerignore excludes specs/, tests/, IDE files per Constitution X

### CI/CD Pipeline

- [ ] T277 Create GitHub Actions workflow .github/workflows/build-and-test.yml with dotnet build, dotnet test
- [ ] T278 [P] Add Docker build step in CI with BuildKit secrets for NuGet credentials per Constitution XIII
- [ ] T279 [P] Add test coverage reporting: minimum 80% coverage for business logic per Constitution III
- [ ] T280 [P] Add Scalar OpenAPI contract validation in CI

### Final Validation

- [ ] T281 Run all integration tests across all user stories: verify 100% pass rate
- [ ] T282 Run quickstart.md validation: verify local development setup works end-to-end
- [ ] T283 Validate all success criteria from spec.md achieved (SC-001 through SC-019)
- [ ] T284 Run constitution compliance check: verify all 14 principles satisfied

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion - BLOCKS all user stories
- **User Stories (Phases 3-10)**: All depend on Foundational phase completion
  - US1 & US2 (P1): Should complete first (foundational capabilities)
  - US3-US5 & US7 (P2): Can start after US1/US2 complete
  - US6 & US8 (P3): Can start after US1-US5 complete
- **Polish (Phase 11)**: Depends on all desired user stories being complete

### User Story Dependencies

- **US1 (Financial Transaction Recording)**: No dependencies on other stories - FOUNDATIONAL
- **US2 (Chart of Accounts Management)**: No dependencies on other stories - FOUNDATIONAL
- **US3 (Period Management)**: Soft dependency on US1 (periods organize transactions)
- **US4 (Reconciliation)**: Depends on US1 (reconciles journal entries vs subledgers)
- **US5 (Tax Tracking)**: Depends on US1 (tax components attached to journal entries)
- **US6 (Financial Reporting)**: Depends on US1, US2, US3 (reports query journal entries by period/account)
- **US7 (Audit Trail)**: Integrated throughout US1-US6 (audit service called by all operations)
- **US8 (Event Integration)**: Depends on US1 (publishes events after journal entry posting)

### Critical Path

**Minimal MVP (User Stories 1 & 2 only)**:
1. Phase 1: Setup (T001-T011)
2. Phase 2: Foundational (T012-T029) ← BLOCKS everything
3. Phase 3: US1 Financial Transaction Recording (T030-T074)
4. Phase 4: US2 Chart of Accounts Management (T075-T104)
5. Validate: Can ingest events, create balanced journal entries, manage chart of accounts

**Full Implementation (All User Stories)**:
- Phase 1-2: Setup + Foundational
- Phase 3-4: US1 & US2 (P1 priority, foundational)
- Phase 5-7 & 9: US3, US4, US5, US7 (P2 priority, can parallelize)
- Phase 8 & 10: US6 & US8 (P3 priority, final capabilities)
- Phase 11: Polish & Cross-Cutting

### Parallel Opportunities

**Within Setup (Phase 1)**:
- T003, T004, T005, T006, T010, T011 can all run in parallel (different files)

**Within Foundational (Phase 2)**:
- T024, T027, T028, T029 can run in parallel (test infrastructure)

**User Stories Can Run in Parallel** (if team capacity allows):
- After Foundational complete, US1 & US2 can proceed in parallel (different entities/services)
- After US1/US2 complete, US3, US4, US5 can proceed in parallel
- US7 (Audit Trail) integrates across all stories, best done incrementally alongside each story

**Within Each User Story**:
- All test tasks marked [P] can run in parallel
- All entity creation tasks marked [P] can run in parallel
- All DTO/Extension tasks marked [P] can run in parallel
- All controller endpoint tasks marked [P] can run in parallel (after service layer complete)

---

## Parallel Example: User Story 1

```bash
# Launch all tests for User Story 1 together:
Task T030: "Create EventProcessingTests"
Task T031: "Write test: InvoiceCreated event"
Task T032: "Write test: PaymentReceived event"
Task T033: "Write test: SupplierInvoice event"
Task T034: "Write test: InventoryMovement event"
Task T035: "Write test: PayrollProcessed event"
Task T036: "Write test: Audit trail creation"
Task T037: "Write test: Duplicate event rejection"

# Launch all entity models for User Story 1 together:
Task T038: "Create JournalEntry entity"
Task T039: "Create JournalEntryLine entity"
Task T040: "Create TaxComponent entity"
Task T041: "Create SubledgerTransaction entity"
Task T042: "Create AuditTrailEntry entity"
Task T043: "Create ProcessedEventRegistry entity"

# Launch all DTOs/Extensions together:
Task T047: "Create JournalEntryResponse DTO"
Task T048: "Create CreateJournalEntryRequest DTO"
Task T049: "Create JournalEntryExtensions"
Task T050: "Create event DTOs"

# Launch all RabbitMQ consumers together:
Task T061: "Create InvoiceCreatedConsumer"
Task T062: "Create PaymentReceivedConsumer"
Task T063: "Create SupplierInvoiceConsumer"
Task T064: "Create InventoryMovementConsumer"
Task T065: "Create PayrollProcessedConsumer"

# Launch all observability tasks together:
Task T069: "Add OpenTelemetry Activity instrumentation"
Task T070: "Add metrics emission"
Task T071: "Add structured logging"
```

---

## Implementation Strategy

### MVP First (User Stories 1 & 2 Only)

**Goal**: Deliver minimal viable accounting service

1. **Phase 1**: Setup (T001-T011) - Project initialization
2. **Phase 2**: Foundational (T012-T029) - Infrastructure
3. **Phase 3**: US1 Financial Transaction Recording (T030-T074) - Core capability
4. **Phase 4**: US2 Chart of Accounts Management (T075-T104) - Required for US1
5. **STOP and VALIDATE**: Test US1 & US2 independently
6. **Deploy/Demo**: Accounting service can ingest events and create balanced journal entries

**Value Delivered**: Automatic financial transaction recording with chart of accounts management

### Incremental Delivery

1. **Foundation** (Phases 1-2): Setup + Foundational → Infrastructure ready
2. **MVP** (Phases 3-4): US1 + US2 → Test independently → Deploy/Demo (Core accounting)
3. **Enhance** (Phase 5): US3 Period Management → Test independently → Deploy/Demo (Period controls)
4. **Quality** (Phases 6-7): US4 Reconciliation + US7 Audit Trail → Test independently → Deploy/Demo (Data integrity)
5. **Compliance** (Phase 8): US5 Tax Tracking → Test independently → Deploy/Demo (Tax reporting)
6. **Insights** (Phase 9): US6 Financial Reporting → Test independently → Deploy/Demo (Management reports)
7. **Integration** (Phase 10): US8 Event Integration → Test independently → Deploy/Demo (Full event-driven architecture)
8. **Polish** (Phase 11): Cross-cutting concerns → Final validation → Production ready

Each increment adds value without breaking previous capabilities.

### Parallel Team Strategy

With 3 developers after Foundational phase completes:

1. **Team completes Setup + Foundational together** (Phases 1-2)
2. **Once Foundational done**:
   - Developer A: User Story 1 (Financial Transaction Recording)
   - Developer B: User Story 2 (Chart of Accounts Management)
   - Developer C: Test infrastructure enhancements
3. **After US1/US2 complete**:
   - Developer A: User Story 3 (Period Management)
   - Developer B: User Story 4 (Reconciliation)
   - Developer C: User Story 5 (Tax Tracking)
4. **Continue in parallel**: US6, US7, US8
5. **Final polish together**: Phase 11

Stories complete and integrate independently, merge frequently.

---

## Task Summary

**Total Tasks**: 294 (284 original + 10 additions for complete coverage)

**Tasks by Phase**:
- Phase 1 (Setup): 12 tasks (11 + T011a bulk import)
- Phase 2 (Foundational): 18 tasks
- Phase 3 (US1 - Transaction Recording): 50 tasks (45 + T071a-e API controllers for FR-032)
- Phase 4 (US2 - Chart of Accounts): 30 tasks
- Phase 5 (US3 - Period Management): 31 tasks
- Phase 6 (US4 - Reconciliation): 31 tasks
- Phase 7 (US5 - Tax Tracking): 23 tasks
- Phase 8 (US6 - Financial Reporting): 27 tasks
- Phase 9 (US7 - Audit Trail): 18 tasks
- Phase 10 (US8 - Event Integration): 26 tasks
- Phase 11 (Polish): 32 tasks (28 + T268a-d archival for FR-029)

**Parallel Opportunities**: 155 tasks marked [P] can run in parallel within their phase (147 + 8 new parallel tasks)

**Story Distribution**:
- US1 (P1): 50 tasks (17.0% of total) - Largest story, core capability, includes transaction query API
- US2 (P1): 30 tasks (10.2%) - Foundational
- US3 (P2): 31 tasks (10.5%) - Period controls
- US4 (P2): 31 tasks (10.5%) - Reconciliation
- US5 (P2): 23 tasks (7.8%) - Tax tracking
- US6 (P3): 27 tasks (9.2%) - Reporting
- US7 (P2): 18 tasks (6.1%) - Audit trail (cross-cutting) + archival validation
- US8 (P3): 26 tasks (8.8%) - Event integration

**MVP Scope** (US1 + US2): 109 tasks (37.1% of total, includes bulk import and transaction query API)

**Independent Test Criteria**:
- ✅ Each user story has explicit "Independent Test" definition
- ✅ Each user story has dedicated test tasks
- ✅ Each user story has checkpoint validation

**Format Validation**: ✅ All tasks follow checklist format (checkbox, ID, [P]/[Story] labels, file paths)

---

## Notes

- [P] tasks = different files, no dependencies - can parallelize
- [Story] label (US1-US8) maps task to specific user story for traceability
- Each user story independently completable and testable per spec.md priorities
- Tests written first, must fail before implementation (TDD per Constitution III)
- Commit after each task or logical group
- Stop at any checkpoint to validate story independently
- Constitution compliance verified in Phase 11 (T284)
- All success criteria (SC-001 through SC-019) validated in Phase 11 (T283)
