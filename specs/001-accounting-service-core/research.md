# Research: Accounting Service Core Technical Decisions

**Feature**: `001-accounting-service-core`
**Date**: 2025-12-05
**Purpose**: Document technical research findings and decisions for implementation

## Overview

This document captures research findings for key technical decisions required to implement the Accounting Service Core. Each section addresses a specific unknown from the Technical Context and provides a concrete decision, rationale, and alternatives considered.

---

## 1. Double-Entry Bookkeeping Validation Patterns

**Research Question**: How should we validate that journal entries maintain balanced debits and credits in C#?

###Decision**: Database-level check constraint + Application-level validation

**Rationale**:
- Double-entry bookkeeping is a fundamental accounting principle requiring zero tolerance for violations
- Defense-in-depth approach provides multiple validation layers
- Database constraint acts as final enforcement even if application logic has bugs
- Application-level validation provides immediate feedback before database round-trip

**Implementation**:
```csharp
// Application-level validation in JournalEntryService
public class JournalEntryService
{
    public async Task<ValidationResult> ValidateBalanceAsync(JournalEntry entry)
    {
        var totalDebits = entry.Lines.Sum(l => l.DebitAmount);
        var totalCredits = entry.Lines.Sum(l => l.CreditAmount);

        if (totalDebits != totalCredits)
        {
            return ValidationResult.Fail(
                $"Journal entry unbalanced: Debits={totalDebits}, Credits={totalCredits}, Variance={totalDebits - totalCredits}"
            );
        }

        return ValidationResult.Success();
    }
}

// Database-level constraint (EF Core migration)
migrationBuilder.Sql(@"
    ALTER TABLE journal_entries
    ADD CONSTRAINT chk_balanced_entry
    CHECK ((SELECT SUM(debit_amount) FROM journal_entry_lines WHERE journal_entry_id = id) =
           (SELECT SUM(credit_amount) FROM journal_entry_lines WHERE journal_entry_id = id))
");
```

**Alternatives Considered**:
1. **Application-only validation**: Rejected - no safety net if bugs bypass validation logic
2. **Database triggers**: Rejected - harder to test, less portable, obscures business logic
3. **Computed columns**: Rejected - doesn't prevent unbalanced entries, only calculates variance

**References**:
- PostgreSQL CHECK constraints: https://www.postgresql.org/docs/current/ddl-constraints.html
- EF Core data annotations: https://learn.microsoft.com/en-us/ef/core/modeling/

---

## 2. PostgreSQL Transaction Isolation Levels for Atomic Journal Entry Creation

**Research Question**: What transaction isolation level ensures atomic journal entry creation with rollback guarantee?

**Decision**: Serializable isolation level for journal entry posting; Read Committed for queries

**Rationale**:
- Serializable prevents phantom reads when checking for duplicate event IDs or concurrent period closures
- Ensures true atomicity - all journal entry components (header, lines, tax, audit) commit together or rollback completely
- Prevents race conditions during financial period closing (two users attempting to post to same period during close)
- Read Committed sufficient for read-only operations (reports, queries) to avoid unnecessary locking

**Implementation**:
```csharp
// EF Core DbContext configuration
public class AccountingDbContext : DbContext
{
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql(connectionString, npgsqlOptions =>
        {
            npgsqlOptions.ExecutionStrategy(c =>
                new NpgsqlRetryingExecutionStrategy(c, maxRetryCount: 3));
        });
    }
}

// Service method with explicit transaction
public async Task<Result<JournalEntry>> PostJournalEntryAsync(Guid entryId)
{
    await using var transaction = await _dbContext.Database.BeginTransactionAsync(
        IsolationLevel.Serializable
    );

    try
    {
        var entry = await _dbContext.JournalEntries
            .Include(e => e.Lines)
            .Include(e => e.TaxComponents)
            .FirstOrDefaultAsync(e => e.Id == entryId);

        if (entry == null) return Result.NotFound();

        // Validate balance
        var validation = await ValidateBalanceAsync(entry);
        if (!validation.IsValid) return Result.Fail(validation.Errors);

        // Check period status
        var period = await _dbContext.FinancialPeriods.FindAsync(entry.PeriodId);
        if (period.Status == PeriodStatus.Closed)
            return Result.Fail("Cannot post to closed period");

        // Update status
        entry.Status = JournalEntryStatus.Posted;
        entry.PostedAt = DateTime.UtcNow;
        entry.PostedBy = _currentUser.Id;

        // Create audit trail
        _dbContext.AuditTrails.Add(new AuditTrailEntry
        {
            EntityType = "JournalEntry",
            EntityId = entry.Id,
            Action = "Posted",
            UserId = _currentUser.Id,
            Timestamp = DateTime.UtcNow
        });

        await _dbContext.SaveChangesAsync();
        await transaction.CommitAsync();

        return Result.Success(entry);
    }
    catch (Exception ex)
    {
        await transaction.RollbackAsync();
        _logger.LogError(ex, "Failed to post journal entry {EntryId}", entryId);
        throw;
    }
}
```

