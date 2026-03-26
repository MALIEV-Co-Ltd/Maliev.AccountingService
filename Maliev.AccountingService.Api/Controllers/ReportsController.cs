using Asp.Versioning;
using Maliev.AccountingService.Api.Services;
using Maliev.AccountingService.Application.Authorization;
using Maliev.Aspire.ServiceDefaults.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Maliev.AccountingService.Api.Controllers;

/// <summary>
/// Controller for generating various financial reports
/// </summary>
[ApiController]
[ApiVersion("1")]
[Route("accounting/v{version:apiVersion}/reports")]
[Authorize]
public class ReportsController : ControllerBase
{
    private readonly IReportingService _reportingService;

    /// <summary>
    /// Initializes a new instance of the <see cref="ReportsController"/> class.
    /// </summary>
    /// <param name="reportingService">The reporting service.</param>
    public ReportsController(IReportingService reportingService)
    {
        _reportingService = reportingService;
    }

    /// <summary>
    /// Generate Balance Sheet
    /// </summary>
    [HttpGet("balance-sheet")]
    [RequirePermission(AccountingPermissions.ReportsBalanceSheet)]
    public async Task<IActionResult> GetBalanceSheet([FromQuery] DateTime? asOfDate)
    {
        var report = await _reportingService.GetBalanceSheetAsync(asOfDate ?? DateTime.UtcNow);
        return Ok(report);
    }

    /// <summary>
    /// Generate Income Statement
    /// </summary>
    [HttpGet("income-statement")]
    [RequirePermission(AccountingPermissions.ReportsIncomeStatement)]
    public async Task<IActionResult> GetIncomeStatement([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
        var report = await _reportingService.GetIncomeStatementAsync(startDate, endDate);
        return Ok(report);
    }

    /// <summary>
    /// Generate Trial Balance
    /// </summary>
    [HttpGet("trial-balance")]
    [RequirePermission(AccountingPermissions.ReportsTrialBalance)]
    public async Task<IActionResult> GetTrialBalance([FromQuery] Guid? periodId, [FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
    {
        var report = await _reportingService.GetTrialBalanceAsync(periodId, startDate, endDate);
        return Ok(report);
    }

    /// <summary>
    /// Export financial reports
    /// </summary>
    [HttpGet("export")]
    [RequirePermission(AccountingPermissions.ReportsExport)]
    public IActionResult ExportReports()
    {
        return Ok(new { message = "Export logic not fully implemented in this migration" });
    }
}
