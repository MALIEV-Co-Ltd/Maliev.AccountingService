using Maliev.AccountingService.Api.Events;
using Maliev.AccountingService.Api.Metrics;
using Maliev.AccountingService.Data.Data;
using Maliev.AccountingService.Data.Models;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Diagnostics;

namespace Maliev.AccountingService.Api.Services;

/// <summary>
/// Service for processing financial events and creating double-entry journal entries
/// </summary>
public class EventProcessingService : IEventProcessingService
{
    private readonly AccountingDbContext _context;
    private readonly IEventIdempotencyService _idempotencyService;
    private readonly IAuditService _auditService;
    private readonly ILogger<EventProcessingService> _logger;
    private readonly AccountingMetrics _metrics;

    // Standard account codes - these should match the seeded chart of accounts
    private const string AR_ACCOUNT = "1200"; // Accounts Receivable
    private const string REVENUE_ACCOUNT = "4000"; // Sales Revenue
    private const string VAT_OUTPUT_ACCOUNT = "2300"; // VAT Output Tax Payable
    private const string CASH_ACCOUNT = "1100"; // Cash/Bank
    private const string EXPENSE_ACCOUNT = "5000"; // Operating Expenses
    private const string VAT_INPUT_ACCOUNT = "1300"; // VAT Input Tax Recoverable
    private const string AP_ACCOUNT = "2100"; // Accounts Payable
    private const string INVENTORY_ACCOUNT = "1400"; // Inventory
    private const string PAYROLL_EXPENSE_ACCOUNT = "5100"; // Payroll Expense
    private const string TAX_PAYABLE_ACCOUNT = "2200"; // Tax Payable
    private const string INSURANCE_PAYABLE_ACCOUNT = "2210"; // Insurance Payable
    private const string PENSION_PAYABLE_ACCOUNT = "2220"; // Pension Payable

    public EventProcessingService(
        AccountingDbContext context,
        IEventIdempotencyService idempotencyService,
        IAuditService auditService,
        ILogger<EventProcessingService> logger,
        AccountingMetrics metrics)
    {
        _context = context;
        _idempotencyService = idempotencyService;
        _auditService = auditService;
        _logger = logger;
        _metrics = metrics;
    }

    public async Task<Guid> ProcessInvoiceCreatedAsync(InvoiceCreatedEvent @event, CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        using var activity = Activity.Current?.Source.StartActivity("ProcessInvoiceCreated");
        activity?.SetTag("event.id", @event.EventId);

        // Record event ingestion
        _metrics.RecordEventIngestion("InvoiceCreated", "Sales");

        // Check idempotency
        // Check idempotency
        if (await _idempotencyService.IsEventProcessedAsync(@event.EventId.ToString(), cancellationToken))
        {
            var existingId = await _idempotencyService.GetJournalEntryIdAsync(@event.EventId.ToString(), cancellationToken);
            _logger.LogWarning("Event {EventId} already processed, returning existing journal entry {JournalEntryId}",
                @event.EventId, existingId);
            return existingId ?? throw new InvalidOperationException("Event was processed but journal entry ID not found");
        }

        // Use execution strategy for retries
        var strategy = _context.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            // Use Serializable isolation for atomic processing
            using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);