**Alternatives Considered**:
1. **Read Committed**: Rejected - allows phantom reads, insufficient for financial integrity
2. **Repeatable Read**: Considered - prevents most issues but still allows phantoms
3. **Snapshot Isolation**: Rejected - PostgreSQL Serializable uses snapshot internally, no advantage

**References**:
- PostgreSQL transaction isolation: https://www.postgresql.org/docs/current/transaction-iso.html
- EF Core transactions: https://learn.microsoft.com/en-us/ef/core/saving/transactions

---

## 3. RabbitMQ Consumer Idempotency with Event ID Deduplication

**Research Question**: How do we prevent duplicate processing of financial events in MassTransit consumers?

**Decision**: Redis-backed event ID registry with TTL + MassTransit message deduplication

**Rationale**:
- Redis provides fast O(1) lookup for event ID existence checks
- TTL automatically expires old entries (24-hour retention matches typical retry window)
- MassTransit InMemoryOutbox provides at-least-once delivery guarantee
- Combination ensures exactly-once semantics for financial transactions

**Implementation**:
```csharp
// Event ID registry service
public interface IEventIdempotencyService
{
    Task<bool> IsEventProcessedAsync(string eventId);
    Task MarkEventProcessedAsync(string eventId, Guid journalEntryId);
}

public class RedisEventIdempotencyService : IEventIdempotencyService
{
    private readonly IDistributedCache _cache;
    private readonly TimeSpan _ttl = TimeSpan.FromHours(24);

    public async Task<bool> IsEventProcessedAsync(string eventId)
    {
        var value = await _cache.GetStringAsync($"event:{eventId}");
        return value != null;
    }

    public async Task MarkEventProcessedAsync(string eventId, Guid journalEntryId)
    {
        await _cache.SetStringAsync(
            $"event:{eventId}",
            journalEntryId.ToString(),
            new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = _ttl }
        );
    }
}

// MassTransit consumer with idempotency check
public class InvoiceCreatedConsumer : IConsumer<InvoiceCreatedEvent>
{
    private readonly IEventIdempotencyService _idempotency;
    private readonly IEventProcessingService _eventProcessing;

    public async Task Consume(ConsumeContext<InvoiceCreatedEvent> context)
    {
        var eventId = context.Message.EventId;

        // Check if already processed
        if (await _idempotency.IsEventProcessedAsync(eventId))
        {
            _logger.LogInformation("Skipping duplicate event {EventId}", eventId);
            return; // Acknowledge without reprocessing
        }

        // Process within transaction
        var result = await _eventProcessing.ProcessInvoiceCreatedAsync(context.Message);

        if (result.IsSuccess)
        {
            // Mark as processed
            await _idempotency.MarkEventProcessedAsync(eventId, result.Value.JournalEntryId);
        }
        else
        {
            throw new InvalidOperationException($"Event processing failed: {result.Error}");
        }
    }
}

// MassTransit configuration with outbox
services.AddMassTransit(x =>
{
    x.AddConsumers(Assembly.GetExecutingAssembly());

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.UseInMemoryOutbox(context); // Ensures at-least-once delivery
        cfg.ConfigureEndpoints(context);
    });

    x.AddEntityFrameworkOutbox<AccountingDbContext>(o =>
    {
        o.UsePostgres();
        o.UseBusOutbox();
    });
});
```

**Alternatives Considered**:
1. **Database-only registry**: Considered - more durable but slower lookups, unnecessary I/O
2. **In-memory HashSet**: Rejected - not shared across service instances, lost on restart
3. **MassTransit built-in deduplication**: Insufficient - doesn't persist across service restarts
4. **No deduplication, rely on DB constraints**: Rejected - violates idempotency requirement

**References**:
- MassTransit idempotency: https://masstransit.io/documentation/patterns/saga
- Redis distributed cache: https://learn.microsoft.com/en-us/aspnet/core/performance/caching/distributed

---

## 4. EF Core Optimistic Concurrency for Period Closing Race Conditions

**Research Question**: How do we prevent concurrent period closing operations from creating data inconsistency?

**Decision**: Row version (timestamp) concurrency tokens + explicit locking for critical operations

**Rationale**:
- Row version provides automatic optimistic concurrency control with minimal performance impact
- Detects concurrent modifications without pessimistic locking for most operations
- For period closing specifically, explicit SELECT FOR UPDATE ensures only one closer wins
- Retry logic handles transient concurrency failures gracefully

