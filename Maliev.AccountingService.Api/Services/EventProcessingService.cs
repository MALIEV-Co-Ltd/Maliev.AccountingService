using Maliev.MessagingContracts.Contracts.Accounting;
using Maliev.MessagingContracts.Contracts.Invoices;
using Maliev.MessagingContracts.Generated;
using Maliev.AccountingService.Api.Metrics;
using Maliev.AccountingService.Data.Data;
using Maliev.AccountingService.Data.Models;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Data;
using System.Diagnostics;

namespace Maliev.AccountingService.Api.Services;

/// <summary>
/// Service for processing financial events and creating double-entry journal entries
/// </summary>
public class EventProcessingService : EventProcessingServiceBase, IEventProcessingService
{
    private readonly AccountingDbContext _context;
    private readonly IEventIdempotencyService _idempotencyService;
    private readonly IAuditService _auditService;
    private readonly IPeriodService _periodService;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<EventProcessingService> _logger;
    private readonly AccountingMetrics _metrics;
    private readonly IMemoryCache _memoryCache;

    /// <summary>
    /// Initializes a new instance of the EventProcessingService class.
    /// </summary>
    public EventProcessingService(
        AccountingDbContext context,
        IEventIdempotencyService idempotencyService,
        IAuditService auditService,
        IPeriodService periodService,
        IPublishEndpoint publishEndpoint,
        ILogger<EventProcessingService> logger,
        AccountingMetrics metrics,
        IMemoryCache memoryCache) : base(context, memoryCache)
    {
        _context = context;
        _idempotencyService = idempotencyService;
        _auditService = auditService;
        _periodService = periodService;
        _publishEndpoint = publishEndpoint;
        _logger = logger;
        _metrics = metrics;
        _memoryCache = memoryCache;
    }

