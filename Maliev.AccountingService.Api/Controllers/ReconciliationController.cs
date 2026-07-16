using Asp.Versioning;
using Maliev.AccountingService.Api.Services;
using Maliev.AccountingService.Application.Authorization;
using Maliev.Aspire.ServiceDefaults.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Maliev.AccountingService.Api.Controllers;

/// <summary>
/// Controller for financial reconciliation operations
/// </summary>
[ApiController]
[ApiVersion("1")]
[Route("accounting/v{version:apiVersion}/reconciliation")]
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
    /// <param name="cancellationToken">A token that can cancel the reconciliation.</param>
    /// <returns>The reconciliation result.</returns>
    [HttpGet("run")]
    [RequirePermission(AccountingPermissions.ReconciliationRun)]
    [Obsolete("Use POST /accounting/v1/reconciliation/run for state-changing reconciliation operations.")]
    public Task<IActionResult> RunReconciliation(
        [FromQuery] string sourceSystem,
        [FromQuery] Guid periodId,
        CancellationToken cancellationToken)
    {
        return ExecuteReconciliationAsync(sourceSystem, periodId, cancellationToken);
    }

    /// <summary>
    /// Runs a reconciliation for a specific source system and period.
    /// </summary>
    /// <param name="sourceSystem">The source system to reconcile (e.g., Sales, Procurement).</param>
    /// <param name="periodId">The ID of the financial period.</param>
    /// <param name="cancellationToken">A token that can cancel the reconciliation.</param>
    /// <returns>The reconciliation result.</returns>
    [HttpPost("run")]
    [RequirePermission(AccountingPermissions.ReconciliationRun)]
    public Task<IActionResult> RunReconciliationPost(
        [FromQuery] string sourceSystem,
        [FromQuery] Guid periodId,
        CancellationToken cancellationToken)
    {
        return ExecuteReconciliationAsync(sourceSystem, periodId, cancellationToken);
    }

    private async Task<IActionResult> ExecuteReconciliationAsync(
        string sourceSystem,
        Guid periodId,
        CancellationToken cancellationToken)
    {
        var result = await _reconciliationService.ReconcileSubledgerAsync(
            sourceSystem,
            periodId,
            cancellationToken);
        return Ok(result);
    }
}