**Implementation**:
```csharp
// Entity with concurrency token
public class FinancialPeriod
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public PeriodStatus Status { get; set; } // Open, Closed, Locked

    [Timestamp] // Row version for optimistic concurrency
    public byte[] RowVersion { get; set; }

    public DateTime? ClosedAt { get; set; }
    public Guid? ClosedBy { get; set; }
}

// DbContext configuration
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<FinancialPeriod>()
        .Property(p => p.RowVersion)
        .IsRowVersion();
}

// Service method with explicit locking for period close
public async Task<Result> ClosePeriodAsync(Guid periodId)
{
    try
    {
        // Use raw SQL for SELECT FOR UPDATE (pessimistic lock)
        var period = await _dbContext.FinancialPeriods
            .FromSqlRaw(@"
                SELECT * FROM financial_periods
                WHERE id = {0}
                FOR UPDATE NOWAIT", periodId)
            .FirstOrDefaultAsync();

        if (period == null) return Result.NotFound();

        if (period.Status == PeriodStatus.Closed)
            return Result.Fail("Period already closed");

        // Validate all transactions in period are balanced
        var unbalanced = await _dbContext.JournalEntries
            .Where(e => e.PeriodId == periodId && e.Status == JournalEntryStatus.Draft)
            .CountAsync();

        if (unbalanced > 0)
            return Result.Fail($"{unbalanced} draft entries must be posted or deleted before closing");

        // Close the period
        period.Status = PeriodStatus.Closed;
        period.ClosedAt = DateTime.UtcNow;
        period.ClosedBy = _currentUser.Id;

        await _dbContext.SaveChangesAsync(); // RowVersion updated automatically

        return Result.Success();
    }
    catch (DbUpdateConcurrencyException ex)
    {
        _logger.LogWarning(ex, "Concurrency conflict closing period {PeriodId}", periodId);
        return Result.Fail("Period was modified by another user. Please retry.");
    }
    catch (Npgsql.NpgsqlException ex) when (ex.SqlState == "55P03") // Lock not available
    {
        _logger.LogWarning(ex, "Period {PeriodId} is locked by another operation", periodId);
        return Result.Fail("Period is being modified by another user. Please retry.");
    }
}
```

**Alternatives Considered**:
1. **Pessimistic locking for all operations**: Rejected - poor scalability, unnecessary for reads
2. **Application-level distributed locks**: Considered - adds complexity, Redis dependency for all operations
3. **Event sourcing**: Rejected - overengineering for current requirements, high migration cost
4. **No concurrency control**: Rejected - unacceptable for financial data integrity

**References**:
- EF Core concurrency tokens: https://learn.microsoft.com/en-us/ef/core/modeling/concurrency
- PostgreSQL row locking: https://www.postgresql.org/docs/current/explicit-locking.html

---

## 5. Exponential Backoff Retry Policy in MassTransit

**Research Question**: How should MassTransit consumers retry transient failures before moving to dead-letter queue?

**Decision**: Exponential backoff with 3 retries, max 60-second delay, immediate DLQ for permanent errors

**Rationale**:
- 3 retries covers ~95% of transient database/network failures per spec (SC-019)
- Exponential backoff (2^n seconds) prevents thundering herd: 2s, 4s, 8s delays
- 60-second max cap prevents excessive delays for high-priority events
- Immediate DLQ routing for permanent errors (validation failures, missing accounts) avoids wasted retries
- Circuit breaker prevents cascading failures if downstream dependency is unhealthy

**Implementation**:
```csharp
// MassTransit retry configuration
services.AddMassTransit(x =>
{
    x.AddConsumers(Assembly.GetExecutingAssembly());

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.UseMessageRetry(r =>
        {
            r.Exponential(3, // Retry count
                TimeSpan.FromSeconds(2), // Initial interval
                TimeSpan.FromSeconds(60), // Max interval
                TimeSpan.FromSeconds(2)); // Interval increment multiplier

            // Don't retry business logic errors
            r.Ignore<ValidationException>();
            r.Ignore<AccountNotFoundException>();
            r.Ignore<ClosedPeriodException>();

            // Only retry transient infrastructure errors
            r.Handle<DbUpdateException>();
            r.Handle<TimeoutException>();
            r.Handle<RabbitMqConnectionException>();
        });

        cfg.UseCircuitBreaker(cb =>
        {
            cb.TrackingPeriod = TimeSpan.FromMinutes(1);
            cb.TripThreshold = 15; // Open circuit after 15 failures in 1 minute
            cb.ActiveThreshold = 10; // Require 10 successes to close circuit
            cb.ResetInterval = TimeSpan.FromMinutes(5); // Try to close after 5 minutes
        });

        cfg.ConfigureEndpoints(context);
    });
});

// Consumer exception handling
public class InvoiceCreatedConsumer : IConsumer<InvoiceCreatedEvent>
{
    public async Task Consume(ConsumeContext<InvoiceCreatedEvent> context)
    {
        try
        {
            // Process event
            await _eventProcessing.ProcessInvoiceCreatedAsync(context.Message);
        }
        catch (ValidationException ex)
        {
            // Business logic error - move to DLQ immediately
            _logger.LogError(ex, "Validation failed for event {EventId}", context.Message.EventId);
            throw; // MassTransit will not retry (configured in Ignore)
        }
        catch (DbUpdateException ex)
        {
            // Transient error - allow retries
            _logger.LogWarning(ex, "Database error processing event {EventId}, will retry",
                context.Message.EventId);
            throw; // MassTransit will retry with exponential backoff
        }
    }
}
```

