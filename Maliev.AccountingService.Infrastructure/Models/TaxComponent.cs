using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Maliev.AccountingService.Infrastructure.Models;

public class TaxComponent
{
    public Guid Id { get; set; }

    [Required]
    public Guid JournalEntryLineId { get; set; }

    [Required]
    [StringLength(50)]
    public string TaxType { get; set; } = string.Empty;

    [Required]
    [Column(TypeName = "decimal(5,2)")]
    public decimal TaxRate { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal TaxableAmount { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal TaxAmount { get; set; }

    [StringLength(50)]
    public string? TaxJurisdiction { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public JournalEntryLine JournalEntryLine { get; set; } = null!;
}