    /// <inheritdoc />
    public async Task<Guid> ProcessInvoiceCreatedAsync(InvoiceCreatedEvent @event, CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        using var activity = Activity.Current?.Source.StartActivity("ProcessInvoiceCreated");
        activity?.SetTag("event.id", @event.MessageId);

        _metrics.RecordEventIngestion("InvoiceCreated", "Sales");

        if (await _idempotencyService.IsEventProcessedAsync(@event.MessageId.ToString(), cancellationToken))
        {
            var existingId = await _idempotencyService.GetJournalEntryIdAsync(@event.MessageId.ToString(), cancellationToken);
            return existingId ?? throw new InvalidOperationException("Event was processed but journal entry ID not found");
        }

        var strategy = _context.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
            try
            {
                var period = await _periodService.GetOrCreatePeriodAsync(@event.Payload.CreatedAt.DateTime, cancellationToken);
                await _periodService.ValidatePeriodForPostingAsync(period.Id, isAdjustingEntry: false, cancellationToken);

                var arAccount = await GetAccountByCodeAsync(AR_ACCOUNT, cancellationToken);
                var revenueAccount = await GetAccountByCodeAsync(REVENUE_ACCOUNT, cancellationToken);

                var journalEntry = new JournalEntry
                {
                    Id = Guid.NewGuid(),
                    PeriodId = period.Id,
                    EntryNumber = await _periodService.GenerateEntryNumberAsync(period.Id, cancellationToken),
                    EntryDate = @event.Payload.CreatedAt.DateTime.ToUniversalTime(),
                    Description = $"Sales Invoice {@event.Payload.InvoiceNumber} - Customer {@event.Payload.CustomerId}",
                    Status = EntryStatus.Posted,
                    SourceSystem = "Sales",
                    SourceEventId = @event.MessageId.ToString(),
                    PostedAt = DateTime.UtcNow,
                    PostedBy = Guid.Empty
                };

                var lines = new List<JournalEntryLine>
                {
                    new() { Id = Guid.NewGuid(), JournalEntryId = journalEntry.Id, AccountId = arAccount.Id, LineSequence = 1, Description = $"Invoice {@event.Payload.InvoiceNumber}", DebitAmount = (decimal)@event.Payload.TotalAmount, ReferenceType = "Invoice", ReferenceId = @event.Payload.InvoiceId.ToString(), CustomerId = @event.Payload.CustomerId },
                    new() { Id = Guid.NewGuid(), JournalEntryId = journalEntry.Id, AccountId = revenueAccount.Id, LineSequence = 2, Description = $"Revenue {@event.Payload.InvoiceNumber}", CreditAmount = (decimal)@event.Payload.TotalAmount, ReferenceType = "Invoice", ReferenceId = @event.Payload.InvoiceId.ToString(), CustomerId = @event.Payload.CustomerId }
                };

                journalEntry.Lines = lines;
                journalEntry.TotalDebit = lines.Sum(l => l.DebitAmount);
                journalEntry.TotalCredit = lines.Sum(l => l.CreditAmount);

                _context.JournalEntries.Add(journalEntry);
                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                await _idempotencyService.MarkEventAsProcessedAsync(@event.MessageId.ToString(), journalEntry.Id, cancellationToken);
                await PublishTransactionPostedEvent(@event, journalEntry, cancellationToken);

                return journalEntry.Id;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                _metrics.RecordProcessingError("InvoiceCreated", ex.GetType().Name);
                throw;
            }
        });
    }

    /// <inheritdoc />
    public async Task<Guid> ProcessPaymentReceivedAsync(PaymentRecordedEvent @event, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.Current?.Source.StartActivity("ProcessPaymentReceived");
        if (await _idempotencyService.IsEventProcessedAsync(@event.MessageId.ToString(), cancellationToken))
        {
            return await _idempotencyService.GetJournalEntryIdAsync(@event.MessageId.ToString(), cancellationToken) ?? Guid.Empty;
        }

        var strategy = _context.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
            try
            {
                var period = await _periodService.GetOrCreatePeriodAsync(@event.Payload.PaymentDate.DateTime, cancellationToken);
                var cashAccount = await GetAccountByCodeAsync(CASH_ACCOUNT, cancellationToken);
                var arAccount = await GetAccountByCodeAsync(AR_ACCOUNT, cancellationToken);

                var journalEntry = new JournalEntry
                {
                    Id = Guid.NewGuid(),
                    PeriodId = period.Id,
                    EntryNumber = await _periodService.GenerateEntryNumberAsync(period.Id, cancellationToken),
                    EntryDate = @event.Payload.PaymentDate.DateTime.ToUniversalTime(),
                    Description = $"Payment {@event.Payload.PaymentNumber}",
                    Status = EntryStatus.Posted,
                    SourceSystem = "Sales",
                    SourceEventId = @event.MessageId.ToString(),
                    PostedAt = DateTime.UtcNow
                };

                journalEntry.Lines = new List<JournalEntryLine>
                {
                    new() { Id = Guid.NewGuid(), JournalEntryId = journalEntry.Id, AccountId = cashAccount.Id, LineSequence = 1, DebitAmount = (decimal)@event.Payload.Amount, ReferenceType = "Payment", ReferenceId = @event.Payload.PaymentId.ToString(), CustomerId = @event.Payload.CustomerId ?? Guid.Empty },
                    new() { Id = Guid.NewGuid(), JournalEntryId = journalEntry.Id, AccountId = arAccount.Id, LineSequence = 2, CreditAmount = (decimal)@event.Payload.Amount, ReferenceType = "Payment", ReferenceId = @event.Payload.PaymentId.ToString(), CustomerId = @event.Payload.CustomerId ?? Guid.Empty }
                };

                _context.JournalEntries.Add(journalEntry);
                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                await _idempotencyService.MarkEventAsProcessedAsync(@event.MessageId.ToString(), journalEntry.Id, cancellationToken);
                await PublishTransactionPostedEvent(@event, journalEntry, cancellationToken);
                return journalEntry.Id;
            }
            catch { await transaction.RollbackAsync(cancellationToken); throw; }
        });
    }

    /// <inheritdoc />
    public async Task<Guid> ProcessSupplierInvoiceAsync(SupplierInvoiceReceivedEvent @event, CancellationToken cancellationToken = default)
    {
        if (await _idempotencyService.IsEventProcessedAsync(@event.MessageId.ToString(), cancellationToken))
            return await _idempotencyService.GetJournalEntryIdAsync(@event.MessageId.ToString(), cancellationToken) ?? Guid.Empty;

        var strategy = _context.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
            try
            {
                var period = await _periodService.GetOrCreatePeriodAsync(@event.Payload.InvoiceDate.DateTime, cancellationToken);
                var expenseAccount = await GetAccountByCodeAsync(EXPENSE_ACCOUNT, cancellationToken);
                var apAccount = await GetAccountByCodeAsync(AP_ACCOUNT, cancellationToken);

                var journalEntry = new JournalEntry
                {
                    Id = Guid.NewGuid(),
                    PeriodId = period.Id,
                    EntryNumber = await _periodService.GenerateEntryNumberAsync(period.Id, cancellationToken),
                    EntryDate = @event.Payload.InvoiceDate.DateTime.ToUniversalTime(),
                    Description = $"Supplier Invoice {@event.Payload.InvoiceNumber}",
                    Status = EntryStatus.Posted,
                    SourceSystem = "Procurement",
                    SourceEventId = @event.MessageId.ToString(),
                    PostedAt = DateTime.UtcNow
                };

                journalEntry.Lines = new List<JournalEntryLine>
                {
                    new() { Id = Guid.NewGuid(), JournalEntryId = journalEntry.Id, AccountId = expenseAccount.Id, LineSequence = 1, DebitAmount = (decimal)@event.Payload.TotalAmount, ReferenceType = "SupplierInvoice", ReferenceId = @event.Payload.SupplierInvoiceId.ToString(), SupplierId = @event.Payload.SupplierId },
                    new() { Id = Guid.NewGuid(), JournalEntryId = journalEntry.Id, AccountId = apAccount.Id, LineSequence = 2, CreditAmount = (decimal)@event.Payload.TotalAmount, ReferenceType = "SupplierInvoice", ReferenceId = @event.Payload.SupplierInvoiceId.ToString(), SupplierId = @event.Payload.SupplierId }
                };

                _context.JournalEntries.Add(journalEntry);
                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                await _idempotencyService.MarkEventAsProcessedAsync(@event.MessageId.ToString(), journalEntry.Id, cancellationToken);
                await PublishTransactionPostedEvent(@event, journalEntry, cancellationToken);
                return journalEntry.Id;
            }
            catch { await transaction.RollbackAsync(cancellationToken); throw; }
        });
    }

    /// <inheritdoc />
    public async Task<Guid> ProcessInventoryMovementAsync(InventoryMovementEvent @event, CancellationToken cancellationToken = default)
    {
        if (await _idempotencyService.IsEventProcessedAsync(@event.MessageId.ToString(), cancellationToken))
            return await _idempotencyService.GetJournalEntryIdAsync(@event.MessageId.ToString(), cancellationToken) ?? Guid.Empty;

        var strategy = _context.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
            try
            {
                var period = await _periodService.GetOrCreatePeriodAsync(@event.Payload.MovementDate.DateTime, cancellationToken);
                var inventoryAccount = await GetAccountByCodeAsync(INVENTORY_ACCOUNT, cancellationToken);
                var apAccount = await GetAccountByCodeAsync(AP_ACCOUNT, cancellationToken);

                var journalEntry = new JournalEntry
                {
                    Id = Guid.NewGuid(),
                    PeriodId = period.Id,
                    EntryNumber = await _periodService.GenerateEntryNumberAsync(period.Id, cancellationToken),
                    EntryDate = @event.Payload.MovementDate.DateTime.ToUniversalTime(),
                    Description = $"Inventory {@event.Payload.MovementNumber}",
                    Status = EntryStatus.Posted,
                    SourceSystem = "Inventory",
                    SourceEventId = @event.MessageId.ToString(),
                    PostedAt = DateTime.UtcNow
                };

                journalEntry.Lines = new List<JournalEntryLine>
                {
                    new() { Id = Guid.NewGuid(), JournalEntryId = journalEntry.Id, AccountId = inventoryAccount.Id, LineSequence = 1, DebitAmount = (decimal)@event.Payload.TotalCost, ReferenceType = "Inventory", ReferenceId = @event.Payload.MovementId.ToString() },
                    new() { Id = Guid.NewGuid(), JournalEntryId = journalEntry.Id, AccountId = apAccount.Id, LineSequence = 2, CreditAmount = (decimal)@event.Payload.TotalCost, ReferenceType = "Inventory", ReferenceId = @event.Payload.MovementId.ToString(), SupplierId = @event.Payload.SupplierId }
                };

                _context.JournalEntries.Add(journalEntry);
                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                await _idempotencyService.MarkEventAsProcessedAsync(@event.MessageId.ToString(), journalEntry.Id, cancellationToken);
                await PublishTransactionPostedEvent(@event, journalEntry, cancellationToken);
                return journalEntry.Id;
            }
            catch { await transaction.RollbackAsync(cancellationToken); throw; }
        });
    }

    /// <inheritdoc />
    public async Task<Guid> ProcessPayrollProcessedAsync(PayrollProcessedEvent @event, CancellationToken cancellationToken = default)
    {
        if (await _idempotencyService.IsEventProcessedAsync(@event.MessageId.ToString(), cancellationToken))
            return await _idempotencyService.GetJournalEntryIdAsync(@event.MessageId.ToString(), cancellationToken) ?? Guid.Empty;

        var strategy = _context.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
            try
            {
                var period = await _periodService.GetOrCreatePeriodAsync(@event.Payload.PaymentDate.DateTime, cancellationToken);
                var payrollAccount = await GetAccountByCodeAsync(PAYROLL_EXPENSE_ACCOUNT, cancellationToken);
                var cashAccount = await GetAccountByCodeAsync(CASH_ACCOUNT, cancellationToken);

                var journalEntry = new JournalEntry
                {
                    Id = Guid.NewGuid(),
                    PeriodId = period.Id,
                    EntryNumber = await _periodService.GenerateEntryNumberAsync(period.Id, cancellationToken),
                    EntryDate = @event.Payload.PaymentDate.DateTime.ToUniversalTime(),
                    Description = $"Payroll {@event.Payload.PayrollNumber}",
                    Status = EntryStatus.Posted,
                    SourceSystem = "Payroll",
                    SourceEventId = @event.MessageId.ToString(),
                    PostedAt = DateTime.UtcNow
                };

                journalEntry.Lines = new List<JournalEntryLine>
                {
                    new() { Id = Guid.NewGuid(), JournalEntryId = journalEntry.Id, AccountId = payrollAccount.Id, LineSequence = 1, DebitAmount = (decimal)@event.Payload.GrossPay, ReferenceType = "Payroll", ReferenceId = @event.Payload.PayrollId.ToString() },
                    new() { Id = Guid.NewGuid(), JournalEntryId = journalEntry.Id, AccountId = cashAccount.Id, LineSequence = 2, CreditAmount = (decimal)@event.Payload.NetPay, ReferenceType = "Payroll", ReferenceId = @event.Payload.PayrollId.ToString() }
                };

                _context.JournalEntries.Add(journalEntry);
                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                await _idempotencyService.MarkEventAsProcessedAsync(@event.MessageId.ToString(), journalEntry.Id, cancellationToken);
                await PublishTransactionPostedEvent(@event, journalEntry, cancellationToken);
                return journalEntry.Id;
            }
            catch { await transaction.RollbackAsync(cancellationToken); throw; }
        });
    }

    private async Task PublishTransactionPostedEvent(BaseMessage sourceEvent, JournalEntry entry, CancellationToken ct)
    {
        await _publishEndpoint.Publish(new TransactionPostedEvent(
            MessageId: Guid.NewGuid(),
            MessageName: nameof(TransactionPostedEvent),
            MessageType: MessageType.Event,
            MessageVersion: "1.0.0",
            PublishedBy: "AccountingService",
            ConsumedBy: Array.Empty<string>(),
            CorrelationId: sourceEvent.CorrelationId,
            CausationId: sourceEvent.MessageId,
            OccurredAtUtc: DateTimeOffset.UtcNow,
            IsPublic: false,
            Payload: new TransactionPostedEventPayload(
                JournalEntryId: entry.Id,
                EntryNumber: entry.EntryNumber,
                EntryDate: entry.EntryDate,
                Description: entry.Description,
                TotalAmount: (double)entry.TotalDebit,
                SourceSystem: entry.SourceSystem ?? "Unknown",
                PostedAt: entry.PostedAt ?? DateTimeOffset.UtcNow
            )
        ), ct);
    }
}