**Alternatives Considered**:
1. **Fixed interval retry**: Rejected - causes thundering herd when multiple consumers fail simultaneously
2. **Unlimited retries**: Rejected - wastes resources on permanent failures, delays DLQ investigation
3. **No retries**: Rejected - doesn't meet 95% transient recovery requirement (SC-019)
4. **Immediate retry**: Rejected - exacerbates transient failures (e.g., brief database unavailability)

**References**:
- MassTransit retry: https://masstransit.io/documentation/configuration/middleware/retry
- Circuit breaker pattern: https://learn.microsoft.com/en-us/azure/architecture/patterns/circuit-breaker

---

## 6. OpenTelemetry Distributed Tracing Across RabbitMQ

**Research Question**: How do we propagate trace context through RabbitMQ messages for end-to-end observability?

**Decision**: W3C Trace Context headers in RabbitMQ message properties + MassTransit built-in propagation

**Rationale**:
- W3C Trace Context is industry standard, supported by OpenTelemetry
- MassTransit automatically propagates Activity (trace) context across message boundaries
- ServiceDefaults configures OpenTelemetry with proper instrumentation
- Enables end-to-end traces from source service event publication → accounting service processing → downstream analytics

**Implementation**:
```csharp
// Program.cs - ServiceDefaults handles OpenTelemetry setup
var builder = WebApplication.CreateBuilder(args);

builder.AddGoogleSecretManagerVolume();
builder.AddServiceDefaults(); // Configures OpenTelemetry with Activity tracking

builder.Services.AddOpenTelemetry()
    .WithTracing(tracing =>
    {
        tracing
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddNpgsql() // PostgreSQL instrumentation
            .AddSource("MassTransit"); // RabbitMQ instrumentation
    });

// MassTransit automatically propagates Activity context
services.AddMassTransit(x =>
{
    x.AddConsumers(Assembly.GetExecutingAssembly());

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.PropagateActivityContext(); // Propagate W3C Trace Context
        cfg.ConfigureEndpoints(context);
    });
});

// Consumer with manual span creation for business operations
public class InvoiceCreatedConsumer : IConsumer<InvoiceCreatedEvent>
{
    private static readonly ActivitySource _activitySource = new("Maliev.Accounting");

    public async Task Consume(ConsumeContext<InvoiceCreatedEvent> context)
    {
        // Activity automatically started by MassTransit with propagated trace context
        using var activity = _activitySource.StartActivity("ProcessInvoiceCreated");
        activity?.SetTag("invoice.id", context.Message.InvoiceId);
        activity?.SetTag("event.id", context.Message.EventId);
        activity?.SetTag("customer.id", context.Message.CustomerId);

        try
        {
            var result = await _eventProcessing.ProcessInvoiceCreatedAsync(context.Message);

            activity?.SetTag("journal.entry.id", result.JournalEntryId);
            activity?.SetStatus(ActivityStatusCode.Ok);
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.RecordException(ex);
            throw;
        }
    }
}

// Service method with nested span
public class EventProcessingService
{
    private static readonly ActivitySource _activitySource = new("Maliev.Accounting");

    public async Task<ProcessingResult> ProcessInvoiceCreatedAsync(InvoiceCreatedEvent evt)
    {
        using var activity = _activitySource.StartActivity("CreateJournalEntry");
        activity?.SetTag("invoice.amount", evt.Amount);
        activity?.SetTag("tax.amount", evt.TaxAmount);

        // Business logic here - nested spans will be children of this activity

        return result;
    }
}
```

**Alternatives Considered**:
1. **Manual trace context headers**: Rejected - error-prone, MassTransit handles automatically
2. **Correlation ID only**: Rejected - insufficient for distributed tracing, no parent-child relationship
3. **Jaeger-specific headers**: Rejected - vendor lock-in, W3C Trace Context is standard
4. **No tracing**: Rejected - violates observability requirement (SC-017)

