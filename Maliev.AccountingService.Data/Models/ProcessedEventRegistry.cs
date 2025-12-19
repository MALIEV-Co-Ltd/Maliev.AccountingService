using System.ComponentModel.DataAnnotations;

namespace Maliev.AccountingService.Data.Models;

public class ProcessedEventRegistry
{
    public Guid Id { get; set; }

    [Required]
    [StringLength(200)]
    public string EventId { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string EventType { get; set; } = string.Empty;

    [Required]
    public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;

    public Guid? JournalEntryId { get; set; }

    [StringLength(100)]
    public string? CorrelationId { get; set; }
}
