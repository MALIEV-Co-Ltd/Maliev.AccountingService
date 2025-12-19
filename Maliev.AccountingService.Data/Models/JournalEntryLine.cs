using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Maliev.AccountingService.Data.Models;

public class JournalEntryLine
{
    public Guid Id { get; set; }

    [Required]
    public Guid JournalEntryId { get; set; }

    [Required]
    public Guid AccountId { get; set; }

    [Required]
    public int LineSequence { get; set; }

    [StringLength(500)]
    public string? Description { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal DebitAmount { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal CreditAmount { get; set; }

    [StringLength(50)]
    public string? ReferenceType { get; set; }

    [StringLength(100)]
    public string? ReferenceId { get; set; }

    public Guid? CustomerId { get; set; }

    public Guid? SupplierId { get; set; }

    // Navigation properties
    public JournalEntry JournalEntry { get; set; } = null!;
    public ChartOfAccount Account { get; set; } = null!;
    public ICollection<TaxComponent> TaxComponents { get; set; } = new List<TaxComponent>();
}
