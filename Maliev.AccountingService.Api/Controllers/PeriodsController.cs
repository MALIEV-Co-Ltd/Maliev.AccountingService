using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;
using Microsoft.EntityFrameworkCore;
using Maliev.AccountingService.Data.Data;
using Maliev.AccountingService.Data.Models;
using Maliev.Aspire.ServiceDefaults.Authorization;
using Microsoft.AspNetCore.Authorization;

namespace Maliev.AccountingService.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("accounting/v{version:apiVersion}/periods")]
[Authorize]
public class PeriodsController : ControllerBase
{
    private readonly AccountingDbContext _dbContext;

    public PeriodsController(AccountingDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// List all financial periods
    /// </summary>
    [HttpGet]
    [RequirePermission("accounting.periods.open")] // Assuming open permission covers reading
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
    [RequirePermission("accounting.periods.open")]
    public IActionResult OpenPeriod()
    {
        return Ok(new { message = "Period opening logic not fully implemented in this migration" });
    }

    /// <summary>
    /// Close an accounting period (Critical)
    /// </summary>
    [HttpPost("{id}/close")]
    [RequirePermission("accounting.periods.close")]
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
    [RequirePermission("accounting.periods.reopen")]
    public async Task<IActionResult> ReopenPeriod(Guid id)
    {
        var period = await _dbContext.FinancialPeriods.FindAsync(id);
        if (period == null) return NotFound();

        period.Status = PeriodStatus.Open;
        await _dbContext.SaveChangesAsync();

        return Ok(new { message = $"Period {period.Name} reopened successfully" });
    }
}
