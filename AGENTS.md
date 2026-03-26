# Maliev.AccountingService Agent Guidelines

This document provides essential instructions for AI agents working on the Maliev.AccountingService repository.

## 1. Environment & Build

- **Platform**: .NET 10.0 (C# 13)
- **Database**: PostgreSQL 18 (Alpine) via Entity Framework Core 10.x
- **Infrastructure**: Redis 7.x, RabbitMQ (MassTransit), OpenTelemetry
- **Solution File**: `Maliev.AccountingService.slnx`

### Common Commands

| Action | Command |
|--------|---------|
| **Build** | `dotnet build` |
| **Run App** | `dotnet run --project Maliev.AccountingService.Api` |
| **Run All Tests** | `dotnet test --verbosity normal` |
| **Run Single Test** | `dotnet test --filter "FullyQualifiedName~Namespace.ClassName.MethodName"` |
| **DB Migration** | `dotnet ef database update --project Maliev.AccountingService.Infrastructure --startup-project Maliev.AccountingService.Infrastructure` |
| **Add Migration** | `dotnet ef migrations add <Name> --project Maliev.AccountingService.Infrastructure --startup-project Maliev.AccountingService.Infrastructure` |

**Important**:
- **TreatWarningsAsErrors** is enabled. Code must be warning-free.
- Integration tests use **Testcontainers**. Docker must be available.
- Do NOT use in-memory databases for testing.

## 2. Code Style & Conventions

### Structure
- **API**: `Maliev.AccountingService.Api` (Controllers, Services, Consumers)
- **Data**: `Maliev.AccountingService.Data` (EF Core Context, Entities, Migrations)
- **Tests**: `Maliev.AccountingService.Tests` (xUnit, Integration Tests)

### Coding Standards
- **Namespaces**: Use file-scoped namespaces (`namespace Maliev.AccountingService.Api.Services;`).
- **Formatting**: Follow standard C# conventions (PascalCase for types/public members, camelCase for locals/parameters).
- **Async**: Use `async/await` for all I/O operations. Suffix async methods with `Async`.
- **DI**: Use constructor injection. Register services in `Program.cs` using scoped/singleton/transient as appropriate.
- **Nullability**: `<Nullable>enable</Nullable>` is on. Handle nulls explicitly.

### Banned Libraries (Strict)
- ❌ **AutoMapper**: Use manual mapping.
- ❌ **FluentValidation**: Use Data Annotations (`[Required]`, `[EmailAddress]`).
- ❌ **FluentAssertions**: Use standard xUnit `Assert` methods.
- ❌ **Microsoft.EntityFrameworkCore.Design**: Must only be in the Infrastructure project (where migrations reside). Do NOT add to API or other projects.

### Logging
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

## 3. Testing Guidelines

- **Framework**: xUnit
- **Mocking**: Moq (use sparingly, prefer integration tests)
- **Integration**: Use `Testcontainers` for real dependencies (Postgres, Redis, RabbitMQ).
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

## 4. Operational Rules

- **Secrets**: NEVER hardcode secrets. Use environment variables.
- **IAM**: Register permissions with IAM service using format `{service}.{resource}.{action}`.
- **Reverting**: Do not revert changes unless explicitly requested or if they break the build.
- **Verification**: ALWAYS run `dotnet build` and `dotnet test` after making changes.

## 5. Aspire & Service Defaults
- The project relies on `Maliev.Aspire.ServiceDefaults` for shared configuration (OpenTelemetry, HealthChecks).
- Ensure `builder.AddServiceDefaults()` is called in `Program.cs`.

## 6. Project Specifics
- **Double-Entry**: Ensure all journal entries balance.
- **Audit**: All financial operations must generate audit logs.


## Git & Version Control — Mandatory Rules

### 🚨 CRITICAL: Always Commit Code Changes (Non-Negotiable)
- **You MUST commit your changes to the local repository after completing any meaningful unit of work.**
- **Never accumulate uncommitted changes.** Do not wait until end of session or until something breaks.
- **Commit early and often** — if a change is meaningful (even a small fix or refactor), commit it.
- **You do NOT need to push to remote** — local commits are sufficient to protect against accidental loss.
- **If you are unsure whether to commit, commit anyway.** Extra commits are harmless; lost work is irreversible.
- This rule applies even if you are just "testing" or "exploring" — use git branches to isolate experimental work and commit those changes too.

### 🚨 CRITICAL: Never Use `git checkout` to Restore Broken Files
- **NEVER use `git checkout` to restore or recover files.** This operation discards uncommitted changes permanently and will result in data loss.
- **To undo/recover from broken files: first commit your current changes, then use `git revert` or `git reset --soft` to safely undo.**

## Database & EF Core — Mandatory Rules

### EF Core Design Package
- ❌ `Microsoft.EntityFrameworkCore.Design` MUST NOT be in Api projects
- ✅ It belongs ONLY in the Infrastructure (or Data) project where migrations live
- Migration commands must target Infrastructure as both project and startup-project (since EF Core Design package is in Infrastructure):
  ```
  dotnet ef migrations add <Name> --project Maliev.<Domain>Service.Infrastructure --startup-project Maliev.<Domain>Service.Infrastructure
  ```

### PostgreSQL xmin Concurrency — Mandatory Pattern
Use shadow property ONLY. Never add a Xmin/xmin property to domain entities.
```csharp
entity.Property<uint>("xmin").HasColumnType("xid").IsRowVersion();
```
- ❌ Never use `UseXminAsConcurrencyToken()` (removed in Npgsql EF v7)
- ❌ Never use entity property `public uint Xmin { get; set; }` or `public uint xmin { get; set; }`
- ❌ Never use `.Ignore(e => e.Xmin)` — remove the entity property instead
