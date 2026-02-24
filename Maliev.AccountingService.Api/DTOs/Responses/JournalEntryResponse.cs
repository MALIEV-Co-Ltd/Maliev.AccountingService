namespace Maliev.AccountingService.Api.DTOs.Responses;

/// <summary>
/// Response DTO for journal entry data
/// </summary>
public class JournalEntryResponse
{
    /// <summary>
    /// Gets or sets the journal entry ID.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the unique entry number.
    /// </summary>
    public string EntryNumber { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the entry date.
    /// </summary>
    public DateTime EntryDate { get; set; }

    /// <summary>
    /// Gets or sets the description.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the status (Draft, Posted).
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the period ID.
    /// </summary>
    public Guid? PeriodId { get; set; }

    /// <summary>
    /// Gets or sets the period name.
    /// </summary>
    public string? PeriodName { get; set; }

    /// <summary>
    /// Gets or sets the total debit amount.
    /// </summary>
    public decimal TotalDebit { get; set; }

    /// <summary>
    /// Gets or sets the total credit amount.
    /// </summary>
    public decimal TotalCredit { get; set; }

    /// <summary>
    /// Gets or sets the source event ID.
    /// </summary>
    public Guid? SourceEventId { get; set; }

    /// <summary>
    /// Gets or sets the reference.
    /// </summary>
    public string? Reference { get; set; }

    /// <summary>
    /// Gets or sets the creation date.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the date when posted.
    /// </summary>
    public DateTime? PostedAt { get; set; }

    /// <summary>
    /// Gets or sets the ID of the user who posted the entry.
    /// </summary>
    public Guid? PostedBy { get; set; }

    /// <summary>
    /// Gets or sets the entry lines.
    /// </summary>
    public List<JournalEntryLineResponse> Lines { get; set; } = new();
}

/// <summary>
/// Response DTO for journal entry line data
/// </summary>
public class JournalEntryLineResponse
{
    /// <summary>
    /// Gets or sets the entry line ID.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the line number.
    /// </summary>
    public int LineNumber { get; set; }

    /// <summary>
    /// Gets or sets the account ID.
    /// </summary>
    public Guid AccountId { get; set; }

    /// <summary>
    /// Gets or sets the account number.
    /// </summary>
    public string AccountNumber { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the account name.
    /// </summary>
    public string AccountName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the debit amount.
    /// </summary>
    public decimal DebitAmount { get; set; }

    /// <summary>
    /// Gets or sets the credit amount.
    /// </summary>
    public decimal CreditAmount { get; set; }

    /// <summary>
    /// Gets or sets the description.
    /// </summary>
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
    public string? Reference { get; set; }

    /// <summary>
    /// Gets or sets the tax components.
    /// </summary>
    public List<TaxComponentResponse>? TaxComponents { get; set; }
}

/// <summary>
/// Response DTO for tax component data
/// </summary>
public class TaxComponentResponse
{
    /// <summary>
    /// Gets or sets the tax component ID.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the tax type.
    /// </summary>
    public string TaxType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the tax rate.
    /// </summary>
    public decimal TaxRate { get; set; }

    /// <summary>
    /// Gets or sets the taxable amount.
    /// </summary>
    public decimal TaxableAmount { get; set; }

    /// <summary>
    /// Gets or sets the tax amount.
    /// </summary>
    public decimal TaxAmount { get; set; }
}
