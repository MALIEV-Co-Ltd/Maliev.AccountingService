using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;
using Microsoft.EntityFrameworkCore;
using Maliev.AccountingService.Data.Data;
using Maliev.AccountingService.Data.Models;
using Maliev.Aspire.ServiceDefaults.Authorization;
using Microsoft.AspNetCore.Authorization;

namespace Maliev.AccountingService.Api.Controllers;

/// <summary>
/// Controller for managing financial periods and fiscal years
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("accounting/v{version:apiVersion}/periods")]
[Authorize]
public class PeriodsController : ControllerBase
{
    private readonly AccountingDbContext _dbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="PeriodsController"/> class.
    /// </summary>
    /// <param name="dbContext">The database context.</param>
    public PeriodsController(AccountingDbContext dbContext)
    {
        _dbContext = dbContext;
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
    public IActionResult OpenPeriod()
    {
        return Ok(new { message = "Period opening logic not fully implemented in this migration" });
    }

    /// <summary>
    /// Close an accounting period (Critical)
    /// </summary>
    [HttpPost("{id}/close")]
    [RequirePermission(AccountingPermissions.PeriodsClose)]
    public async Task<IActionResult> ClosePeriod(Guid id)
    {
        var period = await _dbContext.FinancialPeriods.FindAsync(id);
        if (period == null) return NotFound();

        period.Status = PeriodStatus.Closed;
        await _dbContext.SaveChangesAsync();

        return Ok(new { message = $"Period {period.Name} closed successfully" });
    }

    /// <summary>
    /// Reopen a closed period (Critical)
    /// </summary>
    [HttpPost("{id}/reopen")]
    [RequirePermission(AccountingPermissions.PeriodsReopen)]
    public async Task<IActionResult> ReopenPeriod(Guid id)
    {
        var period = await _dbContext.FinancialPeriods.FindAsync(id);
        if (period == null) return NotFound();

        period.Status = PeriodStatus.Open;
        await _dbContext.SaveChangesAsync();

        return Ok(new { message = $"Period {period.Name} reopened successfully" });
    }
}
