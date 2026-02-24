using Maliev.AccountingService.Data.Data;
using Maliev.AccountingService.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Maliev.AccountingService.Api.Services;

/// <summary>
/// Service for reconciling subledger transactions with the general ledger
/// </summary>
public class ReconciliationService : IReconciliationService
{
    private readonly AccountingDbContext _context;
    private readonly ILogger<ReconciliationService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ReconciliationService"/> class.
    /// </summary>
    /// <param name="context">The database context.</param>
    /// <param name="logger">The logger.</param>
    public ReconciliationService(AccountingDbContext context, ILogger<ReconciliationService> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<ReconciliationResult> ReconcileSubledgerAsync(string sourceSystem, Guid periodId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Running reconciliation for {SourceSystem} in period {PeriodId}", sourceSystem, periodId);

        // Get subledger total from SubledgerTransactions
        var subledgerTotal = await _context.SubledgerTransactions
            .Where(t => t.SourceSystem == sourceSystem && t.JournalEntry!.PeriodId == periodId)
            .SumAsync(t => t.Amount, cancellationToken);

        // Get GL total from JournalEntryLines for corresponding accounts
        var glTotal = await _context.JournalEntryLines
            .Where(l => l.JournalEntry!.SourceSystem == sourceSystem && l.JournalEntry!.PeriodId == periodId && l.JournalEntry!.Status == EntryStatus.Posted)
            .SumAsync(l => l.DebitAmount > 0 ? l.DebitAmount : l.CreditAmount, cancellationToken) / 2;

        var result = new ReconciliationResult
        {
            SourceSystem = sourceSystem,
            SubledgerTotal = subledgerTotal,
            GeneralLedgerTotal = glTotal
        };

        if (!result.IsBalanced)
        {
            result.Discrepancies.Add($"Variance of {result.Variance} detected between subledger and general ledger.");
        }

        return result;
    }
}
