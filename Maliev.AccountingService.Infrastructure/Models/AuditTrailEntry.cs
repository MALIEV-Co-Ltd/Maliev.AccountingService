using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Maliev.AccountingService.Infrastructure.Models;

public class AuditTrailEntry
{
    public Guid Id { get; set; }

    [Required]
    [StringLength(100)]
    public string EntityType { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string EntityId { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string Action { get; set; } = string.Empty;

    [Required]
    public Guid PerformedBy { get; set; }

    [Required]
    public DateTime PerformedAt { get; set; } = DateTime.UtcNow;

    [Column(TypeName = "jsonb")]
    public string? BeforeSnapshot { get; set; }

    [Column(TypeName = "jsonb")]
    public string? AfterSnapshot { get; set; }

    [StringLength(100)]
    public string? CorrelationId { get; set; }

    [StringLength(50)]
    public string? IpAddress { get; set; }

    [StringLength(500)]
    public string? Reason { get; set; }
}
