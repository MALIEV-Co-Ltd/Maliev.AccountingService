using System.ComponentModel.DataAnnotations;

namespace Maliev.AccountingService.Api.DTOs.Requests;

/// <summary>
/// Request DTO for creating a journal entry
/// </summary>
public class CreateJournalEntryRequest
{
    /// <summary>
    /// Gets or sets the entry date.
    /// </summary>
    [Required]
    public DateTime EntryDate { get; set; }

    /// <summary>
    /// Gets or sets the description.
    /// </summary>
    [Required]
    [StringLength(500, MinimumLength = 1)]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the reference.
    /// </summary>
    [StringLength(100)]
    public string? Reference { get; set; }

    /// <summary>
    /// Gets or sets the ISO 4217 transaction currency code.
    /// </summary>
    [StringLength(3, MinimumLength = 3)]
    public string CurrencyCode { get; set; } = "THB";

    /// <summary>
    /// Gets or sets the multiplier from the transaction currency to the accounting base currency.
    /// </summary>
    public decimal ExchangeRateToBase { get; set; } = 1m;

    /// <summary>
    /// Gets or sets the entry lines.
    /// </summary>
    [Required]
    [MinLength(2, ErrorMessage = "Journal entry must have at least 2 lines")]
    public List<CreateJournalEntryLineRequest> Lines { get; set; } = new();
}

/// <summary>
/// Request DTO for creating a journal entry line
/// </summary>
public class CreateJournalEntryLineRequest
{
    /// <summary>
    /// Gets or sets the account ID.
    /// </summary>
    [Required]
    public Guid AccountId { get; set; }

    /// <summary>
    /// Gets or sets the debit amount.
    /// </summary>
    [Range(0, (double)decimal.MaxValue, ErrorMessage = "Debit amount must be non-negative")]
    public decimal DebitAmount { get; set; }

    /// <summary>
    /// Gets or sets the credit amount.
    /// </summary>
    [Range(0, (double)decimal.MaxValue, ErrorMessage = "Credit amount must be non-negative")]
    public decimal CreditAmount { get; set; }

    /// <summary>
    /// Gets or sets the debit amount in the original transaction currency.
    /// </summary>
    [Range(0, (double)decimal.MaxValue, ErrorMessage = "Transaction debit amount must be non-negative")]
    public decimal? TransactionDebitAmount { get; set; }

    /// <summary>
    /// Gets or sets the credit amount in the original transaction currency.
    /// </summary>
    [Range(0, (double)decimal.MaxValue, ErrorMessage = "Transaction credit amount must be non-negative")]
    public decimal? TransactionCreditAmount { get; set; }

    /// <summary>
    /// Gets or sets the description.
    /// </summary>
    [StringLength(500)]
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the customer ID.
    /// </summary>
    public Guid? CustomerId { get; set; }

    /// <summary>
    /// Gets or sets the supplier ID.
    /// </summary>
    public Guid? SupplierId { get; set; }

    /// <summary>
    /// Gets or sets the reference.
    /// </summary>
    [StringLength(100)]
    public string? Reference { get; set; }

    /// <summary>
    /// Gets or sets the tax components.
    /// </summary>
    public List<CreateTaxComponentRequest>? TaxComponents { get; set; }
}

/// <summary>
/// Request DTO for creating a tax component
/// </summary>
public class CreateTaxComponentRequest
{
    /// <summary>
    /// Gets or sets the tax type.
    /// </summary>
    [Required]
    [StringLength(50)]
    public string TaxType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the tax rate.
    /// </summary>
    [Range(0, 100)]
    public decimal TaxRate { get; set; }

    /// <summary>
    /// Gets or sets the taxable amount.
    /// </summary>
    [Range(0, (double)decimal.MaxValue)]
    public decimal TaxableAmount { get; set; }

    /// <summary>
    /// Gets or sets the tax amount.
    /// </summary>
    [Range(0, (double)decimal.MaxValue)]
    public decimal TaxAmount { get; set; }
}