            try
            {
            // Get or create financial period
            var period = await GetOrCreatePeriodAsync(@event.InvoiceDate, cancellationToken);

            // Validate period is open
            if (period.Status != PeriodStatus.Open)
            {
                throw new InvalidOperationException($"Cannot post to closed period {period.Name}");
            }

            // Get accounts
            var arAccount = await GetAccountByCodeAsync(AR_ACCOUNT, cancellationToken);
            var revenueAccount = await GetAccountByCodeAsync(REVENUE_ACCOUNT, cancellationToken);
            var vatOutputAccount = await GetAccountByCodeAsync(VAT_OUTPUT_ACCOUNT, cancellationToken);

            // Create journal entry
            var journalEntry = new JournalEntry
            {
                Id = Guid.NewGuid(),
                PeriodId = period.Id,
                EntryNumber = await GenerateEntryNumberAsync(period.Id, cancellationToken),
                EntryDate = @event.InvoiceDate,
                Description = $"Sales Invoice {@event.InvoiceNumber} - Customer {@event.CustomerId}",
                Status = EntryStatus.Posted,
                SourceSystem = "Sales",
                SourceEventId = @event.EventId.ToString(),
                CreatedBy = Guid.Empty, // System-generated
                PostedAt = DateTime.UtcNow,
                PostedBy = Guid.Empty
            };

            var lines = new List<JournalEntryLine>();

            // Debit: Accounts Receivable (Total Amount)
            lines.Add(new JournalEntryLine
            {
                Id = Guid.NewGuid(),
                JournalEntryId = journalEntry.Id,
                AccountId = arAccount.Id,
                LineSequence = 1,
                Description = $"Invoice {@event.InvoiceNumber}",
                DebitAmount = @event.TotalAmount,
                CreditAmount = 0,
                ReferenceType = "Invoice",
                ReferenceId = @event.InvoiceId.ToString(),
                CustomerId = @event.CustomerId
            });

            // Credit: Revenue (Subtotal Amount)
            lines.Add(new JournalEntryLine
            {
                Id = Guid.NewGuid(),
                JournalEntryId = journalEntry.Id,
                AccountId = revenueAccount.Id,
                LineSequence = 2,
                Description = $"Revenue from invoice {@event.InvoiceNumber}",
                DebitAmount = 0,
                CreditAmount = @event.SubtotalAmount,
                ReferenceType = "Invoice",
                ReferenceId = @event.InvoiceId.ToString(),
                CustomerId = @event.CustomerId
            });

            // Credit: VAT Output Tax (Tax Amount)
            if (@event.TaxAmount > 0)
            {
                var vatLine = new JournalEntryLine
                {
                    Id = Guid.NewGuid(),
                    JournalEntryId = journalEntry.Id,
                    AccountId = vatOutputAccount.Id,
                    LineSequence = 3,
                    Description = $"VAT on invoice {@event.InvoiceNumber}",
                    DebitAmount = 0,
                    CreditAmount = @event.TaxAmount,
                    ReferenceType = "Invoice",
                    ReferenceId = @event.InvoiceId.ToString(),
                    CustomerId = @event.CustomerId
                };
                lines.Add(vatLine);

                // Create tax component
                vatLine.TaxComponents.Add(new TaxComponent
                {
                    Id = Guid.NewGuid(),
                    JournalEntryLineId = vatLine.Id,
                    TaxType = @event.TaxType,
                    TaxRate = @event.TaxRate,
                    TaxableAmount = @event.SubtotalAmount,
                    TaxAmount = @event.TaxAmount
                });
            }

            journalEntry.Lines = lines;
            journalEntry.TotalDebit = lines.Sum(l => l.DebitAmount);
            journalEntry.TotalCredit = lines.Sum(l => l.CreditAmount);

            // Validate balanced entry
            if (journalEntry.TotalDebit != journalEntry.TotalCredit)
            {
                throw new InvalidOperationException(
                    $"Journal entry is not balanced: Debit={journalEntry.TotalDebit}, Credit={journalEntry.TotalCredit}");
            }

            // Create subledger transaction
            var subledgerTx = new SubledgerTransaction
            {
                Id = Guid.NewGuid(),
                JournalEntryId = journalEntry.Id,
                SourceSystem = "Sales",
                TransactionType = "Invoice",
                SourceTransactionId = @event.InvoiceId.ToString(),
                TransactionDate = @event.InvoiceDate,
                Amount = @event.TotalAmount,
                CustomerId = @event.CustomerId
            };

            _context.JournalEntries.Add(journalEntry);
            _context.SubledgerTransactions.Add(subledgerTx);

            // Create processed event registry entry
            _context.ProcessedEventRegistry.Add(new ProcessedEventRegistry
            {
                Id = Guid.NewGuid(),
                EventId = @event.EventId.ToString(),
                JournalEntryId = journalEntry.Id,
                ProcessedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync(cancellationToken);

            // Record audit trail
            await _auditService.RecordAuditAsync(
                "JournalEntry",
                journalEntry.Id.ToString(),
                "Posted",
                null,
                journalEntry,
                "system",
                Activity.Current?.Id,
                null,
                cancellationToken);

            // Mark event as processed in Redis
            await _idempotencyService.MarkEventAsProcessedAsync(@event.EventId.ToString(), journalEntry.Id, cancellationToken);

            await transaction.CommitAsync(cancellationToken);

            activity?.SetTag("journal.entry.id", journalEntry.Id);
            _logger.LogInformation(
                "Successfully processed InvoiceCreated event {EventId}, created journal entry {JournalEntryId}",
                @event.EventId, journalEntry.Id);

            // Record success metrics
            stopwatch.Stop();
            _metrics.RecordProcessingLatency(stopwatch.Elapsed.TotalMilliseconds, "InvoiceCreated", success: true);
            _metrics.RecordTransactionProcessed("InvoiceCreated", @event.TotalAmount);

            return journalEntry.Id;
        }
        catch (Exception ex)
        {
             await transaction.RollbackAsync(cancellationToken);
             _logger.LogError(ex, "Failed to process InvoiceCreated event {EventId}", @event.EventId);

            // Record error metrics
            stopwatch.Stop();
            _metrics.RecordProcessingLatency(stopwatch.Elapsed.TotalMilliseconds, "InvoiceCreated", success: false);
            _metrics.RecordProcessingError("InvoiceCreated", ex.GetType().Name);

            throw;
        }
        });
    }

    public async Task<Guid> ProcessPaymentReceivedAsync(PaymentReceivedEvent @event, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.Current?.Source.StartActivity("ProcessPaymentReceived");
        activity?.SetTag("event.id", @event.EventId);

        // Check idempotency
        if (await _idempotencyService.IsEventProcessedAsync(@event.EventId.ToString(), cancellationToken))
        {
            var existingId = await _idempotencyService.GetJournalEntryIdAsync(@event.EventId.ToString(), cancellationToken);
            _logger.LogWarning("Event {EventId} already processed", @event.EventId);
            return existingId ?? throw new InvalidOperationException("Event was processed but journal entry ID not found");
        }

        // Use execution strategy for retries
        var strategy = _context.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
        using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);

        try
        {
            var period = await GetOrCreatePeriodAsync(@event.PaymentDate, cancellationToken);

            if (period.Status != PeriodStatus.Open)
            {
                throw new InvalidOperationException($"Cannot post to closed period {period.Name}");
            }

            var cashAccount = await GetAccountByCodeAsync(CASH_ACCOUNT, cancellationToken);
            var arAccount = await GetAccountByCodeAsync(AR_ACCOUNT, cancellationToken);

            var journalEntry = new JournalEntry
            {
                Id = Guid.NewGuid(),
                PeriodId = period.Id,
                EntryNumber = await GenerateEntryNumberAsync(period.Id, cancellationToken),
                EntryDate = @event.PaymentDate,
                Description = $"Payment {@event.PaymentNumber} - {@event.PaymentMethod}",
                Status = EntryStatus.Posted,
                SourceSystem = "Sales",
                SourceEventId = @event.EventId.ToString(),
                CreatedBy = Guid.Empty,
                PostedAt = DateTime.UtcNow,
                PostedBy = Guid.Empty
            };

            var lines = new List<JournalEntryLine>
            {
                // Debit: Cash
                new JournalEntryLine
                {
                    Id = Guid.NewGuid(),
                    JournalEntryId = journalEntry.Id,
                    AccountId = cashAccount.Id,
                    LineSequence = 1,
                    Description = $"Payment received {@event.PaymentNumber}",
                    DebitAmount = @event.Amount,
                    CreditAmount = 0,
                    ReferenceType = "Payment",
                    ReferenceId = @event.PaymentId.ToString(),
                    CustomerId = @event.CustomerId
                },
                // Credit: Accounts Receivable
                new JournalEntryLine
                {
                    Id = Guid.NewGuid(),
                    JournalEntryId = journalEntry.Id,
                    AccountId = arAccount.Id,
                    LineSequence = 2,
                    Description = $"Payment against AR {@event.PaymentNumber}",
                    DebitAmount = 0,
                    CreditAmount = @event.Amount,
                    ReferenceType = "Payment",
                    ReferenceId = @event.PaymentId.ToString(),
                    CustomerId = @event.CustomerId
                }
            };

            journalEntry.Lines = lines;
            journalEntry.TotalDebit = lines.Sum(l => l.DebitAmount);
            journalEntry.TotalCredit = lines.Sum(l => l.CreditAmount);

            if (journalEntry.TotalDebit != journalEntry.TotalCredit)
            {
                throw new InvalidOperationException("Journal entry is not balanced");
            }

            var subledgerTx = new SubledgerTransaction
            {
                Id = Guid.NewGuid(),
                JournalEntryId = journalEntry.Id,
                SourceSystem = "Sales",
                TransactionType = "Payment",
                SourceTransactionId = @event.PaymentId.ToString(),
                TransactionDate = @event.PaymentDate,
                Amount = @event.Amount,
                CustomerId = @event.CustomerId
            };

            _context.JournalEntries.Add(journalEntry);
            _context.SubledgerTransactions.Add(subledgerTx);
            _context.ProcessedEventRegistry.Add(new ProcessedEventRegistry
            {
                Id = Guid.NewGuid(),
                EventId = @event.EventId.ToString(),
                JournalEntryId = journalEntry.Id,
                ProcessedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync(cancellationToken);

            await _auditService.RecordAuditAsync(
                "JournalEntry",
                journalEntry.Id.ToString(),
                "Posted",
                null,
                journalEntry,
                "system",
                Activity.Current?.Id,
                null,
                cancellationToken);

            await _idempotencyService.MarkEventAsProcessedAsync(@event.EventId.ToString(), journalEntry.Id, cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            activity?.SetTag("journal.entry.id", journalEntry.Id);
            _logger.LogInformation("Successfully processed PaymentReceived event {EventId}", @event.EventId);

            return journalEntry.Id;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            _logger.LogError(ex, "Failed to process PaymentReceived event {EventId}", @event.EventId);
            throw;
        }
        });
    }

    public async Task<Guid> ProcessSupplierInvoiceAsync(SupplierInvoiceEvent @event, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.Current?.Source.StartActivity("ProcessSupplierInvoice");
        activity?.SetTag("event.id", @event.EventId);

        if (await _idempotencyService.IsEventProcessedAsync(@event.EventId.ToString(), cancellationToken))
        {
            var existingId = await _idempotencyService.GetJournalEntryIdAsync(@event.EventId.ToString(), cancellationToken);
            return existingId ?? throw new InvalidOperationException("Event was processed but journal entry ID not found");
        }

        // Use execution strategy for retries
        var strategy = _context.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
        using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);

        try
        {
            var period = await GetOrCreatePeriodAsync(@event.InvoiceDate, cancellationToken);

            if (period.Status != PeriodStatus.Open)
            {
                throw new InvalidOperationException($"Cannot post to closed period {period.Name}");
            }

            var expenseAccount = await GetAccountByCodeAsync(EXPENSE_ACCOUNT, cancellationToken);
            var vatInputAccount = await GetAccountByCodeAsync(VAT_INPUT_ACCOUNT, cancellationToken);
            var apAccount = await GetAccountByCodeAsync(AP_ACCOUNT, cancellationToken);

            var journalEntry = new JournalEntry
            {
                Id = Guid.NewGuid(),
                PeriodId = period.Id,
                EntryNumber = await GenerateEntryNumberAsync(period.Id, cancellationToken),
                EntryDate = @event.InvoiceDate,
                Description = $"Supplier Invoice {@event.InvoiceNumber} - Supplier {@event.SupplierId}",
                Status = EntryStatus.Posted,
                SourceSystem = "Procurement",
                SourceEventId = @event.EventId.ToString(),
                CreatedBy = Guid.Empty,
                PostedAt = DateTime.UtcNow,
                PostedBy = Guid.Empty
            };

            var lines = new List<JournalEntryLine>
            {
                // Debit: Expense
                new JournalEntryLine
                {
                    Id = Guid.NewGuid(),
                    JournalEntryId = journalEntry.Id,
                    AccountId = expenseAccount.Id,
                    LineSequence = 1,
                    Description = $"Expense from supplier invoice {@event.InvoiceNumber}",
                    DebitAmount = @event.SubtotalAmount,
                    CreditAmount = 0,
                    ReferenceType = "SupplierInvoice",
                    ReferenceId = @event.InvoiceId.ToString(),
                    SupplierId = @event.SupplierId
                }
            };

            // Debit: VAT Input Tax
            if (@event.TaxAmount > 0)
            {
                var vatLine = new JournalEntryLine
                {
                    Id = Guid.NewGuid(),
                    JournalEntryId = journalEntry.Id,
                    AccountId = vatInputAccount.Id,
                    LineSequence = 2,
                    Description = $"VAT on supplier invoice {@event.InvoiceNumber}",
                    DebitAmount = @event.TaxAmount,
                    CreditAmount = 0,
                    ReferenceType = "SupplierInvoice",
                    ReferenceId = @event.InvoiceId.ToString(),
                    SupplierId = @event.SupplierId
                };
                lines.Add(vatLine);

                vatLine.TaxComponents.Add(new TaxComponent
                {
                    Id = Guid.NewGuid(),
                    JournalEntryLineId = vatLine.Id,
                    TaxType = @event.TaxType,
                    TaxRate = @event.TaxRate,
                    TaxableAmount = @event.SubtotalAmount,
                    TaxAmount = @event.TaxAmount
                });
            }

            // Credit: Accounts Payable
            lines.Add(new JournalEntryLine
            {
                Id = Guid.NewGuid(),
                JournalEntryId = journalEntry.Id,
                AccountId = apAccount.Id,
                LineSequence = lines.Count + 1,
                Description = $"Supplier invoice {@event.InvoiceNumber}",
                DebitAmount = 0,
                CreditAmount = @event.TotalAmount,
                ReferenceType = "SupplierInvoice",
                ReferenceId = @event.InvoiceId.ToString(),
                SupplierId = @event.SupplierId
            });

            journalEntry.Lines = lines;
            journalEntry.TotalDebit = lines.Sum(l => l.DebitAmount);
            journalEntry.TotalCredit = lines.Sum(l => l.CreditAmount);

            if (journalEntry.TotalDebit != journalEntry.TotalCredit)
            {
                throw new InvalidOperationException("Journal entry is not balanced");
            }

            var subledgerTx = new SubledgerTransaction
            {
                Id = Guid.NewGuid(),
                JournalEntryId = journalEntry.Id,
                SourceSystem = "Procurement",
                TransactionType = "SupplierInvoice",
                SourceTransactionId = @event.InvoiceId.ToString(),
                TransactionDate = @event.InvoiceDate,
                Amount = @event.TotalAmount,
                SupplierId = @event.SupplierId
            };

            _context.JournalEntries.Add(journalEntry);
            _context.SubledgerTransactions.Add(subledgerTx);
            _context.ProcessedEventRegistry.Add(new ProcessedEventRegistry
            {
                Id = Guid.NewGuid(),
                EventId = @event.EventId.ToString(),
                JournalEntryId = journalEntry.Id,
                ProcessedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync(cancellationToken);

            await _auditService.RecordAuditAsync(
                "JournalEntry",
                journalEntry.Id.ToString(),
                "Posted",
                null,
                journalEntry,
                "system",
                Activity.Current?.Id,
                null,
                cancellationToken);

            await _idempotencyService.MarkEventAsProcessedAsync(@event.EventId.ToString(), journalEntry.Id, cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            activity?.SetTag("journal.entry.id", journalEntry.Id);
            _logger.LogInformation("Successfully processed SupplierInvoice event {EventId}", @event.EventId);

            return journalEntry.Id;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            _logger.LogError(ex, "Failed to process SupplierInvoice event {EventId}", @event.EventId);
            throw;
        }
        });
    }

    public async Task<Guid> ProcessInventoryMovementAsync(InventoryMovementEvent @event, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.Current?.Source.StartActivity("ProcessInventoryMovement");
        activity?.SetTag("event.id", @event.EventId);

        if (await _idempotencyService.IsEventProcessedAsync(@event.EventId.ToString(), cancellationToken))
        {
            var existingId = await _idempotencyService.GetJournalEntryIdAsync(@event.EventId.ToString(), cancellationToken);
            return existingId ?? throw new InvalidOperationException("Event was processed but journal entry ID not found");
        }

        // Use execution strategy for retries
        var strategy = _context.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
        using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);

        try
        {
            var period = await GetOrCreatePeriodAsync(@event.MovementDate, cancellationToken);

            if (period.Status != PeriodStatus.Open)
            {
                throw new InvalidOperationException($"Cannot post to closed period {period.Name}");
            }

            var inventoryAccount = await GetAccountByCodeAsync(INVENTORY_ACCOUNT, cancellationToken);
            var apAccount = await GetAccountByCodeAsync(AP_ACCOUNT, cancellationToken);
            var cashAccount = await GetAccountByCodeAsync(CASH_ACCOUNT, cancellationToken);

            var journalEntry = new JournalEntry
            {
                Id = Guid.NewGuid(),
                PeriodId = period.Id,
                EntryNumber = await GenerateEntryNumberAsync(period.Id, cancellationToken),
                EntryDate = @event.MovementDate,
                Description = $"Inventory Movement {@event.MovementNumber} - {@event.MovementType} - {@event.ProductName}",
                Status = EntryStatus.Posted,
                SourceSystem = "Inventory",
                SourceEventId = @event.EventId.ToString(),
                CreatedBy = Guid.Empty,
                PostedAt = DateTime.UtcNow,
                PostedBy = Guid.Empty
            };

            var lines = new List<JournalEntryLine>();

            // For Purchase movements: Debit Inventory, Credit AP or Cash
            if (@event.MovementType == "Purchase")
            {
                lines.Add(new JournalEntryLine
                {
                    Id = Guid.NewGuid(),
                    JournalEntryId = journalEntry.Id,
                    AccountId = inventoryAccount.Id,
                    LineSequence = 1,
                    Description = $"Inventory purchase {@event.ProductName}",
                    DebitAmount = @event.TotalCost,
                    CreditAmount = 0,
                    ReferenceType = "InventoryMovement",
                    ReferenceId = @event.MovementId.ToString()
                });

                var creditAccount = @event.SupplierId.HasValue ? apAccount : cashAccount;
                lines.Add(new JournalEntryLine
                {
                    Id = Guid.NewGuid(),
                    JournalEntryId = journalEntry.Id,
                    AccountId = creditAccount.Id,
                    LineSequence = 2,
                    Description = $"Payment for inventory purchase",
                    DebitAmount = 0,
                    CreditAmount = @event.TotalCost,
                    ReferenceType = "InventoryMovement",
                    ReferenceId = @event.MovementId.ToString(),
                    SupplierId = @event.SupplierId
                });
            }

            journalEntry.Lines = lines;
            journalEntry.TotalDebit = lines.Sum(l => l.DebitAmount);
            journalEntry.TotalCredit = lines.Sum(l => l.CreditAmount);

            if (journalEntry.TotalDebit != journalEntry.TotalCredit)
            {
                throw new InvalidOperationException("Journal entry is not balanced");
            }

            var subledgerTx = new SubledgerTransaction
            {
                Id = Guid.NewGuid(),
                JournalEntryId = journalEntry.Id,
                SourceSystem = "Inventory",
                TransactionType = "InventoryMovement",
                SourceTransactionId = @event.MovementId.ToString(),
                TransactionDate = @event.MovementDate,
                Amount = @event.TotalCost,
                SupplierId = @event.SupplierId
            };

            _context.JournalEntries.Add(journalEntry);
            _context.SubledgerTransactions.Add(subledgerTx);
            _context.ProcessedEventRegistry.Add(new ProcessedEventRegistry
            {
                Id = Guid.NewGuid(),
                EventId = @event.EventId.ToString(),
                JournalEntryId = journalEntry.Id,
                ProcessedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync(cancellationToken);

            await _auditService.RecordAuditAsync(
                "JournalEntry",
                journalEntry.Id.ToString(),
                "Posted",
                null,
                journalEntry,
                "system",
                Activity.Current?.Id,
                null,
                cancellationToken);

            await _idempotencyService.MarkEventAsProcessedAsync(@event.EventId.ToString(), journalEntry.Id, cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            activity?.SetTag("journal.entry.id", journalEntry.Id);
            _logger.LogInformation("Successfully processed InventoryMovement event {EventId}", @event.EventId);

            return journalEntry.Id;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            _logger.LogError(ex, "Failed to process InventoryMovement event {EventId}", @event.EventId);
            throw;
        }
        });
    }

    public async Task<Guid> ProcessPayrollProcessedAsync(PayrollProcessedEvent @event, CancellationToken cancellationToken = default)
    {
        using var activity = Activity.Current?.Source.StartActivity("ProcessPayrollProcessed");
        activity?.SetTag("event.id", @event.EventId);

        if (await _idempotencyService.IsEventProcessedAsync(@event.EventId.ToString(), cancellationToken))
        {
            var existingId = await _idempotencyService.GetJournalEntryIdAsync(@event.EventId.ToString(), cancellationToken);
            return existingId ?? throw new InvalidOperationException("Event was processed but journal entry ID not found");
        }

        // Use execution strategy for retries
        var strategy = _context.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
        using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);

        try
        {
            var period = await GetOrCreatePeriodAsync(@event.PaymentDate, cancellationToken);

            if (period.Status != PeriodStatus.Open)
            {
                throw new InvalidOperationException($"Cannot post to closed period {period.Name}");
            }

            var payrollExpenseAccount = await GetAccountByCodeAsync(PAYROLL_EXPENSE_ACCOUNT, cancellationToken);

            var journalEntry = new JournalEntry
            {
                Id = Guid.NewGuid(),
                PeriodId = period.Id,
                EntryNumber = await GenerateEntryNumberAsync(period.Id, cancellationToken),
                EntryDate = @event.PaymentDate,
                Description = $"Payroll {@event.PayrollNumber} - Period {@event.PayPeriodStart:yyyy-MM-dd} to {@event.PayPeriodEnd:yyyy-MM-dd}",
                Status = EntryStatus.Posted,
                SourceSystem = "Payroll",
                SourceEventId = @event.EventId.ToString(),
                CreatedBy = Guid.Empty,
                PostedAt = DateTime.UtcNow,
                PostedBy = Guid.Empty
            };

            var lines = new List<JournalEntryLine>
            {
                // Debit: Payroll Expense (Gross Pay)
                new JournalEntryLine
                {
                    Id = Guid.NewGuid(),
                    JournalEntryId = journalEntry.Id,
                    AccountId = payrollExpenseAccount.Id,
                    LineSequence = 1,
                    Description = $"Payroll expense {@event.PayrollNumber}",
                    DebitAmount = @event.GrossPay,
                    CreditAmount = 0,
                    ReferenceType = "Payroll",
                    ReferenceId = @event.PayrollId.ToString()
                }
            };

            // Credit: Various payable accounts for deductions
            int lineSeq = 2;
            foreach (var deduction in @event.Deductions)
            {
                var accountCode = deduction.DeductionType.ToLower() switch
                {
                    "tax" => TAX_PAYABLE_ACCOUNT,
                    "insurance" => INSURANCE_PAYABLE_ACCOUNT,
                    "pension" => PENSION_PAYABLE_ACCOUNT,
                    _ => TAX_PAYABLE_ACCOUNT
                };

                var account = await GetAccountByCodeAsync(accountCode, cancellationToken);

                lines.Add(new JournalEntryLine
                {
                    Id = Guid.NewGuid(),
                    JournalEntryId = journalEntry.Id,
                    AccountId = account.Id,
                    LineSequence = lineSeq++,
                    Description = $"{deduction.DeductionType} deduction",
                    DebitAmount = 0,
                    CreditAmount = deduction.Amount,
                    ReferenceType = "Payroll",
                    ReferenceId = @event.PayrollId.ToString()
                });
            }

            // Credit: Cash (Net Pay)
            var cashAccount = await GetAccountByCodeAsync(CASH_ACCOUNT, cancellationToken);
            lines.Add(new JournalEntryLine
            {
                Id = Guid.NewGuid(),
                JournalEntryId = journalEntry.Id,
                AccountId = cashAccount.Id,
                LineSequence = lineSeq,
                Description = $"Net pay disbursement {@event.PayrollNumber}",
                DebitAmount = 0,
                CreditAmount = @event.NetPay,
                ReferenceType = "Payroll",
                ReferenceId = @event.PayrollId.ToString()
            });

            journalEntry.Lines = lines;
            journalEntry.TotalDebit = lines.Sum(l => l.DebitAmount);
            journalEntry.TotalCredit = lines.Sum(l => l.CreditAmount);

            if (journalEntry.TotalDebit != journalEntry.TotalCredit)
            {
                throw new InvalidOperationException("Journal entry is not balanced");
            }

            var subledgerTx = new SubledgerTransaction
            {
                Id = Guid.NewGuid(),
                JournalEntryId = journalEntry.Id,
                SourceSystem = "Payroll",
                TransactionType = "Payroll",
                SourceTransactionId = @event.PayrollId.ToString(),
                TransactionDate = @event.PaymentDate,
                Amount = @event.GrossPay
            };

            _context.JournalEntries.Add(journalEntry);
            _context.SubledgerTransactions.Add(subledgerTx);
            _context.ProcessedEventRegistry.Add(new ProcessedEventRegistry
            {
                Id = Guid.NewGuid(),
                EventId = @event.EventId.ToString(),
                JournalEntryId = journalEntry.Id,
                ProcessedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync(cancellationToken);

            await _auditService.RecordAuditAsync(
                "JournalEntry",
                journalEntry.Id.ToString(),
                "Posted",
                null,
                journalEntry,
                "system",
                Activity.Current?.Id,
                null,
                cancellationToken);

            await _idempotencyService.MarkEventAsProcessedAsync(@event.EventId.ToString(), journalEntry.Id, cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            activity?.SetTag("journal.entry.id", journalEntry.Id);
            _logger.LogInformation("Successfully processed PayrollProcessed event {EventId}", @event.EventId);

            return journalEntry.Id;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            _logger.LogError(ex, "Failed to process PayrollProcessed event {EventId}", @event.EventId);
            throw;
        }
        });
    }

    private async Task<FinancialPeriod> GetOrCreatePeriodAsync(DateTime transactionDate, CancellationToken cancellationToken)
    {
        // For now, create monthly periods automatically
        var year = transactionDate.Year;
        var month = transactionDate.Month;
        var periodName = $"{year}-{month:D2}";

        var period = await _context.FinancialPeriods
            .FirstOrDefaultAsync(p => p.Name == periodName, cancellationToken);

        if (period == null)
        {
            // Get or create fiscal year
            var fiscalYearName = year.ToString();
            var fiscalYear = await _context.FiscalYears
                .FirstOrDefaultAsync(fy => fy.Name == fiscalYearName, cancellationToken);

            if (fiscalYear == null)
            {
                fiscalYear = new FiscalYear
                {
                    Id = Guid.NewGuid(),
                    Name = fiscalYearName,
                    StartDate = DateTime.SpecifyKind(new DateTime(year, 1, 1), DateTimeKind.Utc),
                    EndDate = DateTime.SpecifyKind(new DateTime(year, 12, 31, 23, 59, 59), DateTimeKind.Utc),
                    PeriodStructure = PeriodStructure.Monthly,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };
                _context.FiscalYears.Add(fiscalYear);
                await _context.SaveChangesAsync(cancellationToken);
            }

            period = new FinancialPeriod
            {
                Id = Guid.NewGuid(),
                FiscalYearId = fiscalYear.Id,
                Name = periodName,
                StartDate = DateTime.SpecifyKind(new DateTime(year, month, 1), DateTimeKind.Utc),
                EndDate = DateTime.SpecifyKind(new DateTime(year, month, DateTime.DaysInMonth(year, month), 23, 59, 59), DateTimeKind.Utc),
                Status = PeriodStatus.Open
            };

            _context.FinancialPeriods.Add(period);
            await _context.SaveChangesAsync(cancellationToken);
        }

        return period;
    }

    private async Task<ChartOfAccount> GetAccountByCodeAsync(string accountCode, CancellationToken cancellationToken)
    {
        var account = await _context.ChartOfAccounts
            .FirstOrDefaultAsync(a => a.AccountNumber == accountCode && a.IsActive, cancellationToken);

        if (account == null)
        {
            throw new InvalidOperationException($"Account with code {accountCode} not found or is inactive");
        }

        return account;
    }

    private async Task<string> GenerateEntryNumberAsync(Guid periodId, CancellationToken cancellationToken)
    {
        var period = await _context.FinancialPeriods.FindAsync(new object[] { periodId }, cancellationToken);
        if (period == null)
        {
            throw new InvalidOperationException($"Period {periodId} not found");
        }

        var count = await _context.JournalEntries
            .CountAsync(j => j.PeriodId == periodId, cancellationToken);

        return $"JE-{period.Name}-{(count + 1):D5}";
    }
}
