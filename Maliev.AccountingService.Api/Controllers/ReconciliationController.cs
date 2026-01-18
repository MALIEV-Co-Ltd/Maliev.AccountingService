using Asp.Versioning;
using Maliev.AccountingService.Api.Services;
using Maliev.AccountingService.Data.Data;
using Maliev.Aspire.ServiceDefaults.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Maliev.AccountingService.Api.Controllers;

/// <summary>
/// Controller for financial reconciliation operations
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("accounting/v{version:apiVersion}/reconciliation")]
[Authorize]
public class ReconciliationController : ControllerBase
{
    private readonly IReconciliationService _reconciliationService;

    /// <summary>
    /// Initializes a new instance of the <see cref="ReconciliationController"/> class.
    /// </summary>
    /// <param name="reconciliationService">The reconciliation service.</param>
    public ReconciliationController(IReconciliationService reconciliationService)
    {
        _reconciliationService = reconciliationService;
    }

    /// <summary>
    /// Runs a reconciliation for a specific source system and period.
    /// </summary>
    /// <param name="sourceSystem">The source system to reconcile (e.g., Sales, Procurement).</param>
    /// <param name="periodId">The ID of the financial period.</param>
    /// <returns>The reconciliation result.</returns>
    [HttpGet("run")]
    [RequirePermission(AccountingPermissions.ReconciliationRun)]
    public async Task<IActionResult> RunReconciliation([FromQuery] string sourceSystem, [FromQuery] Guid periodId)
    {
        var result = await _reconciliationService.ReconcileSubledgerAsync(sourceSystem, periodId);
        return Ok(result);
    }
}
