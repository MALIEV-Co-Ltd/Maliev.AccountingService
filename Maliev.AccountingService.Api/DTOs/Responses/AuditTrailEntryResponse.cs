namespace Maliev.AccountingService.Api.DTOs.Responses;

/// <summary>
/// Response DTO for audit trail entry data
/// </summary>
public class AuditTrailEntryResponse
{
    public Guid Id { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public Guid EntityId { get; set; }
    public string Operation { get; set; } = string.Empty;
    public Guid PerformedBy { get; set; }
    public DateTime PerformedAt { get; set; }
    public string? BeforeValue { get; set; }
    public string? AfterValue { get; set; }
    public string? IpAddress { get; set; }
    public Guid? CorrelationId { get; set; }
}
