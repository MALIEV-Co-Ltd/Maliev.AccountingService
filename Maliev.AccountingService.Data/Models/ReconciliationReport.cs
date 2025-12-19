using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Maliev.AccountingService.Data.Models;

public class ReconciliationReport
{
    public Guid Id { get; set; }

    [Required]
    public Guid PeriodId { get; set; }

    [Required]
    [StringLength(50)]
    public string ReconciliationType { get; set; } = string.Empty;

    [Required]
    public DateTime RunAt { get; set; } = DateTime.UtcNow;

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal SubledgerTotal { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal GeneralLedgerTotal { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Variance { get; set; }

    [Required]
    [StringLength(50)]
    public string Status { get; set; } = "InProgress";

    [Column(TypeName = "jsonb")]
    public string? DiscrepancyDetails { get; set; }

    public Guid RunBy { get; set; }

    public DateTime? ResolvedAt { get; set; }

    public Guid? ResolvedBy { get; set; }

    [StringLength(1000)]
    public string? ResolutionNotes { get; set; }

    // Navigation properties
    public FinancialPeriod Period { get; set; } = null!;
}
