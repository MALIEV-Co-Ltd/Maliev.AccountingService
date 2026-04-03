# Maliev.AccountingService Agent Guidelines

This document provides essential instructions for AI agents working on the Maliev.AccountingService repository.

## 1. Environment & Build

- **Platform**: .NET 10.0 (C# 13)
- **Database**: PostgreSQL 18 (Alpine) via Entity Framework Core 10.x
- **Infrastructure**: Redis 7.x, RabbitMQ (MassTransit), OpenTelemetry
- **Solution File**: `Maliev.AccountingService.slnx`

### Common Commands

All commands run from within this service directory (`B:\maliev\Maliev.AccountingService`).

```powershell
# Build (treats warnings as errors — all must be fixed)
dotnet build Maliev.AccountingService.slnx

# Run all tests
dotnet test Maliev.AccountingService.slnx --verbosity normal

# Run a single test method
dotnet test --filter "FullyQualifiedName~InvoiceServiceTests.CreateInvoice_ShouldPersistToDatabase"

# Run all tests in a class
dotnet test --filter "FullyQualifiedName~InvoiceServiceTests"

# Run with code coverage
dotnet test Maliev.AccountingService.slnx --collect:"XPlat Code Coverage"

# Format check
dotnet format Maliev.AccountingService.slnx

# Run App
dotnet run --project Maliev.AccountingService.Api

# EF Core migrations (Data project only)
dotnet ef migrations add <Name> --project Maliev.AccountingService.Infrastructure --startup-project Maliev.AccountingService.Infrastructure
```

**Important**:
- **TreatWarningsAsErrors** is enabled. Code must be warning-free.
- Integration tests use **Testcontainers**. Docker must be available.
- Do NOT use in-memory databases for testing.

## 2. Code Style & Conventions

### Structure
- **API**: `Maliev.AccountingService.Api` (Controllers, Consumers, Middleware)
- **Data**: `Maliev.AccountingService.Data` (EF Core Context, Entities, Migrations)
- **Tests**: `Maliev.AccountingService.Tests` (xUnit, Integration Tests)

### C# Naming & Formatting
- **Namespaces**: File-scoped (`namespace Maliev.AccountingService.Api.Services;`)
- **Classes/Methods/Properties**: `PascalCase`
- **Private fields**: `_camelCase` (underscore prefix)
- **Parameters/locals**: `camelCase`
- **Async methods**: Suffix with `Async` (e.g., `CreateInvoiceAsync`)
- **Interfaces**: Prefix with `I` (e.g., `IInvoiceService`)
- **Permissions**: GCP-style `{domain}.{plural-resource}.{action}` as `public const string` in a `Permissions` static class
  - Valid: `accounting.invoices.create`, `accounting.journal-entries.post`
  - Invalid: `accounting.invoice.create` (singular), `accounting.create` (missing resource)
- **XML docs**: Required on ALL public methods and properties
- **Nullable**: Enabled (`<Nullable>enable</Nullable>`). Use `?` explicitly
- **Imports**: System first, then third-party, then local. Alphabetize within groups. Remove unused `using`
- **Braces**: Allman style (new line) for methods and control structures. Expression-bodied for properties/accessors
- **Indentation**: 4 spaces, LF line endings, UTF-8, trim trailing whitespace

### C# Patterns
- **DI**: Constructor injection with `private readonly` fields
- **Controllers**: `[ApiController]`, `[ApiVersion("1")]`, `[Route("accounting/v{version:apiVersion}")]`
- **Logging**: `ILogger<T>` with structured placeholders (never interpolate): `_logger.LogInformation("Processing {InvoiceId}", invoiceId)`
- **Error handling**: Global exception middleware. Return `ProblemDetails` / `ErrorResponse` DTOs. Never expose stack traces
- **JSON**: Check existing conventions in this service for naming policy
- **Manual mapping**: Static extension methods (`ToDto()`, `ToEntity()`). AutoMapper is banned
- **Validation**: `System.ComponentModel.DataAnnotations` on DTOs. FluentValidation is banned

### Logging (High-Performance Pattern)
- Use `[LoggerMessage]` source generator for high-performance logging.
- Define log methods in `partial class Log` nested within the using class.
- Example:
  ```csharp
  internal static partial class Log
  {
      [LoggerMessage(Level = LogLevel.Information, Message = "Processing invoice {InvoiceId}")]
      public static partial void ProcessingInvoice(ILogger logger, Guid invoiceId);
  }
  ```

### API Design
- **Versioning**: All endpoints are prefixed with `/accounting/v1/`.
- **Documentation**: Use XML documentation comments (`///`) for all public members.
- **DTOs**: Use `record` or `class` for DTOs. Avoid exposing entities directly.

### Banned Libraries (Build Will Fail)

| Banned | Use Instead |
|--------|-------------|
| AutoMapper | Manual mapping extensions |
| FluentValidation | DataAnnotations or manual validation |
| FluentAssertions | Standard xUnit `Assert.*` |
| Swashbuckle/Swagger | Scalar (at `/accounting/scalar`) |
| InMemoryDatabase (EF Core) | Testcontainers with real PostgreSQL |
| Microsoft.EntityFrameworkCore.Design (in Api) | Only in Data/Infrastructure project where migrations live |

## 3. Testing Guidelines

- **Framework**: xUnit with standard `Assert` (`Assert.Equal`, `Assert.NotNull`, etc.)
- **Naming**: `MethodName_StateUnderTest_ExpectedBehavior` or `HTTP_METHOD_Path_Scenario_ExpectedStatus`
- **Coverage**: Minimum 80% per service
- **Integration tests**: `BaseIntegrationTestFactory<TProgram, TDbContext>` with Testcontainers (PostgreSQL, Redis, RabbitMQ). Never InMemoryDatabase
- **System tests** (Tier 3): `AspireTestFixture` with `[Collection("AspireDomainTests")]` — shared AppHost, never one per class
- **Eventual consistency**: Use `TestHelpers.WaitForAsync`. Never `Task.Delay`
- **MassTransit consumers**: Must have consumer tests using `AddMassTransitTestHarness()`
- **Configuration**: Test configuration must live in test fixtures, NOT `Program.cs`.

### Example Test Pattern
```csharp
public class InvoiceServiceTests : IClassFixture<IntegrationTestFixture>
{
    private readonly IntegrationTestFixture _fixture;

    public InvoiceServiceTests(IntegrationTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task CreateInvoice_ShouldPersistToDatabase()
    {
        // Arrange
        var service = _fixture.GetService<IInvoiceService>();
        var dto = new CreateInvoiceDto { ... };

        // Act
        var result = await service.CreateAsync(dto);

        // Assert
        Assert.NotNull(result);
        Assert.NotEqual(Guid.Empty, result.Id);
    }
}
```

### Testing Strategy (4-Tier Pyramid Context)

This service's tests cover **Tier 1 (Unit)** and **Tier 2 (Service Integration)** of the Maliev testing pyramid:

| Tier | What to Test | Infrastructure |
|------|-------------|---------------|
| **Unit** | Business logic, domain models, service methods with mocked dependencies | None (mocks only) |
| **Service Integration** | API endpoints, database persistence, permission enforcement, input validation | `BaseIntegrationTestFactory` + Testcontainers (Postgres/Redis/RabbitMQ) |

**Tier 3 (System Integration)** — cross-service workflows and event chains — is tested in `Maliev.Aspire.Tests/`.

> Full ecosystem test strategy: `Maliev.Aspire.Tests/TEST_PLAN.md`

## 4. Mandatory Rules

- **`TreatWarningsAsErrors = true`**: Zero warnings allowed. No suppression
- **`[RequirePermission("accounting.resources.action")]`**: On all endpoints, not plain `[Authorize]`
- **API versioning**: All routes versioned (`v1/`)
- **Service prefix**: Routes prefixed with `/accounting`
- **Scalar docs**: Configured at `/accounting/scalar`
- **Secrets**: Never hardcoded. Use GCP Secret Manager or environment variables
- **Async/await**: All the way down. Pass `CancellationToken`
- **EF Core Design package**: Only in Data/Infrastructure project, never in Api
- **PostgreSQL xmin**: Shadow property only — `entity.Property<uint>("xmin").HasColumnType("xid").IsRowVersion()`. Never add entity property
- **Temporary files**: Generate in `/temp` folder, clean up afterwards
- **Verification**: ALWAYS run `dotnet build` and `dotnet test` after making changes.

## 5. Aspire & Service Defaults
- The project relies on `Maliev.Aspire.ServiceDefaults` for shared configuration (OpenTelemetry, HealthChecks).
- Ensure `builder.AddServiceDefaults()` is called in `Program.cs`.

## 6. Project Specifics
- **Double-Entry**: Ensure all journal entries balance.
- **Audit**: All financial operations must generate audit logs.
- **IAM**: Register permissions with IAM service using format `accounting.{plural-resource}.{action}`.

## Git & Version Control — Mandatory Rules

- Each `Maliev.*` folder is an independent git repo. Work within this service directory for git commands.
- **Commit early and often** after every meaningful unit of work. Do not accumulate changes
- **Never use `git checkout` to restore files** — commit first, then `git revert` or `git reset --soft`
- Feature branches merged to `develop` via PR. Do not push without being asked
- Extra commits are harmless; lost work is irreversible. If unsure whether to commit, commit anyway.

## Database & EF Core — Mandatory Rules

### EF Core Design Package
- `Microsoft.EntityFrameworkCore.Design` MUST NOT be in Api projects
- It belongs ONLY in the Data/Infrastructure project where migrations live
- Migration commands must target Infrastructure as both project and startup-project:
  ```
  dotnet ef migrations add <Name> --project Maliev.AccountingService.Infrastructure --startup-project Maliev.AccountingService.Infrastructure
  ```

### PostgreSQL xmin Concurrency — Mandatory Pattern
Use shadow property ONLY. Never add a Xmin/xmin property to domain entities.
```csharp
entity.Property<uint>("xmin").HasColumnType("xid").IsRowVersion();
```
- Never use `UseXminAsConcurrencyToken()` (removed in Npgsql EF v7)
- Never use entity property `public uint Xmin { get; set; }` or `public uint xmin { get; set; }`
- Never use `.Ignore(e => e.Xmin)` — remove the entity property instead