/// <summary>
/// Base class for financial event processing services.
/// </summary>
public abstract class EventProcessingServiceBase
{
    /// <summary>Accounts Receivable code.</summary>
    protected const string AR_ACCOUNT = "1200";
    /// <summary>Revenue account code.</summary>
    protected const string REVENUE_ACCOUNT = "4000";
    /// <summary>VAT Output account code.</summary>
    protected const string VAT_OUTPUT_ACCOUNT = "2300";
    /// <summary>Cash/Bank account code.</summary>
    protected const string CASH_ACCOUNT = "1100";
    /// <summary>Operating Expenses code.</summary>
    protected const string EXPENSE_ACCOUNT = "5000";
    /// <summary>VAT Input account code.</summary>
    protected const string VAT_INPUT_ACCOUNT = "1300";
    /// <summary>Accounts Payable code.</summary>
    protected const string AP_ACCOUNT = "2100";
    /// <summary>Inventory account code.</summary>
    protected const string INVENTORY_ACCOUNT = "1400";
    /// <summary>Payroll Expense code.</summary>
    protected const string PAYROLL_EXPENSE_ACCOUNT = "5100";

    private readonly AccountingDbContext _context;
    private readonly IMemoryCache _memoryCache;

    /// <summary>
    /// Initializes a new instance of the EventProcessingServiceBase class.
    /// </summary>
    protected EventProcessingServiceBase(AccountingDbContext context, IMemoryCache memoryCache)
    {
        _context = context;
        _memoryCache = memoryCache;
    }

    /// <summary>
    /// Retrieves an active account by its number.
    /// </summary>
    protected async Task<ChartOfAccount> GetAccountByCodeAsync(string accountCode, CancellationToken ct)
    {
        return await _memoryCache.GetOrCreateAsync($"account_{accountCode}", async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
            return await _context.ChartOfAccounts.FirstOrDefaultAsync(a => a.AccountNumber == accountCode && a.IsActive, ct);
        }) ?? throw new InvalidOperationException($"Account {accountCode} not found");
    }
}