**References**:
- W3C Trace Context: https://www.w3.org/TR/trace-context/
- OpenTelemetry .NET: https://opentelemetry.io/docs/languages/net/
- MassTransit observability: https://masstransit.io/documentation/configuration/observability

---

## 7. Redis-Based Processed Event Registry with TTL

**Research Question**: How should we configure Redis for idempotency tracking to balance memory vs. detection window?

**Decision**: 24-hour TTL with hash data structure, memory-optimized eviction policy

**Rationale**:
- 24-hour TTL covers maximum retry window (exponential backoff max + DLQ investigation time)
- Hash structure stores event ID → journal entry ID mapping for debugging
- `volatile-lru` eviction policy automatically removes oldest entries if memory limit reached
- Estimated memory: ~100 bytes/event × 240,000 events/day = ~24MB (negligible)

**Implementation**:
```csharp
// Redis configuration in Program.cs
builder.AddRedisDistributedCache(instanceName: "Accounting:", options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("redis");
    options.InstanceName = "Accounting:";
});

// Idempotency service with structured data
public class RedisEventIdempotencyService : IEventIdempotencyService
{
    private readonly IDistributedCache _cache;
    private readonly ILogger<RedisEventIdempotencyService> _logger;
    private static readonly TimeSpan TTL = TimeSpan.FromHours(24);

    public async Task<ProcessedEventInfo?> GetProcessedEventAsync(string eventId)
    {
        var json = await _cache.GetStringAsync($"event:{eventId}");

        if (json == null)
            return null;

        return JsonSerializer.Deserialize<ProcessedEventInfo>(json);
    }

    public async Task MarkEventProcessedAsync(string eventId, ProcessedEventInfo info)
    {
        var json = JsonSerializer.Serialize(info);

        await _cache.SetStringAsync(
            $"event:{eventId}",
            json,
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TTL
            }
        );

        _logger.LogDebug("Marked event {EventId} as processed with journal entry {JournalEntryId}",
            eventId, info.JournalEntryId);
    }
}

public record ProcessedEventInfo(
    Guid JournalEntryId,
    DateTime ProcessedAt,
    string SourceSystem
);

// appsettings.json Redis configuration
{
  "ConnectionStrings": {
    "redis": "localhost:6379,abortConnect=false,connectRetry=3,connectTimeout=5000"
  }
}

// Docker Compose for local development
services:
  redis:
    image: redis:7-alpine
    ports:
      - "6379:6379"
    command: redis-server --maxmemory 256mb --maxmemory-policy volatile-lru
    volumes:
      - redis-data:/data
```

**Alternatives Considered**:
1. **7-day TTL**: Rejected - excessive memory usage, events older than 24h won't be retried anyway
2. **1-hour TTL**: Rejected - insufficient window for manual DLQ investigation and reprocessing
3. **PostgreSQL table**: Considered - more durable but unnecessary I/O overhead, slower lookups
4. **String values only**: Rejected - loses debugging information (when processed, which journal entry)

**References**:
- Redis eviction policies: https://redis.io/docs/manual/eviction/
- Distributed caching in ASP.NET: https://learn.microsoft.com/en-us/aspnet/core/performance/caching/distributed

---

## 8. Financial Report Query Optimization Strategies

**Research Question**: Should we use materialized views or runtime aggregation for financial reports?

**Decision**: Runtime aggregation with indexed queries initially; materialized views for specific reports if needed

**Rationale**:
- Financial reports require real-time accuracy (especially for open periods)
- PostgreSQL query optimizer handles indexed aggregations efficiently for most use cases
- Spec allows 2-minute report generation window (SC-004), sufficient for indexed queries
- Materialized views add complexity (refresh scheduling, staleness) without proven need
- Can add materialized views incrementally if profiling shows specific bottlenecks

