using Asp.Versioning;
using Maliev.AccountingService.Api.Services;
using Maliev.AccountingService.Infrastructure.Data;
using Maliev.Aspire.ServiceDefaults.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Maliev.AccountingService.Api.Controllers;

/// <summary>
/// Controller for managing financial periods and fiscal years
/// </summary>
[ApiController]
[ApiVersion("1")]
[Route("accounting/v{version:apiVersion}/periods")]
[Authorize]
public class PeriodsController : ControllerBase
{
    private readonly AccountingDbContext _dbContext;
    private readonly IPeriodService _periodService;

    /// <summary>
    /// Initializes a new instance of the <see cref="PeriodsController"/> class.
    /// </summary>
    /// <param name="dbContext">The database context.</param>
    /// <param name="periodService">The period service.</param>
    public PeriodsController(AccountingDbContext dbContext, IPeriodService periodService)
    {
        _dbContext = dbContext;
        _periodService = periodService;
    }

    /// <summary>
    /// List all financial periods
    /// </summary>
    [HttpGet]
    [RequirePermission(AccountingPermissions.PeriodsOpen)] // Assuming open permission covers reading
    public async Task<IActionResult> GetPeriods()
    {
        var periods = await _dbContext.FinancialPeriods
            .Include(p => p.FiscalYear)
            .OrderByDescending(p => p.StartDate)
            .Select(p => new
            {
                p.Id,
                p.Name,
                p.StartDate,
                p.EndDate,
                Status = p.Status.ToString(),
                FiscalYear = p.FiscalYear.Name
            })
            .ToListAsync();

        return Ok(periods);
    }

    /// <summary>
    /// Open a new period (handled automatically by JournalEntries usually, but exposed here)
    /// </summary>
    [HttpPost("open")]
    [RequirePermission(AccountingPermissions.PeriodsOpen)]
    public async Task<IActionResult> OpenPeriod([FromQuery] DateTime date)
    {
        var period = await _periodService.GetOrCreatePeriodAsync(date);
        return Ok(new
        {
            message = $"Period {period.Name} is open",
            periodId = period.Id
        });
    }

    /// <summary>
    /// Close an accounting period (Critical)
    /// </summary>
    [HttpPost("{id}/close")]
    [RequirePermission(AccountingPermissions.PeriodsClose)]
    public async Task<IActionResult> ClosePeriod(Guid id)
    {
        try
        {
            await _periodService.ClosePeriodAsync(id);
            return Ok(new { message = "Period closed successfully" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Reopen a closed period (Critical)
    /// </summary>
    [HttpPost("{id}/reopen")]
    [RequirePermission(AccountingPermissions.PeriodsReopen)]
    public async Task<IActionResult> ReopenPeriod(Guid id)
    {
        try
        {
            await _periodService.ReopenPeriodAsync(id);
            return Ok(new { message = "Period reopened successfully" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
