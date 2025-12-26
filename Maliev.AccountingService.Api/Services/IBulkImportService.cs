using Maliev.AccountingService.Api.Models;

namespace Maliev.AccountingService.Api.Services;

/// <summary>
/// Service for bulk importing chart of accounts and opening balances from CSV/JSON files
/// </summary>
public interface IBulkImportService
{
    /// <summary>
    /// Imports chart of accounts from CSV or JSON file
    /// </summary>
    /// <param name="stream">File stream containing CSV or JSON data</param>
    /// <param name="fileName">Original file name (used to detect format)</param>
    /// <param name="dryRun">If true, only validates without importing</param>
    /// <returns>Import result with statistics and errors</returns>
    Task<BulkImportResult> ImportChartOfAccountsAsync(Stream stream, string fileName, bool dryRun = false);

    /// <summary>
    /// Imports opening balances from CSV or JSON file
    /// </summary>
    /// <param name="stream">File stream containing CSV or JSON data</param>
    /// <param name="fileName">Original file name (used to detect format)</param>
    /// <param name="dryRun">If true, only validates without importing</param>
    /// <returns>Import result with statistics and errors</returns>
    Task<BulkImportResult> ImportOpeningBalancesAsync(Stream stream, string fileName, bool dryRun = false);
}