**Implementation**:
```csharp
// Indexed query for trial balance (example)
public async Task<TrialBalanceReport> GenerateTrialBalanceAsync(Guid periodId)
{
    using var activity = Activity.Current?.Source.StartActivity("GenerateTrialBalance");
    activity?.SetTag("period.id", periodId);

    var stopwatch = Stopwatch.StartNew();

    // Aggregation query with proper indexes
    var accountBalances = await _dbContext.JournalEntryLines
        .Where(l => l.JournalEntry.PeriodId == periodId &&
                    l.JournalEntry.Status == JournalEntryStatus.Posted)
        .GroupBy(l => new { l.AccountId, l.Account.Name, l.Account.Type })
        .Select(g => new AccountBalance
        {
            AccountId = g.Key.AccountId,
            AccountName = g.Key.Name,
            AccountType = g.Key.Type,
            DebitTotal = g.Sum(l => l.DebitAmount),
            CreditTotal = g.Sum(l => l.CreditAmount),
            Balance = g.Sum(l => l.DebitAmount) - g.Sum(l => l.CreditAmount)
        })
        .OrderBy(a => a.AccountType)
        .ThenBy(a => a.AccountName)
        .ToListAsync();

    stopwatch.Stop();
    _logger.LogInformation("Generated trial balance for period {PeriodId} in {ElapsedMs}ms",
        periodId, stopwatch.ElapsedMilliseconds);

    activity?.SetTag("report.rows", accountBalances.Count);
    activity?.SetTag("report.generation.ms", stopwatch.ElapsedMilliseconds);

    return new TrialBalanceReport
    {
        PeriodId = periodId,
        GeneratedAt = DateTime.UtcNow,
        AccountBalances = accountBalances,
        TotalDebits = accountBalances.Sum(a => a.DebitTotal),
        TotalCredits = accountBalances.Sum(a => a.CreditTotal)
    };
}

// Database indexes (EF Core migration)
protected override void Up(MigrationBuilder migrationBuilder)
{
    // Composite index for report queries
    migrationBuilder.CreateIndex(
        name: "IX_JournalEntryLines_PeriodId_Status_AccountId",
        table: "journal_entry_lines",
        columns: new[] { "period_id", "status", "account_id" });

    // Index for posted entries only
    migrationBuilder.CreateIndex(
        name: "IX_JournalEntries_PeriodId_Status",
        table: "journal_entries",
        columns: new[] { "period_id", "status" },
        filter: "status = 'Posted'"); // Partial index for posted entries only
}

// If needed: Materialized view for historical periods (closed, won't change)
// Only implement if profiling shows closed period reports exceed 2-minute target
```

**Alternatives Considered**:
1. **Always use materialized views**: Rejected - premature optimization, adds refresh complexity
2. **Cached report results**: Considered - useful for frequently accessed reports, implement after MVP
3. **Pre-aggregated summary tables**: Rejected - duplicates data, complex consistency maintenance
4. **In-memory caching**: Considered - useful for chart of accounts hierarchy, not for transaction data

**References**:
- PostgreSQL query optimization: https://www.postgresql.org/docs/current/sql-createindex.html
- EF Core query performance: https://learn.microsoft.com/en-us/ef/core/performance/

---

## 9. Audit Trail Immutability Guarantees with EF Core

**Research Question**: How do we ensure audit trail entries cannot be modified or deleted?

**Decision**: Append-only table with database-level REVOKE UPDATE/DELETE + EF Core query filter

**Rationale**:
- Database permissions provide strongest guarantee against malicious or accidental modification
- Append-only design (no UPDATE/DELETE queries) prevents accidental EF Core mutations
- Query filter prevents EF Core from loading audit entries for modification
- Temporal tables considered but rejected (adds complexity, audit trail IS the temporal record)

