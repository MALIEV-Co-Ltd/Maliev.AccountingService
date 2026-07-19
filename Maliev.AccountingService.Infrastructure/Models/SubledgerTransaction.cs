using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Maliev.AccountingService.Infrastructure.Models;

public class SubledgerTransaction
{
    public Guid Id { get; set; }

    [Required]
    [StringLength(50)]
    public string SourceSystem { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string SourceTransactionId { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string TransactionType { get; set; } = string.Empty;

    [Required]
    public DateTime TransactionDate { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }

    [StringLength(500)]
    public string? Description { get; set; }

    public Guid? CustomerId { get; set; }

    public Guid? SupplierId { get; set; }

    [Column(TypeName = "jsonb")]
    public string? Metadata { get; set; }

    public Guid? JournalEntryId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public JournalEntry? JournalEntry { get; set; }
}
