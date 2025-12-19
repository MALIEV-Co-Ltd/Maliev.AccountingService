using System.ComponentModel.DataAnnotations;

namespace Maliev.AccountingService.Api.DTOs.Requests;

/// <summary>
/// Request DTO for creating a journal entry
/// </summary>
public class CreateJournalEntryRequest
{
    [Required]
    public DateTime EntryDate { get; set; }

    [Required]
    [StringLength(500, MinimumLength = 1)]
    public string Description { get; set; } = string.Empty;

    [StringLength(100)]
    public string? Reference { get; set; }

    [Required]
    [MinLength(2, ErrorMessage = "Journal entry must have at least 2 lines")]
    public List<CreateJournalEntryLineRequest> Lines { get; set; } = new();
}

/// <summary>
/// Request DTO for creating a journal entry line
/// </summary>
public class CreateJournalEntryLineRequest
{
    [Required]
    public Guid AccountId { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Debit amount must be non-negative")]
    public decimal DebitAmount { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Credit amount must be non-negative")]
    public decimal CreditAmount { get; set; }

    [StringLength(500)]
    public string? Description { get; set; }

    public Guid? CustomerId { get; set; }

    public Guid? SupplierId { get; set; }

    [StringLength(100)]
    public string? Reference { get; set; }

    public List<CreateTaxComponentRequest>? TaxComponents { get; set; }
}

/// <summary>
/// Request DTO for creating a tax component
/// </summary>
public class CreateTaxComponentRequest
{
    [Required]
    [StringLength(50)]
    public string TaxType { get; set; } = string.Empty;

    [Range(0, 100)]
    public decimal TaxRate { get; set; }

    [Range(0, double.MaxValue)]
    public decimal TaxableAmount { get; set; }

    [Range(0, double.MaxValue)]
    public decimal TaxAmount { get; set; }
}