**Implementation**:
```csharp
// Audit trail entity (read-only after creation)
public class AuditTrailEntry
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string EntityType { get; private set; }
    public Guid EntityId { get; private set; }
    public string Action { get; private set; } // Created, Updated, Posted, Closed, etc.
    public Guid UserId { get; private set; }
    public DateTime Timestamp { get; private set; } = DateTime.UtcNow;
    public string BeforeValues { get; private set; } // JSON snapshot
    public string AfterValues { get; private set; } // JSON snapshot
    public string IpAddress { get; private set; }
    public string CorrelationId { get; private set; }

    // Private constructor for EF Core
    private AuditTrailEntry() { }

    // Factory method for creating audit entries
    public static AuditTrailEntry Create(
        string entityType,
        Guid entityId,
        string action,
        Guid userId,
        object beforeValues,
        object afterValues,
        string ipAddress,
        string correlationId)
    {
        return new AuditTrailEntry
        {
            EntityType = entityType,
            EntityId = entityId,
            Action = action,
            UserId = userId,
            BeforeValues = JsonSerializer.Serialize(beforeValues),
            AfterValues = JsonSerializer.Serialize(afterValues),
            IpAddress = ipAddress,
            CorrelationId = correlationId
        };
    }
}

// DbContext configuration
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<AuditTrailEntry>(entity =>
    {
        entity.ToTable("audit_trail");

        // No update or delete - append-only
        entity.HasQueryFilter(e => true); // Always include in queries

        // All properties are read-only after creation
        entity.Property(e => e.Id).ValueGeneratedNever();
        entity.Property(e => e.EntityType).IsRequired();
        entity.Property(e => e.Action).IsRequired();
        entity.Property(e => e.BeforeValues).HasColumnType("jsonb");
        entity.Property(e => e.AfterValues).HasColumnType("jsonb");

        // Indexes for querying
        entity.HasIndex(e => new { e.EntityType, e.EntityId, e.Timestamp });
        entity.HasIndex(e => e.UserId);
        entity.HasIndex(e => e.Timestamp);
    });
}

// Database migration with permission revocation
protected override void Up(MigrationBuilder migrationBuilder)
{
    migrationBuilder.CreateTable(
        name: "audit_trail",
        columns: table => new
        {
            id = table.Column<Guid>(nullable: false),
            entity_type = table.Column<string>(maxLength: 100, nullable: false),
            entity_id = table.Column<Guid>(nullable: false),
            action = table.Column<string>(maxLength: 50, nullable: false),
            user_id = table.Column<Guid>(nullable: false),
            timestamp = table.Column<DateTime>(nullable: false),
            before_values = table.Column<string>(type: "jsonb", nullable: true),
            after_values = table.Column<string>(type: "jsonb", nullable: true),
            ip_address = table.Column<string>(maxLength: 45, nullable: true),
            correlation_id = table.Column<string>(maxLength: 100, nullable: true)
        },
        constraints: table =>
        {
            table.PrimaryKey("PK_audit_trail", x => x.id);
        });

    // Revoke UPDATE and DELETE permissions (app user can only INSERT and SELECT)
    migrationBuilder.Sql(@"
        REVOKE UPDATE, DELETE ON audit_trail FROM accounting_app_user;
        GRANT SELECT, INSERT ON audit_trail TO accounting_app_user;
    ");
}

// Audit service for creating entries
public class AuditService
{
    private readonly AccountingDbContext _dbContext;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public async Task LogActionAsync<T>(
        string action,
        Guid entityId,
        T beforeState,
        T afterState)
    {
        var correlationId = Activity.Current?.Id ?? Guid.NewGuid().ToString();
        var ipAddress = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();

        var auditEntry = AuditTrailEntry.Create(
            entityType: typeof(T).Name,
            entityId: entityId,
            action: action,
            userId: _currentUser.Id,
            beforeValues: beforeState,
            afterValues: afterState,
            ipAddress: ipAddress,
            correlationId: correlationId
        );

        _dbContext.Set<AuditTrailEntry>().Add(auditEntry);
        await _dbContext.SaveChangesAsync();
    }
}
```

**Alternatives Considered**:
1. **Temporal tables**: Rejected - PostgreSQL temporal tables track row versions, audit trail IS the temporal record
2. **Separate audit database**: Considered - useful for very high volume, implement if audit > 10M entries
3. **Event sourcing**: Rejected - overengineering, audit trail sufficient for compliance
4. **Blockchain**: Rejected - unnecessary complexity and cost for internal audit requirements

**References**:
- PostgreSQL permissions: https://www.postgresql.org/docs/current/sql-grant.html
- Audit logging patterns: https://learn.microsoft.com/en-us/azure/architecture/patterns/audit

---

## 10. Chart of Accounts Hierarchical Structure Representation

**Research Question**: Should we use adjacency list or nested sets for chart of accounts hierarchy?

**Decision**: Adjacency list with recursive CTE queries

**Rationale**:
- Chart of accounts has shallow hierarchy (typically 3-4 levels: Assets → Current Assets → Cash → Bank Account)
- Modifications are infrequent (chart of accounts is relatively stable)
- Adjacency list is simpler to understand and maintain
- PostgreSQL recursive CTEs efficiently handle tree traversal for reporting
- Nested sets complex to maintain (requires updating all nodes on insert/delete)

