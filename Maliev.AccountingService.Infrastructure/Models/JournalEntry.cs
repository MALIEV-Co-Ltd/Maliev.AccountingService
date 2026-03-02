using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Maliev.AccountingService.Infrastructure.Models;

public class JournalEntry
{
    public Guid Id { get; set; }

    [Required]
    public Guid PeriodId { get; set; }

    [Required]
    [StringLength(50)]
    public string EntryNumber { get; set; } = string.Empty;

    [Required]
    public DateTime EntryDate { get; set; }

    [Required]
    [StringLength(500)]
    public string Description { get; set; } = string.Empty;

    [Required]
    public EntryStatus Status { get; set; } = EntryStatus.Draft;

    [StringLength(50)]
    public string? SourceSystem { get; set; }

    [StringLength(100)]
    public string? SourceEventId { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalDebit { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalCredit { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Required]
    public Guid CreatedBy { get; set; }

    public DateTime? PostedAt { get; set; }

    public Guid? PostedBy { get; set; }

    [Timestamp]
    public byte[]? RowVersion { get; set; }

    // Navigation properties
    public FinancialPeriod Period { get; set; } = null!;
    public ICollection<JournalEntryLine> Lines { get; set; } = new List<JournalEntryLine>();
    public AdjustingEntryApproval? AdjustingEntryApproval { get; set; }
}

public enum EntryStatus
{
    Draft,
    Posted
}
