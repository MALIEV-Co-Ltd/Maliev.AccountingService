using Maliev.AccountingService.Data.Models;

namespace Maliev.AccountingService.Api.Services;

/// <summary>
/// Service for managing financial periods and fiscal years
/// </summary>
public interface IPeriodService
{
    /// <summary>
    /// Gets an existing financial period or creates a new one for the given date.
    /// </summary>
    /// <param name="transactionDate">The date of the transaction.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The financial period.</returns>
    Task<FinancialPeriod> GetOrCreatePeriodAsync(DateTime transactionDate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates a unique entry number for a journal entry within a specific period.
    /// </summary>
    /// <param name="periodId">The ID of the financial period.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A unique entry number string.</returns>
    Task<string> GenerateEntryNumberAsync(Guid periodId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Closes a financial period, preventing further postings.
    /// </summary>
    /// <param name="id">The period ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Success or failure message.</returns>
    Task ClosePeriodAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Reopens a closed financial period.
    /// </summary>
    /// <param name="id">The period ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task ReopenPeriodAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates if a period allows posting for a standard user or requires adjustment approval.
    /// </summary>
    /// <param name="periodId">The period ID.</param>
    /// <param name="isAdjustingEntry">Whether this is an adjusting entry.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task ValidatePeriodForPostingAsync(Guid periodId, bool isAdjustingEntry = false, CancellationToken cancellationToken = default);
}