**Implementation**:
```csharp
// Entity with self-referencing relationship
public class ChartOfAccount
{
    public Guid Id { get; set; }
    public string AccountNumber { get; set; } // e.g., "1000", "1100", "1110"
    public string Name { get; set; }
    public AccountType Type { get; set; } // Asset, Liability, Equity, Revenue, Expense
    public string Category { get; set; } // Current Assets, Fixed Assets, etc.
    public bool IsActive { get; set; }

    // Hierarchical relationship (adjacency list)
    public Guid? ParentAccountId { get; set; }
    public ChartOfAccount ParentAccount { get; set; }
    public ICollection<ChartOfAccount> ChildAccounts { get; set; }

    // Metadata
    public DateTime CreatedAt { get; set; }
    public DateTime? ModifiedAt { get; set; }
}

// DbContext configuration
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<ChartOfAccount>(entity =>
    {
        entity.ToTable("chart_of_accounts");

        entity.HasKey(e => e.Id);

        entity.Property(e => e.AccountNumber).IsRequired().HasMaxLength(20);
        entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
        entity.Property(e => e.Type).IsRequired().HasConversion<string>();
        entity.Property(e => e.Category).HasMaxLength(100);

        // Self-referencing relationship
        entity.HasOne(e => e.ParentAccount)
            .WithMany(e => e.ChildAccounts)
            .HasForeignKey(e => e.ParentAccountId)
            .OnDelete(DeleteBehavior.Restrict); // Prevent cascade delete

        // Indexes
        entity.HasIndex(e => e.AccountNumber).IsUnique();
        entity.HasIndex(e => e.ParentAccountId);
        entity.HasIndex(e => e.Type);
    });
}

// Service method to get account hierarchy
public async Task<List<AccountHierarchy>> GetAccountHierarchyAsync(AccountType? filterType = null)
{
    // PostgreSQL recursive CTE for hierarchical query
    var sql = @"
        WITH RECURSIVE account_tree AS (
            -- Base case: root accounts (no parent)
            SELECT
                id,
                account_number,
                name,
                type,
                category,
                parent_account_id,
                0 as level,
                ARRAY[account_number] as path
            FROM chart_of_accounts
            WHERE parent_account_id IS NULL
              AND (@filterType IS NULL OR type = @filterType)

            UNION ALL

            -- Recursive case: child accounts
            SELECT
                c.id,
                c.account_number,
                c.name,
                c.type,
                c.category,
                c.parent_account_id,
                t.level + 1,
                t.path || c.account_number
            FROM chart_of_accounts c
            INNER JOIN account_tree t ON c.parent_account_id = t.id
            WHERE c.is_active = true
        )
        SELECT * FROM account_tree
        ORDER BY path;
    ";

    var accounts = await _dbContext.Database
        .SqlQueryRaw<AccountHierarchy>(sql,
            new NpgsqlParameter("@filterType", filterType ?? (object)DBNull.Value))
        .ToListAsync();

    return accounts;
}

public record AccountHierarchy(
    Guid Id,
    string AccountNumber,
    string Name,
    string Type,
    string Category,
    Guid? ParentAccountId,
    int Level,
    string[] Path
);

// Extension method to build tree structure from flat list
public static List<AccountNode> BuildTree(this List<AccountHierarchy> flatList)
{
    var lookup = flatList.ToDictionary(a => a.Id);
    var roots = new List<AccountNode>();

    foreach (var account in flatList)
    {
        var node = new AccountNode(account);

        if (account.ParentAccountId == null)
        {
            roots.Add(node);
        }
        else if (lookup.TryGetValue(account.ParentAccountId.Value, out var parent))
        {
            var parentNode = roots.FindNode(parent.Id) ?? new AccountNode(parent);
            parentNode.Children.Add(node);
        }
    }

    return roots;
}
```

**Alternatives Considered**:
1. **Nested sets (left/right pointers)**: Rejected - complex updates, premature optimization for shallow hierarchy
2. **Materialized path (store full path string)**: Considered - simpler queries but wastes space, harder to maintain
3. **Closure table**: Rejected - requires separate junction table, overengineering for 3-4 level hierarchy
4. **Flat structure with no hierarchy**: Rejected - chart of accounts inherently hierarchical

**References**:
- PostgreSQL recursive queries: https://www.postgresql.org/docs/current/queries-with.html
- Tree structures in SQL: https://learn.microsoft.com/en-us/sql/relational-databases/hierarchical-data-sql-server

---

## Summary of Key Decisions

| Topic | Decision | Primary Rationale |
|-------|----------|-------------------|
| Double-entry validation | DB constraint + app validation | Defense-in-depth for zero tolerance requirement |
| Transaction isolation | Serializable for posting, Read Committed for queries | Prevents race conditions, ensures atomicity |
| Idempotency | Redis registry + event ID tracking | Fast lookups, automatic TTL cleanup |
| Concurrency control | Row version + explicit locking | Optimistic for most ops, pessimistic for period close |
| Retry policy | 3 retries, exponential backoff, max 60s | Covers 95% transient failures per spec |
| Distributed tracing | W3C Trace Context via MassTransit | Standard protocol, automatic propagation |
| Event registry TTL | 24 hours | Balances detection window vs. memory |
| Report optimization | Indexed queries initially | Meets 2-min target, add mat views if needed |
| Audit immutability | Append-only + DB permissions | Strongest guarantee, simple design |
| Chart hierarchy | Adjacency list + recursive CTE | Simple maintenance, efficient for shallow trees |

## Next Steps

1. Proceed to Phase 1: Generate data-model.md with entity schemas based on these decisions
2. Create API contracts in contracts/ directory
3. Generate quickstart.md for local development setup
4. Update agent context with new technology patterns discovered during research

All research topics resolved - no NEEDS CLARIFICATION items remaining.
