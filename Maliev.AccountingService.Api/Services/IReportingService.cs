using Maliev.AccountingService.Api.DTOs.Responses;

namespace Maliev.AccountingService.Api.Services;

/// <summary>
/// Service for generating financial reports
/// </summary>
public interface IReportingService
{
    /// <summary>
    /// Generates a Trial Balance report for a specific period or date range.
    /// </summary>
    Task<TrialBalanceResponse> GetTrialBalanceAsync(Guid? periodId, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates a Balance Sheet report as of a specific date.
    /// </summary>
    Task<BalanceSheetResponse> GetBalanceSheetAsync(DateTime asOfDate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates an Income Statement report for a specific date range.
    /// </summary>
    Task<IncomeStatementResponse> GetIncomeStatementAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
}
