using Asp.Versioning;
using Maliev.AccountingService.Api.Services;
using Maliev.AccountingService.Infrastructure.Data;
using Maliev.Aspire.ServiceDefaults.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Maliev.AccountingService.Api.Controllers;

/// <summary>
/// Controller for bulk importing chart of accounts and opening balances
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("accounting/v{version:apiVersion}/bulk-import")]
public class BulkImportController : ControllerBase
{
    private readonly IBulkImportService _bulkImportService;
    private readonly ILogger<BulkImportController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="BulkImportController"/> class.
    /// </summary>
    /// <param name="bulkImportService">The bulk import service.</param>
    /// <param name="logger">The logger.</param>
    public BulkImportController(IBulkImportService bulkImportService, ILogger<BulkImportController> logger)
    {
        _bulkImportService = bulkImportService;
        _logger = logger;
    }

    /// <summary>
    /// Bulk import chart of accounts from CSV or JSON file
    /// </summary>
    /// <param name="file">CSV or JSON file containing chart of accounts</param>
    /// <param name="dryRun">If true, only validates without importing (default: false)</param>
    /// <returns>Import result with statistics and any errors</returns>
    /// <response code="200">Import completed (check Success field for actual result)</response>
    /// <response code="400">Invalid file or file format</response>
    /// <response code="401">Unauthorized</response>
    [HttpPost("chart-of-accounts")]
    [RequirePermission(AccountingPermissions.AccountsCreate)]
    [ProducesResponseType(typeof(Models.BulkImportResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ImportChartOfAccounts(
        IFormFile file,
        [FromQuery] bool dryRun = false)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new { error = "No file uploaded" });
        }

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (extension != ".csv" && extension != ".json")
        {
            return BadRequest(new { error = "File must be .csv or .json format" });
        }

        _logger.LogInformation("Received chart of accounts import request: {FileName}, Size: {Size} bytes, DryRun: {DryRun}",
            file.FileName, file.Length, dryRun);

        using var stream = file.OpenReadStream();
        var result = await _bulkImportService.ImportChartOfAccountsAsync(stream, file.FileName, dryRun);

        return Ok(result);
    }

    /// <summary>
    /// Bulk import opening balances from CSV or JSON file
    /// </summary>
    /// <param name="file">CSV or JSON file containing opening balances</param>
    /// <param name="dryRun">If true, only validates without importing (default: false)</param>
    /// <returns>Import result with statistics and any errors</returns>
    /// <response code="200">Import completed (check Success field for actual result)</response>
    /// <response code="400">Invalid file or file format</response>
    /// <response code="401">Unauthorized</response>
    [HttpPost("opening-balances")]
    [RequirePermission(AccountingPermissions.AccountsCreate)]
    [ProducesResponseType(typeof(Models.BulkImportResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ImportOpeningBalances(
        IFormFile file,
        [FromQuery] bool dryRun = false)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new { error = "No file uploaded" });
        }

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (extension != ".csv" && extension != ".json")
        {
            return BadRequest(new { error = "File must be .csv or .json format" });
        }

        _logger.LogInformation("Received opening balances import request: {FileName}, Size: {Size} bytes, DryRun: {DryRun}",
            file.FileName, file.Length, dryRun);

        using var stream = file.OpenReadStream();
        var result = await _bulkImportService.ImportOpeningBalancesAsync(stream, file.FileName, dryRun);

        return Ok(result);
    }
}
