using Maliev.AccountingService.Data.Data;
using Maliev.AccountingService.Data.Models;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace Maliev.AccountingService.Api.Services;

/// <summary>
/// Implementation of IPeriodService for managing financial periods and fiscal years
/// </summary>
public class PeriodService : IPeriodService
{
    private readonly AccountingDbContext _context;
    private readonly IAuditService _auditService;
    private readonly ILogger<PeriodService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="PeriodService"/> class.
    /// </summary>
    /// <param name="context">The accounting database context.</param>
    /// <param name="auditService">The audit service.</param>
    /// <param name="logger">The logger.</param>
    public PeriodService(
        AccountingDbContext context,
        IAuditService auditService,
        ILogger<PeriodService> logger)
    {
        _context = context;
        _auditService = auditService;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<FinancialPeriod> GetOrCreatePeriodAsync(DateTime transactionDate, CancellationToken cancellationToken = default)
    {
        var year = transactionDate.Year;
        var month = transactionDate.Month;
        var periodName = $"{year}-{month:D2}";

        // Fast path: Try to get without transaction first
        var period = await _context.FinancialPeriods
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Name == periodName, cancellationToken);

        if (period != null) return period;

        // If we are already in a transaction, don't use execution strategy and don't start new transaction
        if (_context.Database.CurrentTransaction != null)
        {
            return await GetOrCreatePeriodInternalAsync(transactionDate, cancellationToken);
        }

        // If not found, we need to create it. Handle creation with a retry strategy
        // but avoid global Serializable locks for standard lookups.
        var strategy = _context.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async () =>
        {
            // Re-check if it was created by another process before starting transaction
            var p = await _context.FinancialPeriods
                .FirstOrDefaultAsync(px => px.Name == periodName, cancellationToken);
            if (p != null) return p;

            using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);

            try
            {
                var result = await GetOrCreatePeriodInternalAsync(transactionDate, cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                return result;
            }
            catch (DbUpdateException) // Handle potential race condition on unique index
            {
                await transaction.RollbackAsync(cancellationToken);

                // Final re-check
                return await _context.FinancialPeriods
                    .FirstAsync(px => px.Name == periodName, cancellationToken);
            }
            catch (Exception)
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        });
    }

    private async Task<FinancialPeriod> GetOrCreatePeriodInternalAsync(DateTime transactionDate, CancellationToken cancellationToken)
    {
        var year = transactionDate.Year;
        var month = transactionDate.Month;
        var periodName = $"{year}-{month:D2}";

        var period = await _context.FinancialPeriods
            .FirstOrDefaultAsync(p => p.Name == periodName, cancellationToken);

        if (period != null) return period;

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
            // SaveChanges will be called by the outer context or here
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

        return period;
    }

    /// <inheritdoc />
    public async Task<string> GenerateEntryNumberAsync(Guid periodId, CancellationToken cancellationToken = default)
    {
        // Check if we already have a transaction
        if (_context.Database.CurrentTransaction != null)
        {
            return await GenerateEntryNumberInternalAsync(periodId, cancellationToken);
        }

        var strategy = _context.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
            try
            {
                var entryNumber = await GenerateEntryNumberInternalAsync(periodId, cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                return entryNumber;
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        });
    }

    private async Task<string> GenerateEntryNumberInternalAsync(Guid periodId, CancellationToken cancellationToken)
    {
        var period = await _context.FinancialPeriods.FindAsync(new object[] { periodId }, cancellationToken);
        if (period == null)
        {
            throw new InvalidOperationException($"Period {periodId} not found");
        }

        // Use database sequence for thread-safe sequential numbering
        var command = _context.Database.GetDbConnection().CreateCommand();
        command.CommandText = "SELECT nextval('journal_entry_number_seq')";

        if (command.Connection!.State != ConnectionState.Open)
        {
            await command.Connection.OpenAsync(cancellationToken);
        }

        var sequenceValue = (long)(await command.ExecuteScalarAsync(cancellationToken) ?? 1L);

        return $"JE-{period.Name}-{sequenceValue:D5}";
    }

    /// <inheritdoc />
    public async Task ClosePeriodAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var period = await _context.FinancialPeriods
            .Include(p => p.JournalEntries)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

        if (period == null)
        {
            throw new InvalidOperationException($"Period {id} not found");
        }

        if (period.Status == PeriodStatus.Closed)
        {
            return;
        }

        // Validate: All entries must be posted (no drafts)
        var draftEntries = await _context.JournalEntries
            .AnyAsync(j => j.PeriodId == id && j.Status == EntryStatus.Draft, cancellationToken);

        if (draftEntries)
        {
            throw new InvalidOperationException($"Cannot close period {period.Name} because it contains draft journal entries.");
        }

        // Validate: Total debits must equal total credits for the period
        var balances = await _context.JournalEntryLines
            .Where(l => l.JournalEntry.PeriodId == id)
            .GroupBy(l => 1)
            .Select(g => new
            {
                TotalDebit = g.Sum(l => l.DebitAmount),
                TotalCredit = g.Sum(l => l.CreditAmount)
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (balances != null && balances.TotalDebit != balances.TotalCredit)
        {
            throw new InvalidOperationException($"Cannot close period {period.Name} because total debits ({balances.TotalDebit}) do not equal total credits ({balances.TotalCredit}).");
        }

        var beforeState = period.Status;
        period.Status = PeriodStatus.Closed;
        period.ClosedAt = DateTime.UtcNow;
        period.ClosedBy = Guid.Empty; // TODO: Get from context

        await _context.SaveChangesAsync(cancellationToken);

        await _auditService.RecordAuditAsync(
            "FinancialPeriod",
            period.Id.ToString(),
            "Closed",
            beforeState,
            period.Status,
            "system",
            null,
            null,
            cancellationToken);

        _logger.LogInformation("Financial period {PeriodName} closed successfully", period.Name);
    }

    /// <inheritdoc />
    public async Task ReopenPeriodAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var period = await _context.FinancialPeriods.FindAsync(new object[] { id }, cancellationToken);
        if (period == null)
        {
            throw new InvalidOperationException($"Period {id} not found");
        }

        if (period.Status == PeriodStatus.Open)
        {
            return;
        }

        var beforeState = period.Status;
        period.Status = PeriodStatus.Open;

        await _context.SaveChangesAsync(cancellationToken);

        await _auditService.RecordAuditAsync(
            "FinancialPeriod",
            period.Id.ToString(),
            "Reopened",
            beforeState,
            period.Status,
            "system",
            null,
            null,
            cancellationToken);

        _logger.LogInformation("Financial period {PeriodName} reopened successfully", period.Name);
    }

    /// <inheritdoc />
    public async Task ValidatePeriodForPostingAsync(Guid periodId, bool isAdjustingEntry = false, CancellationToken cancellationToken = default)
    {
        var period = await _context.FinancialPeriods.FindAsync(new object[] { periodId }, cancellationToken);
        if (period == null)
        {
            throw new InvalidOperationException($"Period {periodId} not found");
        }

        if (period.Status == PeriodStatus.Closed && !isAdjustingEntry)
        {
            throw new InvalidOperationException($"Cannot post to closed period {period.Name}");
        }

        if (period.Status == PeriodStatus.Locked)
        {
            throw new InvalidOperationException($"Period {period.Name} is locked and cannot be modified even with adjustments.");
        }
    }
}