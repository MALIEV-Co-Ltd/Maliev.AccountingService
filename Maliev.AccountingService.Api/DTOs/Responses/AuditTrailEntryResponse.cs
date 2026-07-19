namespace Maliev.AccountingService.Api.DTOs.Responses;

/// <summary>
/// Response DTO for audit trail entry data
/// </summary>
public class AuditTrailEntryResponse
{
    /// <summary>
    /// Gets or sets the audit entry ID.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the type of the audited entity.
    /// </summary>
    public string EntityType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the ID of the audited entity.
    /// </summary>
    public Guid EntityId { get; set; }

    /// <summary>
    /// Gets or sets the operation performed (e.g., Created, Updated).
    /// </summary>
    public string Operation { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the ID of the user who performed the operation.
    /// </summary>
    public Guid PerformedBy { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the operation was performed.
    /// </summary>
    public DateTime PerformedAt { get; set; }

    /// <summary>
    /// Gets or sets the value before the operation.
    /// </summary>
    public string? BeforeValue { get; set; }

    /// <summary>
    /// Gets or sets the value after the operation.
    /// </summary>
    public string? AfterValue { get; set; }

    /// <summary>
    /// Gets or sets the IP address of the user.
    /// </summary>
    public string? IpAddress { get; set; }

    /// <summary>
    /// Gets or sets the correlation ID for the operation.
    /// </summary>
    public Guid? CorrelationId { get; set; }
}
