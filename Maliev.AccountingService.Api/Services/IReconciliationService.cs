namespace Maliev.AccountingService.Api.Services;

/// <summary>
/// Interface for financial reconciliation service
/// </summary>
public interface IReconciliationService
{
    /// <summary>
    /// Reconciles subledger transactions against the general ledger for a specific source system and period.
    /// </summary>
    Task<ReconciliationResult> ReconcileSubledgerAsync(string sourceSystem, Guid periodId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Represents the result of a reconciliation operation
/// </summary>
public class ReconciliationResult
{
    /// <summary>Source system name</summary>
    public string SourceSystem { get; set; } = string.Empty;
    /// <summary>Total from the subledger</summary>
    public decimal SubledgerTotal { get; set; }
    /// <summary>Total from the general ledger</summary>
    public decimal GeneralLedgerTotal { get; set; }
    /// <summary>Difference between subledger and GL</summary>
    public decimal Variance => SubledgerTotal - GeneralLedgerTotal;
    /// <summary>True if the variance is zero</summary>
    public bool IsBalanced => Math.Abs(Variance) < 0.01m;
    /// <summary>List of detected discrepancies</summary>
    public List<string> Discrepancies { get; set; } = new();
}
